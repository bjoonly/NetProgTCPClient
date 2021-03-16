using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Client
{

    public partial class MainWindow : Window
    {
        DispatcherTimer timer = null;
        int interval = 15;
        TcpClient tcpClient = null;
        IPEndPoint iPAddress =new IPEndPoint(IPAddress.Parse("127.0.0.1"),7777);

        int i = 15;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0,0,0,0,1000);
         
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            i--;
            if (i == 0)
            {
                Task.Run(() => GetScreen());
                i = interval;
            }
            
            
        }
        private void GetScreen()
        {
           
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(iPAddress);


                    XmlSerializer serializer = new XmlSerializer(typeof(FileTransferInfo));
                    var info = (FileTransferInfo)serializer.Deserialize(tcpClient.GetStream());
                    using (FileStream fs = new FileStream(info.Name, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(info.Data, 0, info.Data.Length);
                    }
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {

                        listBox.Items.Add(info);

                    }));
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                
                tcpClient.Close();
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (int.TryParse(item.Tag.ToString(), out int sec))
            {
              
                interval =  sec;
                i = sec;
       
            }          
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => GetScreen()).ContinueWith((result)=>timer.Start());
         
            
           
        }
    }
    [Serializable] 
    public class FileTransferInfo
    {
     
        public string Name { get; set; }
      
        public byte[] Data { get; set; }
    }
}
