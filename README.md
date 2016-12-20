
# How it works


1. Search Call Opcode witch call Builder method

2. Call Initializer method pair with Builder name

3. Get Builder method result with exist parameters

4. Replace Call and all paramters Opcodes to result Opcode






# Example


Sample SourceCode
-----------------

<pre><code>
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
</code></pre>

It compiles like this ( using ILSPY )

<pre><code>
.method private hidebysig static 
    void Main (
        string[] args
    ) cil managed 
{
    // Method begins at RVA 0x2050
    // Code size 17 (0x11)
    .maxstack 1
    .entrypoint
    .locals init (
        [0] int32 result
    )

    IL_0000: nop
    IL_0001: ldc.i4.s 40
    IL_0003: call int32 Test.Program::Fib(int32)
    IL_0008: stloc.0
    IL_0009: ldloc.0
    IL_000a: call void [mscorlib]System.Console::WriteLine(int32)
    IL_000f: nop
    IL_0010: ret
}
</code></pre>

after run PreCS

<pre><code>
.method private hidebysig static 
    void Main (
        string[] args
    ) cil managed 
{
    // Method begins at RVA 0x2050
    // Code size 15 (0xf)
    .maxstack 1
    .entrypoint
    .locals init (
        [0] int32
    )

    IL_0000: nop
    IL_0001: ldc.i4 102334155
    IL_0006: stloc.0
    IL_0007: ldloc.0
    IL_0008: call void [mscorlib]System.Console::WriteLine(int32)
    IL_000d: nop
    IL_000e: ret
}

    IL_0001: ldc.i4.s 40
    IL_0003: call int32 Test.Program::Fib(int32)

    is replaced by

    IL_0001: ldc.i4 102334155

</code></pre>
