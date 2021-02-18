using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Chatroom
{
    public partial class Form1 : Form
    {
        private Socket tcpClient;
        private byte[] data = new byte[1024];
        private string MsgContainer;//存放子线程接收到的消息，准备显示在对话框中
        private Thread receiveMsgThread;//消息接收线程
        private bool isConnected=false;//是否连接
        private string myName;//用户名
        public Form1()
        {
            InitializeComponent();
            MsgContainer = "";
            textBoxIP.Text = GetIP();//默认使用本机ip作为主机ip，方便测试
        }
        private void ReceiveMsg()
        {
            //while (!tcpClient.Poll(10, SelectMode.SelectRead))
            while (tcpClient.Connected&& isConnected)//只要没有断开连接，就不停地接收消息
            {
                try
                {
                    int len = tcpClient.Receive(data);
                    lock (MsgContainer)
                    {
                        MsgContainer = Encoding.UTF8.GetString(data, 0, len);
                    }
                }catch(Exception e)
                {
                    MessageBox.Show("Disconnected: " + e.ToString());
                }
            }
            MsgContainer = "Disconnected";
            isConnected = false;
            tcpClient.Close();
        }
        private bool Connect()
        {
            string ip = textBoxIP.Text;
            if(ip == "")
            {
                MessageBox.Show("Please enter the server IP!");
                return false;
            }
            else
            {
                try
                {
                    tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ip), 8099));
                    myName = textBoxName.Text;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Connection failed!" + e.ToString());
                    return false;
                }
                isConnected = true;
                return true;
            }
        }
        private bool Disconnect()
        {
            if (tcpClient== null)
            {
                return true;
            }
            if (tcpClient.Connected)
            {
                string ms = textBoxName.Text + "is Disconnected";
                byte[] d = Encoding.UTF8.GetBytes(ms);
                tcpClient.Send(d);

                //让线程去关闭连接
                isConnected = false;
                receiveMsgThread.Join();
                //tcpClient.Close();
                return true;
            }

            return false;
        }
        private string GetIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting local IP: " + ex.Message);
                return "";
            }
        }
        private bool SendMsg(string m)
        {
            byte[] d = Encoding.UTF8.GetBytes(m);
            try
            {
                tcpClient.Send(d);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to send: " + e.ToString());
                return false;
            }
            return true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //使用timer控件，生成每隔一个固定时间t执行的函数，
            //使用函数更新对话框中的内容，这是因为在子线程中不能修改主线程创建的控件richTextBox
            lock (MsgContainer)
            {
                if (MsgContainer != null && MsgContainer != "")
                {
                    richTextBox1.AppendText(MsgContainer + "\r");
                    MsgContainer = "";
                    richTextBox1.ScrollToCaret();
                }
            }
            if (!isConnected)
            {
                buttonConnect.Enabled = true;
                textBoxName.Enabled = true;
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if(isConnected)
            {
                if (SendMsg(myName + ": " + textBox1.Text))
                    textBox1.Text = "";
            }
            else
            {
                MessageBox.Show("Please connect to the server.");
            }
        }
        
        private void buttonExit_Click(object sender, EventArgs e)
        {
            Disconnect();
            Application.Exit();
        }
        
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if(Connect())
            {
                //开启接收消息的线程，在后台不断接收消息
                receiveMsgThread = new Thread(ReceiveMsg);
                receiveMsgThread.IsBackground = true;
                receiveMsgThread.Start();
                //
                buttonConnect.Enabled = false;
                textBoxName.Enabled = false;
                isConnected = true;
                SendMsg(myName+ " is Connected");
            }
        }

        private void buttonDisCon_Click(object sender, EventArgs e)
        {
            if (Disconnect())
            {
                buttonConnect.Enabled = true;
                textBoxName.Enabled = true;
            }
        }
    }
}
