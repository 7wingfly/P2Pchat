// Written by Benjamin Watkins 2015
// watkins.ben@gmail.com

using System.Windows;
using System.Windows.Input;
using System.Net;
using Shared;

namespace P2PChat
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {   
        public Client client;
        public new string Name;
        public IPEndPoint RemoteEP;
        public long ID;

        public ChatWindow(Client _client, string _Name, IPEndPoint _RemoteEP, long _ID)
        {
            InitializeComponent();

            client = _client;
            Name = _Name;
            RemoteEP = _RemoteEP;
            ID = _ID;

            Title = Name + " via " + RemoteEP;
        }

        public void ReceiveMessage(Message M)
        {
            txtConversation.Text += M.From + ": " + M.Content + '\n';
            txtConversation.CaretIndex = txtConversation.Text.Length;
            txtConversation.ScrollToEnd();
            txtMessage.Focus();            
        }

        private void SendMessage()
        {
            Message M = new Message(client.LocalClientInfo.Name, Name, txtMessage.Text);
            client.SendMessageUDP(M, RemoteEP);
            txtConversation.Text += client.LocalClientInfo.Name + ": " + txtMessage.Text + '\n';
            txtMessage.Text = string.Empty;
            txtConversation.CaretIndex = txtConversation.Text.Length;
            txtConversation.ScrollToEnd();
            txtMessage.Focus();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }
    }
}

    
