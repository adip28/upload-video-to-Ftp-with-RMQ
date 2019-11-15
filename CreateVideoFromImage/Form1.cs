//using AForge.Video.FFMPEG;
using AForge.Video.FFMPEG;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using WMPLib;

namespace CreateVideoFromImage
{
    public partial class Form1 : Form
    {
        /*const string Path = @"G:\BasukiRachmat\";
        const string PathDuration = @"G:\BasukiRachmat\abodulmuluk.mp4";
        const string OutputVideoPath = @"G:\BasukiRachmat\output\";
        const string VideoPath = @"G:\kuliah\project\PPTIK ITB\ATCS\video_cctv\abodulmuluk.mp4";
        const string paths = @"G:\kuliah\project\PPTIK ITB\ATCS\video_cctv\abodulmuluk.mp4";*/

        const string BasePath = "D:/ATCS/";
        //const string BaseInputVideoPath = @"G:\";
        const string BaseOutputVideoPath = @"D:\ATCS\VideoOutput\";
        private int _tick = 0;
        public Form1()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] lines = System.IO.File.ReadAllLines(@"config2.txt");
            foreach (string line in lines)
            {
                string[] words = line.Split('|');
                string directory = words[0];
                string filename = words[1];
                string filefullpath = words[2];
                string pattern = "*.mp4";
                string InputVideoPath = filefullpath;

                if (Directory.Exists(InputVideoPath))
                {

                    var dirInfo = new DirectoryInfo(InputVideoPath);
                    if (dirInfo.GetFiles().Length != 0)
                    {
                        var file = (from f in dirInfo.GetFiles(pattern) orderby f.LastWriteTime descending select f).First();
                        ConvertVideoToImg(file.FullName, directory, filename);
                        Console.WriteLine(file.FullName);
                    }
                    else
                    {
                        Console.WriteLine("Tidak ada File " + InputVideoPath);
                    }
                }
               
            }

        }

       

        private void timer1_Tick(object sender, EventArgs e)
        {
            _tick++;
            label1.Text = _tick.ToString();
            if (_tick == 1) {
                string[] lines = System.IO.File.ReadAllLines(@"config2.txt");
                foreach (string line in lines)
                {
                    string[] words = line.Split('|');
                    string directory = words[0];
                    string filename = words[1];
                    string filefullpath = words[2];
                    string pattern = "*.mp4";
                    string InputVideoPath = filefullpath;
                    /*if (!Directory.Exists(InputVideoPath))
                        Directory.CreateDirectory(InputVideoPath);*/
                    var dirInfo = new DirectoryInfo(InputVideoPath);
                    if (dirInfo.GetFiles().Length != 0)
                    {
                        var file = (from f in dirInfo.GetFiles(pattern) orderby f.LastWriteTime descending select f).First();
                        try
                        {
                            ConvertVideoToImg(file.FullName, directory, filename);
                        }
                        catch {
                            Console.WriteLine("Gagal Convert Video" + filename);
                        }
                        
                        Console.WriteLine(file.FullName);
                    }
                    else
                    {
                        Console.WriteLine("Tidak ada File " + InputVideoPath);
                    }

                }
                _tick = 0;
            }
            
        }

        public void ConvertVideoToImg(string VideoFile, string directory,string filename)
        {
            String BaseImgPath = BasePath + "img/"+filename+"/";
            if (!Directory.Exists(BaseImgPath))
                Directory.CreateDirectory(BaseImgPath);
            //Process.Start("ffmpeg.exe", "-ss 00:06:40 -i "+p+" -t 00:06:50 -c copy out7.avi");
            //Process.Start("ffmpeg.exe", "-sseof -10 -i out6.avi -c copy output.avi");

            //Process.Start("ffmpeg.exe", "-i "+p+" -vf fps=1 ./img/%d.png");
            //var inputArgs = "-i " + VideoFile + " -vf fps=8 -s 640x360 " + BaseImgPath +"%d.jpeg";
            var inputArgs = "-sseof -10 -i " + VideoFile + " -s 640x360 " + BaseImgPath + "%d.jpeg";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"{inputArgs}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };
            process.Start();

            var ffmpegIn = process.StandardInput.BaseStream;
            ffmpegIn.Flush();
            ffmpegIn.Close();
            process.WaitForExit();
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(BaseImgPath);
            int count = dir.GetFiles().Length;
            if (count > 0)
            {
                try
                {
                    CreateVideo(BaseImgPath, directory, filename, count);

                }
                catch 
                {
                }
            }


        }

        public void CreateVideo(string BaseImgPath, string directory,string filename, int fileCount)
        {

            var imageCount = fileCount;
            string outputVideoPath = BaseOutputVideoPath + directory + "/" ;
            if (!Directory.Exists(outputVideoPath))
                Directory.CreateDirectory(outputVideoPath);
            using (var videoWriter = new VideoFileWriter())
            {
                videoWriter.Open(outputVideoPath + filename+".mp4", 640, 360, 25, VideoCodec.MPEG4,100000);
                for (int imageFrame = 1; imageFrame < imageCount; imageFrame++)
                {
                    var imgPath = string.Format("{0}{1}.jpeg", BaseImgPath, imageFrame);
                    using (Bitmap image = Bitmap.FromFile(imgPath) as Bitmap)
                    {
                        videoWriter.WriteVideoFrame(image);
                    }
                }
                videoWriter.Close();
                string[] filePaths = Directory.GetFiles(BaseImgPath);
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
                Console.WriteLine("Success Create Videos");

            }
        }

    }
}
