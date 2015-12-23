// Written by Benjamin Watkins 2015
// watkins.ben@gmail.com

using System;

namespace Shared
{
    public enum NotificationsTypes { ServerShutdown, Disconnected }

    [Serializable]
    public class Notification : IP2PBase
    {
        public long ID { get; set; }

        public NotificationsTypes Type { get; set; }
        public object Tag { get; set; }

        public Notification(NotificationsTypes _Type, object _Tag)
        {
            Type = _Type;
            Tag = _Tag;
        }
    }

    [Serializable]
    public class Message : IP2PBase
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public long ID { get; set; }
        public long RecipientID { get; set; }        

        public Message(string from, string to, string content)
        {
            From = from;
            To = to;
            Content = content;
        }
    }

    [Serializable]
    public class Req : IP2PBase
    {
        public long ID { get; set; }
        public long RecipientID { get; set; }       

        public Req(long Sender_ID, long Recipient_ID)
        {
            ID = Sender_ID;
            RecipientID = Recipient_ID;
        }
    }  

    [Serializable]
    public class Ack : IP2PBase
    {
        public long ID { get; set; }
        public long RecipientID { get; set; }
        public bool Responce { get; set; }

        public Ack(long Sender_ID)
        {
            ID = Sender_ID;
        }
    }

    [Serializable]
    public class KeepAlive : IP2PBase
    {
        public long ID { get; set; }
    }
}
