using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Media;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace ChatAppGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string textEntry = "";
        IPHostEntry ipHost = new IPHostEntry();
        IPAddress ipAddr;
        IPEndPoint localEndPoint;
        Socket sock;
        Window1 conncetDialogue = new Window1();
        BackgroundWorker backgroundWorkerRec = new BackgroundWorker();

        public static MainWindow AppWindow;

        bool setupComplete = false;
        string _hostname = "";
        int _port = 0;
        string _username = "";
        bool connected = false;
        

        public MainWindow()
        {
            InitializeComponent();

            Timer UsrlistClock = new Timer();
            UsrlistClock.Elapsed += new ElapsedEventHandler(SendListEvent);
            UsrlistClock.Interval = 15000;
            UsrlistClock.Start();


            AppWindow = this;
            backgroundWorkerRec.WorkerSupportsCancellation = true;

            if (!setupComplete)
            {
                conncetDialogue.ShowDialog();
            }

            if (setupComplete)
            {
                Connect();

                backgroundWorkerRec.DoWork += new DoWorkEventHandler(backgroundWorkerRec_DoWork);
                backgroundWorkerRec.RunWorkerAsync();
                ExecuteClient("USER" + _username);
                Window1.connectingWindow.CloseConnecting();
            }
        }

        public static void SendListEvent(object source, ElapsedEventArgs e)
        {
            if (AppWindow.connected)
            {
                try
                {
                    AppWindow.ExecuteClient("LIST");
                }
                catch (NullReferenceException err)
                {
                    Console.WriteLine("Connection Error! -- {0}", err);
                }
            }
        }

        public void SetHost(string host, string port, string username)
        {
            _hostname = host;
            _port = int.Parse(port);
            _username = username;
            setupComplete = true;
            conncetDialogue.Hide();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textEntry = EntryBox.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string dataString = "DATA" + textEntry;
            EntryBox.Clear();
            ExecuteClient(dataString);
        }

        public void Connect()
        {
            try
            {
                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                ipHost = Dns.GetHostEntry(_hostname);
                ipAddr = ipHost.AddressList[0];
                localEndPoint = new IPEndPoint(ipAddr, _port);

                try
                {
                    // Creation TCP/IP Socket using  
                    // Socket Class Costructor 
                    sock = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    sock.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sock.RemoteEndPoint.ToString());
                    connected = true;
                }
                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }


        void ExecuteClient(string entry)
        {

            try
            {

                // Creation of messagge that 
                // we will send to Server 
                byte[] messageSent = Encoding.UTF8.GetBytes(entry);
                int byteSent = sock.Send(messageSent);

                
            }

            // Manage of Socket's Exceptions 
            catch (ArgumentNullException ane)
            {

                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }

            catch (SocketException se)
            {

                Console.WriteLine("SocketException : {0}", se.ToString());
            }

            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventHandler e)
        {
            
        }

        private void backgroundWorkerRec_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<string> userList = new ObservableCollection<string>();

            ObservableCollection<string> recievedMessages = new ObservableCollection<string>();
            this.Dispatcher.Invoke(() =>
            {
                messagesWindow.ItemsSource = recievedMessages;
            });
            this.Dispatcher.Invoke(() =>
            {
                UsersList.ItemsSource = userList;
            });
            while (true)
            {
                try
                {
                    // Data buffer 
                    byte[] messageReceived = new byte[1024];

                    // We receive the messagge using  
                    // the method Receive(). This  
                    // method returns number of bytes 
                    // received, that we'll use to  
                    // convert them to string 
                    int byteRecv = sock.Receive(messageReceived);
                    if (byteRecv != 0)
                    {
                        Console.WriteLine("Message from Server -> {0}",
                              Encoding.ASCII.GetString(messageReceived,
                                                         0, byteRecv));
                        string message = Encoding.ASCII.GetString(messageReceived,
                                                         0, byteRecv);
                        string secondSubstring = message.Substring(4, 4);


                        if (message.Substring(0, 3) == "250")
                        {
                            if (secondSubstring == "LIST")
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    userList.Clear();
                                });

                                string[] split = message.Split('|');
                                for (int i = 1; i < split.Length; i++)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        userList.Add(split[i]);
                                    });

                                }
                                this.Dispatcher.Invoke(() =>
                                {
                                    messagesWindow.InvalidateArrange();
                                    messagesWindow.UpdateLayout();
                                });
                            }
                        }
                        else
                        {                            
                            this.Dispatcher.Invoke(() =>
                            {
                                recievedMessages.Add(message);
                                messagesWindow.ScrollIntoView(recievedMessages[recievedMessages.IndexOf(recievedMessages.Last())]);
                                messagesWindow.InvalidateArrange();
                                messagesWindow.UpdateLayout();
                                if (WindowState == WindowState.Minimized)
                                {
                                    SoundPlayer player = new SoundPlayer("notif.wav");
                                    player.Play();
                                }
                            });
                        }
                    }
                }
                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception er)
                {
                    Console.WriteLine("Unexpected exception : {0}", er.ToString());
                }
            }

        }

        void Disconnect()
        {
            // Close Socket using  
            // the method Close() 
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
            backgroundWorkerRec.CancelAsync();
        }
        #region Closing the Program
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
        #endregion
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ExecuteClient("LIST");
        }

    }

    public class Message
    {
        public Message(string value, Image img) { message = value; image = img; }
        public string message { get; set; }
        public Image image { get; set; }
    }
}
