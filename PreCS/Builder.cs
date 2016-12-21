using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodDic = System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>;
using MethodDefDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.MethodDefinition>;
using CodeHelper;

namespace PreCS
{
    class Builder
    {
        private MethodDic _builders;
        private MethodDic _initializers;
        private MethodDic _methods;
        private MethodDefDic _defMethods;

        public Builder(MethodDic methods, MethodDefDic defMethods)
        {
            _methods = methods;
            _defMethods = defMethods;

            _builders = methods
                .Where(m => m.Value.IsDefined(typeof(BuilderAttribute)))
                .ToDictionary(p => p.Key, p => p.Value);//Cache BuilderMethods

            _initializers = methods
                .Where(m => m.Value.IsDefined(typeof(InitializerAttribute)))
                .ToDictionary(p => p.Value.GetCustomAttribute<InitializerAttribute>().TargetName, p => p.Value);//Cache InitializerMethods
        }

        public void Run()
        {
            foreach (var method in _defMethods)
            {
                var body = method.Value.Body;
                var il = body.GetILProcessor();

                var replaceList = new List<(Instruction oldInst, Instruction newInst)>();
                var removeList = new List<Instruction>();

                foreach (var instruction in body.Instructions)
                {
                    switch (instruction.OpCode.Code)
                    {
                        case Code.Call:
                        case Code.Calli:
                        case Code.Callvirt:
                            {
                                var calledMethod = (instruction.Operand as MethodReference).Resolve();

                                if (!_methods.ContainsKey(Program.GetFullName(calledMethod)))//Defined by Extern Library
                                    break;

                                var calledRefMethod = _methods[Program.GetFullName(calledMethod)];
                                if (_builders.ContainsValue(calledRefMethod))
                                {
                                    MethodInfo builder = _builders[Program.GetFullName(calledMethod)];
                                    MethodInfo initializer = _initializers[builder.GetCustomAttribute<BuilderAttribute>().Name];

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

                                    removeList.AddRange(addTemp);//Remove parameter opcodes

                                    initializer.Invoke(null, null);//Invoke Initializer

                                    var result = builder.Invoke(null, callStack.ToArray());//Invoke Builder

                                    replaceList.Add((instruction, il.Save(result)));//Replace Call OpCode to result

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
        }
    }
}
