using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodDic = System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>;
using MethodDefDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.MethodDefinition>;
using FieldDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.FieldDefinition>;
using PropertyDic = System.Collections.Generic.Dictionary<string, Mono.Cecil.PropertyDefinition>;

namespace PreCS.Workers
{
    abstract class Worker
    {
        private const BindingFlags ALL_BIND =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        protected Dictionary<Type, MethodDefDic> _targetMethods;
        protected Dictionary<Type, FieldDic> _targetFields;
        protected Dictionary<Type, PropertyDic> _targetProperties;
        protected MethodDic _methods;
        protected MethodDefDic _defMethods;

        protected abstract (Type attributeType, Func<CustomAttribute[], IMemberDefinition, string> keySelector)[] GetTargetAttributes();

        public Worker(Type[] types, TypeDefinition[] defTypes)
        {
            _defMethods = defTypes
                .SelectMany(t => t.Methods)
                .Where(t => !t.IsConstructor)
                .ToDictionary(m => Program.GetFullName(m));
            _methods =
                (from type in types
                 from method in type.GetMethods(ALL_BIND)
                 let name = Program.GetFullName(method)
                 where _defMethods.ContainsKey(name)
                 select (name, method)).ToDictionary(p => p.Item1, p => p.Item2);

            var targetAttributes = GetTargetAttributes();
            _targetFields = new Dictionary<Type, FieldDic>();
            _targetMethods = new Dictionary<Type, MethodDefDic>();
            _targetProperties = new Dictionary<Type, PropertyDic>();

            foreach(var a in targetAttributes)
            {
                _targetFields.Add(a.attributeType,
                    defTypes.SelectMany(t => t.Fields)
                    .Where(m => m.CustomAttributes.Where(at => at.AttributeType.Resolve().IsSubclassof(a.attributeType.FullName)).Any())
                    .ToDictionary(m => a.keySelector(m.CustomAttributes.Where(at => at.AttributeType.Resolve().IsSubclassof(a.attributeType.FullName)).ToArray(), m)));

                _targetMethods.Add(a.attributeType,
                    defTypes.SelectMany(t => t.Methods)
                    .Where(m => m.CustomAttributes.Where(at => at.AttributeType.Resolve().IsSubclassof(a.attributeType.FullName)).Any())
                    .ToDictionary(m => a.keySelector(m.CustomAttributes.Where(at => at.AttributeType.Resolve().IsSubclassof(a.attributeType.FullName)).ToArray(), m)));

                _targetProperties.Add(a.attributeType,
                    defTypes.SelectMany(t => t.Properties)
                    .Where(m => m.CustomAttributes.Where(at => at.AttributeType.Resolve().IsSubclassof(a.attributeType.FullName)).Any())
                    .ToDictionary(m => a.keySelector(m.CustomAttributes.Where(at => at.AttributeType.Resolve().IsSubclassof(a.attributeType.FullName)).ToArray(), m)));
            }
        }

        public abstract void Run();
        
    }
}
