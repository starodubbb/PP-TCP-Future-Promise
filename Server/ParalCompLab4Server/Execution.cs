using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ParalCompLab4Server
{
    internal class Execution
    {
        static public void StartExecution(int[,] matrix, int n, int threadAmount, TaskCompletionSource<long> tcs1, EndPoint? endPoint)
        {
            Stopwatch stopWatch = new Stopwatch();
            Console.WriteLine($"{endPoint} \tStart processing matrix - thread #{Thread.CurrentThread.ManagedThreadId}");
            //Thread.Sleep(5000);
            stopWatch.Start();
            if (threadAmount == 1)
            {
                OneThreadExecuting(matrix, n);
            }
            else
            {
                ManyThreadsExecuting(matrix, n, threadAmount);
            }
            stopWatch.Stop();
            Console.WriteLine($"{endPoint} \tPerforming is done - thread #{Thread.CurrentThread.ManagedThreadId}");
            tcs1.SetResult(stopWatch.ElapsedMilliseconds);
        }
        static public void OneThreadExecuting(int[,] matrix, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int minValue = matrix[i, 0];
                int minCell = 0;
                for (int j = 1; j < n; j++)
                {
                    if (minValue > matrix[i, j])
                    {
                        minValue = matrix[i, j];
                        minCell = j;
                    }
                }
                matrix[i, minCell] = matrix[i, n - i - 1];
                matrix[i, n - i - 1] = minValue;
            }
        }

        static public void ManyThreadsExecuting(int[,] matrix, int n, int threadsAmount)
        {
            int firstLine = 0;
            int lastLine = 0;
            Thread[] threads = new Thread[threadsAmount];
            if (n >= 2)
            {
                if (n >= threadsAmount)
                {
                    int threadLinesAmount1 = n / threadsAmount;
                    int threadLinesAmount2 = threadLinesAmount1 + 1;
                    int threadsAmount2 = n - threadLinesAmount1 * threadsAmount;
                    int threadsAmount1 = threadsAmount - threadsAmount2;

                    //Console.WriteLine($"threadsAmount1 = {threadsAmount1}, threadLinesAmount1 = {threadLinesAmount1}");
                    //Console.WriteLine($"threadsAmount2 = {threadsAmount2}, threadLinesAmount2 = {threadLinesAmount2}");

                    for (int i = 0; i < threadsAmount1; i++)
                    {
                        lastLine = firstLine + threadLinesAmount1 - 1;
                        ThreadArgumentMatrix threadArgument = new ThreadArgumentMatrix(matrix, n, firstLine, lastLine);
                        threads[i] = new Thread(new ThreadStart(threadArgument.threadFunction));
                        threads[i].Start();
                        firstLine = lastLine + 1;
                    }
                    for (int i = threadsAmount1; i < threadsAmount; i++)
                    {
                        lastLine = firstLine + threadLinesAmount2 - 1;
                        ThreadArgumentMatrix threadArgument = new ThreadArgumentMatrix(matrix, n, firstLine, lastLine);
                        threads[i] = new Thread(new ThreadStart(threadArgument.threadFunction));
                        threads[i].Start();
                        firstLine = lastLine + 1;
                    }

                    for (int i = 0; i < threadsAmount; i++)
                    {
                        threads[i].Join();
                    }
                }
            }
        }
    }
}
