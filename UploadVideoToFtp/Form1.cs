using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadVideoToFtp
{
    public partial class Form1 : Form
    {
        const string BaseOutputVideoPath = @"D:\ATCS\VideoOutput\";
        private int _tick = 0;
        public Form1()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _tick++;
            this.Text = _tick.ToString();
            if (_tick == 1)
            {
                string[] lines = System.IO.File.ReadAllLines(@"config2.txt");

                foreach (string line in lines)
                {
                    string[] words = line.Split('|');
                    string directory = words[0];
                    string filename = words[1];
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(BaseOutputVideoPath + directory + "/");
                    if (dir.Exists)
                    {
                        int count = dir.GetFiles().Length;

                        if (count > 0)
                        {
                            try
                            {
                                uploadtoftp(directory, filename);
                                Console.WriteLine("Berhasil Upload file" + filename);
                                label1.Text = "Berhasil Upload file " + filename;
                            }
                            catch
                            {
                                label1.Text = "Gagal Upload file " + filename;
                                Console.WriteLine("GAGAL UPLOAD FILE" + filename);
                            }
                            try
                            {
                                publishRmq(directory, filename);
                                label2.Text = "Success Publis Rmq " + filename;
                            }
                            catch
                            {
                                label2.Text = "Gagal Publis Rmq " + filename;
                            }

                        }
                                                   
                    }
                    else {
                        label1.Text = "CCTV Sedang dalam perbaikan" + filename;
                    }

                }
                _tick = 0;
            }

        }

        public void uploadtoftp(String directory, String filename)
        {

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://atcs-lampung.pptik.id/atcs%20lampung/videos/" + directory + "/" + filename + ".mp4");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = true;
            request.Credentials = new NetworkCredential("atcs_lampung", "atcslampung123!");

            try
            {

                using (FileStream fs = File.OpenRead(BaseOutputVideoPath + directory + "/" + filename + ".mp4"))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(buffer, 0, buffer.Length);
                    requestStream.Flush();
                    requestStream.Close();
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                    response.Close();
                    //Console.WriteLine("Success Upload Image to FTP");
                }
            }
            catch {

            }
            

           

        }

        private static void publishRmq(string path, string name)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "atcs_lampung";
            factory.Password = "atcslampung123!";
            factory.VirtualHost = "/atcs_lampung";
            factory.HostName = "rmq2.pptik.id";


            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {


                dynamic msg = new JObject();
                msg.path = path;
                msg.filename = name;

                String jsonified = JsonConvert.SerializeObject(msg);
                byte[] body = Encoding.UTF8.GetBytes(jsonified);

                channel.BasicPublish(exchange: "amq.topic",
                                     routingKey: "atcs_video",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", msg);
                connection.Close();
                channel.Close();

            }
        }
    }
}
