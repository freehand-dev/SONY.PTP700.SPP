

cns_protocol = Proto("CNS",  "Sony Camera Network System")

-- Packet Header
_header					= ProtoField.new(	"[Header]",							"cns.header",						ftypes.BYTES)

-- Message50
_is_message50 			= ProtoField.new(	"[Message] is Type = 50",			"cns.is_message50",					ftypes.BOOLEAN)

-- Handshake
local _handshake = {
	unknown_1			= ProtoField.new(	"[Handshake][Unknonw_1]",			"cns.handshake.unknown_1",			ftypes.BYTES),	
	mode				= ProtoField.new(	"[Handshake][Mode]",				"cns.handshake.mode",				ftypes.BYTES),	
	id					= ProtoField.new(	"[Handshake][Id]",					"cns.handshake.id",					ftypes.BYTES),	
	unknown_2			= ProtoField.new(	"[Handshake][Unknonw_2]",			"cns.handshake.unknown_2",			ftypes.BYTES),	
	device_type			= ProtoField.new(	"[Handshake][Device Type]",			"cns.handshake.device_type",		ftypes.BYTES),	
	model				= ProtoField.new(	"[Handshake][Model]",				"cns.handshake.model",				ftypes.BYTES),	
	serial_number		= ProtoField.new(	"[Handshake][Serial Number]",		"cns.handshake.serial_number",		ftypes.BYTES),	
	unknonw_3			= ProtoField.new(	"[Handshake][Unknown 3]",			"cns.handshake.unknonw_3",			ftypes.BYTES)	
}

local _messageResponse = {
	header				= ProtoField.new(	"[MessageResponse][Header]",		"cns.message_response.header",		ftypes.BYTES),
	size				= ProtoField.new(	"[MessageResponse]Length]",			"cns.message_response.size",		ftypes.UINT8),
	id 					= ProtoField.new(	"[MessageResponse][ID]",			"cns.message_response.id",			ftypes.BYTES)
}

-- Message
local _message = {
	header				= ProtoField.new(	"[Message][Header]",				"cns.message.header",				ftypes.BYTES),
	size				= ProtoField.new(	"[Message]Length]",					"cns.message.size",					ftypes.UINT8),
	id 					= ProtoField.new(	"[Message][ID]",					"cns.message.id",					ftypes.BYTES),
	type 				= ProtoField.new(	"[Message][Type]",					"cns.message.type",					ftypes.BYTES),
	cmd_buffer			= ProtoField.new(	"[Message][Buffer]",				"cns.message.cmd_buffer",			ftypes.BYTES)	
}

local _message50 = {
	unknown_1 			= ProtoField.new(	"[Message50] Unknown1", 			"cns.message50.unknown_1",  		ftypes.BYTES),
	sub_type 			= ProtoField.new(	"[Message50] SUB TYPE",  			"cns.message50.sub_type", 			ftypes.BYTES),
	ccu_no_1 			= ProtoField.new(	"[Message50] CCU no ", 				"cns.message50.ccu_no_1",  			ftypes.BYTES),
	unknown_2 			= ProtoField.new( 	"[Message50] Unknown2", 			"cns.message50.unknown_2", 			ftypes.BYTES),
	ccu_no_2 			= ProtoField.new( 	"[Message50] CCU no", 				"cns.message50.ccu_no_2", 			ftypes.BYTES),
	cmd_size 			= ProtoField.new(	"[Message50] CMD SIZE",				"cns.message50.cmd_size",  			ftypes.UINT16),
	cmd_buffer 			= ProtoField.new(	"[Message50] CMD Buffer",			"cns.message50.cmd_buffer",			ftypes.BYTES)	
}		


cns_protocol.fields = {
	_header, 

	_handshake.unknown_1,	
	_handshake.mode,
	_handshake.id,	
	_handshake.unknown_2,	
	_handshake.device_type,	
	_handshake.model,	
	_handshake.serial_number,
	_handshake.unknonw_3,

	_messageResponse.header,
	_messageResponse.size,
	_messageResponse.id,

	_message.header,
	_message.size,
	_message.id,
	_message.type,
	_message.cmd_buffer,

	_is_message50,
	_message50.unknown_1,
	_message50.sub_type,
	_message50.ccu_no_1,
	_message50.unknown_2,
	_message50.ccu_no_2,
	_message50.cmd_size,
	_message50.cmd_buffer
 }
		
function GetPacketType(header)
	if header == 0x01 then 
		return "handshake reply" 
	elseif header == 0x02 then 
		return "handshake request" 
	elseif  header == 0x03 then 
		return "handshake acknowledge" 
	elseif  header == 0x08 then 
		return "heartbeat" 
	elseif  header == 0x09 then 
		return "heartbeat acknowledge" 
	elseif  header == 0x0a then 
		return "notify" 
	elseif  header == 0x0b then 
		return "notify acknowledge" 
	elseif  header == 0x0c then 
		return "undefined (ID missmatch)" 
	elseif  header == 0x0d then 
		return "error" 
	elseif  header == 0x0e then 
		return "message request" 
	elseif  header == 0x0f then 
		return "message response" 
	else
		return "unknown"
	end
end




function cns_protocol.dissector(tvb, pinfo, tree)
	pinfo.cols.protocol = cns_protocol.name;
	
	subtree = tree:add(cns_protocol, tvb())
	
	pkt_header = tvb(0,1):le_uint()
	pkt_header_str = GetPacketType(pkt_header)

	-- header
	subtree:add_le(_header, tvb(0,1)):append_text(" (" .. pkt_header_str .. ")")

	-- handshake
	if pkt_header == 0x01 or pkt_header == 0x02 or pkt_header == 0x03 then				
		pareseHandshake(tvb, pinfo, tree);		
	end

	-- request
	if pkt_header == 0x0e then	
		-- Packet Header (0,1)
		subtree:add_le(_message.header, tvb(0,1))
		-- Packet Size (1,1)
		subtree:add_le(_message.size, tvb(1,1))
		-- ID (2,2)
		subtree:add(_message.id, tvb(2,2))
		if tvb:len() > 4 then
			-- Type (4,1)		
			subtree:add(_message.type, tvb(4, 1))
			parseCommand(tvb, pinfo, tree, 0)
		end		
	end

	-- response
	if pkt_header == 0x0f then	
		-- Packet Header (0,1)
		subtree:add_le(_messageResponse.header, tvb(0,1))
		-- Packet Size (1,1)
		subtree:add_le(_messageResponse.size, tvb(1,1))
		-- Packet Size (1,1)
		subtree:add_le(_messageResponse.id, tvb(2,2))
		-- exists reqiest packet
		if tvb:len() > 4 then
			-- Packet Header (0,1)
			subtree:add_le(_message.header, tvb(4,1))
			-- Packet Size (1,1)
			subtree:add_le(_message.size, tvb(5,1))
			-- ID (2,2)
			subtree:add(_message.id, tvb(6,2))
			if tvb:len() > 4 then
				-- Type (4,1)		
				subtree:add(_message.type, tvb(8, 1))
				parseCommand(tvb, pinfo, tree, 4)
			end			
		end	
	end
end

function parseCommand(tvb, pinfo, tree, offset)

	-- buffer
	subtree:add(_message.cmd_buffer, tvb(offset + 4, (tvb:len() - (offset + 4))))

	_type = tvb(offset + 4, 1):le_uint()
	if _type == 0x50 then
		subtree:add(_is_message50, true)
		-- UNKNOWN (5,1)
		subtree:add(_message50.unknown_1, tvb(offset + 5, 1))		
		-- SUB TYPE (6,1)
		subtree:add(_message50.sub_type, tvb(offset + 6, 1))
		-- CCU no (7,2)
		subtree:add(_message50.ccu_no_1, tvb(offset + 7, 2))
		-- Unknown (9,2)
		subtree:add(_message50.unknown_2, tvb(offset + 9, 2))
		-- CCU no (11,2)
		subtree:add(_message50.ccu_no_2, tvb(offset + 11, 2))
		-- CMD SIZE (13,2)
		subtree:add(_message50.cmd_size, tvb(offset + 13, 2))
		-- CMD BUFFER (15,...)
		subtree:add(_message50.cmd_buffer, tvb(offset + 15, (tvb:len() - (offset + 15))))
	end
end

function pareseHandshake(tvb, pinfo, tree)
	-- UNKNOWN1 
	subtree:add(_handshake.unknown_1, tvb(2,3))
	-- MODE
	cns_mode = tvb(5,1):le_uint()
	cns_mode_str = "unknown"
	if cns_mode == 0 then cns_mode_str = "legacy" end
	if cns_mode == 1 then cns_mode_str = "bridge" end
	if cns_mode == 2 then cns_mode_str = "mcs" end
	subtree:add_le(_handshake.mode, tvb(5,1)):append_text(" (" .. cns_mode_str .. ")")
	-- ID
	subtree:add(_handshake.id, tvb(6,2))
	-- UNKNOWN2
	subtree:add(_handshake.unknown_2, tvb(8,1))
	-- DEVICE TYPE
	subtree:add(_handshake.device_type, tvb(9,2))
	-- MODEL
	subtree:add(_handshake.model, tvb(11,1))
	-- SERIAL NUMBER
	subtree:add(_handshake.serial_number, tvb(12,4))
	-- UNKNOWN3
	subtree:add(_handshake.unknonw_3, tvb(16,4))
end


local tcp_port = DissectorTable.get("tcp.port")
tcp_port:add(7700,cns_protocol)

if (gui_enabled()) then 
	local filename = ""

	local ws_fieldlist = {"frame.number","ip.src","ip.dst","cns.header","cns.is_message50"}

	local ws_fields = {}
		for fieldnum = 1, #ws_fieldlist do
			ws_fields[fieldnum] = Field.new(ws_fieldlist[fieldnum])
		end
	
	local function export_packets()
	-- create tap
	local tap = Listener.new("frame", "cns") -- filter on tcp
	-- this function will be called for every packet which matches the Listener filter
	function tap.packet(pinfo,tvb,tapdata)
		-- read all the fields we want into local variables and sanitize
		local output_fields = {}
		for fieldnum = 1, #ws_fieldlist do
			output_fields[fieldnum] = ws_fields[fieldnum]()

			if output_fields[fieldnum] then
				-- field exists, get value and sanitize it
				-- we use .label rather than .value, so that frame.time field is coerced into a string rather than an integer (epoch)
				--  and boolean types return 1/0 rather than true/false
				output_fields[fieldnum] = string("\""):append_text(output_fields[fieldnum].label):append_text("\"")
				if output_fields[fieldnum] == "(none)" then
					-- probably the tcp.analysis fields, which either don't exist if not applicable, or have a value of nil/(none) if they do. Convert to 1
					-- note that these fields return "nil" if tostring(field.value) but "(none)" if tostring(field) or field.label
					output_fields[fieldnum] = 1
				elseif tonumber(output_fields[fieldnum]) == nil then
					-- must be a string field, quote it
					-- note, will also quote IP addresses
					output_fields[fieldnum] = '"'..output_fields[fieldnum]..'"'
				end
			else
				-- field doesn't exist in this packet or has nil value
				output_fields[fieldnum] = ""
			end
		end -- for fieldnum loop
		
		-- now write this packet to file
		thisline = ""
		for fieldnum = 1, #output_fields do
			thisline = thisline..output_fields[fieldnum]..","
		end
		file:write(thisline.."\n")
		file:flush()
	end --tap.packet function

	-- this is where things actually start happening for an existing capture.
	-- Not sure what will happen if you run a live capture with this function, but I assume it will be bad
	retap_packets()
	tap:remove()
end -- export packets function


	local function export_capture(filename)
		file = assert(io.open(filename, "w+b"))
		-- output the header line
		local thisline = table.concat(ws_fieldlist,",").."\n"
		file:write(thisline)
		file:flush()
		export_packets()
		file:close()
	end

	-- menu functions
	local function Sony_cns_export()
		--local current_dir= "/Users/"
		local filename = "Enter path and filename (" .. filename .. ")"
		local response = new_dialog("Wireshark cns-tap: Export to CSV",export_capture,filename)
	end
	local function Sony_cns_help()
		browser_open_url("https://gitlab.com/tpc-Produktion/sonymicgainremote")
	end

	-- register the menu
	register_menu("Sony cns/Export cns packets to file",Sony_cns_export,MENU_TOOLS_UNSORTED)
	register_menu("Sony cns/Help", Sony_cns_help, MENU_TOOLS_UNSORTED)

end --gui enabled
