using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    class Program
    {
        public static int Solution(int[][] A)
        {
            int m = A.Length;
            int n = A[0].Length;

            int result = int.MinValue;

            for(int row1 = 0; row1 < m; row1++)
            {
                for(int col1 = 0; col1 < n; col1++)
                {
                    for(int row2 = 0; row2 < m; row2++)
                    {
                        for(int col2 = 0; col2 < n; col2++)
                        {
                            if(row1 != row2 && col1 != col2)
                            {
                                int sum = A[row1][col1] + A[row2][col2];
                                result = Math.Max(result, sum);
                            }
                        }
                    }
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            int[][] A =
            {
                new int[] {1, 4},
                new int[] {2, 3},

            };

            int[][] A1 =
            {
                new int[] {15, 1, 5},
                new int[] {16, 3, 8},
                new int[] {2, 6, 4},

            };

            int[][] A2 =
            {
                new int[] {12, 12},
                new int[] {12, 12},
                new int[] {0, 7},

            };

            int[][] A3 =
            {
                new int[] {1, 2, 14},
                new int[] {8, 3, 15},

            };

            Console.WriteLine(Solution(A3));
            Console.ReadKey();
        }
    }
}
