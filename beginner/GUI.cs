using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;

using System.Net;
using System.Net.Sockets;
using System.IO;

namespace cam_aforge1
{
    public partial class GUI : Form
    {
        private bool DeviceExist = false;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource = null;
        public static Bitmap _latestFrame;
        GUIElements myCanvas;
        Bitmap img;

        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string recieve;
        public String TextToSend;

        int tickCount = 0;
        int volumeLevel = 0;
        int stepcount = 0;
        int arraysize = 8;
        int weightselect;
        int[] weightarry = { 1, 4, 9 };
        int zoomvid;
        int x;
        bool clearbut = false;
        int y;
        int Rotatevid;
        int shapeselect = 0;

            Socket s = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream,
                                ProtocolType.Tcp);
            Socket sListen = new Socket(AddressFamily.InterNetwork,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);
    


        int[] sizearry = { 50,100, 200 };
        int SizeSelect;
        Graphics g;
        Bitmap img3;
        Pen myPen = new Pen(Color.Red);
        Point p = new Point();
        bool flag = false;
        Color ColourSelect;
        Color[] colourarry = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple, Color.Black, Color.White };
     
        string[] step = {"Loparoscopic excision of appendix ",
                            "Step1:",
                            "Step2:",
                            "Step3:", 
                            "Step4:", 
                            "Step5:", 
                            "Step6:",
                            "finish"};
        string[] step1 = {"",
                            "placement of trocars",
                            "visualization of mesoappendix and appendiceal base",
                            "division of mesoappendix and excision of appendix", 
                            "Irrigation and suction", 
                            "removal of ports and retrieval of speciment", 
                            "closure",
                            ""};
        //Constructs the gui
        public GUI()
        {
            myCanvas = new GUIElements(this);
            InitializeComponent();
            //Add custom events here, ie
            //viewFinder.MouseDown += new MouseEventHandler(viewFinder_MouseDown);

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress address in localIP)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIPtextBox.Text = address.ToString();
                }
            }
        }

        //Generally don't have to change this
        private void getCamList()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                vidSrc.Items.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    vidSrc.Items.Add(device.Name);
                }
                vidSrc.SelectedIndex = 0; //make dafault to first cam
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                vidSrc.Items.Add("No capture device on your system");
            }
        }

        //Generally don't have to change this
        private void rfsh_Click(object sender, EventArgs e)
        {
            getCamList();
        }

        //Generally don't have to change this
        private void start_Click(object sender, EventArgs e)
        {
            if (start.Text == "&Start")
            {
                if (DeviceExist)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[vidSrc.SelectedIndex].MonikerString);
                    videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                    CloseVideoSource();
                    videoSource.DesiredFrameSize = new Size(160, 120);
                    //videoSource.DesiredFrameRate = 10;
                    videoSource.Start();
                    label2.Text = "Device running...";
                    start.Text = "&Stop";
                    timer1.Enabled = true;
                    
                }
                else
                {
                    label2.Text = "Error: No Device selected.";
                }
            }
            else
            {
                if (videoSource.IsRunning)
                {
                    timer1.Enabled = false;
                    CloseVideoSource();
                    label2.Text = "Device stopped.";
                    start.Text = "&Start";                    
                }
            }
        }

        //Generally don't have to change this
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            img = (Bitmap)eventArgs.Frame.Clone();
            //Bitmap image = (Bitmap)img;
            
            Bitmap img2;
            RotateBilinear ro = new RotateBilinear(Rotatevid, true);
            img2 = ro.Apply(img);
            if (zoomvid > 0)
            {
                img3 = zoom(img2, new Size(zoomvid, zoomvid));
            }
            else
                img3 = img2;
            myCanvas.g = Graphics.FromImage(img3);
            
                myCanvas.Run(x, y, shapeselect, ColourSelect, SizeSelect, weightselect,clearbut);
            
            myCanvas.g.Dispose();


            viewFinder.Image = img3;
            //viewFinder.Image = img;
            
            
           
        }

        //Generally don't have to change this
        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }
        }

        //Generally don't have to change this
        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = "Device running... " + videoSource.FramesReceived.ToString() + " FPS";
        }

        //Generally don't have to change this
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseVideoSource();
        }
        
        //Button for changing camera control
        private void ctrl_Click(object sender, EventArgs e)
        {
            CamControl.show_Controls();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Step 7: Let's activate this button. Uncomment lines 143-144 to add functionality
            //to the button.
            tickCount++;
            countDisp.Text = tickCount.ToString();

            //Step 8: The button_Click method can be used to call a method that you wrote
            //on the GUIElements Class. Uncomment line 149 to call the ButtonWasClicked method
            //from GUIElements.
            myCanvas.ButtonWasClicked();

            //Note that the syntax for calling methods in the GUIElements class is always in the form of
            //`myCanvas.NameOfMethod();`.
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.button6.Text == "Start Video")
            {
                //axWindowsMediaPlayer1.URL = textBox3.Text;
              //  axWindowsMediaPlayer1.Ctlcontrols.play();
                this.button6.Text = "Stop Video";
            }

            else if (this.button6.Text == "Stop Video")
            {
            //    axWindowsMediaPlayer1.URL = textBox3.Text;
             //   axWindowsMediaPlayer1.Ctlcontrols.stop();
                this.button6.Text = "Start Video";
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start Timer")
            {
                this.button1.Text = "Stop Timer";

            }
            else if (button1.Text == "Stop Timer")
            {
                this.button1.Text = "Restart Timer";

            }
            else
            {
                this.button1.Text = "Start Timer";

            }

            myCanvas.ButtonWasClicked1();
        }


        private void button4_Click(object sender, EventArgs e)
        {

            stepcount++;
            if (stepcount == arraysize)
            { stepcount = 0; }
            this.textBox1.Text = step[stepcount];
            this.textBox2.Text = step1[stepcount];
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                textBox3.Text = openFileDialog1.FileName;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "BloodOn")
            {
                this.button2.Text = "BloodOff";

            }
            else if (button2.Text == "BloodOff")
            {
                this.button2.Text = "BloodOn";

            }
            myCanvas.ButtonWasClicked2();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (volumeLevel < 100)
            {
                volumeLevel = volumeLevel + 10;
                this.button8.Text = "Volume Decrease";
             //   axWindowsMediaPlayer1.settings.volume = volumeLevel;
            }
            if (volumeLevel == 100)
            {
                this.button7.Text = "Volume Max";
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (volumeLevel > 0)
            {
                volumeLevel = volumeLevel - 10;
                this.button7.Text = "Volume Increase";
                
              //  axWindowsMediaPlayer1.settings.volume = volumeLevel;
            }
            if (volumeLevel == 0)
            {
                this.button8.Text = "Volume Min";
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
            /*
             OpenFileDialog openFileDialog1 = new OpenFileDialog();
        if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            img.Save(openFileDialog1.FileName);
        }
             */
             
            pictureBox1.Image = img3;
           // img.Save(@"C:\Users\Fonda Chau\Desktop\School\YEAR 3\Elec371\CamDevKit1\cam_aforge1\IMG.png");
            img3.Save(Application.StartupPath + DateTime.Now.ToString("yyyyMMddhhmmss") + "IMG.png");
        

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void viewFinder_MouseClick(object sender, MouseEventArgs e)
        {
            p = new Point(e.X, e.Y);
            x = p.X;
            y = p.Y;
            clearbut = false;
            
            if (listBox2.SelectedIndex < 8 && listBox2.SelectedIndex>=0)
            {
                ColourSelect = colourarry[listBox2.SelectedIndex];
            }
            else
                ColourSelect = colourarry[0];
            if (listBox3.SelectedIndex < 3 && listBox3.SelectedIndex >= 0)
            {
                SizeSelect = sizearry[listBox3.SelectedIndex];
            }
            else
                SizeSelect = sizearry[0];
            if (listBox4.SelectedIndex < 3 && listBox4.SelectedIndex >= 0)
            {
                weightselect = weightarry[listBox4.SelectedIndex];
            }
            else
                weightselect = weightarry[0];
            if (listBox1.SelectedIndex == 0)
            {
                shapeselect = 0;
            }
            else
                shapeselect = 1;
            
        }

        private void viewFinder_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Rotatevid = trackBar1.Value;
            
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            zoomvid = trackBar2.Value;
        }


        Bitmap zoom(Bitmap img, Size size)
        {
            Bitmap bmp = new Bitmap(img, img.Width + (img.Width * size.Width / 100), img.Height + (img.Height * size.Height / 100));
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            return bmp;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(ServerPorttextBox.Text));
            listener.Start();
            client = listener.AcceptTcpClient();
            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true;

            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.WorkerSupportsCancellation = true;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(ClientIPtextBox.Text), int.Parse(ClientPorttextBox.Text));
            
            client = new TcpClient();

            s.Connect(IpEnd);

            try
            {
                client.Connect(IpEnd);

                if (client.Connected)
                {
                    ChatScreentextBox.AppendText("Connected to server" + "\n");
                    STW = new StreamWriter(client.GetStream());
                    STR = new StreamReader(client.GetStream());
                    STW.AutoFlush = true;
                    backgroundWorker1.RunWorkerAsync();
                    backgroundWorker2.WorkerSupportsCancellation = true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            IPEndPoint IPE = new IPEndPoint(IPAddress.Parse(ClientIPtextBox.Text), int.Parse(ClientPorttextBox.Text));
            while (client.Connected)
            {
                try
                {
                    recieve = STR.ReadLine();
                    this.ChatScreentextBox.Invoke(new MethodInvoker(delegate()
                    {
                        ChatScreentextBox.AppendText("You:" + recieve + "\n");
                    }));
                    recieve = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                s.Connect(IPE);

                while (button11.Text=="button10")
                {

                    byte[] buffer = new byte[1000000];

                    s.Receive(buffer, buffer.Length, SocketFlags.None);

                    MemoryStream ms = new MemoryStream(buffer);

                    ms.Write(buffer, 0, buffer.Length);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);

                        pictureBox2.Image = bitmap;
                    

                }
                
            }

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            IPEndPoint IPE = new IPEndPoint(IPAddress.Parse(ClientIPtextBox.Text), int.Parse(ClientPorttextBox.Text));
         
            if (client.Connected)
            {
                STW.WriteLine(TextToSend);
                this.ChatScreentextBox.Invoke(new MethodInvoker(delegate()
                {
                    ChatScreentextBox.AppendText("Me:" + TextToSend + "\n");
                }));
            }
            else
            {
                MessageBox.Show("Sending failed");
            }
            backgroundWorker2.CancelAsync();

            sListen.Bind(IPE);
            sListen.Listen(2);

            while (button11.Text == "button10")
            {
                Socket clientSocket;
                clientSocket = sListen.Accept();

                var converter = new System.Drawing.ImageConverter();
                byte[] buffer = (byte[])converter.ConvertTo(img3, typeof(byte[]));
                clientSocket.Send(buffer, buffer.Length, SocketFlags.None);
            }
        }

        private void Sendbutton_Click(object sender, EventArgs e)
        {
            if (MessagetextBox.Text != "")
            {
                TextToSend = MessagetextBox.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            MessagetextBox.Text = "";
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            clearbut = true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (button10.Text == "button10")
            {
                button10.Text = "yo";
            }
            else
                button10.Text = "button10";
        }

    }
}