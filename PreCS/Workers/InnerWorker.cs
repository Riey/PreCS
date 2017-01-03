using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodDic = System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>;
using MethodDefDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.MethodDefinition>;
using CodeHelper;

namespace PreCS.Workers
{
    class InnerWorker : Worker
    {
        private MethodDic _innerMethods;

        protected override (Type attributeType, Func<Attribute[], MethodInfo, string> keySelector)[] targetAttributes =>
            new(Type attributeType, Func<Attribute[], MethodInfo, string> keySelector)[]
            {
                (typeof(InnerAttribute), (a, m)=> Program.GetFullName(m)),
            };


        public InnerWorker(MethodDic methods, MethodDefDic defMethods) : base(methods, defMethods)
        {
            _innerMethods = _targetMethods[typeof(InnerAttribute)];
        }

        public override void Run()
        {
            foreach(var innerMethod in _innerMethods)
            {
                Type targetType = innerMethod.Value.DeclaringType;
                Type returnType = innerMethod.Value.ReturnType;
                ParameterInfo[] parameterInfos = innerMethod.Value.GetParameters();

            }
        }
    }
}
