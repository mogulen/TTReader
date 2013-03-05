using System;
using System.IO;
using System.Collections.Generic;
namespace Log
{
	public class File
	{
        private bool _active;
        private string _path, _fileName, _fileformat, _message, _timeformat;
        //private static object locker = new object();

        private List<String> _Waiting = new List<String>();

        public File()
        {
            try
            {
                //_fileformat = "yyyy-MM-dd";
                //_timeformat = "HH:mm:ss - ";
                Clear();

                _path = System.AppDomain.CurrentDomain.BaseDirectory;

                if (!System.IO.Directory.Exists(_path))
                    Directory.CreateDirectory(_path);

                _active = true;
            }
            catch (Exception ex)
            {
                _message = ex.Message;
                _active = false;
            }
        }
        public File(string ph)
        {
            try{
                //_fileformat = "yyyy-MM-dd";
                Clear();

                Path = ph;
            } 
            catch (Exception ex)
            {
                _message = ex.Message;
                _active = false;
            }
        }
        public void Clear()
		{
            ClearWaitingMessages();
            _message = "";
            _path = "";
            _fileformat = "yyyy-MM-dd";
            _timeformat = "HH:mm:ss - ";
            _active = false;
		}
        public void ClearWaitingMessages()
        {
            _Waiting.Clear();
        }
		public void Add(string str)
		{
			Add(DateTime.Now,str);	
		}
        public void Add(DateTime td, string str)
		{

            if (_active)
            {
                //string dateStr; //, timeStr;//, logStr;

                //dateStr = DateTime.Now.ToString(_fileformat);
                //timeStr = DateTime.Now.ToString(_timeformat);


                _fileName = _path + DateTime.Now.ToString(_fileformat) + ".log";
                //logStr = timeStr + " - " + str;

                if (_timeformat.Length > 0)
                    _Waiting.Add(DateTime.Now.ToString(_timeformat) + str);
                else
                    _Waiting.Add(str);

                try
                {
                    //using (FileStream fs = new FileStream(_fileName, FileMode.Append, FileAccess.ReadWrite, FileShare.Read))
                    using (FileStream fs = new FileStream(_fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        using (StreamWriter sw = new StreamWriter(fs)) 
                        {

                            foreach (String line in _Waiting)
                                sw.WriteLine(line);
                        
                            sw.Flush();
                            sw.Close();
                        }
                        //fs.Flush();
                    }
                    
                    _Waiting.Clear();
                }
                catch (Exception ex)
                {
                    //Fil låst, alt. har inte rättigheter.
                    if (_timeformat.Length > 0)
                    {
                        String Tm = DateTime.Now.ToString(_timeformat);
                        _Waiting.Add("[" + _Waiting.Count.ToString() + "] " + Tm + "Error writing to logfile, " + ex.Message);
                        _Waiting.Add("[" + _Waiting.Count.ToString() + "] " + Tm + str);
                    }
                    else
                    {
                        _Waiting.Add("[" + _Waiting.Count.ToString() + "] Error writing to logfile, " + ex.Message);
                        _Waiting.Add("[" + _Waiting.Count.ToString() + "] " + str);
                    }
                    //_Waiting.Add(ex.Message);

                    if (_Waiting.Count > 128)
                        _Waiting.Clear();

                    _message = ex.Message;
                }
            }
		}
        public void WritePendingMessages()
        {
            Add("Write pending mesages");
        }
        public void TruncateFiles(int days)
        {
            
            if (days > 0)
                days = days * -1;
            
            DateTime TruncateTime = DateTime.Now.AddDays(days);
            DirectoryInfo Dir = new DirectoryInfo(_path);

            foreach (FileInfo File in Dir.GetFiles("*.log"))
                if (File.LastWriteTime < TruncateTime)
                    File.Delete();

        }
        public String[] Read(String Filename)
        {
            List<String> Array = new List<string>();

            StreamReader Sr = new StreamReader(new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            while (!Sr.EndOfStream)
                Array.Add(Sr.ReadLine());

            Sr.Close();

            return Array.ToArray();
        }
        public String[] Read(DateTime Date)
        {
            return Read(_path + Date.ToString(_fileformat) + ".log");
        }
        public String[] Read()
        {
            return Read(DateTime.Now);
        }
        public bool Active
        {
            get 
            { 
                return _active; 
            }
            set
            {
                if (value == _active){
                    if (_active)
                        Add("Log file is already Active");
                    else
                        Add("Log file is already Inactive");
                } else {
                    
                    if (value)
                        Add("Activate logfile");
                    else
                        Add("Inactive logfile");

                    _active = value;
                }
            }
        }
        public string Path
        {
            get { return _path; }
            set 
            {
                try
                {

                    _path = value + "\\";
                    if (System.IO.Directory.Exists(_path))
                        Directory.CreateDirectory(_path);

                    _active = true;
                }
                catch (Exception ex)
                {
                    _message = ex.Message;
                    _active = false;
                }
            }
        }
        public string FileFormat
        {
            get { return _fileformat; }
            set { _fileformat = value; }
        }
        public string TimeFormat
        {
            get { return _timeformat; }
            set { _timeformat = value; }
        }
        public int PendingMesasges
        {
            get
            {
                return _Waiting.Count;
            }
        }
        public string FullName
        {
            get { return _fileName; }
        }
        public String Message
        {
            get{
                return _message;
            }
        }
	}
}