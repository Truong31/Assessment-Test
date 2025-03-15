using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    class Program
    {
        public static string Solution(string S)
        {
            for(int i = 0; i < S.Length - 1; i++)
            {
                if (S[i] > S[i + 1])
                {
                    return S.Remove(i, 1);
                }
            }

            return S.Remove(S.Length - 1, 1);
        }

        static void Main(string[] args)
        {
            string S = "acb";
            string S1 = "hot";
            string S2 = "codility";
            string S3 = "aaaa";

            Console.WriteLine(Solution(S3));
            Console.ReadKey();
        }
    }
}
