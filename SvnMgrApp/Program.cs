using NLog;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;
using SvnMgrClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SvnMgrApp
{
    class Program
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public static bool Quit { get; internal set; }

        static void Main(string[] args)
        {
            try
            {
                if (args == null || args.Length == 0 || args[0].ToLower() != "-s")
                {
                    var doWhile = true;
                    do
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("请输入命令：");
                        Console.WriteLine("\t run：直接运行");
                        Console.WriteLine("\t install：安装服务");
                        Console.WriteLine("\t uninstall：卸载服务");
                        Console.WriteLine("\t start：启动服务");
                        Console.WriteLine("\t stop：停止服务");
                        Console.WriteLine("\t quit：退出");
                        Console.ForegroundColor = oldColor;
                        var cmd = Console.ReadLine();
                        switch (cmd.ToLower())
                        {
                            case "run":
                                doWhile = !preRun(false, args);
                                break;
                            case "install":
                                install();
                                break;
                            case "uninstall":
                                uninstall();
                                break;
                            case "start":
                                start();
                                break;
                            case "stop":
                                stop();
                                break;
                            case "quit":
                                doWhile = false;
                                break;
                        }
                    } while (doWhile);
                }
                else
                {
                    preRun(true, args);
                }
            }catch(Exception exc)
            {
                logger.Error(exc);
            }
            finally
            {
                LogManager.Flush();
            }
        }

        static bool preRun(bool ser, string[] args)
        {
            var quit = args.Length > 1 && args[1].ToLower() == "quit";
            int port = 0;
            if (args.Length > 2 && int.TryParse(args[2], out int p))
            {
                port = p;
            }

            if (quit)
            {
                stopSer(port);
                return false;
            }
            else
            {
                return run(ser);
            }
        }

        private static void stopSer(int port)
        {
            var config = (SuperSocket.SocketEngine.Configuration.SocketServiceConfig)ConfigurationManager.GetSection("superSocket");
            var ip = config.Servers[0].Ip;
            if (string.IsNullOrEmpty(ip) || ip.ToLower() == "any")
            {
                ip = "127.0.0.1";
            }
            if (port == 0)
            {
                port = config.Servers[0].Port;
            }
            using (var client =new MgrClient(ip, port))
            {
               var connected = client.Connect();
                logger.Debug("connected :" + connected);
                var rlt = client.Send("QUIT","");
                logger.Debug("Send Result:" + rlt);
            }
        }

        static bool run(bool ser)
        {
            var bootstrap = BootstrapFactory.CreateBootstrap();

            //Setup the appServer
            if (!bootstrap.Initialize()) //Setup with listening port
            {
                logger.Error("Failed to setup!");
                return false;
            }

            var rlt = bootstrap.Start();
            //Try to start the appServer
            if (rlt== StartResult.Failed)
            {
                logger.Error("Failed to start!");
                return false;
            }
            if (ser)
            {
                logger.Info("The server started successfully!");
                

                do {
                    Task.Delay(1000).Wait();
                } while (!Quit);

                Task.Delay(1000).Wait();
            }
            else
            {
                Console.WriteLine($"The server started successfully,input q to quit!");
                while (Console.ReadKey().KeyChar != 'q')
                {
                    Console.WriteLine();
                    continue;
                }
                Console.WriteLine();

                //Stop the appServer
                bootstrap.Stop();

                Console.WriteLine("The server was stopped!");
                Console.ReadKey();
            }
            return true;
        }

        static void install()
        {
            var ser = "SvnMgrservice";
            var xmlFile = ser+".xml";
            var exePath = Path.GetFullPath("SvnMgrApp.exe");
            var doc = XDocument.Load(xmlFile);
            doc.Root.Element("executable").Value = exePath;
            doc.Root.Element("stopexecutable").Value = exePath;
            doc.Save(xmlFile);
            var rlt = SvnHelper2.ExecutScript($"{Path.GetFullPath(ser)}.exe install");
            Console.WriteLine(rlt);
        }

        static void uninstall()
        {
            var ser = "SvnMgrservice";
            var rlt = SvnHelper2.ExecutScript($"{Path.GetFullPath(ser)}.exe uninstall");
            Console.WriteLine(rlt);
        }

        static void start()
        {
            var ser = "SvnMgrservice";
            var rlt = SvnHelper2.ExecutScript($"{Path.GetFullPath(ser)}.exe start");
            Console.WriteLine(rlt);
        }

        static void stop()
        {
            var ser = "SvnMgrservice";
            var rlt = SvnHelper2.ExecutScript($"{Path.GetFullPath(ser)}.exe stop");
            Console.WriteLine(rlt);
        }

    }

}
