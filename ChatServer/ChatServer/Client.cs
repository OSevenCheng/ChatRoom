using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
namespace ChatServer
{
    class Client
    {
        private Socket cSocket;
        private Thread cThread;
        private byte[] data = new byte[1024];
        public Client(Socket s)
        {
            cSocket = s;
            cThread = new Thread(ReceiveMsg);
            cThread.Start();
        }
        private void ReceiveMsg()//接收来自客户端的消息
        {
            while(!cSocket.Poll(10, SelectMode.SelectRead))
            {
                try
                {
                    int len = cSocket.Receive(data);
                    //Console.WriteLine("Receive a message:");
                    string Message = Encoding.UTF8.GetString(data, 0, len);
                    //Console.WriteLine(Message);
                    Program.BroadcastMsg(Message);
                }
                catch(Exception e)
                {

                }
            }
            cSocket.Close();
        }
        public void SendMsg(string msg)//向客户端发送消息
        {
            cSocket.Send(Encoding.UTF8.GetBytes(msg));
        }
        public bool isConnected
        {
            get { return cSocket.Connected;}
        }
    }
}
