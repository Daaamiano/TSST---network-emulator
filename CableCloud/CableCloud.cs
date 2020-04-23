﻿using DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CableCloud
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int bufferSize = 1024;
        public byte[] buffer = new byte[bufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class Tunnel
    {

        public string incomingObject;
        public string destinationObject;
        public int destinationPort;
        public Tunnel(string incomingObject, int destinationPort, string destinationObject)
        {
            this.incomingObject = incomingObject;
            this.destinationObject = destinationObject;
            this.destinationPort = destinationPort;
        }
    }

    class CableCloud
    {

        private Dictionary<string, Socket> connectedSockets = new Dictionary<string, Socket>();
        private static ManualResetEvent done = new ManualResetEvent(false);
        Dictionary<int, Tunnel> tunnels = new Dictionary<int, Tunnel>();

        public CableCloud(string configFilePath)
        {
            Console.Title = "Cable Cloud";
            LoadTunnels(configFilePath);
            Start(5001);
        }

        public void Start(int myPort)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = IPAddress.Parse("127.0.0.1");

            IPEndPoint localEndPoint = new IPEndPoint(address, myPort);

            Socket cloudSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                cloudSocket.Bind(localEndPoint);
                cloudSocket.Listen(100);

                while (true)
                {
                    done.Reset();
                    //Logs.ShowLog(LogType.INFO, "Waiting for a incomming connection...");
                    cloudSocket.BeginAccept(new AsyncCallback(AcceptCallback), cloudSocket);
                    done.WaitOne();
                }

            }
            catch (Exception e)
            {
                Logs.ShowLog(LogType.ERROR, e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            done.Set();

            Socket cloudSocketListener = (Socket)ar.AsyncState;
            Socket cloudSocketHandler = cloudSocketListener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = cloudSocketHandler;
            cloudSocketHandler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = null;
            handler = state.workSocket;
            Package package = new Package();

            try
            {
                int read = handler.EndReceive(ar);
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
                var content = state.sb.ToString();                
               // Logs.ShowLog(LogType.INFO, ("Read {" + content.Length.ToString() + "} bytes from socket. \n Data : " + content));
                package = DeserializeFromJson(content);               

                if (package.message == "CONNECTED")
                {                    
                    Logs.ShowLog(LogType.CONNECTED, $"Connection with {package.sourceName} established.");
                    try
                    {
                        connectedSockets.Add(package.sourceName, handler);
                        //Console.WriteLine("dodano do tablicy socket: " + package.sourceName);                        
                    }
                    catch
                    {
                        Logs.ShowLog(LogType.CONNECTED, $"Router {package.sourceName} reconnected.");
                    }
                    Send(handler, content);
                    //handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    Logs.ShowLog(LogType.INFO, ("Read {" + content.Length.ToString() + "} bytes from " + tunnels[package.incomingPort].incomingObject));
                    package.incomingPort = tunnels[package.incomingPort].destinationPort;                    
                    content = SerializeToJson(package);                    
                    //Console.WriteLine("wysylam na router::   " + tunnels[package.incomingPort].incomingObject);
                    Send(connectedSockets[tunnels[package.incomingPort].incomingObject], content); // to jak przesylanie wiadomosci na nowy port
                    Logs.ShowLog(LogType.INFO, "Sent {" + content.Length.ToString() + "} bytes to " +  tunnels[package.incomingPort].incomingObject);
                }
                state.sb.Clear();
                handler.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, new AsyncCallback(ReadCallback), state);               
            }
            catch (Exception e)
            {
                
                Logs.ShowLog(LogType.ERROR, "Connection with router lost.");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

        private void LoadTunnels(string configFilePath)
        {
            foreach (var row in File.ReadAllLines(configFilePath))
            {

                var splitRow = row.Split(", ");
                if (splitRow[0] == "#X")
                {
                    continue;
                }                
                tunnels.Add(int.Parse(splitRow[0]), new Tunnel(splitRow[1], int.Parse(splitRow[2]), splitRow[3]));
            }
        }

        private void Send(Socket handler, string content)
        {
            byte[] data = Encoding.ASCII.GetBytes(content);
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int sent = handler.EndSend(ar);
                //Logs.ShowLog(LogType.INFO, "Sent {" + sent.ToString() + "} bytes to client");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public string SerializeToJson(Package package)
        {
            string jsonString;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            jsonString = JsonSerializer.Serialize(package, options);

            return jsonString;
        }

        public Package DeserializeFromJson(string serializedString)
        {
            Package package = new Package();
            package = JsonSerializer.Deserialize<Package>(serializedString);
            return package;
        }
    }
}
