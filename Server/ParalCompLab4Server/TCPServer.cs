using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ParalCompLab4Server
{
    internal class TCPServer
    {
        public void Server()
        {
            using Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //вказуємо локальну точку (адрес), на якій сокет буде приймати підключення від клієнтів (127.0.0.1:8888)
                tcpServer.Bind(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8888));
                tcpServer.Listen();    // запуск прослуховування підключень
                Console.WriteLine("Server is started. Waiting connections... ");
                while (true)
                {
                    var tcpClient = tcpServer.Accept(); // отримуємо підключення у вигляді TcpClient
                    Console.WriteLine($"\n{tcpClient.RemoteEndPoint} \tConnection installed");
                    Thread thr = new Thread(() => ProcessClient(tcpClient));
                    thr.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void ProcessClient(Socket tcpClient)
        {
            Console.WriteLine($"{tcpClient.RemoteEndPoint} \tnew thread #{Thread.CurrentThread.ManagedThreadId}");
            int threadsAmount = 0;
            int n = 0;
            int[,] matrix = null;

            while (true)
            {
                threadsAmount = ReceiveInt(tcpClient);
                //Console.WriteLine($"threadsAmount = {threadsAmount}");
                n = ReceiveInt(tcpClient);
                //Console.WriteLine($"n = {n}");
                matrix = ReceiveMatrixInt(tcpClient, n);
                //Console.WriteLine("Matrix is received");

                if ((threadsAmount > 0) && (n > 0) && (matrix.Length == (n * n)))
                {
                    SendBool(tcpClient, true);  //відправляємо true, що всі дані отримані, і вони є коректними
                    Console.WriteLine($"{tcpClient.RemoteEndPoint} \tAll data is received - thread #{Thread.CurrentThread.ManagedThreadId}");
                    break;
                }
                else
                {
                    SendBool(tcpClient, false);  //відправляємо false, що отримані дані не є коректними
                }
            }

            bool start = ReceiveBool(tcpClient);        //отримуємо true, щоб почати оброблювати масив

            if (!start)
            {
                SendBool(tcpClient, false);   //відправляємо false, що обробка матриці не почалася, з'єднання з клієнтом зачиняється
                Console.WriteLine($"{tcpClient.RemoteEndPoint} \tConnection closed");
                tcpClient.Shutdown(SocketShutdown.Both);
                tcpClient.Close();
                return;
            }

            TaskCompletionSource<long> tcs1 = new TaskCompletionSource<long>();     //promise
            Task<long> t1 = tcs1.Task;                                              //future

            

            Thread thr = new Thread(() => Execution.StartExecution(matrix, n, threadsAmount, tcs1, tcpClient.RemoteEndPoint));
            thr.Start();

            SendBool(tcpClient, true);     //відправляємо true, що обробка матриці почалася

            while (true)
            {
                bool query = ReceiveBool(tcpClient);
                if (query)
                {
                    TaskStatus status = t1.Status;
                    if (status != TaskStatus.RanToCompletion)
                    {
                        SendBool(tcpClient, false);
                    }
                    else
                    {
                        SendBool(tcpClient, true);
                        SendLong(tcpClient, t1.Result);
                        SendMatrixInt(tcpClient, matrix, n);
                        break;
                    }
                }
            }

            Console.WriteLine($"{tcpClient.RemoteEndPoint} \tConnection closed");
            tcpClient.Shutdown(SocketShutdown.Both);
            tcpClient.Close();
        }

        public int Receive(Socket socket, byte[] buffer)
        {
            try
            {
                int bytes = socket.Receive(buffer);
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool ReceiveBool(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[sizeof(bool)];
                int bytes = socket.Receive(buffer);
                return BitConverter.ToBoolean(buffer, 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int ReceiveInt(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[sizeof(int)];
                int bytes = socket.Receive(buffer);
                return BitConverter.ToInt16(buffer, 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int[,] ReceiveMatrixInt(Socket socket, int n)
        {
            int[,] matrix = new int[n, n];
            int size = n * n;
            byte[] buffer = new byte[sizeof(int) * size];
            int bytes = socket.Receive(buffer);
            //Console.WriteLine($"Received matrix {bytes} bytes");

            int currentCell = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = BitConverter.ToInt16(buffer, currentCell * 4);
                    currentCell++;
                }
            }
            return matrix;
        }

        public int Send(Socket senderSocket, byte[] data)
        {
            try
            {
                int bytes = senderSocket.Send(data);
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int SendBool(Socket senderSocket, bool data)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(data);
                int bytes = senderSocket.Send(buffer);
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int SendInt(Socket senderSocket, int data)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(data);
                int bytes = senderSocket.Send(buffer);
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int SendLong(Socket senderSocket, long data)
        {
            try
            {
                byte[] buffer = BitConverter.GetBytes(data);
                int bytes = senderSocket.Send(buffer);
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int SendMatrixInt(Socket senderSocket, int[,] matrix, int n)
        {
            int size = n * n;
            int sizeOf = sizeof(int);
            byte[] buffer = new byte[sizeof(int) * size];

            int currentByte = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    byte[] cellBuffer = BitConverter.GetBytes(matrix[i, j]);
                    //add value to buffer
                    for (int k = 0; k < sizeOf; k++)
                    {
                        buffer[currentByte] = cellBuffer[k];
                        currentByte++;
                    }
                }
            }
            int bytes = senderSocket.Send(buffer);
            return bytes;

        }
    }
}
