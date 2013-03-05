using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTNITFReader
{
    public delegate void MessageHandler(string str);
    public delegate void NewsHandler(TTData TTArticle);
    public delegate void TimeStampHandler(DateTime Timestamp);

    class TTFileReader
    {
        private System.Timers.Timer _timer = new System.Timers.Timer();
        public TTArticleDatabase DB;
        public String Path;
        public String[] Dest;
        private int _interval;

        public event MessageHandler OnMessage;
        public event MessageHandler OnError;
        public event NewsHandler OnNewArticle;
        public event TimeStampHandler OnNextScan;
        public TTFileReader()
        {
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);    
        }

        public void Open()
        {
            _timer.Start();
        }
        public void Close()
        {
            _timer.Stop();
        }
        public int Scaninterval
        {
            get
            {
                return (int)_timer.Interval / 1000;
            }
            set
            {
                _interval = value;
                _timer.Interval = value * 1000;
            }
        }
        public bool Active
        {
            get
            {
                return _timer.Enabled;
            }
            set
            {
                _timer.Enabled = value;
            }
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DoIt();

            //Message("Next scan in " + (Timer.Interval / 1000).ToString() + " sec.");
            if (OnNextScan != null)
                OnNextScan(DateTime.Now.AddSeconds(_interval));

        }
        private void DoIt()
        {
            //if (DB.Connect())
            if (DB.Connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    DB.Disconnect();
                    DB.Connect();
                }
                catch (Exception ex)
                {
                    Error("Error connect to DB: " + ex.Message);
                }
            }


            if (DB.Connection.State == System.Data.ConnectionState.Open)
            {

                Scan(Path);

                //WriteLine("Done!");
            }
            else
            {
                Error("Databse connection not open!");
            }

        }
        private void Scan(String Path)
        {

            

            System.IO.DirectoryInfo Dir = new System.IO.DirectoryInfo(Path);

            //foreach (String File in System.IO.Directory.GetFiles(Path, "*.xml"))
            foreach (System.IO.FileInfo File in Dir.GetFiles("*.xml"))
            {
                try
                {
                    TTData TTData = new TTData();

                    Message("Load file: " + File.Name);
                    TTData.Load(File);

                    if (TTData.DestinationContains(Dest))
                    {
                        //Message("Add article to DB.");

                        DB.Add(TTData);

                        Newarticle(TTData);

                    }
                    else
                    {
                        Message("Don't save to DB");
                    }

                    //Message("Remove file: " + File.Name);
                    File.Delete();
                    //System.IO.File.Delete(File.FullName, Path + @"\error\" + File.Name);
                }
                catch (Exception ex)
                {
                    Error("Error: " + ex.Message);
                    if (System.IO.Directory.Exists(Path + @"\error\"))
                    {
                        Message("Move file to: " + Path + @"\error\");
                        System.IO.File.Move(File.FullName, Path + @"\error\" + File.Name);
                    }
                }
            }
        }
        private void Newarticle(TTData TTArticle)
        {
            if (OnNewArticle != null)
                OnNewArticle(TTArticle);
        }
        private void Message(String str)
        {
            if (OnMessage != null)
                OnMessage(str);
        }
        private void Error(String str)
        {
            if (OnError != null)
                OnError(str);
        }
    }
}
