using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SONY.PTP700.SPP;
using SONY.PTP700.SPP.Events;
using SONY.PTP700.SPP.PacketFactory;
using SONY.PTP700.SPP.PacketFactory.Command;
using SONY.PTP700.SPP.Utils;

namespace AppMcs
{
   
    class Program
    {
        static readonly object _lockConsole = new object();

        static MsuClient _msuClient;

        static void Main(string[] args)
        {

            Console.Clear();
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Exiting...");
                Environment.Exit(0);
            };

            Console.Write("Enter MSU IP Address [192.168.0.1]: ");
            string _ip_str = Console.ReadLine();
            Console.Write("Enter RCP Number [25]: ");
            string _rcp_no_str = Console.ReadLine();


            // ILoggerFactory
            var loggerFactory = LoggerFactory.Create(builder => {
                    builder.AddFilter("Microsoft", LogLevel.Debug)
                           .AddFilter("System", LogLevel.Debug)
                           .AddFilter("SampleApp.Program", LogLevel.Debug)
                           .AddConsole();
                }
            );
            var _logger = loggerFactory.CreateLogger("Program");

            // Get an instance of the service
            _msuClient = new MsuClient(loggerFactory)
            {
                Host = IPAddress.Parse((String.IsNullOrEmpty(_ip_str) ? "192.168.0.1" : _ip_str)),
                SerialNumber = 0x0001b034,
                RcpId = (String.IsNullOrEmpty(_rcp_no_str) ? (byte)25 : Convert.ToByte(_rcp_no_str)),
            };

            _msuClient.CcuClient.OnHandShake += EventOnHandShake;
            _msuClient.OnHandShake += EventOnHandShake;
            _msuClient.OnChangeAssigment += EventOnChangeAssigment;
            _msuClient.OnChangePermisionControl += EventOnChangePermisionControl;
            _msuClient.OnError += EventOnError;
            _msuClient.CcuClient.OnError += EventOnError;
            _msuClient.CcuClient.OnChangeCameraPowerState += EventOnChangeCameraPowerState;


            _msuClient.CcuClient.CommandHandlerList.EventSubscriber("::CCU:FUNCTION:CAM_PW", delegate (Message50.SPpCommandPair command)
            {
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} ");
            });

            

            _msuClient.CcuClient.CommandHandlerList.EventSubscriber("::CCU:FUNCTION:00", delegate (Message50.SPpCommandPair command)
            {
                var _ccu_function_00 = new CcuFunction00(command.ToBytes());
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} BARS={ _ccu_function_00.Bars.ToString() } CHROMA={ _ccu_function_00.Chroma.ToString() } CCUSkinGate={ _ccu_function_00.CCUSkinGate.ToString() }");
            });

            _msuClient.CcuClient.CommandHandlerList.EventSubscriber("::CHU:FUNCTION:MIC1_GAIN_SELECT", delegate (Message50.SPpCommandPair command)
            {
                var _mic1_gain_select = new mic1_gain_select(command.ToBytes());
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} Value = { _mic1_gain_select.Value.ToString() }");
            });

            _msuClient.CcuClient.CommandHandlerList.EventSubscriber("::CHU:FUNCTION:MIC2_GAIN_SELECT", delegate (Message50.SPpCommandPair command)
            {
                var _mic2_gain_select = new mic2_gain_select(command.ToBytes());
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} Value = { _mic2_gain_select.Value.ToString() }");
            });


            try
            {
                _msuClient.Connect();
            }
            catch (Exception e)
            {
                _logger?.LogError($"!!!!!!!!!Connected error { e.Message }");
            }


            //var _msuClient.GetCcuList();


            Console.WriteLine("Press ESC to Exit");
       
            var taskKeys = new Task(ReadKeys);
            taskKeys.Start();
            taskKeys.Wait();


            _msuClient.Disconnect();

            Console.WriteLine("!!!!!!!!!Disconnected");
        }

        private static void ReadKeys()
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo();

            while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
            {
                try
                {
                    key = Console.ReadKey(true);

                    lock (_lockConsole)
                    {
                        switch (key.Key)
                        {
                            // ch1 Inc
                            case ConsoleKey.D1 when key.Modifiers == ConsoleModifiers.Control:
                                //_msuClient.CcuClient.Camera.Power
                                //_msuClient.CcuClient.Camera.Cable
                                //_msuClient.CcuClient.Camera.MicGain(CH1).Inc!
                                _msuClient.CcuClient.mic1_gain_select(MicGainSelect.MicGainValue.Inc);
                                break;
                            // ch1 Dec
                            case ConsoleKey.D1 when key.Modifiers == ConsoleModifiers.Alt:
                                _msuClient.CcuClient.mic1_gain_select(MicGainSelect.MicGainValue.Dec);
                                break;

                            // ch2 Inc
                            case ConsoleKey.D2 when key.Modifiers == ConsoleModifiers.Control:
                                _msuClient.CcuClient.mic2_gain_select(MicGainSelect.MicGainValue.Inc);
                                break;

                            // ch2 Dec
                            case ConsoleKey.D2 when key.Modifiers == ConsoleModifiers.Alt:
                                _msuClient.CcuClient.mic2_gain_select(MicGainSelect.MicGainValue.Dec);
                                break;

                            case ConsoleKey.B:
                                _msuClient.CcuClient.SetBars();
                                break;
                            case ConsoleKey.L:
                                var _ccuList = String.Join<int>(",", _msuClient.GetOnlineCcuList().ToArray());
                                Console.WriteLine($"GetOnlineCcuList={ _ccuList }");
                                break;
                            case ConsoleKey.A:
                                _msuClient.CcuClient.SetActive();
                                break;
                            case ConsoleKey.P:
                                var _cmdReturn = _msuClient.SetPara();
                                Console.WriteLine($"SetPara={ Convert.ToString(_cmdReturn) }");
                                break;

                            case ConsoleKey.D1 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(11);
                                break;

                            case ConsoleKey.D2 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(12);
                                break;

                            case ConsoleKey.D3 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(13);
                                break;

                            case ConsoleKey.D4 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(14);
                                break;

                            case ConsoleKey.D5 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(15);
                                break;

                            case ConsoleKey.D6 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(16);
                                break;


                            case ConsoleKey.D0 when key.Modifiers == ConsoleModifiers.Shift:
                                _msuClient.Assign(0);
                                break;

                            case ConsoleKey.D1:
                                _msuClient.Assign(1);
                                break;

                            case ConsoleKey.D2:
                                _msuClient.Assign(2);
                                break;

                            case ConsoleKey.D3:
                                _msuClient.Assign(3);
                                break;

                            case ConsoleKey.D4:
                                _msuClient.Assign(4);
                                break;

                            case ConsoleKey.D5:
                                _msuClient.Assign(5);
                                break;

                            case ConsoleKey.D6:
                                _msuClient.Assign(6);
                                break;

                            case ConsoleKey.D7:
                                _msuClient.Assign(7);
                                break;

                            case ConsoleKey.D8:
                                _msuClient.Assign(8);
                                break;

                            case ConsoleKey.D9:
                                _msuClient.Assign(9);
                                break;

                            case ConsoleKey.D0:
                                _msuClient.Assign(10);
                                break;

                            case ConsoleKey.C:
                                _msuClient.CcuClient.Call(true);
                                break;
                            case ConsoleKey.DownArrow:
                                Console.WriteLine("DownArrow was pressed");
                                break;

                            case ConsoleKey.RightArrow:
                                Console.WriteLine("RightArrow was pressed");
                                break;

                            case ConsoleKey.LeftArrow:
                                Console.WriteLine("LeftArrow was pressed");
                                break;

                            case ConsoleKey.Escape:
                                break;

                            default:
                                if (Console.CapsLock && Console.NumberLock)
                                {
                                    Console.WriteLine(key.KeyChar);
                                }
                                break;
                        }
                    }
                } 
                catch (Exception e)
                {
                    Console.WriteLine($"[Opperation Error] { e.Message }");
                }
  
            }

            Console.WriteLine("Exit from ReadKeys");
        }

        private static void EventOnHandShake(Object sender, PacketReceivedEventArgs args)
        {
            var _handshake = (args.Packet as HandShake);
            Console.ForegroundColor = ConsoleColor.Blue;
            try
            {
                Console.WriteLine($"SerialNumber is { _handshake?.SerialNumber.ToString() }");
                Console.WriteLine($"DeviceType is { _handshake?.Type.ToString() }");
                Console.WriteLine($"DeviceModel is { _handshake?.Model.ToString() }");
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static void EventOnChangeAssigment(Object sender, RcpAssigmentEventArgs args)
        {
            Console.WriteLine($"[OnChangeAssigment] { Convert.ToString(args.CcuNo) }");
        }

        private static void EventOnChangePermisionControl(Object sender, PermissionControlEventArgs args)
        {
            Console.WriteLine($"[OnChangePermisionControl] { args.Permisions.ToString() }");
        }


        private static void EventOnChangeCameraPowerState(Object sender, CameraPowerStateEventArgs args)
        {
            Console.WriteLine($"[OnChangeCameraPowerState] { args.State.ToString() }");
        }


        private static void EventOnError(Object sender, PacketReceivedEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ { ((sender is CcuClient) ? "CCU" : "MSU") } ][{ args.Packet.PacketType.ToString() }] { ByteUtils.ToHexString(args.Packet.Payload) }");
            Console.ResetColor();
        }


    }
}
