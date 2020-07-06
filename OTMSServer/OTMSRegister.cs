using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Data.SqlClient;
using WpfApp1;

namespace OTMSServer
{
    class OTMSRegister
    {
        public static InvalidData GetRegister(Register a)
        {
            MultiServer.Program.SQLConn.Close();
            InvalidData err = new InvalidData();
            String Command = "SELECT CLIENT_EMAIL_ID FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '"+a.Emailid+"'";
            try
            {
                MultiServer.Program.SQLConn.Open();
            }
            catch { }
            SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            SqlDataReader Reader = cmd.ExecuteReader();
            int k=0;
            if(Reader.Read())
            {
                err.Emailid = false;            
            }            
            else
            {
                Reader.Close();
                if (a.Image == null)
                {
                    Command = "INSERT INTO CLIENT_REGISTER (CLIENT_NAME,CLIENT_EMAIL_ID,CLIENT_PASSWORD) VALUES('"+a.Name+"','"+a.Emailid+"','"+a.Password+"')";
                    cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                }
                else
                {
                    Command = "INSERT INTO CLIENT_REGISTER (CLIENT_NAME,CLIENT_EMAIL_ID,CLIENT_PASSWORD,CLIENT_PHOTO) VALUES('" + a.Name + "','" + a.Emailid + "','" + a.Password+"',@PHOTO)";
                    cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                    cmd.Parameters.Add("@PHOTO", System.Data.SqlDbType.Image, a.Image.Length).Value = a.Image;
                }
                cmd.ExecuteNonQuery();
                Command = "SELECT CLIENT_ID FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID ='" + a.Emailid + "'";
                cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                SqlDataReader Reader1 = cmd.ExecuteReader();
                if (Reader1.Read())
                {
                    k = (int)Reader1["CLIENT_ID"];
                }
                Reader1.Close();
                Command = "CREATE TABLE "+ a.Name+ k.ToString() +"SEND (SEND_ID INTEGER IDENTITY NOT NULL PRIMARY KEY,SEND_MESSAGE VARBINARY(MAX) NOT NULL,RECIVER_EMAIL_ID VARCHAR(150) NOT NULL,SEND_TIME DATETIME NOT NULL)";
                cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                cmd.ExecuteNonQuery();
                Command = "CREATE TABLE " + a.Name + k.ToString() + "RECEIVE (RECEIVE_ID INTEGER IDENTITY NOT NULL PRIMARY KEY,RECEIVE_MESSAGE VARBINARY(MAX) NOT NULL,SENDER_EMAIL_ID VARCHAR(150) NOT NULL,RECEIVE_TIME DATETIME NOT NULL)";
                cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
                cmd.ExecuteNonQuery();
                err.Emailid = true;
                //MultiServer.Program.result = "OK";
            }
            MultiServer.Program.SQLConn.Close();
            return err;
        }
    }

    class OTMSLogin
    {
        public static bool GetLogin(Loginob a)
        {
            MultiServer.Program.SQLConn.Close();
            bool valid=true;
            String Emailid = "", password = "";
            String Command = "SELECT CLIENT_EMAIL_ID, CLIENT_PASSWORD FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + a.Emailid + "'";
            try
            {
                MultiServer.Program.SQLConn.Open();
            }
            catch { }
            SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            SqlDataReader Reader = cmd.ExecuteReader();
            if (Reader.Read())
            {
                Emailid = Reader["CLIENT_EMAIL_ID"].ToString();
                password = Reader["CLIENT_PASSWORD"].ToString();
                if(Emailid == a.Emailid && password == a.Password)
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }
            }
            else
            {
                valid = false;
            }
            MultiServer.Program.SQLConn.Close();
            return valid;
        }
    }

    class TextMessage
    {
        public static void ProcessMessage(textmessage textmessage)
        {
            MultiServer.Program.SQLConn.Close();
            string Clientname = "", ClientID = "";
            string Command = "SELECT CLIENT_ID,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + textmessage.Sendemail + "'";
            try
            {
                MultiServer.Program.SQLConn.Open();
            }
            catch { }
            SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            SqlDataReader Reader = cmd.ExecuteReader();
            if (Reader.Read())
            {
                ClientID = ((int)Reader["CLIENT_ID"]).ToString();
                Clientname = Reader["CLIENT_NAME"].ToString();
            }
            Reader.Close();
            byte[] mes = Encoding.ASCII.GetBytes(textmessage.Message);
            string Tablename = Clientname + ClientID + "SEND";
            Command = "INSERT INTO " + Tablename + "(SEND_MESSAGE,RECIVER_EMAIL_ID,SEND_TIME) VALUES (@MESSAGE,'" + textmessage.Toemail + "','" + DateTime.Now + "');";
            cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            cmd.Parameters.Add("@MESSAGE", System.Data.SqlDbType.Image, mes.Length).Value = mes;
            cmd.ExecuteNonQuery();
            Command = "SELECT CLIENT_ID,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + textmessage.Toemail + "'";
            try
            {
                MultiServer.Program.SQLConn.Open();
            }
            catch { }
            cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            SqlDataReader Reader1 = cmd.ExecuteReader();
            if (Reader1.Read())
            {
                ClientID = ((int)Reader1["CLIENT_ID"]).ToString();
                Clientname = Reader1["CLIENT_NAME"].ToString();
            }
            Reader1.Close();
            Tablename = Clientname + ClientID + "RECEIVE";
            Command = "INSERT INTO " + Tablename + "(RECEIVE_MESSAGE,SENDER_EMAIL_ID,RECEIVE_TIME) VALUES (@MESSAGE,'" + textmessage.Sendemail + "','" + DateTime.Now + "');";
            cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            cmd.Parameters.Add("@MESSAGE", System.Data.SqlDbType.Image, mes.Length).Value = mes;
            cmd.ExecuteNonQuery();
        }
    }

    class MediaMessage
    {
        public static void ProcessMediaMessage(media mediamessage)
        {
            MultiServer.Program.SQLConn.Close();
            string Clientname = "", ClientID = "";
            string Command = "SELECT CLIENT_ID,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + mediamessage.Sendemail + "'";
            try
            {
                MultiServer.Program.SQLConn.Open();
            }
            catch { }
            SqlCommand cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            SqlDataReader Reader = cmd.ExecuteReader();
            if (Reader.Read())
            {
                ClientID = ((int)Reader["CLIENT_ID"]).ToString();
                Clientname = Reader["CLIENT_NAME"].ToString();
            }
            Reader.Close();
            byte[] mes = mediamessage.Message;
            string Tablename = Clientname + ClientID + "SEND";
            Command = "INSERT INTO " + Tablename + "(SEND_MESSAGE,RECIVER_EMAIL_ID,SEND_TIME) VALUES (@MESSAGE,'" + mediamessage.Toemail + "','" + DateTime.Now + "');";
            cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            cmd.Parameters.Add("@MESSAGE", System.Data.SqlDbType.Image, mes.Length).Value = mes;
            cmd.ExecuteNonQuery();
            Command = "SELECT CLIENT_ID,CLIENT_NAME FROM CLIENT_REGISTER WHERE CLIENT_EMAIL_ID = '" + mediamessage.Toemail + "'";
            try
            {
                MultiServer.Program.SQLConn.Open();
            }
            catch { }
            cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            SqlDataReader Reader1 = cmd.ExecuteReader();
            if (Reader1.Read())
            {
                ClientID = ((int)Reader1["CLIENT_ID"]).ToString();
                Clientname = Reader1["CLIENT_NAME"].ToString();
            }
            Reader1.Close();
            Tablename = Clientname + ClientID + "RECEIVE";
            Command = "INSERT INTO " + Tablename + "(RECEIVE_MESSAGE,SENDER_EMAIL_ID,RECEIVE_TIME) VALUES (@MESSAGE,'" + mediamessage.Sendemail + "','" + DateTime.Now + "');";
            cmd = new SqlCommand(Command, MultiServer.Program.SQLConn);
            cmd.Parameters.Add("@MESSAGE", System.Data.SqlDbType.Image, mes.Length).Value = mes;
            cmd.ExecuteNonQuery();
        }
    }
}
