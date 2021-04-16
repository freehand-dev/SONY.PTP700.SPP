using System;
using System.Collections.Generic;
using SONY.PTP700.SPP;
using SONY.PTP700.SPP.PacketFactory;
using SONY.PTP700.SPP.PacketFactory.Command;
using SONY.PTP700.SPP.Utils;
using Xunit;

namespace XUnitTestSpp
{
    public class UnitTest1
    {
        [Fact]
        public void ParseMultiPacket()
        {
            // 3 packet in one data buffer
            byte[] data = new byte[] { 0x0f, 0x02, 0x64, 0x6c, 0x0f, 0x02, 0x64, 0x6d, 0x0e, 0x0b, 0x79, 0x49, 0x02, 0x80, 0x00, 0x00, 0x01, 0x00, 0xc0, 0x80, 0xc0 };
            
            //Arrange
            BasicPacket _packet = BasicPacket.InitPacket(data);

            //Act
            bool isValid = _packet != null && _packet.NextPacket != null && _packet.NextPacket.NextPacket != null;

            //Assert
            Assert.True(isValid, $"Failed to parse multipacket data");

        }


        [Fact]
        public void ParseMessage50_cmdGP_50_0()
        {
            // 
            byte[] data = new byte[] { 0x0e, 0x18, 0x48, 0x9f, 0x50, 0x18, 0x01, 0x05, 0x00, 0x18, 0x40, 0x05, 0x00, 0x00, 0x0b, 0x50, 0x02, 0x08, 0x12, 0x00, 0xc2, 0x50, 0xc3, 0x00, 0xcb, 0x20 };

            //Arrange
            BasicPacket _packet = BasicPacket.InitPacket(data);
            Message50.SPpCommandPair[] _commands = (_packet as Message50).Commands.ToArray();

            //Act
            bool isValid = _packet != null;

            //Assert
            Assert.True(isValid, $"Failed to parse Message50 CMD_GP = 50 packet");
        }


        [Fact]
        public void ParseMessage30_cmdGP_03_0()
        {
            // 
            byte[] data = new byte[] { 0x0e, 0x0f, 0x34, 0xf4, 0x30, 0x06, 0x00, 0x12, 0x90, 0x01, 0x91, 0x81, 0x81, 0x82, 0x81, 0x03, 0x81 };

            //Arrange
            BasicPacket _packet = BasicPacket.InitPacket(data);
            Message30.SPpCommandPair[] _commands = (_packet as Message30).Commands.ToArray();

            //Act & Assert
            Assert.True((_packet != null), $"Failed to parse Message50 CMD_GP = 50 packet");
            Assert.True((_commands.Length == 3), $"Command Length is incorect");
            Assert.True((_commands[0].CMD_GP == 0x81 && _commands[0].PARAM0 == 0x81), $"Command Index 0  is incorect");
            Assert.True((_commands[1].CMD_GP == 0x82 && _commands[0].PARAM0 == 0x81), $"Command Index 0  is incorect");
            Assert.True((_commands[2].CMD_GP == 0x03 && _commands[0].PARAM0 == 0x81), $"Command Index 0  is incorect");

        }









        [Fact]
        public void ParseMessage20_GetCcuList()
        {
            // 
            byte[] data = new byte[] { 0x0e, 0x1a, 0x62, 0x27, 0x20, 0x92, 0xd9, 0xfe, 0x12, 0x90, 0x02, 0x31, 0xd9, 0xfe, 0x58, 0xff, 0x00, 0x02, 0x58, 0x06, 0x5a, 0x00, 0x01, 0x58, 0x0d, 0x5a, 0x00, 0x01 };
            List<int> list = new List<int>();

            //Arrange
            BasicPacket _packet = BasicPacket.InitPacket(data);
            Message20.SPpCommandPair[] _commands = (_packet as Message20).Commands.ToArray();

            //Act & Assert
            Assert.True((_packet != null), $"Failed to parse Message20 CMD_GP = 20 packet");

            for (int i = 0; i < _commands.Length; i++)
            {
                if (ByteUtils.HasPrefix(_commands[i].ToArray(), new byte[] { 0xd9, 0xfe, 0x58, 0xff }, 0))
                {
                    
                    var buffer = _commands[i].ToArray();
                    int size = ByteUtils.ReverseBytes(BitConverter.ToUInt16(buffer, 4));

                    var pos = 6;
                    for (var j = 0; j < size;  j++)
                    {

                        list.Add((int)buffer[pos + 1]);
                        pos += 5;
                    }
                }
            }

            Assert.Equal(2, list.Count);
            Assert.Equal(6, list[0]);
            Assert.Equal(13, list[1]);

        }    


    }
}
