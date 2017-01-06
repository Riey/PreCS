using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodDic = System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>;
using MethodDefDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.MethodDefinition>;
using FieldDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.FieldDefinition>;
using PropertyDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.PropertyDefinition>;
using CodeHelper;

namespace PreCS.Workers
{
    class ThroughWorker : Worker
    {
        private MethodDefDic _throughMethods;
        private PropertyDic _throughProperties;
        private FieldDic _throughFields;

        protected override (Type attributeType, Func<CustomAttribute[], IMemberDefinition, string> keySelector)[] GetTargetAttributes()
        {
            return new(Type attributeType, Func<CustomAttribute[], IMemberDefinition, string> keySelector)[]
                        {
                            (typeof(ThroughAttribute), (a, m)=> Program.GetFullName(m)),
                        };
        }

        public ThroughWorker(Type[] types, TypeDefinition[] defTypes) : base(types, defTypes)
        {
            _throughMethods = _targetMethods[typeof(ThroughAttribute)];
            _throughProperties = _targetProperties[typeof(ThroughAttribute)];
            _throughFields = _targetFields[typeof(ThroughAttribute)];
        }

        public override void Run()
        {
            foreach(var throughMethod in _throughMethods)
            {

            }
        }
    }
}
