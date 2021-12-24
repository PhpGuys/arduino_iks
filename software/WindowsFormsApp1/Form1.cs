using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
                    string command = port.ReadExisting();
                    Console.WriteLine(command);
                    if (command.Contains("sending..."))
                    {
                        Thread.Sleep(200);
                        //control.id
                        richTextBox1.Text = port.ReadExisting();
                        byte ptr1_1 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        Console.WriteLine("ptr1_1", ptr1_1);
                        Thread.Sleep(200);
                        richTextBox1.Text = port.ReadExisting();
                        byte ptr1_2 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        Console.WriteLine(ptr1_2);
                        Thread.Sleep(200);

                        //control.code
                        richTextBox1.Text = port.ReadExisting();
                        byte ptr2_1 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        Console.WriteLine(ptr2_1);
                        Thread.Sleep(200);

                        //control.letter
                        richTextBox1.Text = port.ReadExisting();
                        byte ptr3_1 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        Console.WriteLine(ptr3_1);
                        Thread.Sleep(200);

                        ////control.digit
                        //richTextBox1.Text = port.ReadExisting();
                        //byte ptr4_1 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        //Console.WriteLine(ptr4_1);
                        //Thread.Sleep(200);
                        //richTextBox1.Text = port.ReadExisting();
                        //byte ptr4_2 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        //Console.WriteLine(ptr4_2);
                        //Thread.Sleep(200);


                        ////control.identifier
                        //richTextBox1.Text = port.ReadExisting();
                        //byte ptr5_1 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        //Console.WriteLine(ptr5_1);
                        //Thread.Sleep(200);
                        //richTextBox1.Text = port.ReadExisting();
                        //byte ptr5_2 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        //Console.WriteLine(ptr5_2);
                        //Thread.Sleep(200);
                        //richTextBox1.Text = port.ReadExisting();
                        //byte ptr5_3 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        //Console.WriteLine(ptr5_3);
                        //Thread.Sleep(200);
                        //richTextBox1.Text = port.ReadExisting();
                        //byte ptr5_4 = Convert.ToByte(richTextBox1.Text.Replace("\n", ""));
                        //Console.WriteLine(ptr5_4);
                        //Thread.Sleep(200);
                        int id;
                        char code;
                        char letter;
                        unsafe
                        {
                            
                            byte* ptr1 = (byte*)&id;
                            *ptr1 = ptr1_1;
                            *(ptr1 + 1) = ptr1_2;

                            
                            byte* ptr2 = (byte*)&code;
                            *ptr2 = ptr2_1;

                            
                            byte* ptr3 = (byte*)&letter;
                            *ptr3 = ptr3_1;

                            //int digit;
                            //byte* ptr4 = (byte*)&digit;
                            //*ptr4 = ptr4_1;
                            //*(ptr4 + 1) = ptr4_2;

                            //float identifier;
                            //byte* ptr5 = (byte*)&identifier;
                            //*ptr5 = ptr5_1;
                            //*(ptr4 + 1) = ptr5_2;
                            //*(ptr4 + 2) = ptr5_3;
                            //*(ptr4 + 3) = ptr5_4;

                            Console.WriteLine($"Считали параметр id: {id}");
                            Console.WriteLine($"Считали параметр code: {code}");
                            Console.WriteLine($"Считали параметр letter: {letter}");
                            //Console.WriteLine($"Считали параметр digit: {digit}");
                            //Console.WriteLine($"Считали параметр identifier: {identifier}");

                        }

                        StreamReader f = new StreamReader("../../../../hardware/sketch_nov17a/file.h");

                        while (!f.EndOfStream)
                        {
                            string s = f.ReadLine();
                            CreateControlType(s);
                            Console.WriteLine(s);
                        
                        }
                        f.Close();
                    }
                    
                    //if(richTextBox1.Text.Contains("button"))
                    //{
                    //    Console.WriteLine("<" +richTextBox1.Text.ToString() + ">");
                    //    Button button = new Button();
                    //    button.Left = rand.Next(10, 250);
                    //    button.Top = rand.Next(10, 250);
                    //    button.Name = "btn" + rand.Next(10, 200);
                    //    button.Text = "btn" + rand.Next(10, 200);
                    //    button.Click += ButtonOnClick;
                    //    this.Controls.Add(button);
                    //}
                    //else if (richTextBox1.Text.Contains("textbox"))
                    //{
                    //    Console.WriteLine("<" + richTextBox1.Text.ToString() + ">");
                    //    TextBox tbox = new TextBox();
                    //    tbox.Left = rand.Next(10, 250);
                    //    tbox.Top = rand.Next(10, 250);
                    //    tbox.Name = "textbox" + rand.Next(10, 200);
                    //    tbox.Text = "textbox" + rand.Next(10, 200);
                    //    this.Controls.Add(tbox);
                    //}

                    //else
                    //{
                    //    TextBox tbox = new TextBox();
                    //    tbox.Left = rand.Next(10, 250);
                    //    tbox.Top = rand.Next(10, 250);
                    //    tbox.Name = "name" ;
                    //    tbox.Text = "0";
                    //    this.Controls.Add(tbox);
                    //}





                });
            Thread.Sleep(3000);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            port.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void CreateControlType(string s)
        {
            Random rand = new Random();
            string str = s + " ";
            if (str.Contains("type:F"))
            {
                TextBox tbox = new TextBox();
                tbox.Top = rand.Next(50, 250);
                tbox.Left = rand.Next(50, 250);
                int start = str.IndexOf("name:");
                int end = str.LastIndexOf(" ");
                if (end > start)
                {
                    int length = end - start;
                    string result = str.Substring(start, length);
                    tbox.Name = result;
                }
                Controls.Add(tbox);
            }
            if (str.Contains("type:H"))
            {
                CheckBox cbox = new CheckBox();
                cbox.Top = rand.Next(50, 250);
                cbox.Left = rand.Next(50, 250);
                int start = str.IndexOf("name:");
                int end = str.LastIndexOf(" ");
                if (end > start)
                {
                    int length = end - start;
                    string result = str.Substring(start, length);
                    cbox.Name = result;
                }
                Controls.Add(cbox);
            }
            if (str.Contains("type:B"))
            {
                Button cbox = new Button();
                cbox.Top = rand.Next(100, 250);
                cbox.Left = rand.Next(100, 250);
                int start = str.IndexOf("name:");
                int end = str.LastIndexOf(" ");
                if (end > start)
                {
                    int length = end - start;
                    string result = str.Substring(start, length);
                    cbox.Name = result;
                }

                Controls.Add(cbox);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!port.IsOpen) return;
            port.Write("1");
        }
    }
}
