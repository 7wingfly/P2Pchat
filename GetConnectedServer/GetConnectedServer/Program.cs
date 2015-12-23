// Written by Benjamin Watkins 2015
// watkins.ben@gmail.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;

namespace GetConnectedServer
{
    class Program
    {
        static int Port = 50;

        static IPEndPoint TCPEndPoint = new IPEndPoint(IPAddress.Any, Port);
        static TcpListener TCP = new TcpListener(TCPEndPoint);
       
        static IPEndPoint UDPEndPoint = new IPEndPoint(IPAddress.Any, Port);
        static UdpClient UDP = new UdpClient(UDPEndPoint);

        static List<ClientInfo> Clients = new List<ClientInfo>();

        static void Main(string[] args)
        {
            Thread ThreadTCP = new Thread(new ThreadStart(TCPListen));
            Thread ThreadUDP = new Thread(new ThreadStart(UDPListen));

            ThreadTCP.Start();
            ThreadUDP.Start();
                       
            e:  Console.WriteLine("Type 'exit' to shutdown the server");

            if (Console.ReadLine().ToUpper() == "EXIT")
            {
                Console.WriteLine("Shutting down...");
                BroadcastTCP(new Notification(NotificationsTypes.ServerShutdown, null));              
                Environment.Exit(0);
            }
            else
            {
                goto e;
            }
        }        

        static void TCPListen()
        {
            TCP.Start();

            Console.WriteLine("TCP Listener Started");

            while (true)
            {
                try
                {
                    TcpClient NewClient = TCP.AcceptTcpClient();

                    Action<object> ProcessData = new Action<object>(delegate (object _Client)
                    {
                        TcpClient Client = (TcpClient)_Client;
                        Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);                      

                        byte[] Data = new byte[4096];
                        int BytesRead = 0;

                        while (Client.Connected)
                        {
                            try
                            {
                                BytesRead = Client.GetStream().Read(Data, 0, Data.Length);                             
                            }
                            catch
                            {
                                Disconnect(Client);
                            }

                            if (BytesRead == 0)
                                break;
                            else if (Client.Connected)
                            {
                                IP2PBase Item = Data.ToP2PBase();
                                ProcessItem(Item, ProtocolType.Tcp, null, Client);
                            }
                        }

                        Disconnect(Client);                     
                    });

                    Thread ThreadProcessData = new Thread(new ParameterizedThreadStart(ProcessData));
                    ThreadProcessData.Start(NewClient);
                }
                catch (Exception ex)
                {
                    Console.Write("TCP Error: {0}", ex.Message);
                }
            }
        }

        static void Disconnect(TcpClient Client)
        {
            ClientInfo CI = Clients.FirstOrDefault(x => x.Client == Client);

            if (CI != null)
            {
                Clients.Remove(CI);               
                Console.WriteLine("Client Disconnected {0}", Client.Client.RemoteEndPoint.ToString());
                Client.Close();

                BroadcastTCP(new Notification(NotificationsTypes.Disconnected, CI.ID));
            }
        }

        static void UDPListen()
        {
            Console.WriteLine("UDP Listener Started");

            while (true)
            {
                byte[] ReceivedBytes = null;

                try
                {
                    ReceivedBytes = UDP.Receive(ref UDPEndPoint);                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("UDP Error: {0}", ex.Message);
                }

                if (ReceivedBytes != null)
                {
                    IP2PBase Item = ReceivedBytes.ToP2PBase();
                    ProcessItem(Item, ProtocolType.Udp, UDPEndPoint);
                }
            }
        }

        static void ProcessItem(IP2PBase Item, ProtocolType Protocol, IPEndPoint EP = null, TcpClient Client = null)
        {
            if (Item.GetType() == typeof(ClientInfo))
            {
                ClientInfo CI = Clients.FirstOrDefault(x => x.ID == ((ClientInfo)Item).ID);

                if (CI == null)
                {
                    CI = (ClientInfo)Item;
                    Clients.Add(CI);

                    if (EP != null)
                        Console.WriteLine("Client Added: UDP EP: {0}:{1}, Name: {2}", EP.Address, EP.Port, CI.Name);
                    else if (Client != null)
                        Console.WriteLine("Client Added: TCP EP: {0}:{1}, Name: {2}", ((IPEndPoint)Client.Client.RemoteEndPoint).Address, ((IPEndPoint)Client.Client.RemoteEndPoint).Port, CI.Name);
                }
                else
                {
                    CI.Update((ClientInfo)Item);

                    if (EP != null)
                        Console.WriteLine("Client Updated: UDP EP: {0}:{1}, Name: {2}", EP.Address, EP.Port, CI.Name);
                    else if (Client != null)
                        Console.WriteLine("Client Updated: TCP EP: {0}:{1}, Name: {2}", ((IPEndPoint)Client.Client.RemoteEndPoint).Address, ((IPEndPoint)Client.Client.RemoteEndPoint).Port, CI.Name);
                }

                if (EP != null)
                    CI.ExternalEndpoint = EP;

                if (Client != null)
                    CI.Client = Client;

                BroadcastTCP(CI);

                if (!CI.Initialized)
                {
                    if (CI.ExternalEndpoint != null & Protocol == ProtocolType.Udp)
                        SendUDP(new Message("Server", CI.Name, "UDP Communication Test"), CI.ExternalEndpoint);

                    if (CI.Client != null & Protocol == ProtocolType.Tcp)
                        SendTCP(new Message("Server", CI.Name, "TCP Communication Test"), CI.Client);

                    if (CI.Client != null & CI.ExternalEndpoint != null)
                    {
                        foreach (ClientInfo ci in Clients)                                          
                            SendUDP(ci, CI.ExternalEndpoint);                       

                        CI.Initialized = true;
                    }
                }
            }
            else if (Item.GetType() == typeof(Message))
            {
                Console.WriteLine("Message from {0}:{1}: {2}", UDPEndPoint.Address, UDPEndPoint.Port, ((Message)Item).Content);
            }           
            else if (Item.GetType() == typeof(Req))
            {
                Req R = (Req)Item;

                ClientInfo CI = Clients.FirstOrDefault(x => x.ID == R.RecipientID);

                if (CI != null)
                    SendTCP(R, CI.Client);
            }            
        }

        static void SendTCP(IP2PBase Item, TcpClient Client)
        {
            if (Client != null && Client.Connected)
            {
                byte[] Data = Item.ToByteArray();

                NetworkStream NetStream = Client.GetStream();
                NetStream.Write(Data, 0, Data.Length);            
            }
        }

        static void SendUDP(IP2PBase Item, IPEndPoint EP)
        {
            byte[] Bytes = Item.ToByteArray();
            UDP.Send(Bytes, Bytes.Length, UDPEndPoint);
        }

        static void BroadcastTCP(IP2PBase Item)
        {
            foreach (ClientInfo CI in Clients.Where(x => x.Client != null))
                SendTCP(Item, CI.Client);
        }

        static void BroadcastUDP(IP2PBase Item)
        {
            foreach (ClientInfo CI in Clients)
                SendUDP(Item, CI.ExternalEndpoint);
        }       
    }
}

    
