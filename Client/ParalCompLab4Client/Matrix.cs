using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParalCompLab4Client
{
    internal class Matrix
    {
        static public int[,] CreateMatrix(int n)
        {
            int[,] matrix = new int[n, n];
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = rnd.Next(-255, 255);
                }
            }
            return matrix;
        }
        static void PrintMatrix(int[,] matrix, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{matrix[i, j]}\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

        }
        static public bool CheckResultMatrix(int[,] matrix, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int minValue = matrix[i, 0];
                int minCell = 0;
                for (int j = 1; j < n; j++)
                {
                    if ((minValue > matrix[i, j]) || ((j == (n - i - 1)) && (minValue == matrix[i, j])))
                    {
                        minValue = matrix[i, j];
                        minCell = j;
                    }
                }
                if (minCell != (n - i - 1))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
