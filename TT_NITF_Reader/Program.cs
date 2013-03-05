using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTNITFReader
{
   
    class Program
    {
        static private Service.Service WinService;
        static private TTFileReader Reader = new TTFileReader();
        static private Log.File LogFile = new Log.File()
            ;
        static private int _port, _supervisor_cnt;

        static private TV4.TextTv.Supervisor Supervisor = new TV4.TextTv.Supervisor();

        private static event MessageHandler OnWriteLine;

        static void Main(string[] args)
        {

            try
            {
                WinService = new Service.Service();
                _supervisor_cnt = 0;
                
                LoadSettings();

                WriteLine("Create TT-reader.", true);
                Reader.OnError += new MessageHandler(Reader_OnError);
                Reader.OnMessage += new MessageHandler(Reader_OnMessage);
                Reader.OnNewArticle += new NewsHandler(Reader_OnNewArticle);
                Reader.OnNextScan += new TimeStampHandler(Reader_OnNextScan);


                WriteLine("Create service and telnet connection.", true);
                WinService.ShowServiceNameInLogg = true;
                WinService.OnMessage += new Service.MessageHandler(WinService_OnMessage);

                WinService.OnServiceStart += new Service.ServiceStartHandler(Service_OnServiceStart);
                WinService.OnServiceEnd += new Service.ServiceEndHandler(Service_OnServiceEnd);

                WinService.OnConsoleStart += new Service.ConsoleStartHandler(WinService_OnConsoleStart);
                
                //WinService.Telnet
                WinService.Telnet.OnConnect += new Telnet.ConnectHandler(TelnetMonitor_OnConnect);
                WinService.Telnet.OnDisconnect += new Telnet.DisConnectHandler(TelnetMonitor_OnDisconnect);
                WinService.Telnet.OnDataReceived += new Telnet.KeyDownHandler(TelnetMonitor_OnDataReceived);

                WinService.Start(args, _port);

            }
            catch (Exception ex)
            {
                WriteError("Error: " + ex.Message);
            }
        }

        static void WinService_OnMessage(string str)
        {
            WriteLine(str);
        }

        static void WinService_OnConsoleStart(string[] args)
        {
            WriteLine("Start in Consolemode", true);

            Reader.Open();

            while (Console.ReadKey().Key != ConsoleKey.Q)
                WriteConsole("To Quit press Q");

            WriteConsole("Stopping!");

            Reader.Close();

            Supervisor.Send("Quit", 30);
        }
        static void Service_OnServiceEnd()
        {
            Reader.Close();
            WriteLine("OnServiceStop", true);
            Supervisor.Send("Quit", 30);
        }
        static void Service_OnServiceStart()
        {
            WriteLine("Start in Servicemode");
            Reader.Open();
            WriteConsole("Next scan in " + Reader.Scaninterval.ToString() + " sec.");
        }

        static void Reader_OnNextScan(DateTime Timestamp)
        {
            WriteConsole("Next scan at " + Timestamp.ToString("HH:mm:ss"));
            WriteMonitor("Next scan at " + Timestamp.ToString("HH:mm:ss") + "\r");

            _supervisor_cnt++;

            if (_supervisor_cnt > 10)
            {
                Supervisor.Send(20);
                _supervisor_cnt = 0;
            }
        }

        static void Reader_OnNewArticle(TTData TTArticle)
        {
            WriteMonitor();
            WriteMonitor("\r\n" + TTArticle.SendDate.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n\r\n" + TTArticle.Headline + "\r\n\r\n");

            foreach (String Txt in TTArticle.LongArticle.Text)
                WriteMonitor(Txt + "\r\n\r\n");

            WriteMonitor();
        }
        static void Reader_OnMessage(string str)
        {
            WriteLine(str);
            WriteMonitor(DateTime.Now.ToString("HH:mm:ss - ") + str + "\n\r");
        }

        static void Reader_OnError(string str)
        {
            WriteError("Error: " + str);
        }
        static void TelnetMonitor_OnDataReceived(Telnet.Session session)
        {
            
            switch(session.Key)
            {
                case ConsoleKey.S:
                    Reader.Active = !Reader.Active;

                    if (Reader.Active)
                    {
                        WriteLine("Reader started, by user (" + session.Name + ")");
                        WriteMonitor("Reader started\r\n");
                    }
                    else
                    {
                        WriteLine("Reader stopped, by user (" + session.Name + ")");
                        WriteMonitor("Reader stopped\r\n");
                    }
                    break;
                case ConsoleKey.R:
                    WriteLine("Reload settings.xml, by user (" + session.Name + ")");
                    LoadSettings();
                    break;
                case ConsoleKey.Q:
                case ConsoleKey.E:
                case ConsoleKey.X:
                    try
                    {
                        WriteLine(session.Name + " is disconnecting");
                        session.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex.Message, true);
                    }

                    break;
                default:
                    session.Write(ServiceMenu);
                    break;

            }

        }

        static void TelnetMonitor_OnDisconnect()
        {
            WriteLine("Client disconnected");
        }

        static void TelnetMonitor_OnConnect(Telnet.Session session)
        {
            WriteLine(session.Name + " is connecting");

            OnWriteLine += new MessageHandler(session.Write);

            
            session.WriteLine();
            session.WriteLine("Welcome to " + WinService.Name);
            session.WriteLine();
            session.Write(ServiceMenu);
            session.WriteLine();
        }
        private static string ServiceMenu
        {
            get
            {
                String Ret = "Service Menu\n\r";

                Ret += "\n\r S - Stop/Start timer";
                Ret += "\n\r R - Reload settings.xml";
                Ret += "\n\r Q/E/X - Exit";
                Ret += "\n\r";

                return Ret;
            }
        }
        private static void LoadSettings()
        {
            try
            {
                System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
                WriteLine("Load settings", false);
                WriteMonitor("\r\nLoad settings\r\n\r\n");

                WriteLine("Open: " + System.AppDomain.CurrentDomain.BaseDirectory + @"\settings.xml", false);
                _doc.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\settings.xml");

                //Timer.Enabled = false;

                if (_doc.SelectSingleNode("settings/log/@path").InnerText.Length > 0)
                {
                    WriteLine("Logpath: " + _doc.SelectSingleNode("settings/log/@path").InnerText, false);
                    WriteMonitor("Logpath: " + _doc.SelectSingleNode("settings/log/@path").InnerText+"\r\n");
                    LogFile = new Log.File(_doc.SelectSingleNode("settings/log/@path").InnerText);
                }
                else
                {
                    WriteLine("Logpath: " + System.IO.Directory.GetCurrentDirectory(), false);
                    WriteMonitor("Logpath: " + System.IO.Directory.GetCurrentDirectory() + "\r\n");
                    LogFile = new Log.File(System.IO.Directory.GetCurrentDirectory());
                }

                //WinService = new TV4.TextTv.Service.Service();
                //WinService.Port = int.Parse(_doc.SelectSingleNode("settings/telnet/@port").InnerText);

                _port = int.Parse(_doc.SelectSingleNode("settings/telnet/@port").InnerText);
                //TelnetMonitor.Port = _port;

                WriteLine("Telnet port: " + _port.ToString());
                WriteMonitor("Telnet port: " + _port.ToString() + "\r\n");

                WriteLine("Database: " + _doc.SelectSingleNode("settings/database").InnerText);
                WriteMonitor("Database: " + _doc.SelectSingleNode("settings/database").InnerText + "\r\n");
                Reader.DB = new TTArticleDatabase(_doc.SelectSingleNode("settings/database").InnerText);
                Reader.DB.OnError += new DBErrorEventHandler(DB_OnError);

                Reader.Path = _doc.SelectSingleNode("settings/path").InnerText;
                WriteLine("Scanpath: " + Reader.Path);
                WriteMonitor("Scanpath: " + Reader.Path + "\r\n");

                List<string> Lst = new List<string>();

                WriteLine("Filter:");
                WriteMonitor("Filter:\r\n");

                foreach (System.Xml.XmlNode n in _doc.SelectNodes("settings/filter/itm"))
                {
                    Lst.Add(n.InnerText);
                    WriteLine(" Item: " + n.InnerText);
                    WriteMonitor(" Item: " + n.InnerText + "\r\n");

                }

                Reader.Dest = Lst.ToArray();

                //Timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);

                Reader.Scaninterval = int.Parse(_doc.SelectSingleNode("settings/interval").InnerText);
                WriteLine("Timer: " + _doc.SelectSingleNode("settings/interval").InnerText);
                WriteMonitor("Timer: " + _doc.SelectSingleNode("settings/interval").InnerText + "\r\n");

                Supervisor = new TV4.TextTv.Supervisor(_doc.SelectSingleNode("settings/supervisor"));
                Supervisor.Send("Start", 10);
                
                //Timer.Start();
            }
            catch (Exception ex)
            {
                WriteError("Error: " + ex.Message);
            }
        }

        private static void DB_OnError(Exception Ex)
        {
            WriteError("DB Error: " + Ex.Message);
        }
        private static void WriteLine(String Str)
        {
            WriteLine(Str, true);
        }
        private static void WriteLine(string Str, bool file)
        {
            WriteConsole(Str);
            if (file)
                WriteLogg(Str);
        }
        private static void WriteError(string Str)
        {
            WriteLine(Str, true);
            WriteMonitor("\n\r" + DateTime.Now.ToString("HH:mm.ss - ") + Str + "\n\r");
        }
        private static void WriteConsole(string Str)
        {
            //if (Console. != null)
            Console.WriteLine(DateTime.Now.ToString("HH:mm.ss - ") + Str);
        }
        private static void WriteLogg(string Str)
        {
            LogFile.Add(Str);
        }
        private static void WriteMonitor()
        {
            WriteMonitor("\n\r");
        }
        private static void WriteMonitor(string Str)
        {
            if (OnWriteLine != null)
                OnWriteLine(Str);
        }
    }
}
