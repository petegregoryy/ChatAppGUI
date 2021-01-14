using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
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

        BackgroundWorker backgroundWorkerRec = new BackgroundWorker();

       

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorkerRec.WorkerSupportsCancellation = true;
            try
            {
                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                ipHost = Dns.GetHostEntry("192.168.1.155");
                ipAddr = ipHost.AddressList[0];
                localEndPoint = new IPEndPoint(ipAddr, 65432);

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

            backgroundWorkerRec.DoWork += new DoWorkEventHandler(backgroundWorkerRec_DoWork);
            backgroundWorkerRec.RunWorkerAsync();
            ExecuteClient("USERcsharpClient");
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

            ObservableCollection<string> recievedMessages = new ObservableCollection<string>();
            this.Dispatcher.Invoke(() =>
            {
                messagesWindow.ItemsSource = recievedMessages;
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
                        this.Dispatcher.Invoke(() =>
                        {
                            recievedMessages.Add(message);
                            messagesWindow.ScrollIntoView(recievedMessages[recievedMessages.IndexOf(recievedMessages.Last())]);
                            messagesWindow.InvalidateArrange();
                            messagesWindow.UpdateLayout();
                        });

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

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }
    }
}
