using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParalCompLab4Server
{
    public class ThreadArgumentMatrix
    {
        public int[,] matrix;
        public int n = 0;
        public int firstLine;
        public int lastLine;

        public ThreadArgumentMatrix(int[,] matrix, int n, int firstLine, int lastLine)
        {
            this.matrix = matrix;
            this.n = n;
            this.firstLine = firstLine;
            this.lastLine = lastLine;
        }

        public void setLines(int firstLine, int lastLine)
        {
            this.firstLine = firstLine;
            this.lastLine = lastLine;
        }

        public void threadFunction()
        {
            //Console.WriteLine($"firstLine = {firstLine}, lastLine = {lastLine}");
            for (int i = firstLine; i <= lastLine; i++)
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

    }
}
