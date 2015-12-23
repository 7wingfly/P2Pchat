// Written by Benjamin Watkins 2015
// watkins.ben@gmail.com

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Shared
{
    public enum ConnectionTypes { Unknown, LAN, WAN }

    [Serializable]
    public class ClientInfo : IP2PBase
    {
        public string Name { get; set; }
        public long ID { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public IPEndPoint InternalEndpoint { get; set; }
        public ConnectionTypes ConnectionType { get; set; }
        public bool UPnPEnabled { get; set; }
        public List<IPAddress> InternalAddresses = new List<IPAddress>();        

        [NonSerialized] //server use only
        public TcpClient Client;

        [NonSerialized] //server use only
        public bool Initialized;

        public bool Update(ClientInfo CI)
        {
            if (ID == CI.ID)
            {
                foreach (PropertyInfo P in CI.GetType().GetProperties())
                    if (P.GetValue(CI) != null)
                        P.SetValue(this, P.GetValue(CI));

                if (CI.InternalAddresses.Count > 0)
                {
                    InternalAddresses.Clear();
                    InternalAddresses.AddRange(CI.InternalAddresses);
                }
            }

            return (ID == CI.ID);
        }

        public override string ToString()
        {
            if (ExternalEndpoint != null)
                return Name + " (" + ExternalEndpoint.Address + ")";
            else
                return Name + " (UDP Endpoint Unknown)";
        }

        public ClientInfo Simplified()
        {
            return new ClientInfo()
            {
                Name = this.Name,
                ID = this.ID,
                InternalEndpoint = this.InternalEndpoint,
                ExternalEndpoint = this.ExternalEndpoint                                
            };
        }
    }    
}
