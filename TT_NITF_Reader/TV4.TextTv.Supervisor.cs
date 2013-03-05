// Supervisor! skapar filer till övervakningen.
// Uppdaterad 2008-05-14, Martin. 
// Uppdaterad 2011-12-05, supervisor(xmlNode) tar även attibutet path;
using System;
using System.IO;
using System.Xml;

namespace TV4.TextTv
{
	/// <summary>
	/// Summary description for Supervisor.
	/// </summary>
	/// 
	public class Supervisor
	{
		private bool _enabled_to_writedata;
		private string _filename, _host, _application, _name, _message, _errormessage;
		private DateTime _nextreport;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="host"></param>
		/// <param name="application"></param>
		/// <param name="name"></param>
        public Supervisor(System.Xml.XmlNode node)
        {
            Init(node);
        }
        public Supervisor(string filename, string host, string application, string name)
		{
			Init( filename, host, application, name);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="application"></param>
		/// <param name="name"></param>
		public Supervisor(string filename, string application, string name)
		{
			//Init(filename, System.Environment.MachineName, application, name);
            Init(filename, System.Net.Dns.GetHostName(), application, name);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="name"></param>
		public Supervisor(string filename, string name)
		{

            //System.Net.Dns.GetHostName();
            //Init(filename, System.Environment.MachineName, GetDefExeName(), name);
            Init(filename, System.Net.Dns.GetHostName(), GetDefExeName(), name);

        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		public Supervisor(string filename)
		{
			//Init(filename, System.Environment.MachineName, GetDefExeName(), GetDefAppName());
            Init(filename, System.Net.Dns.GetHostName(), GetDefExeName(), GetDefAppName());
		}
		/// <summary>
		/// 
		/// </summary>
		public Supervisor()
		{
			//Init(GetDefXmlName(), System.Environment.MachineName, GetDefExeName(), GetDefAppName());
            Init(GetDefXmlName(), System.Net.Dns.GetHostName(), GetDefExeName(), GetDefAppName());
        }
		        private void Init(string filename, string host, string application, string name)
		{
			
			//_enabled_to_writedata = true;
			//if (File.Exists(filename))
			//{
				_filename = filename;
				_host = host; 
				_application = application;
				_name = name;
			//}
			//else
			//{
			//	_errormessage = "File or path is not supported!";

			//	_filename = "";
			//	_host = host; 
			//	_application = application;
			//	_name = name;
		//	}
		}
        private void Init(System.Xml.XmlNode node)
        {

            
            _filename = TV4.TextTv.Xml.ReadString(node, "@file", "");

            if (_filename.Length == 0)
            {
                String _path = TV4.TextTv.Xml.ReadString(node, "@path", "");
                
                if (_path.Length > 0)
                _filename =  _path +  "\\" + GetDefXmlName();
            }
            _host = TV4.TextTv.Xml.ReadString(node, "@host", System.Net.Dns.GetHostName());
            _application = TV4.TextTv.Xml.ReadString(node, "@application", GetDefExeName());
            _name = TV4.TextTv.Xml.ReadString(node, "@name", GetDefAppName());
        }
        private string GetDefAppName()
		{
			FileInfo file = new FileInfo(System.AppDomain.CurrentDomain.FriendlyName);
			return file.Name.Replace(file.Extension,"");
		}
		private string GetDefExeName()
		{
			FileInfo file = new FileInfo(System.AppDomain.CurrentDomain.FriendlyName);
			return file.Name;
		}
		private string GetDefXmlName()
		{
			FileInfo file = new FileInfo(System.AppDomain.CurrentDomain.FriendlyName);
			return file.Name.Replace(file.Extension,".xml");
		}
        public void SetPath(String inpath)
        {
            if (inpath.Length > 0)
                _filename = inpath + "\\" + GetDefXmlName();
        }
        public string HOST
		{
			get
			{
				return this._host;
			}
		}
        public string NAME
        {
            get
            {
                return this._name;
            }
        }
        public string APPLICATION
        {
            get
            {
                return this._application;
            }
        }
        public string FILENAME
        {
            get
            {
                return this._filename;
            }
        }
        /*
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                //_path = value;

                try
                {
                    DirectoryInfo dir =	new DirectoryInfo(value);
				
                    if (dir.Exists)
                    {
                        _path = value;
                    }
                    else
                    {
                        //_path = "C:\\";
                        _path = "";
                        _errormessage = "Path doesn't exists!";
                        //_enabled_to_writedata = false;
                    }
                }
                catch (Exception e)
                {
                    _path = "";
                    _errormessage = "[Supervisor] - " + e.Message;
                    //_enabled_to_writedata = false;
                }
            }
        }
        */ 
		public string Errormessage
		{
			get
			{
				return _errormessage;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public void Send()
		{
			MakeFile();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="min"></param>
		public void Send(int min)
		{
			Send("Allt ok!",min);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public void Send(string message)
		{
			Send(message,0);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="min"></param>
		public void Send(string message, int min)
		{
			_message = message;
			_nextreport = DateTime.Now.AddMinutes(min);
			MakeFile();
		}
		private void MakeFile()
		{
			//FileStream aFile = new FileStream(_filename,FileMode.Truncate);
			//StreamWriter sw = new  StreamWriter(aFile);
			
			_errormessage = "";
            Console.WriteLine(DateTime.Now.ToString("HH:mm.ss - ") + "[Supervisor] | Create file " + _filename); 
			//if ((_filename.Length > 0) && (_path.Length > 0))
            if (_filename.Length > 0)
			{

				try
				{
					//StreamWriter sw = new StreamWriter(_path +"\\"+_filename);
                    StreamWriter sw = new StreamWriter(_filename);

					sw.WriteLine("<statusreport>");
					sw.WriteLine("<host>" + _host + "</host>");
					sw.WriteLine("<application>" + _application + "</application>");
					sw.WriteLine("<name>" + _name + "</name>");
					sw.WriteLine("<severity level=\"0\"/>");
					sw.WriteLine("<message>" + _message + "</message>");
					sw.WriteLine("<timestamp>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</timestamp>");
					sw.WriteLine("<nextreport>" + _nextreport.ToString("yyyy-MM-dd HH:mm:ss") + "</nextreport>");
					sw.WriteLine("</statusreport>");
			
					sw.Close();
				}
				catch (Exception e)
				{
					_errormessage = e.Message;
					//_enabled_to_writedata = false;
				}

			}
			else
			{
				_errormessage = "Unable to write to supervisor!, File("+_filename+")";
				//_enabled_to_writedata = false;
			}
		}

	}
}
