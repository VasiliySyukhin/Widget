using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.IO;
using Newtonsoft.Json;


namespace WidgetWindows
{
    
    public partial class Form1 : Form
    {
        
        private string tmpInfo = string.Empty;
        public string filename;
        private bool isFileChanged;

        private void GetCPUTemperature()
        {
            tmpInfo = string.Empty;

            Visitor visitor = new Visitor();

            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.Accept(visitor);

            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            tmpInfo += computer.Hardware[i].Sensors[j].Name + ": " +
                                computer.Hardware[i].Sensors[j].Value.ToString() + "\n";


                        }
                    }
                }
            }
            CPUTemperatur.Text = tmpInfo;

            computer.Close();
        }
        public Form1()
        {
            InitializeComponent();

            init();
            Weather();
            

            string lineBts = "";
            using (WebClient ws = new WebClient())

                lineBts = ws.DownloadString("https://blockchain.info/ticker");

            Match match2 = Regex.Match(lineBts, "\"RUB\":.{\n.*?\"15m\":.(\\d+.\\d+)");
            Bts.Text = match2.Groups[1].Value;

            this.MouseDown += new MouseEventHandler((o, e) =>
            {
                base.Capture = false;
                Message message = Message.Create(base.Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
                this.WndProc(ref message);
            });
            this.ShowInTaskbar = false;
        }

        public void init()
        {
            filename = "";
            isFileChanged = false;
        }
        


        private void Close_Click(object sender, EventArgs e)
        {
            SaveUnsavedFile();

            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                GetCPUTemperature();

                Thread.Sleep(3000);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string line = "";
            byte[] reply;

            

            using (WebClient webClient = new WebClient())
            {
                reply = webClient.DownloadData("https://www.cbr-xml-daily.ru/daily.xml");

                var convertedBytes = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, reply);

                line = Encoding.UTF8.GetString(convertedBytes);
            }

            Match match = Regex.Match(line, "<Name>Доллар США</Name><Value>(.*?)</Value>");
            Match match1 = Regex.Match(line, "<Name>Евро</Name><Value>(.*?)</Value>");

            lbl_Usd.Text = match.Groups[1].Value;
            lbl_Eur.Text = match1.Groups[1].Value;
        }
        

        private void Open_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            SaveUnsavedFile();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamReader sr = new StreamReader(openFileDialog1.FileName);
                    textBox1.Text = sr.ReadToEnd();
                    sr.Close();
                    filename = openFileDialog1.FileName;
                    isFileChanged = false;
                }
                catch 
                {
                    MessageBox.Show("Невозможно открыть файл");
                }
            }
        }
        public void SaveFile(string _filename)
        {
            
            if (_filename == "")
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    _filename = saveFileDialog1.FileName;
                }
            }
            try
            {
                StreamWriter sw = new StreamWriter(_filename + ".txt"); 
                sw.Write(textBox1.Text);
                sw.Close();
                filename = _filename;
                isFileChanged = false;
            }
            catch
            {
                MessageBox.Show("Невозможно сохранить файл!");
            }
        }
        private void Save_Click(object sender, EventArgs e)
        {
            SaveFile(filename);
        }

        private void NewDocument_Click(object sender, EventArgs e)
        {
            
            SaveUnsavedFile();
            
            textBox1.Text = "";
            filename = "";
            isFileChanged = false;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            this.Text = this.Text.Replace('*', ' ');
            if (!isFileChanged)
            {
                isFileChanged = true;
                this.Text = "+" + this.Text;
            }
        }
        public void SaveUnsavedFile()
        {
            if(isFileChanged)
            {
                DialogResult result = MessageBox.Show("Сохранить изменения в файле?", "Сохранение файла", MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes)
                {
                    SaveFile(filename);
                }
            }
        }
        public void Weather()
        {
            string url = "https://api.openweathermap.org/data/2.5/weather?q=Arkhangelsk&units=metric&appid=1aaa8d2dd9b56b349811d4124737d90a";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

            Match match3 = Regex.Match(response, "\"temp\":(.*?),\"feels_like\"");

            label4.Text = match3.Groups[1].Value + "°C";

        }
        

        private void timer2_Tick(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            Time.Text = dt.ToLongTimeString();
            Date.Text = dt.ToShortDateString();
        }
    }
}