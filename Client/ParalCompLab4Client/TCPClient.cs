using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ParalCompLab4Client
{
    internal class TCPClient
    {
        public void Client()
        {
            using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tcpClient.Connect("127.0.0.1", 8888);
                Console.WriteLine($"Connection to {tcpClient.RemoteEndPoint} installed");
                ClientTask(tcpClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        void ClientTask(Socket socket)
        {
            int threadAmount = 4;
            int n = 8000;
            int[,] matrix = Matrix.CreateMatrix(n);

            long time = 0;

            while (true)  //відправка даних
            {
                SendInt(socket, threadAmount);
                SendInt(socket, n);
                SendMatrixInt(socket, matrix, n);

                Console.WriteLine("All data is sended");

                bool response = ReceiveBool(socket);    //true що всі дані отримані і вони є коректними
                if(response) 
                {
                    break;
                }
            }

            SendBool(socket, true);   //відправляємо true, щоб сервер почав оброблювати масив
            
            bool receiveStart = ReceiveBool(socket);   //отримуємо true, що сервер вже почав оброблювати масив
            if(!receiveStart)
            {
                Console.WriteLine("Server didn't start processing matrix. Close connection..");
                return;
            }

            Console.WriteLine("Server started processing matrix");
            while(true) //перевірка статусу
            {
                Thread.Sleep(100);
                SendBool(socket, true);   //відправляємо запит на отримання статусу
                bool status = ReceiveBool(socket);    //отримуємо статус (true - сервер обробив матрицю, false - в процесі оброблення)
                if(!status)
                {
                    Console.WriteLine("Status = processing");
                }
                else
                {
                    Console.WriteLine("Status = done");
                    break;
                }
            }

            time = ReceiveLong(socket);
            Console.WriteLine($"Time processing matrix = {time} ms");

            matrix = new int[n, n];
            matrix = ReceiveMatrixInt(socket, matrix, n);
            bool checkResultMatrix = Matrix.CheckResultMatrix(matrix, n);

            if(checkResultMatrix)
            {
                Console.WriteLine("Check = Processing done successful :)");
            }
            else
            {
                Console.WriteLine("Check = Processing done unsuccessful, something went wrong :(");
            }

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
        public int ReceiveLong(Socket socket)
        {
            try
            {
                byte[] buffer = new byte[sizeof(int)];
                int bytes = socket.Receive(buffer);
                return BitConverter.ToInt32(buffer, 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int[,] ReceiveMatrixInt(Socket socket, int[,] matrix, int n)
        {
            
            int size = n * n;
            byte[] buffer = new byte[sizeof(int) * size];
            int bytes = socket.Receive(buffer);

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
