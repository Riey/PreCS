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
using System.Runtime.CompilerServices;

namespace PreCS.Workers
{
    class ThroughWorker : Worker
    {
        private MethodDefDic _throughMethods;
        private PropertyDic _throughProperties;

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
                
                var targetName = (string)throughAttribute.ConstructorArguments[0].Value;

                IMemberDefinition target = type.GetMember(targetName);
                FieldDefinition fieldTarget = null;
                PropertyDefinition propertyTarget = null;


                bool isStatic = false;

                if (target == null)
                {
                    Console.WriteLine($"Can't find member name {targetName}");
                    continue;
                }

                if (target is FieldDefinition)
                {
                    fieldTarget = (FieldDefinition)target;
                    isStatic = fieldTarget.IsStatic;
                }

                else if (target is PropertyDefinition)
                {
                    propertyTarget = (PropertyDefinition)target;
                    if(propertyTarget.GetMethod==null)
                    {
                        Console.WriteLine($"Property {targetName} has no get accessor");
                        continue;
                    }
                    isStatic = propertyTarget.GetMethod.IsStatic;
                }

                else
                {
                    Console.WriteLine($"Member {targetName} is not Property or Field");
                    continue;
                }

                var targetMethod = MethodSearcher.SearchMethod(target.DeclaringType, throughMethod.Value.Name, throughMethod.Value.Parameters);

                if (targetMethod == null)
                {
                    Console.WriteLine($"Can't find method name {targetName} in Type {target.DeclaringType}");
                    continue;
                }
                else if (targetMethod.IsStatic)
                {
                    Console.WriteLine($"Static method {targetMethod.Name} is not vaild");
                    continue;
                }

                LoadTarget(il, fieldTarget, propertyTarget);

                int parameterCount = throughMethod.Value.Parameters.Count;

                for (int i = 0; i < parameterCount; i++)
                {
                    il.Append(il.LoadArg(isStatic ? i : i + 1));
                }

                il.Emit(OpCodes.Callvirt, targetMethod);
                il.Emit(OpCodes.Ret);
            }

            foreach (var throughProperty in _throughProperties)
            {
                var type = throughProperty.Value.DeclaringType;

                var throughAttribute = throughProperty.Value.CustomAttributes.Where(a => a.AttributeType.Resolve().IsSubclassof(typeof(ThroughAttribute))).First();

                var targetName = (string)throughAttribute.ConstructorArguments[0].Value;

                IMemberDefinition target = type.GetMember(targetName);
                FieldDefinition fieldTarget = null;
                PropertyDefinition propertyTarget = null;


                bool isStatic = false;

                if (target == null)
                {
                    Console.WriteLine($"Can't find member name {targetName}");
                    continue;
                }

                if (target is FieldDefinition)
                {
                    fieldTarget = (FieldDefinition)target;
                    isStatic = fieldTarget.IsStatic;
                }

                else if (target is PropertyDefinition)
                {
                    propertyTarget = (PropertyDefinition)target;
                    if (propertyTarget.GetMethod == null)
                    {
                        Console.WriteLine($"Property {targetName} has no get accessor");
                        continue;
                    }
                    isStatic = propertyTarget.GetMethod.IsStatic;
                }

                else
                {
                    Console.WriteLine($"Target name {targetName} is not Property or Field");
                    continue;
                }

                var targetProperty = target.DeclaringType.Properties.Where(p => p.Name == throughProperty.Value.Name).FirstOrDefault();

                if (targetProperty == null)
                {
                    Console.WriteLine($"Can't find property name {throughProperty.Value.Name} in Type {target.DeclaringType}");
                }

                if (throughProperty.Value.GetMethod != null)
                {
                    if(targetProperty.GetMethod==null)
                    {
                        Console.WriteLine($"Property {targetProperty.FullName} has no get accessor");
                        continue;
                    }

                    var getMethod = throughProperty.Value.GetMethod;

                    var body = getMethod.Body;
                    var il = body.GetILProcessor();

                    //if auto property
                    if (getMethod.CustomAttributes.Where(a => a.AttributeType.Resolve().IsSubclassof(typeof(CompilerGeneratedAttribute))).Any())
                    {
                        var temp = getMethod.CustomAttributes.ToArray();
                        getMethod.CustomAttributes.Clear();

                        foreach(var t in temp)
                        {
                            if (t.AttributeType.Resolve().IsSubclassof(typeof(CompilerGeneratedAttribute)))
                                continue;
                            getMethod.CustomAttributes.Add(t);
                        }
                        //Delete CompilerGeneratedAttribute

                        type.Fields.Remove((body.Instructions[1].Operand as FieldReference).Resolve());
                        //Delete Auto-Generated Field
                    }

                    body.Instructions.Clear();

                    LoadTarget(il, fieldTarget, propertyTarget);

                    if (targetProperty.GetMethod.IsStatic)
                    {
                        Console.WriteLine($"Static method {targetProperty.Name} is not vaild");
                        continue;
                    }

                    il.Emit(OpCodes.Callvirt, targetProperty.GetMethod);
                    il.Emit(OpCodes.Ret);
                }

                if (throughProperty.Value.SetMethod != null)
                {
                    if (targetProperty.SetMethod == null)
                    {
                        Console.WriteLine($"Property {targetProperty.FullName} has no set accessor");
                        continue;
                    }

                    var setMethod = throughProperty.Value.SetMethod;

                    var body = setMethod.Body;
                    var il = body.GetILProcessor();

                    //if auto property
                    if (setMethod.CustomAttributes.Where(a => a.AttributeType.Resolve().IsSubclassof(typeof(CompilerGeneratedAttribute))).Any())
                    {
                        var temp = setMethod.CustomAttributes.ToArray();
                        setMethod.CustomAttributes.Clear();

                        foreach (var t in temp)
                        {
                            if (t.AttributeType.Resolve().IsSubclassof(typeof(CompilerGeneratedAttribute)))
                                continue;
                            setMethod.CustomAttributes.Add(t);
                        }
                        //Delete CompilerGeneratedAttribute

                        FieldDefinition autoField = (body.Instructions[2].Operand as FieldReference).Resolve();

                        type.Fields.Remove(autoField);
                        //Delete Auto-Generated Field
                    }

                    body.Instructions.Clear();

                    LoadTarget(il, fieldTarget, propertyTarget);

                    if (targetProperty.SetMethod.IsStatic)
                    {
                        Console.WriteLine($"Static method {targetProperty.Name} is not vaild");
                        continue;
                    }

                    if (throughProperty.Value.SetMethod.IsStatic)
                        il.Emit(OpCodes.Ldarg_0);
                    else
                        il.Emit(OpCodes.Ldarg_1);
                    //load 'value'
                    il.Emit(OpCodes.Callvirt, targetProperty.SetMethod);
                    il.Emit(OpCodes.Ret);
                }
            }
        }

        private static void LoadTarget(ILProcessor il, FieldDefinition fieldTarget, PropertyDefinition propertyTarget)
        {
            if (fieldTarget != null)
            {
                if (fieldTarget.IsStatic)
                    il.Emit(OpCodes.Ldsfld, fieldTarget);
                else
                {
                    il.Append(il.LoadArg(0));
                    il.Emit(OpCodes.Ldfld, fieldTarget);
                }
            }
            else
            {
                var getMethod = propertyTarget.GetMethod;
                if (getMethod.IsStatic)
                    il.Emit(OpCodes.Call, getMethod);
                else
                {
                    il.Append(il.LoadArg(0));
                    il.Emit(OpCodes.Callvirt, getMethod);
                }
            }
        }
    }
}
