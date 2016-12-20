using CodeHelper;
using System;

namespace Test
{
    class Program
    {


        static void Main(string[] args)
        {
            int result = Fib(40);
            Console.WriteLine(result);
        }

        [Builder("Fib")]
        static int Fib(int num)
        {
            if (num < 2)
                return num;
            else if (Helper.LoadField<int[]>("Cache")[num] == 0)
                Helper.LoadField<int[]>("Cache")[num] = Fib(num - 2) + Fib(num - 1);
            return Helper.LoadField<int[]>("Cache")[num];
        }

        [Initializer("Fib")]
        static void FibInit()
        {
            Helper.SaveField("Cache", new int[50]);
        }
    }
}
