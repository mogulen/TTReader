using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Telnet
{
    /// <summary>
    /// Telnet
    /// 
    /// Martin Melander 2013-01-25
    /// 
    /// </summary>

    public delegate void ConnectHandler(Session session);
    public delegate void DisConnectHandler();
    public delegate void KeyDownHandler(Session session);
    public delegate void ErrorHandler(Exception ex);
    public class Connection
    {
        public int Port = 23;

        private bool _active = false;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private System.Threading.Thread MonitorThread;

        public event ConnectHandler OnConnect;
        public event DisConnectHandler OnDisconnect;
        public event KeyDownHandler OnDataReceived;
        public event ErrorHandler OnError;

        public Connection()
        {
            MonitorThread = null;
            
            //View = new MonitorView();				
        }
        private void StartMonitorThread()
        {
            StopMonitorThread();
            
            //WriteLn("[Monitor thread] - Create");
            MonitorThread = new Thread(new ThreadStart(StartMonitor));

            //WriteLn("[Monitor thread] - Start");
            MonitorThread.Start();

        }
        private void StopMonitorThread()
        {
            try
            {
                //WriteLn("[Monitor thread] - Abort");
                if (MonitorThread != null)
                    MonitorThread.Abort();
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(ex);
                //WriteLn("Error (listenerThread.Abort(): " + ex.Message);
            }

        }
        public bool Enabled
        {
            get
            {
                return _active;
            }
            set
            {
                if (value)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        public void Close()
        {
            _active = false;
            allDone.Set();
            StopMonitorThread();
        }
        public void Open()
        {
            StartMonitorThread();
        }
        //public void StartListening()
        private void StartMonitor()
        {

            //WriteLn("[Monitor thread] - Start");

            if (Port > 0)
            {

                _active = true;

                //IPAddress ipadress = IPAddress.Any;

                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //listener.Bind(new IPEndPoint(ipadress, Port));
                listener.Bind(new IPEndPoint(IPAddress.Any, Port));
                listener.Listen(1000);

                while (_active)
                {
                    allDone.Reset(); //
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne(); //Väntar tills tråden är klar
                }
            }
        }
        // -------------------------------------------------------------------------------------
        // Uppkoppling av telnet
        // -------------------------------------------------------------------------------------
        private void AcceptCallback(IAsyncResult ar)
        {

            allDone.Set(); //Tråden klar fortsätt.

            Telnet.Session Session = new Telnet.Session(ar);

            if (OnConnect != null)
                OnConnect(Session);

            //Vänta på data
            Session.BeginReceive(new AsyncCallback(ReadCallback));

        }
        // -------------------------------------------------------------------------------------
        // Data kommer
        // -------------------------------------------------------------------------------------
        private void ReadCallback(IAsyncResult ar)
        {
            Telnet.Session Session = (Telnet.Session)ar.AsyncState;

            Session.EndReceive(ar);

            if ((Session.BytesRead > 0) && (Session.Socket.Connected))
            {
                if (OnDataReceived != null)
                    OnDataReceived(Session);
                
                //Vänta på data
                if (Session.Socket.Connected)
                    Session.BeginReceive(new AsyncCallback(ReadCallback));
                else
                    DoDisconnect(Session);
            }
            else
            {
                DoDisconnect(Session);
            }
        }
        private void DoDisconnect(Session Session)
        {
                
            if (OnDisconnect != null)
                    OnDisconnect();

                //Koppla ner
                if (Session.Socket.Connected)
                    Session.Close();
        }
    }
    public class Session
    {
        public System.Net.Sockets.Socket Socket;

        private string _buffer;
        private int _bytesread;
        private byte[] _bytebuffer = new byte[8192];
        public Session()
        {
            Init(null);
        }
        public Session(string buffer)
        {
            Init(null, buffer);
        }
        public Session(Socket socket)
        {
            Init(socket);
        }
        public Session(Socket socket, string buf)
        {
            Init(socket, buf);
        }
        public Session(IAsyncResult ar)
        {
            Init(((Socket)ar.AsyncState).EndAccept(ar));
        }
        private void Init(Socket socket)
        {
            if (socket != null)
                Socket = socket;

            _buffer = "";
            _bytesread = 0;
        }
        private void Init(Socket socket, string buf)
        {
            if (socket != null)
                Socket = socket;

            _buffer = buf;
            _bytesread = 0;
        }
        /*
        private void Clear()
        {
        
        }
        */ 
        public string StringBuffer
        {
            get
            {
                return _buffer;
            }
        }
        public byte[] ByteBuffer
        {
            get
            {
                byte[] Ret = new byte[_buffer.Length];

                for (int b = 0; b < Ret.Length; b++)
                    Ret[b] = _bytebuffer[b];

                return Ret;
            }
        }
        public int BytesRead
        {
            get
            {
                return _bytesread;
            }
        }
        public ConsoleKey Key
        {
            get
            {
                return ToConsoleKey(_buffer);
            }
        }
        public IPAddress IPAddress
        {
            get
            {
                return (IPAddress.Parse(Socket.LocalEndPoint.ToString().Split(':')[0]));
            }
        }
        public string Name
        {
            get
            {
                return Dns.GetHostEntry(IPAddress).HostName;
            }
        }
        public void Write(String str)
        {
            //Telnet.Write(Socket, str);
            Send(str);
        }
        public void WriteLine()
        {
            Send("\n\r");
        }

        public void WriteLine(string str)
        {
            //Telnet.WriteLine(Socket, str);
            Send(str + "\n\r");
        }
        public void Write(int Line, int Column, String text)
        {
            //<ESC>[Line;ColumnH;
            Send("\u001B[" + Line.ToString() + ";" + Column.ToString() + "H");
            Send(text);
        }
        public void WriteLine(string str, Boolean dummy)
        {
            //Telnet.WriteLine(Socket, str);
            Send(str + "\n\r");
        }
        public void CursorHome()
        {
            //<ESC>[H;
            SendESC("[H");
        }
        public void CursorStartOfLine()
        {
            Send("\r");
        }
        public void CursorLeft(int step)
        {
            SendESC("[" + step.ToString() + "D");
        }
        public void CursorRight(int step)
        {
            SendESC("[" + step.ToString() + "C");
        }
        public void CursorUp(int step)
        {
            SendESC("[" + step.ToString() + "A");
        }
        public void CursorDown(int step)
        {
            SendESC("[" + step.ToString() + "B");
        }
        public void MoveCursor(int line, int column)
        {
            SendESC("[" + line.ToString() + ";" + column.ToString() + "H");
        }
        public void ClearTerminalLineCursorRight()
        {
            //<ESC>[2j;
            SendESC("[1K");
        }
        public void ClearTerminalLineCursorLeft()
        {
            //<ESC>[2j;
            SendESC("[1K");
        }
        public void ClearTerminalLine()
        {
            //<ESC>[2j;
            SendESC("[2K");
        }
        public void ClearTerminalCursorDown()
        {
            //<ESC>[2j;
            SendESC("[0J");
        }
        public void ClearTerminalCursorUp()
        {
            //<ESC>[2j;
            SendESC("[1J");
        }
        public void ClearTerminal()
        {
            //<ESC>[2j;
            SendESC("[2J");
        }
        public void SendESC(String CodeString)
        {
            Send("\u001B" + CodeString);
        }
        private void Send(String data)
        {
            Send(Encoding.ASCII.GetBytes(data));
            /*
            if (Socket != null)
            {
                if (Socket.Connected)
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(data);

                    Socket.Send(byteData, byteData.Length, 0);
                }
            }
            */
        }
        private void Send(byte[] data)
        {
            if (Socket != null)
            {
                if (Socket.Connected)
                {
                    Socket.Send(data, data.Length, 0);
                }
            }
        }
        //public void BeginReceive(System.AsyncCallback callback, TelnetSession session)
        public void BeginReceive(AsyncCallback callback)
        {
            //session.Socket.BeginReceive(_bytebuffer, 0, 8192, 0, callback, session);
            Socket.BeginReceive(_bytebuffer, 0, 8192, 0, callback, this);
        }
        public void EndReceive(IAsyncResult result)
        {
            _bytesread = Socket.EndReceive(result);
            _buffer = Encoding.ASCII.GetString(_bytebuffer, 0, _bytesread);
            //_buffer = Encoding.ASCII.GetString(_bytebuffer);
        }
        public void Close()
        {
            Socket.Shutdown(SocketShutdown.Both);
            
        }
        public void Disconnect()
        {
            Socket.Disconnect(false);
            _buffer = "";
            _bytesread = 0;

        }
        public void Shutdown()
        {
            Socket.Shutdown(SocketShutdown.Both);
            _buffer = "";
            _bytesread = 0;

        }
        private ConsoleKey ToConsoleKey(String str)
        {
            // http://ascii-table.com/ansi-escape-sequences-vt-100.php
            int num = 0;
            str = str.ToUpper();

            if (str.Length < 3)
            {
                num = (int)str[0];
                
                switch (num)
                {
                    case 23: //Return
                        num = 13;
                        break;
                    case 42: // *
                        num = 106;
                        break;
                    case 43: // +
                        num = 107;
                        break;
                    case 44: // ,
                        num = 110;
                        break;
                    case 45: // -
                        num = 109;
                        break;
                    case 47: // /
                        num = 111;
                        break;
                    case 127: //Delete
                        num = 46;
                        break;
                }
            }
            else
            {

                string tmp = str.Substring(1);

                switch (tmp)
                {
                    case "[A": //Upp
                        num = 38;
                        break;
                    case "[B": //Down
                        num = 40;
                        break;
                    case "[D": //Left
                        num = 37;
                        break;
                    case "[C": //Right
                        num = 39;
                        break;
                    case "[P": //Pause
                        num = 19;
                        break;

                    case "[2~": //Insert
                        num = 45;
                        break;
                    case "[1~": //Home
                        num = 36;
                        break;
                    case "[5~": //PageUp
                        num = 33;
                        break;
                    case "[6~": //PageDown
                        num = 34;
                        break;
                    case "[4~": //End
                        num = 35;
                        break;

                    case "OP": //F1 op
                        num = 112;
                        break;
                    case "OQ": //F2 oq
                        num = 113;
                        break;
                    case "OR": //F3 or
                        num = 114;
                        break;
                    case "OS": //F4 os
                        num = 115;
                        break;

                    case "[15~": //F5 op
                        num = 116;
                        break;
                    case "[17~": //F6 oq
                        num = 117;
                        break;
                    case "[18~": //F7 or
                        num = 118;
                        break;
                    case "[19~": //F8 os
                        num = 119;
                        break;
                    case "[20~": //F9 op
                        num = 120;
                        break;
                    case "[21~": //F10 oq
                        num = 121;
                        break;
                    case "[23~": //F11 or
                        num = 122;
                        break;
                    case "[24~": //F12 os
                        num = 123;
                        break;

                }

            }
            return (ConsoleKey)num;
        }

    }
}
