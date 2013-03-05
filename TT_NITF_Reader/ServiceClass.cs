using System;
//using System.Threading;
using System.Text;
using System.Runtime.InteropServices;


namespace Service
{
    /// <summary>
    /// Windowsserice
    /// 
    /// Martin Melander 2013-01-25
    /// 
    /// </summary>
    //public delegate void MonitorStart();
    public delegate void ServiceStartHandler();
    public delegate void ServiceEndHandler();
    public delegate void ConsoleStartHandler(string[] args);
    public delegate void ConsoleEndHandler();
    public delegate void MessageHandler(string str);

    public class Service : System.ServiceProcess.ServiceBase
    {

        /// <summary> 
        /// Required designer variable.
        /// </summary>

        //private System.Threading.Thread MonitorThread;

        private System.IO.FileInfo _fileInfo;
        private string _name;
        private int _port;

        public bool ShowServiceNameInLogg = false;

        public Telnet.Connection Telnet = new Telnet.Connection();

        public event ConsoleStartHandler OnConsoleStart;
        public event ConsoleEndHandler OnConsoleEnd;
        public event ServiceStartHandler OnServiceStart;
        public event ServiceEndHandler OnServiceEnd;
        public event MessageHandler OnMessage;
        
        public Service()
        {
            // InitializeComponent();
            _fileInfo = new System.IO.FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + "\\" + System.AppDomain.CurrentDomain.FriendlyName);
            _name = _fileInfo.Name.Substring(0, _fileInfo.Name.Length - 4);
        
        }
        public void Start(int port)
        {
            _port = port;
            Start();
        }
        public void Start(string[] args, int port)
        {
            Port = port;
            Start(args);
        }
        public void Start(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                if (args.Length == 0)
                {
                    ConsoleStart(args);
                }
                else
                {
                    switch (args[0].ToString().ToUpper())
                    {
                        case "/I":
                        case "/INSTALL":
                            {
                                Install();
                            }
                            break;
                        case "/U":
                        case "/UNINSTALL":
                            {
                                UnInstall();
                            }
                            break;
                            /*
                        case "/?":
                            {
                            
                            }
                            break;
                             */
                        default:
                            {               
                                ConsoleStart(args);        
                            }
                            break;
                    }
                }
            }
            else
            {
                Start();
                //Start(port);
            }
        }
        public void Start()
        {
            WriteLn("Start");

            ///Telnet.Open();

            /*
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service() };
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
            */

            //System.ServiceProcess.ServiceBase.Run(new System.ServiceProcess.ServiceBase[] { new Service() });

            //System.ServiceProcess.ServiceBase.Run(new System.ServiceProcess.ServiceBase[] { this });

            System.ServiceProcess.ServiceBase.Run(this);
        }
        private void ConsoleStart(string[] args)
        {
            Telnet.Open();
            if (OnConsoleStart != null)
            {
                OnConsoleStart(args);
                Telnet.Close();
            }
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            WriteLn("InitializeComponent");
            //-->components = new System.ComponentModel.Container();
            //-->this.ServiceName = ServiceName;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            WriteLn("Dispose");
            //-->if( disposing )
            //-->{
            //-->	if (components != null) 
            //-->	{
            //-->		components.Dispose();
            //-->	}
            //-->}
            //-->base.Dispose( disposing );
        }

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            WriteLn("OnStart");            
            
            if (OnServiceStart != null)
                OnServiceStart();

            Telnet.Open();
        }
        protected override void OnStop()
        {
            WriteLn("OnStop");

            if (OnServiceEnd != null)
                OnServiceEnd();

            Telnet.Close();

            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
        public void WriteLn(string str)
        {

            if (OnMessage != null)
            {
                if (ShowServiceNameInLogg)
                    OnMessage("[" + _name + "] - " + str);
                else
                    OnMessage(str);
            
            }

        }
        public void Install()
        {
            ServiceInstaller c = new ServiceInstaller();

            //_name = System.AppDomain.CurrentDomain.FriendlyName.Substring(0, System.AppDomain.CurrentDomain.FriendlyName.Length - 4);

            WriteLn("Trying to install " + System.Environment.CurrentDirectory + "\\" + System.AppDomain.CurrentDomain.FriendlyName + " as " + _name);

            if (c.InstallService(System.Environment.CurrentDirectory + "\\" + System.AppDomain.CurrentDomain.FriendlyName, _name, _name))
                WriteLn(_name + " was successfully installed as a service");
            else
                WriteLn(_name + " failed to install as a service. (already installed?)");

        }
        public void UnInstall()
        {
            ServiceInstaller c = new ServiceInstaller();

            //_name = System.AppDomain.CurrentDomain.FriendlyName.Substring(0, System.AppDomain.CurrentDomain.FriendlyName.Length - 4);

            WriteLn("Trying to Uninstall " + _name);

            if (c.UnInstallService(_name))
                WriteLn(_name + " was succesfully uninstalled!");
            else
                WriteLn(_name + " failed to be uninstalled! (not installed?)");

        }
        /*
        public void ConsoleMode()
        {
            WriteLn("Console operated application");

            //if (OnServiceStart != null)
            //    OnServiceStart();

            if (OnConsoleStart != null)
                OnConsoleStart();

            //Telnet.Port = Port;
            Telnet.Open();

            WriteLn("Console application Opend");
            
            WriteLn("To quit application, press Q and return");
            while (System.Console.ReadLine().ToUpper() != "Q")
                WriteLn("To quit application, press Q and enter");

            // Q has been pressed.
            WriteLn("Shutting down");
            
            if (OnConsoleEnd != null)
                OnConsoleEnd();
                        
            WriteLn("Abort monitor thread");

            Telnet.Close();

            WriteLn("Shutting down ready");

        }
        */ 
        public System.IO.FileInfo FileInfo
        {
            get {return _fileInfo;}
        }
        public string Name
        {
            get { return _name; }
        }
        public int Port
        {

            get { return _port; }
            set
            {
                _port = value;
                WriteLn("Setting telnetport to " + value.ToString());
                Telnet.Port = _port;
            }
        }

    }
    public class ServiceInstaller
    {
        #region Private Variables

        //private string _servicePath;
        //private string _serviceName;
        //private string _serviceDisplayName;

        #endregion Private Variables

        #region DLLImport

        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);
        [DllImport("Advapi32.dll")]
        public static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
            int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName,
            string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);
        [DllImport("advapi32.dll")]
        public static extern void CloseServiceHandle(IntPtr SCHANDLE);
        [DllImport("advapi32.dll")]
        public static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);
        [DllImport("advapi32.dll")]
        public static extern int DeleteService(IntPtr SVHANDLE);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        #endregion DLLImport


        /// <summary>
        /// This method installs and runs the service in the service conrol manager.
        /// </summary>
        /// <param name="svcPath">The complete path of the service.</param>
        /// <param name="svcName">Name of the service.</param>
        /// <param name="svcDispName">Display name of the service.</param>
        /// <returns>True if the process went thro successfully. False if there was any error.</returns>
        public bool InstallService(string svcPath, string svcName, string svcDispName)
        {
            #region Constants declaration.
            int SC_MANAGER_CREATE_SERVICE = 0x0002;
            int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

            //
            int SERVICE_DEMAND_START = 0x00000003;
            int SERVICE_ERROR_NORMAL = 0x00000001;

            int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            int SERVICE_QUERY_CONFIG = 0x0001;
            int SERVICE_CHANGE_CONFIG = 0x0002;
            int SERVICE_QUERY_STATUS = 0x0004;
            int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
            int SERVICE_START = 0x0010;
            int SERVICE_STOP = 0x0020;
            int SERVICE_PAUSE_CONTINUE = 0x0040;
            int SERVICE_INTERROGATE = 0x0080;
            int SERVICE_USER_DEFINED_CONTROL = 0x0100;

            int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL);
            //int SERVICE_AUTO_START = 0x00000002;
            #endregion Constants declaration.

            try
            {
                Console.WriteLine("Create handle");
                IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);

                if (sc_handle.ToInt32() != 0)
                {
                    Console.WriteLine("Handle != 0");

                    //Console.WriteLine("User: " + user.ToString());
                    IntPtr sv_handle = CreateService(sc_handle, svcName, svcDispName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_DEMAND_START, SERVICE_ERROR_NORMAL, svcPath, null, 0, null, null, null);
                    //IntPtr sv_handle = CreateService(sc_handle,svcName,svcDispName,SERVICE_ALL_ACCESS,SERVICE_WIN32_OWN_PROCESS, SERVICE_DEMAND_START,SERVICE_ERROR_NORMAL,svcPath,null,0,null,user,pwd);

                    if (sv_handle.ToInt32() == 0)
                    {

                        CloseServiceHandle(sc_handle);
                        return false;
                    }
                    else
                    {
                        CloseServiceHandle(sc_handle);
                        return true;
                    }
                }
                else
                    //Console.WriteLine("SCM not opened successfully");
                    return false;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
                return false;
            }
        }


        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public bool UnInstallService(string svcName)
        {
            int GENERIC_WRITE = 0x40000000;
            IntPtr sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);

            if (sc_hndl.ToInt32() != 0)
            {
                int DELETE = 0x10000;
                IntPtr svc_hndl = OpenService(sc_hndl, svcName, DELETE);
                //Console.WriteLine(svc_hndl.ToInt32());
                if (svc_hndl.ToInt32() != 0)
                {
                    int i = DeleteService(svc_hndl);
                    if (i != 0)
                    {
                        CloseServiceHandle(sc_hndl);
                        return true;
                    }
                    else
                    {
                        CloseServiceHandle(sc_hndl);
                        return false;
                    }
                }
                else
                    return false;
            }
            else
                return false;
        }
    }


}
