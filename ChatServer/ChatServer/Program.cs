using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace ChatServer
{
    class Program
    {
        static private List<Client> clientList = new List<Client>();

        static public void BroadcastMsg(string msg)
        {
            List<Client> unConnected = new List<Client>();
            foreach(var c in clientList)
            {
                if (c.isConnected)
                {
                    //Console.WriteLine("Broadcasting a message");
                    c.SendMsg(msg);
                    //Console.WriteLine("Broadcasting succeed");
                }
                else
                    unConnected.Add(c);
            }
            foreach(var u in unConnected)
            {
                clientList.Remove(u);
                Console.WriteLine("A client is disconnected");
            }
        }
        static public string GetIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
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
        static void Main(string[] args)
        {
            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpServer.Bind(new IPEndPoint(IPAddress.Parse(GetIP()), 8099));
            tcpServer.Listen(100);
            Console.WriteLine("Waiting for connection...");
            while(true)
            {
                Socket cs = tcpServer.Accept();
                clientList.Add(new Client(cs));
                Console.WriteLine("Accept a client");
            }
        }
    }
}
