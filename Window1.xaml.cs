using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatAppGUI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        string host = "";
        string port = "";
        string username = "";
        MainWindow m = new MainWindow();
        public Window1()
        {
            InitializeComponent();
        }

        private void AddressBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            host = AddressBox.Text;
        }

        private void PortBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            port = PortBox.Text;
        }

        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            username = UsernameBox.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
