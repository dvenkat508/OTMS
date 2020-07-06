using System;
using System.Collections.Generic;
using WpfApp1;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTMSServer
{
    [Serializable]
    class LoginReturn
    {
        public List<list> sob = new List<list>();
        public list s = new list();
        public byte[] files(List<socket> a,Socket soc)
        {
            MultiServer.Program.SQLConn.Close();
            // List<list> sob = new List<list>(); 
            byte[] file = {};
            byte[] img = {};
            string name = "";
            foreach (socket i in a)
            {
                if(i.sock != soc)
                {
                    String Command = "SELECT CLIENT_PHOTO,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + i.email + "'";
                    try
                    { MultiServer.Program.SQLConn.Open(); }
                    catch { }
                    SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                    SqlDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {                        
                        name = Reader["CLIENT_NAME"].ToString();
                        if(Reader["CLIENT_PHOTO"]==null)
                        {
                            img = null;
                        }
                        else
                        {
                            img = (byte[])Reader["CLIENT_PHOTO"];
                        }
                        sob.Add(new list(name, i.email, img));                        
                    }                    
                    Reader.Close();
                }                
            }
            StreamWriter stream = null;
            XmlSerializer xmlSerializer;
            xmlSerializer = new XmlSerializer(typeof(List<list>));
            MemoryStream memoryStream = new MemoryStream();
            stream = new StreamWriter(memoryStream);
            XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
            xs.Add("", "");
            xmlSerializer.Serialize(stream, sob, xs);
            file = memoryStream.GetBuffer();
            MultiServer.Program.SQLConn.Close();
            return file;
        }
        public byte[] mydata(List<socket> a, Socket soc)
        {
            byte[] dat = { };
            byte[] img = { };
            string name = "";            
            foreach (socket temp in a)
            {
                if (temp.sock==soc)
                {
                    String Command = "SELECT CLIENT_PHOTO,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + temp.email + "'";
                    try
                    { MultiServer.Program.SQLConn.Open(); }
                    catch { }
                    SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                    SqlDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        name = Reader["CLIENT_NAME"].ToString();
                        if (Reader["CLIENT_PHOTO"] == null)
                        {
                            img = null;
                        }
                        else
                        {
                            img = (byte[])Reader["CLIENT_PHOTO"];
                        }
                        s.Name = name;
                        s.Email = temp.email;
                        s.Img = img;
                    }
                    Reader.Close();
                }
            }
            StreamWriter stream = null;
            XmlSerializer xmlSerializer;
            xmlSerializer = new XmlSerializer(typeof(list));
            MemoryStream memoryStream = new MemoryStream();
            stream = new StreamWriter(memoryStream);
            XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
            xs.Add("", "");
            xmlSerializer.Serialize(stream, s, xs);
            dat = memoryStream.GetBuffer();
            MultiServer.Program.SQLConn.Close();
            return dat;
        }
    }
}
