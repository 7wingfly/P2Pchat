// Written by Benjamin Watkins 2015
// watkins.ben@gmail.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using Shared;

namespace P2PChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client client = new Client();

        List<ChatWindow> ChatWindows = new List<ChatWindow>();

        public MainWindow()
        {
            InitializeComponent();

            client.OnServerConnect += Client_OnServerConnect;
            client.OnServerDisconnect += Client_OnServerDisconnect;
            client.OnResultsUpdate += Client_OnResultsUpdate;
            client.OnClientAdded += Client_OnClientAdded;
            client.OnClientUpdated += Client_OnClientUpdated;
            client.OnClientRemoved += Client_OnClientRemoved;
            client.OnClientConnection += Client_OnClientConnection;
            client.OnMessageReceived += Client_OnMessageReceived;  
        }        

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            client.ConnectOrDisconnect();
        }        

        private void Client_OnServerConnect(object sender, EventArgs e)
        {
            btnConnect.Content = "Disconnect";
            chkUPnP.IsEnabled = false;
        }

        private void Client_OnServerDisconnect(object sender, EventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                btnConnect.Content = "Connect";
                lstClients.Items.Clear();
                chkUPnP.IsEnabled = true;

                for (int c = 0; c < ChatWindows.Count - 1; c++)
                    ChatWindows[c].Close();
            });
        }

        private void Client_OnResultsUpdate(object sender, string e)
        {
            try
            {
                Dispatcher.Invoke(delegate
                {
                    txtResults.Text += e + '\n';
                    txtResults.CaretIndex = txtResults.Text.Length;
                    txtResults.ScrollToEnd();
                });
            }        
            catch
            {

            }
        }

        private void Client_OnClientAdded(object sender, ClientInfo e)
        {
            Dispatcher.Invoke(delegate
            {
                lstClients.Items.Add(e);
            });
        }        

        private void Client_OnClientUpdated(object sender, ClientInfo e)
        {          
            Dispatcher.Invoke(delegate
            {
                foreach (ClientInfo CI in lstClients.Items)
                    if (CI.ID == e.ID)
                        CI.Update(e);

                RefreshDetails();
            });
        }

        private void Client_OnClientRemoved(object sender, ClientInfo e)
        {
            int i = -1;
            ChatWindow Chat = null;

            foreach (ClientInfo CI in lstClients.Items)
                if (CI.ID == e.ID)
                    i = lstClients.Items.IndexOf(CI);

            foreach (ChatWindow CW in ChatWindows)
                if (CW.ID == e.ID)
                    Chat = CW;

            Dispatcher.Invoke(delegate
            {
                if (i != -1)
                    lstClients.Items.RemoveAt(i);

                if (Chat != null)
                    Chat.Close();

                RefreshDetails();
            });
        }

        private void Client_OnClientConnection(object sender, IPEndPoint e)
        {
            Dispatcher.Invoke(delegate 
            { 
                ChatWindow chat = ChatWindows.FirstOrDefault(C => C.RemoteEP.Equals(e));

                if (chat == null)
                {
                    chat = new ChatWindow(client, ((ClientInfo)sender).Name, e, ((ClientInfo)sender).ID);
                    ChatWindows.Add(chat);                  
                    chat.Closed += delegate { ChatWindows.Remove(chat); };
                    chat.Show();
                }
                else
                {
                    chat.Focus();
                    chat.BringIntoView();
                }

                chat.txtMessage.Focus();
            });
        }

        private void Client_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                ChatWindow chat = ChatWindows.FirstOrDefault(C => C.RemoteEP.Equals((IPEndPoint)sender));

                if (chat == null)
                {
                    chat = new ChatWindow(client, e.clientInfo.Name, e.EstablishedEP, e.clientInfo.ID);
                    ChatWindows.Add(chat);
                    chat.Closed += delegate { ChatWindows.Remove(chat); };
                    chat.Show();
                }
                else
                {
                    chat.Focus();
                    chat.BringIntoView();
                }

                chat.ReceiveMessage(e.message);
            });       
        }

        private void btnConnectClient_Click(object sender, RoutedEventArgs e)
        {
            if (lstClients.SelectedItem != null)
            {
                ClientInfo CI = (ClientInfo)lstClients.SelectedItem;
                client.ConnectToClient(CI);
            }
        }   

        private void lstClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDetails();
        }

        public void RefreshDetails()
        {
            if (lstClients.SelectedItem != null)
            {
                ClientInfo CI = (ClientInfo)lstClients.SelectedItem;

                lblName.Content = "Name: " + CI.Name;
                lblUPnP.Content = "UPnP Enabled: " + CI.UPnPEnabled;
                lblExtEP.Content = "Ext EP: " + (CI.ExternalEndpoint != null ? CI.ExternalEndpoint.ToString() : "None");
                lblIntEP.Content = "Int EP: " + (CI.InternalEndpoint != null ? CI.InternalEndpoint.ToString() : "None");
                lblConType.Content = "Method: " + CI.ConnectionType.ToString();

                lblIPs.Content = "Int IPs: ";

                foreach (IPAddress IP in CI.InternalAddresses)
                    lblIPs.Content += IP + ", ";

                btnConnectClient.IsEnabled = (CI.ID != client.LocalClientInfo.ID);
            }
            else
            {
                lblName.Content = "Name: ";
                lblUPnP.Content = "UPnP Enabled: ";
                lblExtEP.Content = "Ext EP: ";
                lblIntEP.Content = "Int EP: ";
                lblConType.Content = "Type: ";
                lblIPs.Content = "Int IPs: ";
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtResults.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.ClearUpUPnP();

            for (int c = 0; c < ChatWindows.Count; c++)
                ChatWindows[c].Close();
        }

        private void chkUPnP_Checked(object sender, RoutedEventArgs e)
        {
            client.UPnPEnabled = (bool)chkUPnP.IsChecked;
        }
    }
}
