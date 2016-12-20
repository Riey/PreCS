using CodeHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PreCS
{
    class Program
    {
        static void Main(string[] args)
        {
            var refAsm = Assembly.Load(File.ReadAllBytes(args[0]));
            var refModule = refAsm.ManifestModule;
            var asm = AssemblyDefinition.ReadAssembly(args[0]);
            var module = asm.MainModule;
            var builderAttribute = module.Import(typeof(BuilderAttribute));

            var allTypes = module.GetTypes().Skip(1).ToArray();
            var allRefTypes = refModule.GetTypes();

            var allMethods = (
                from type in allTypes
                from method in type.Methods
                let fullName = $"{type.FullName}.{method.Name}"
                where !method.IsConstructor
                select (fullName, method))
                .ToDictionary(t => t.Item1, t => t.Item2);
            var allRefMethods = (
                from type in allRefTypes
                from method in type.GetRuntimeMethods()
                let fullName = $"{type.FullName}.{method.Name}"
                where allMethods.ContainsKey(fullName)
                select (fullName, method))
                .ToDictionary(t => t.Item1, t => t.Item2);

            var builderMethods = allRefMethods.Where(m => m.Value.IsDefined(typeof(BuilderAttribute)))
                .ToDictionary(p => p.Key, p => p.Value);
            var initializerMethods = allRefMethods.Where(m => m.Value.IsDefined(typeof(InitializerAttribute))).ToArray()
                .ToDictionary(p => p.Value.GetCustomAttribute<InitializerAttribute>().TargetName, p => p.Value);

            foreach (var method in allMethods)
            {
                var body = method.Value.Body;
                var il = body.GetILProcessor();
                List<(Instruction oldInst, Instruction newInst)> replaceList = new List<(Instruction, Instruction)>();
                List<Instruction> removeList = new List<Instruction>();
                foreach (var instruction in body.Instructions)
                {
                    switch (instruction.OpCode.Code)
                    {
                        case Code.Call:
                        case Code.Calli:
                        case Code.Callvirt:
                            {
                                var calledMethod = (instruction.Operand as MethodReference).Resolve();
                                if (!allRefMethods.ContainsKey(GetFullName(calledMethod)))
                                    break;
                                var calledRefMethod = allRefMethods[GetFullName(calledMethod)];
                                if (builderMethods.ContainsValue(calledRefMethod))
                                {
                                    MethodInfo builder = builderMethods[GetFullName(calledMethod)];
                                    MethodInfo initializer = initializerMethods[builder.GetCustomAttribute<BuilderAttribute>().Name];

                                    List<Instruction> addTemp = new List<Instruction>();
                                    Stack<object> callStack = new Stack<object>();
                                    int parameterLength = calledMethod.Parameters.Count;
                                    Instruction temp = instruction.Previous;
                                    try
                                    {
                                        for (int i = 0; i < parameterLength; i++)
                                        {
                                            callStack.Push(temp.Convert());
                                            addTemp.Add(temp);
                                            temp = temp.Previous;
                                        }
                                    }
                                    catch (ArgumentException)
                                    {
                                        break;
                                    }

                                    removeList.AddRange(addTemp);
                                    initializer.Invoke(null, null);
                                    var result = builder.Invoke(null, callStack.ToArray());
                                    replaceList.Add((instruction, il.Save(result)));
                                    Helper.ClearField();
                                }
                                break;
                            }
                    }
                }

                foreach (var replace in replaceList)
                    il.Replace(replace.oldInst, replace.newInst);
                foreach (var remove in removeList)
                    il.Remove(remove);
            }

            asm.Write(args[0]);
        }
        
        private static int GetArgIndex(Instruction instruction)
        {
            return (instruction.Operand == null) ? int.Parse(instruction.OpCode.Code.ToString().Substring(6, 1)) : (instruction.Operand as ParameterDefinition).Index;
        }

        private static string GetFullName(MethodDefinition method) => method.DeclaringType.FullName + "." + method.Name;
        private static string GetFullName(MethodInfo method) => method.DeclaringType.FullName + "." + method.Name;
    }
}
