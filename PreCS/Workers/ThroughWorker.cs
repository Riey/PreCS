using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var throughMethod in _throughMethods)
            {
                TypeDefinition type = throughMethod.Value.DeclaringType;

                MethodBody body = throughMethod.Value.Body;
                body.Instructions.Clear();

                ILProcessor il = body.GetILProcessor();
                var throughAttribute = throughMethod.Value.CustomAttributes.Where(a => a.AttributeType.Resolve().IsSubclassof(typeof(ThroughAttribute))).First();

                var targetType = (TargetType)throughAttribute.ConstructorArguments[0].Value;
                var targetName = (string)throughAttribute.ConstructorArguments[1].Value;

                IMemberDefinition target = null;
                FieldDefinition fieldTarget = null;
                PropertyDefinition propertyTarget = null;
                bool isStatic = false;

                switch (targetType)
                {
                    case TargetType.Field:
                        target = fieldTarget = type.Fields.Where(f => f.Name == targetName).FirstOrDefault();
                        isStatic = fieldTarget?.IsStatic ?? false;
                        break;
                    case TargetType.Property:
                        target = propertyTarget = type.Properties.Where(f => f.Name == targetName && f.GetMethod != null).FirstOrDefault();
                        isStatic = propertyTarget?.GetMethod.IsStatic ?? false;
                        break;
                }

                if (target == null)
                {
                    Console.WriteLine($"Can't find {targetType} name {targetName}");
                    continue;
                }

                var targetMethod = MethodSearcher.SearchMethod(target.DeclaringType, throughMethod.Value.Name, throughMethod.Value.Parameters);

                if (targetMethod == null)
                {
                    Console.WriteLine($"Can't find method name {targetName}");
                    continue;
                }

                if (!targetMethod.IsStatic)
                {
                    if (targetType == TargetType.Field)
                    {
                        if (fieldTarget.IsStatic)
                            il.Emit(OpCodes.Ldsfld, fieldTarget);
                        else
                        {
                            il.Append(il.LoadArg(0));
                            il.Emit(OpCodes.Ldfld, fieldTarget);
                        }
                    }
                    else if (targetType == TargetType.Property)
                    {
                        var getMethod = (target as PropertyDefinition).GetMethod;
                        if (getMethod.IsStatic)
                            il.Emit(OpCodes.Call, getMethod);
                        else
                        {
                            il.Append(il.LoadArg(0));
                            il.Emit(OpCodes.Callvirt, getMethod);
                        }
                    }
                }

                int parameterCount = throughMethod.Value.Parameters.Count;
                
                for (int i = 0; i < parameterCount; i++)
                {
                    il.Append(il.LoadArg(isStatic ? i : i + 1));
                }

                if (targetMethod.IsStatic)
                    il.Emit(OpCodes.Call, targetMethod);
                else
                    il.Emit(OpCodes.Callvirt, targetMethod);

                il.Emit(OpCodes.Ret);
            }
        }
    }
}
