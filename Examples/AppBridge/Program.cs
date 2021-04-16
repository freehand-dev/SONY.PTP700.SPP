using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SONY.PTP700.SPP;
using SONY.PTP700.SPP.Events;
using SONY.PTP700.SPP.PacketFactory;
using SONY.PTP700.SPP.PacketFactory.Command;
using SONY.PTP700.SPP.Utils;

namespace AppBridge
{
    class Program
    {


        static CcuClient _сcuClient;

        static void Main(string[] args)
        {

            Console.Clear();
            Console.CancelKeyPress += (sender, e) =>
            {

                Console.WriteLine("Exiting...");
                Environment.Exit(0);
            };


            // ILoggerFactory
            var loggerFactory = LoggerFactory.Create(builder => {
                    builder.AddFilter("Microsoft", LogLevel.Debug)
                           .AddFilter("System", LogLevel.Debug)
                           .AddFilter("SampleApp.Program", LogLevel.Debug)
                           .AddConsole();
                }
            );
            var _logger = loggerFactory.CreateLogger("Program");


            _сcuClient = new CcuClient(loggerFactory);
            _сcuClient.SerialNumber = 0x0001b033;
            _сcuClient.Host = IPAddress.Parse("192.168.0.113");

            _сcuClient.OnHandShake += delegate (Object sender, PacketReceivedEventArgs args)
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
            };

            _сcuClient.OnError += delegate (Object sender, PacketReceivedEventArgs args)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"packet.type: { args.Packet.PacketType.ToString() } { ByteUtils.ToHexString(args.Packet.Payload) }");
                Console.ResetColor();
            };

            _сcuClient.CommandHandlerList.EventSubscriber("::CCU::FUNCTION::00", delegate(Message50.SPpCommandPair command) 
            {
                var _ccu_function_00 = new CcuFunction00(command.ToBytes());
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} BARS={ _ccu_function_00.Bars.ToString() } CHROMA={ _ccu_function_00.Chroma.ToString() } CCUSkinGate={ _ccu_function_00.CCUSkinGate.ToString() }");
            });

            _сcuClient.CommandHandlerList.EventSubscriber("::MIC1::GAIN::SELECT", delegate (Message50.SPpCommandPair command)
            {
                var _mic1_gain_select = new mic1_gain_select(command.ToBytes());
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} Value = { _mic1_gain_select.Value.ToString() }");
            });

            _сcuClient.CommandHandlerList.EventSubscriber("::MIC2::GAIN::SELECT", delegate (Message50.SPpCommandPair command)
            {
                var _mic2_gain_select = new mic2_gain_select(command.ToBytes());
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())} Value = { _mic2_gain_select.Value.ToString() }");
            });


            _сcuClient.CommandHandlerList.EventSubscriber("UNKNOWN", delegate (Message50.SPpCommandPair command)
            {
                Console.WriteLine($"[{command.Name}] {ByteUtils.ToHexString(command.ToBytes())}");
            });

            try
            {
                _сcuClient.Connect();
            } 
            catch (Exception e)
            {
                Console.WriteLine($"!!!!!!!!!Connected error { e.Message }");
            }

   
            Console.WriteLine("Press ESC to Exit");

            var taskKeys = new Task(ReadKeys);
            taskKeys.Start();
            taskKeys.Wait();


            _сcuClient.Disconnect();

            Console.WriteLine("!!!!!!!!!Disconnected");

        }

        private static void ReadKeys()
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo();

            while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
            {

                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.B:
                        var x = _сcuClient.GetBarsAsync();
                        break;
                    case ConsoleKey.A:
                        _сcuClient.SetActive();
                        break;

                    case ConsoleKey.C:
                        _сcuClient.Connect();
                        break;
                    case ConsoleKey.D:
                        _сcuClient.Disconnect();
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

            Console.WriteLine("Exit from ReadKeys");
        }

    }
}
