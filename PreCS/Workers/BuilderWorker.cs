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
using FieldDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.FieldDefinition>;
using PropertyDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.PropertyDefinition>;
using CodeHelper;

namespace PreCS.Workers
{
    class BuilderWorker: Worker
    {
        private MethodDefDic _builders;
        private MethodDefDic _initializers;

        protected override (Type attributeType, Func<CustomAttribute[], IMemberDefinition, string> keySelector)[] GetTargetAttributes()
        {
            return new(Type attributeType, Func<CustomAttribute[], IMemberDefinition, string> keySelector)[]
                            {
                                (typeof(BuilderAttribute),(a, m) => Program.GetFullName(m)),
                                (typeof(InitializerAttribute), (a, m) => a[0].ConstructorArguments[0].Value as string),
                            };
        }

        public BuilderWorker(Type[] types, TypeDefinition[] defTypes) : base(types, defTypes)
        {
            _builders = _targetMethods[typeof(BuilderAttribute)];
            _initializers = _targetMethods[typeof(InitializerAttribute)];
        }

        public override void Run()
        {
            foreach (var method in _defMethods)
            {

                if (_builders.ContainsKey(method.Key))
                    continue;
                if (_methods[method.Key].CustomAttributes.Where(a => a.AttributeType.IsSubclassOf(typeof(InitializerAttribute))).Any())
                    continue;

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
                                if (_builders.ContainsValue(calledMethod))
                                {
                                    var builder = calledRefMethod;
                                    MethodInfo initializer = _methods[Program.GetFullName(_initializers[builder.GetCustomAttribute<BuilderAttribute>().Name])];

                                    int parameterLength = calledMethod.Parameters.Count;
                                    object[] args;

                                    try
                                    {
                                        args = StackAnalyser.PopObjects(instruction.Previous, parameterLength, removeList);//Remove parameter opcodes
                                    }
                                    catch (ArgumentException)
                                    {
                                        break;
                                    }

                                    initializer.Invoke(null, null);//Invoke Initializer

                                    var result = builder.Invoke(null, args);//Invoke Builder

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
