using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTNITFReader
{
    public class TTTopicData
    {
        
        public int Id, Subref, SubrefType;
        public string Code, HSource;

        public TTTopicData()
        {
        
        }
        public TTTopicData(System.Xml.XmlNode Node)
        {
            if (Node != null)
            {
                String[] Temp = Node.InnerText.Split(':');

                HSource = Temp[0];

                Id = int.Parse(Temp[1].Substring(0, 2));
                Subref = int.Parse(Temp[1].Substring(2, 3));
                SubrefType = int.Parse(Temp[1].Substring(5, 3));

                Code = Temp[2];

            }
            else
            {
                Clear();
            }

        }
        public void Clear()
        {
            Id = 0;
            Subref = 0;
            SubrefType = 0;
            Code = "";
        }
        public String Value
        {
            get
            {
                return HSource + ":" + Id.ToString("D2") + Subref.ToString("D3") + SubrefType.ToString("D3") + ":" + Code;
            }
            set
            {
                if (value != "")
                {

                    String[] Temp = value.Split(':');

                    if (Temp.Length == 3)
                    {
                        HSource = Temp[0];

                        Id = int.Parse(Temp[1].Substring(0, 2));
                        Subref = int.Parse(Temp[1].Substring(2, 3));
                        SubrefType = int.Parse(Temp[1].Substring(5, 3));
                        Code = Temp[2];

                    }
                    else
                    {
                        Clear();
                    }
                }
                else
                {
                    Clear();
                }

            }
        }
    }
    public class TTArticleData
    {
        //public int Type;
        public string Headline;
        public string[] Text;

        public void SetText(System.Xml.XmlNode Node)
        {
            SetText(Node.SelectNodes("INGRESS"));
            SetText(Node.SelectNodes("P"));    
        }
        public void SetText(System.Xml.XmlNodeList Nodes)
        {
                    
            List<string> Temp = new List<string>();

            foreach (System.Xml.XmlNode n in Nodes)
                Temp.Add(n.InnerText);

            Text = Temp.ToArray();
        }
    }
    public class TTShortArticle : TTArticleData
    {
        public enum CategoryType {Inrikes, Utrikes, Ekonomi, IT, Sport, Kändis, Film, Kultur, Motor, Mode, Nöje, Spel}
        public int Id; //, RefId;
        public int[] RefId;
        public int[] Category = new int[12];

        public TTShortArticle()
        {
            for (int p = 0; p < Category.Length; p++)
                Category[p] = 0;

        }
        public TTShortArticle(System.Xml.XmlNode Node)
        {
            for (int p = 0; p < Category.Length; p++)
                Category[p] = 0;

            Id = GetAttribute(Node, "ID");
            RefId = GetRefNr(Node, "REFNR");

            Category[(int)CategoryType.Inrikes] = GetAttribute(Node, "ALLMI");
            Category[(int)CategoryType.Utrikes] = GetAttribute(Node, "ALLMU");
            Category[(int)CategoryType.Ekonomi] = GetAttribute(Node, "EKO");
            Category[(int)CategoryType.IT] = GetAttribute(Node, "IT");
            Category[(int)CategoryType.Sport] = GetAttribute(Node, "SPT");
            Category[(int)CategoryType.Kändis] = GetAttribute(Node, "BNG");
            Category[(int)CategoryType.Film] = GetAttribute(Node, "FRC");
            Category[(int)CategoryType.Kultur] = GetAttribute(Node, "KLT");
            Category[(int)CategoryType.Motor] = GetAttribute(Node, "MOTOR");
            Category[(int)CategoryType.Mode] = GetAttribute(Node, "MDE");
            Category[(int)CategoryType.Nöje] = GetAttribute(Node, "NOJ");
            Category[(int)CategoryType.Spel] = GetAttribute(Node, "SPEL");

            Headline = Node.SelectSingleNode("RUBBERUBRIK").InnerText.Replace("FLASH:","");
            SetText(Node.SelectSingleNode("RUBBETEXT"));

        }
        private int GetAttribute(System.Xml.XmlNode Node, string name)
        {
            System.Xml.XmlAttribute attr = Node.Attributes[name];
            if (attr != null)
            {
                if (attr.InnerText != "")
                    return int.Parse(attr.InnerText);
                else
                    return 0;
            }
            else
            {
                return 0;
            }
        }
        private int[] GetRefNr(System.Xml.XmlNode Node, string name)
        {
            System.Xml.XmlAttribute attr = Node.Attributes[name];
            List<int> Ret = new List<int>();
            if (attr != null)
            {
                if (attr.InnerText != "")
                {
                    foreach (string tmp in attr.InnerText.Split(' ')) 
                        Ret.Add(int.Parse(tmp));
                }
                else
                    Ret.Add(0);
            }
            else
            {
                Ret.Add(0);
            }

            return Ret.ToArray();
        }
    }
    public class TTLongArticle : TTArticleData
    {
        public String City, Source;

        public TTLongArticle(System.Xml.XmlNode Node)
        {

            City = GetInnertext(Node.SelectSingleNode("DAT/ORT"));
            Source = GetInnertext(Node.SelectSingleNode("DAT/SOURCE"));

            Headline = Node.SelectSingleNode("RUBRIK").InnerText.Replace("FLASH:","");
            SetText(Node.SelectSingleNode("BRODTEXT"));

        }
        private String GetInnertext(System.Xml.XmlNode Node)
        {
            if (Node != null)
                return Node.InnerText;
            else
                return "";
        }
    }

    public class TTData
    {
        public string Id;
        public string Source;
        public string[] Product;
        public string[] Destination;
        public DateTime SendDate;
        //public string Type;
        public int Type;
        public string Slugg;
        public string Status;
        public int Prio;
        public TTTopicData[] Topic;
        public string FixId;
        public int Action;
        public string Headline;
        public string HSource;
        public string Writer;
        public string Lanuage;
        public DateTime Embargo;
        public string[] RefId;
        public int Nr;

        public TTShortArticle ShortArticle;
        public TTLongArticle LongArticle;

        public TTData()
        {
        }
        public void Load(System.IO.FileInfo File)
        {
            Load(File.FullName);
        }
        public void Load(String Filename)
        {
            System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
            //XmlResolver = null;
            _doc.Load(Filename);

            Load(_doc);

        }
        public void Load(System.Xml.XmlDocument Doc)
        {
            Load(Doc.SelectSingleNode("TTNITF"));        
        }
        public void Load(System.Xml.XmlNode Node)
        {
            Id = GetValue(Node.SelectSingleNode("HEAD/UNO"));
            Source = GetValue(Node.SelectSingleNode("HEAD/SERVID"));
            Product = GetValue(Node.SelectNodes("HEAD/PRODID/@TKOD"));

            SendDate = ToDateTime(GetValue(Node.SelectSingleNode("HEAD/DATESENT")), GetValue(Node.SelectSingleNode("HEAD/TIMESENT")));

            Destination = GetValue(Node.SelectNodes("HEAD/DEST"));

            String[] Temp = GetValue(Node.SelectSingleNode("HEAD/TEXTTYP")).Split(':');

            if (Temp.Length > 1)
                Type = int.Parse(Temp[0]);
            else 
                Type = 1;

            //Type = GetValue(Node.SelectSingleNode("HEAD/TEXTTYP"));
            Slugg = GetValue(Node.SelectSingleNode("HEAD/SLUGG"));
            Status = GetValue(Node.SelectSingleNode("HEAD/EDSTAT"));
            Prio = GetInt(Node.SelectSingleNode("HEAD/URG/@PRIO"));
            //Category = GetValue(Node.SelectNodes("HEAD/SUBREF"));
            Topic = GetTopicArray(Node.SelectNodes("HEAD/SUBREF"));

            FixId = GetValue(Node.SelectSingleNode("HEAD/FIXID"));
            Action = GetInt(Node.SelectSingleNode("HEAD/ACTADV/@ACTION"));
            Headline = GetValue(Node.SelectSingleNode("HEAD/HEADLINE"));
            Headline = Headline.Replace("FLASH:","");
            HSource = GetValue(Node.SelectSingleNode("HEAD/HSOURCE")); 
            Writer = GetValue(Node.SelectSingleNode("HEAD/WRITER"));
            Lanuage = GetValue(Node.SelectSingleNode("HEAD/LANG"));
            Embargo = ToDateTime(GetValue(Node.SelectSingleNode("HEAD/EMBARGO")));
            RefId = GetValue(Node.SelectNodes("HEAD/REFUNO"));
            Nr = GetInt(Node.SelectSingleNode("HEAD/NR"));

            ShortArticle = new TTShortArticle(Node.SelectSingleNode("BODY/RUBBE"));
            LongArticle = new TTLongArticle(Node.SelectSingleNode("BODY/TEXT"));


            if (Id.Length == 0)
            {
                Id = SendDate.ToString("yyyyMMdd") + ":" + HSource + ":" + Nr.ToString("D4") +":0";

            }

        }
        public bool DestinationContains(String[] DestList)
        {
            bool ret = false;

            foreach (String d in DestList)
            {
                if (Destination.Contains(d))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
        private int GetInt(System.Xml.XmlNode Node)
        {
            if (Node != null)
            {
                if (Node.InnerText != "")
                    return int.Parse(Node.InnerText);
                else
                    return 0;
            }
            else
                return 0;
        }
        private string GetValue(System.Xml.XmlNode Node)
        {
            if (Node != null)
                return Node.InnerText;
            else
                return "";
        }
        private string[] GetValue(System.Xml.XmlNodeList Nodes)
        {
            if (Nodes != null)
            {
                List<string> List = new List<string>();

                foreach(System.Xml.XmlNode Node in Nodes)
                    List.Add(GetValue(Node));

                return List.ToArray();

            }
            else
                return null;
        }
        private TTTopicData[] GetTopicArray(System.Xml.XmlNodeList Nodes)
        {
            List<TTTopicData> Ret = new List<TTTopicData>();

            foreach(System.Xml.XmlNode Node in Nodes)
                Ret.Add(new TTTopicData(Node));

            return Ret.ToArray();
        }
        private DateTime ToDateTime(String Date, String Time)
        {
            
            if ((Date.Length > 7) && (Time.Length > 6))
            {
                Date = Date.Substring(0,4) + "-" + Date.Substring(4,2) + "-" + Date.Substring(6,2);
                Time = Time.Substring(0,2) + ":" + Time.Substring(2,2) + ":" + Time.Substring(4,2);

                DateTime Ret = DateTime.Parse(Date + "T" + Time);

                string[] temp = Time.Split('+');

                if (temp.Length > 1)
                    Ret.AddHours(int.Parse(temp[1]));

                return DateTime.Parse(Date + "T" + Time);
            } else {
                return DateTime.Now;
            }
        }
        private DateTime ToDateTime(String Date)
        {
            if (Date.Length > 6)
            {
                string[] Temp = Date.Split('+');
                DateTime Ret = DateTime.Parse(Temp[0]);

                if (Temp.Length > 1)
                    Ret.AddHours(int.Parse(Temp[1]));

                return Ret;
            }
            else
            {
                return DateTime.Now;
            }
        }
    
    }
}
