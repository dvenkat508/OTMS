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
    class GetActiveUsersList
    {
        public List<list> sob = new List<list>();
        public List<list> s = new List<list>();
        public void toall(List<socket> a,Socket soc)
        {
            MultiServer.Program.SQLConn.Close();
            byte[] file = { };
            byte[] img = { };
            string name = "";
            list k = new list();
            sob.Clear();
            foreach (socket i in a)
            {    
                if(i.email!="")
                {
                    String Command = "SELECT CLIENT_PHOTO,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + i.email + "'";
                    try
                    {
                        MultiServer.Program.SQLConn.Open();
                    }
                    catch { }
                    SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                    SqlDataReader Reader = cmd.ExecuteReader();
                    if (Reader.Read())
                    {
                        name = Reader["CLIENT_NAME"].ToString();
                        try
                        {
                            img = (byte[])Reader["CLIENT_PHOTO"];
                        }
                        catch
                        {
                            img = null;
                        }
                    }
                    sob.Add(new list(name, i.email, img));
                    Reader.Close();                    
                }               
            }
            //sob.Clear();
            foreach (socket i  in  a)
            {
                if(i.sock != soc)
                {
                    s = sob;
                    foreach(list j in s)
                    {
                        if(i.email == j.Email)
                        {
                            s.Remove(j);
                            break;
                        }
                    }
                    StreamWriter stream = null;
                    XmlSerializer xmlSerializer;
                    xmlSerializer = new XmlSerializer(typeof(List<list>));
                    MemoryStream memoryStream = new MemoryStream();
                    stream = new StreamWriter(memoryStream);
                    XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
                    xs.Add("", "");
                    xmlSerializer.Serialize(stream, s, xs);
                    file = memoryStream.GetBuffer();
                    byte[] dat = Encoding.ASCII.GetBytes("ACTIVE_USER_LIST$" + Encoding.ASCII.GetString(file) + "$0");
                    i.sock.Send(dat);
                }                
            }            
            MultiServer.Program.SQLConn.Close();
        }
    }
}
