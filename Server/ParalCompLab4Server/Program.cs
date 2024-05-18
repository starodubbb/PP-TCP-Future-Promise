using System.Diagnostics;

namespace ParalCompLab4Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TCPServer tcpServer = new TCPServer();
            tcpServer.Server();
        }
    }
}