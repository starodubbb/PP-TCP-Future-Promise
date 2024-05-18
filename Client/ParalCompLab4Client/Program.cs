namespace ParalCompLab4Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
           TCPClient tcpClient = new TCPClient();
            tcpClient.Client();          
        }
    }
}