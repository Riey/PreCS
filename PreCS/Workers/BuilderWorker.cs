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
using FieldDic = System.Collections.Generic.Dictionary<string, System.Reflection.FieldInfo>;
using PropertyDic = System.Collections.Generic.Dictionary<string, System.Reflection.PropertyInfo>;
using CodeHelper;

namespace PreCS.Workers
{
    class BuilderWorker: Worker
    {
        private MethodDic _builders;
        private MethodDic _initializers;

        protected override (Type attributeType, Func<Attribute[], MemberInfo, string> keySelector)[] GetTargetAttributes()
        {
            return new(Type attributeType, Func<Attribute[], MemberInfo, string> keySelector)[]
                            {
                                (typeof(BuilderAttribute),(a, m) => Program.GetFullName(m)),
                                (typeof(InitializerAttribute), (a, m) =>(a[0] as InitializerAttribute).TargetName),
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
                                if (_builders.ContainsValue(calledRefMethod))
                                {
                                    MethodInfo builder = _builders[Program.GetFullName(calledMethod)];
                                    MethodInfo initializer = _initializers[builder.GetCustomAttribute<BuilderAttribute>().Name];

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
