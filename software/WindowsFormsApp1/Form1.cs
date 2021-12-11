using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SerialPort port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);

        public Form1()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
            port.Open();
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            richTextBox1.Invoke(
                (ThreadStart)delegate ()
                {
                    richTextBox1.Text = port.ReadExisting();
                });
            Thread.Sleep(1000);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            port.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
