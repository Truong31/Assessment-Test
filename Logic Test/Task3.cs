using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task3
{
    class Program
    {
        public static int Solution(int[] A)
        {
            int[] count = new int[A.Length + 1];

            int missingNum = 1;
            int move = 0;

            foreach (int i in A)
                count[i]++;

            for(int i = 1; i <= A.Length; i++)
            {
                while (count[i] > 1)
                {
                    while (count[missingNum] > 0 && missingNum < A.Length)
                    {
                        missingNum++;
                    }

                    if (missingNum > A.Length) break;

                    count[i]--;
                    count[missingNum]++;
                    move += Math.Abs(missingNum - i);
                }
            }

            return (move > 1000000000) ? -1 : move;
        }

        static void Main(string[] args)
        {
            int[] A = { 1, 2, 1};
            int[] A1 = { 2, 1, 4, 4 };
            int[] A2 = { 6, 2, 3, 5, 6, 3 };

            Console.WriteLine(Solution(A2));
            Console.ReadKey();
        }
    }
}
