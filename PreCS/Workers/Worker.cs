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

namespace PreCS.Workers
{
    abstract class Worker
    {
        protected Dictionary<Type, MethodDic> _targetMethods;
        protected MethodDic _methods;
        protected MethodDefDic _defMethods;

        protected abstract (Type attributeType, Func<Attribute[], MethodInfo, string> keySelector)[] targetAttributes { get; }

        public Worker(MethodDic methods, MethodDefDic defMethods)
        {
            _methods = methods;
            _defMethods = defMethods;

            var targetMethods = new Dictionary<Type, MethodDic>();
            var targetAttributes = this.targetAttributes;

            foreach(var a in targetAttributes)
            {
                var type = a.attributeType;

                var dic = (
                    from m in methods
                    where m.Value.IsDefined(type)
                    select (a.keySelector(m.Value.GetCustomAttributes(type).ToArray(), m.Value), m.Value))
                        .ToDictionary(t => t.Item1, t => t.Item2);

                targetMethods.Add(a.attributeType, dic);
            }

            _targetMethods = targetMethods;
        }

        public abstract void Run();
        
    }
}
