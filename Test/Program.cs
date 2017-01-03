using CodeHelper;
using System;

namespace Test
{
    class Program
    {

        [Temporary]
        private static long[] _cache;

        static void Main(string[] args)
        {
            long result = Fib(5 + 5);
            Console.WriteLine(result);
        }

        [Builder("Fib")]
        [Temporary]
        static long Fib(long num)
        {
            if (num < 2)
                return num;
            else if (_cache[num] == 0)
                _cache[num] = Fib(num - 2) + Fib(num - 1);
            return _cache[num];
        }

        [Initializer("Fib")]
        [Temporary]
        static void FibInit()
        {
            _cache = new long[1000];
        }
    }
}
