using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Xml.Serialization;

namespace Server
{
    [Serializable] 
    public class FileTransferInfo
    {     
        public string Name { get; set; }     
        public byte[] Data { get; set; }
    }
    class Program
    {
        static int count = 1;
        static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        static int port = 7777;
        static void Main(string[] args)
        {
         
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, port);           
            TcpListener server = new TcpListener(localEndPoint);                              
            try
            {
                server.Start(10);

                Console.WriteLine("Server started! Waiting for connection...");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connect from "+client.Client.RemoteEndPoint+" at " +DateTime.Now);
                    Task.Run(() => Work(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                server.Stop();
            }        
        }
        static void Work(TcpClient client) 
        {
            try
            {
                Console.WriteLine("Sending...");
                string path = $"Screenshot{count}.png";
                Graphics graph = null;

                var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
                graph = Graphics.FromImage(bmp);
                graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                bmp.Save(path);
                FileTransferInfo info = new FileTransferInfo();
                info.Name = Path.GetFileName(path);
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {

                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, fileData.Length);
                    info.Data = fileData;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(FileTransferInfo));
                using (NetworkStream stream = client.GetStream())
                {
                    serializer.Serialize(stream, info);
                }
                Console.WriteLine("Screenshot send at "+DateTime.Now+"\n"+new string('-',100)+"\n\n");
                File.Delete(path);
                count++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
              
                client.Close();
            }
        }
        
    }
}
