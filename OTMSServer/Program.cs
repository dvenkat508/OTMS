using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using WpfApp1;
using OTMSServer;
using System.Reflection;

namespace MultiServer
{
    internal sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            try
            {
                string ToAssemblyName = assemblyName.Split(',')[0];
                Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly ass in Assemblies)
                {
                    if (ass.FullName.Split(',')[0] == ToAssemblyName)
                    {
                        typeToDeserialize = ass.GetType(typeName);
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return typeToDeserialize;
        }
    }
    class Program
    {
        
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //private static readonly List<Socket> clientSockets = new List<Socket>();
        private static List<socket> client = new List<socket>();
        private const int BUFFER_SIZE = 2048000;
        private const int PORT = 975;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        public static SqlConnection SQLConn;
        private static string test="";
        public static Register da = new Register();
        public const string ConnectionString = @"Data Source=DESKTOP-6L6UO45\SERVERSQL;Initial Catalog=OTMS;Persist Security Info=True;User ID=sa;Password=Dvenkat1999;";
        //public static string result;
        static void Main()
        {
            try
            {
                SQLConn = new SqlConnection(ConnectionString);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (socket socket in client)
            {
                socket.sock.Shutdown(SocketShutdown.Both);
                socket.sock.Close();
            }
            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }
            bool fls = true;
            foreach (socket g in client)
            {
                if(g.sock == socket)
                {
                    fls = false;
                }
            }
            if(fls)
            { client.Add(new socket(socket)); }            
            //clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        public static GetActiveUsersList all = new GetActiveUsersList();
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;
            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                socket i = null;
                foreach (socket j in client)
                {
                    if (j.sock == current)
                        i = j;                        
                }                
                client.Remove(i);
                all.toall(client, current);
                current.Close();
                return;
            }
            if (received != 0)
            {
                byte[] recBuf = new byte[received];                
                Array.Copy(buffer, recBuf, received);
                string text = Encoding.ASCII.GetString(recBuf);
                test = test + text;                
                if (test.Substring(test.Length-2,2)=="$0")
                {
                    string first = test.Substring(0, test.IndexOf("$"));
                    if (first == "Register") // Client requested time
                    {                        
                        string data = test.Substring(test.IndexOf("$") + 1, test.IndexOf("$0") - test.IndexOf("$") - 1);
                        Console.WriteLine("Register Message recived");
                        byte[] d = Encoding.ASCII.GetBytes(data);
                        XmlSerializer xmlSerializer;
                        MemoryStream memStream = null;
                        xmlSerializer = new XmlSerializer(typeof(Register));
                        memStream = new MemoryStream(d);
                        object objectFromXml = xmlSerializer.Deserialize(memStream);
                        //Console.WriteLine("Message recived1");
                        Register a = (Register)objectFromXml;
                        da.Name = a.Name;
                        da.Image = a.Image;
                        da.Password = a.Password;
                        da.Emailid = a.Emailid;
                        InvalidData check = OTMSRegister.GetRegister(da);                        
                        test = "";
                        if(check.Emailid)
                        {
                            byte[] dat = Encoding.ASCII.GetBytes("OK");
                            current.Send(dat);
                        }
                        else
                        {
                            byte[] dat = Encoding.ASCII.GetBytes("Used Email ID");
                            current.Send(dat);
                        }
                    }
                    else if (first == "Login") // Client wants to exit gracefully
                    {
                        LoginReturn ats = new LoginReturn();
                        Loginob dt=new Loginob();                        
                        string data = test.Substring(test.IndexOf("$") + 1, test.IndexOf("$0") - test.IndexOf("$") - 1);
                        Console.WriteLine("Login Message recived");
                        byte[] d = Encoding.ASCII.GetBytes(data);
                        XmlSerializer xmlSerializer;
                        MemoryStream memStream = null;
                        xmlSerializer = new XmlSerializer(typeof(Loginob));
                        memStream = new MemoryStream(d);
                        object objectFromXml = xmlSerializer.Deserialize(memStream);
                        //Console.WriteLine("Message recived1");
                        Loginob a = (Loginob)objectFromXml;
                        dt.Emailid = a.Emailid;
                        dt.Password = a.Password;                        
                        bool check = OTMSLogin.GetLogin(dt);
                        test = "";
                        if (check)
                        {   
                            foreach(socket i in client)
                            {
                                if(i.sock==current)
                                    i.email = a.Emailid;
                            }
                            byte[] my = ats.mydata(client, current);
                            byte[] send = ats.files(client,current);
                            byte[] dat = Encoding.ASCII.GetBytes("OK$" + Encoding.ASCII.GetString(my) +"$1" + Encoding.ASCII.GetString(send) + "$0");
                            current.Send(dat);
                            //Console.WriteLine();
                            all.toall(client, current);
                        }
                        else
                        {
                            byte[] dat = Encoding.ASCII.GetBytes("Email or Password is Incorrect$0");
                            current.Send(dat);
                        }                    
                    }
                    else if (first == "TextMessage") // Client wants to exit gracefully
                    {
                        textmessage dt = new textmessage();
                        string data = test.Substring(test.IndexOf("$") + 1, test.IndexOf("$0") - test.IndexOf("$") - 1);                        
                        byte[] d = Encoding.ASCII.GetBytes(data);
                        XmlSerializer xmlSerializer;
                        MemoryStream memStream = null;
                        xmlSerializer = new XmlSerializer(typeof(textmessage));
                        memStream = new MemoryStream(d);
                        object objectFromXml = xmlSerializer.Deserialize(memStream);
                        //Console.WriteLine("Message recived1");
                        textmessage a = (textmessage)objectFromXml;
                        dt = new textmessage(a.Message, a.Sendemail, a.Toemail);
                        Console.WriteLine("Text Message recived from" + a.Sendemail);
                        TextMessage.ProcessMessage(dt);
                        byte[] rec = Encoding.ASCII.GetBytes(first + "$" + Encoding.ASCII.GetString(d) + "$0");
                        socket j = new socket();                        
                        test = "";
                        foreach (socket i in client)
                        {
                            if (i.email == dt.Toemail)
                            {
                                j = i;
                                break;
                            }
                        }
                        j.sock.Send(rec);
                    }
                    else if (first == "MediaMessage") // Client wants to exit gracefully
                    {                        
                        string data = test.Substring(test.IndexOf("$") + 1, test.IndexOf("$0") - test.IndexOf("$") - 1);
                        byte[] d = Encoding.ASCII.GetBytes(data);
                        XmlSerializer xmlSerializer;
                        MemoryStream memStream = null;
                        xmlSerializer = new XmlSerializer(typeof(media));
                        memStream = new MemoryStream(d);
                        object objectFromXml = xmlSerializer.Deserialize(memStream);
                        //Console.WriteLine("Message recived1");
                        media a = (media)objectFromXml;                        
                        Console.WriteLine("Media Message recived from" + a.Sendemail);
                        MediaMessage.ProcessMediaMessage(a);
                        byte[] rec = Encoding.ASCII.GetBytes(first + "$" + Encoding.ASCII.GetString(d) + "$0");
                        socket j = new socket();
                        test = "";
                        foreach (socket i in client)
                        {
                            if (i.email == a.Toemail)
                            {
                                j = i;
                                break;
                            }
                        }
                        j.sock.Send(rec);
                    }
                    else
                    {
                        Console.WriteLine("Text is an invalid request");
                        byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                        current.Send(data);
                        Console.WriteLine("Warning Sent");
                    }
                }
            }
            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
    }
}