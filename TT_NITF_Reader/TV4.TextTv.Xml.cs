using System;
using System.Xml;

namespace TV4.TextTv
{
	/// <summary>
	/// Summary description for Xml.
	/// </summary>
	public class Xml
	{
        private static System.Xml.XmlNode tmp_node;
        
        public Xml()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static XmlNode AppendChild(ref XmlNode node, string name, string val) 
		{
			XmlNode new_node = node.OwnerDocument.CreateNode(XmlNodeType.Element, name, "");
			new_node.InnerText = val;
			node.AppendChild(new_node);
			return new_node;
		}

		public static void AppendAttribute(ref XmlNode node, string name, string val) 
		{
			XmlAttribute new_attr = node.OwnerDocument.CreateAttribute(name);
			new_attr.Value = val;
			node.Attributes.SetNamedItem(new_attr);
		}

		public static XmlDocument CreateNewXmlDocument(string rootnode) 
		{

            XmlDocument xml_doc = new XmlDocument();

            xml_doc.LoadXml(rootnode);

            return xml_doc;


            //return CreateNewXmlDocument(new System.IO.MemoryStream(System.Text.ASCIIEncoding.Default.GetBytes(rootnode)));
		}
        public static XmlDocument CreateNewXmlDocument(System.IO.MemoryStream stream)
        {


            /*
            XmlTextReader StringReader = new XmlTextReader(stream);
            
            XmlDocument xml_doc = new XmlDocument();
            //StringReader.

            xml_doc.Load(StringReader);

            return xml_doc;
            */

            XmlDocument xml_doc = new XmlDocument();

            xml_doc.Load(stream);

            return xml_doc;
 
            
        }
        public static XmlDocument CreateNewXmlDocument(System.IO.Stream stream)
        {
            XmlDocument xml_doc = new XmlDocument();

            xml_doc.Load(stream);

            return xml_doc;
        }
        public static XmlNode CreateNewXmlNode(String XMLstring)
        {
            System.Xml.XmlDocument tmpxml = new XmlDocument();
            //XmlTextReader xmlReader = new XmlTextReader(new System.IO.StringReader(XMLstring));

            //return (tmpxml.ReadNode(xmlReader));
            if (XMLstring.Length > 0)
                return (tmpxml.ReadNode(new XmlTextReader(new System.IO.StringReader(XMLstring))));
            else
                return null;

        }

        public static bool ReadBool(XmlNode node, string tag, bool def)
        {
            bool ret = false;

            try
            {
                tmp_node = node.SelectSingleNode(tag);

                if (tmp_node != null)
                {

                    switch (tmp_node.InnerText.ToLower())
                    {
                        case "yes":
                        case "true":
                        case "ja":
                        case "1":
                            ret = true;
                            break;
                        case "no":
                        case "nej":
                        case "false":
                        case "0":
                            ret = false;
                            break;
                    }
                }
                else
                {
                    ret = def;
                }

            }
            catch
            {
                ret = def;
            }

            return ret;

        }
        public static int ReadInteger(XmlNode node, string tag, int def)
        {
            int ret;

            try
            {
                tmp_node = node.SelectSingleNode(tag);

                if (tmp_node != null)
                    ret = int.Parse(tmp_node.InnerText);
                else
                    ret = def;
            }
            catch
            {
                ret = def;
            }

            return ret;

        }
        public static float ReadFloat(XmlNode node, string tag, float def)
        {
            float ret;

            try
            {
                tmp_node = node.SelectSingleNode(tag);

                if (tmp_node != null)
                    ret = float.Parse(tmp_node.InnerText);
                else
                    ret = def;
                }
            catch
            {
                ret = def;
            }

            return ret;

        }
        public static string ReadString(XmlNode node, string tag, string def)
        {
            string ret = "";

            try
            {
                tmp_node = node.SelectSingleNode(tag);

                if (tmp_node != null)
                    ret = tmp_node.InnerText;
                else
                    ret = def;
            }
            catch
            {
                ret = def;
            }

            return ret;

        }
        public static DateTime ReadDateTime(XmlNode node, string tag, string def)
        {
            try
            {
                return DateTime.Parse(ReadString(node, tag, def));
            }
            catch
            {
                return DateTime.Parse(def);
            }
        }
        public static DateTime ReadDateTime(XmlNode node, string tag, DateTime def)
        {
            try
            {
                return DateTime.Parse(ReadString(node, tag, def.ToString("s")));
            }
            catch
            {
                return def;
            }
        }


	}
}
