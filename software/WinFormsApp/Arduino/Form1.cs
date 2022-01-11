using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO.Ports;
using System.Reflection;

namespace Arduino
{
    public partial class Form1 : Form, ILogger
    {

        ArduinoDevice device;
        Parser parser;

        SerialPort port = new SerialPort("COM3", 9600);

        
        private void Setup()
        {

            device = new(port, this);
            parser = new Parser(device);


            parser.Parse(@"file.h");

            int y = 10;
            foreach (var v in parser.Controls)
            {
                v.Location = new Point(20, y);
                Controls.Add(v);
                y += v.Height;
            }



        }
        public Form1()
        {
            InitializeComponent();
            Setup();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        //    device.Update();
        }

        public void Log(string message)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(Environment.NewLine + message)));
            }
            else
            {
                richTextBox1.AppendText(Environment.NewLine + message);
                richTextBox1.ScrollToCaret();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            device.Kill();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            port.DataReceived += (sender, args) =>
            {
                this?.Invoke(new Action(() => { device.OnData(sender, args); }));
            };
            device.Connect();
            timer1.Start();
            device.RequestAllValues();
        }
    }
}