using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodDic = System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>;
using MethodDefDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.MethodDefinition>;
using FieldDic = System.Collections.Generic.Dictionary<string, System.Reflection.FieldInfo>;
using PropertyDic = System.Collections.Generic.Dictionary<string, System.Reflection.PropertyInfo>;
using CodeHelper;

namespace PreCS.Workers
{
    class ThroughWorker : Worker
    {
        private MethodDic _throughMethods;

        protected override (Type attributeType, Func<Attribute[], MemberInfo, string> keySelector)[] GetTargetAttributes()
        {
            return new(Type attributeType, Func<Attribute[], MemberInfo, string> keySelector)[]
                        {
                            (typeof(ThroughAttribute), (a, m)=> Program.GetFullName(m)),
                        };
        }

        public ThroughWorker(Type[] types, TypeDefinition[] defTypes) : base(types, defTypes)
        {
            _throughMethods = _targetMethods[typeof(ThroughAttribute)];
        }

        public override void Run()
        {
            foreach(var throughMethod in _throughMethods)
            {
                Type targetType = throughMethod.Value.DeclaringType;
                Type returnType = throughMethod.Value.ReturnType;
                ParameterInfo[] parameterInfos = throughMethod.Value.GetParameters();

            }
        }
    }
}
