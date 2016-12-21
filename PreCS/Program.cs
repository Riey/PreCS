using CodeHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace PreCS
{
    class Program
    {
        static void Main(string[] args)
        {
            var asm = Assembly.Load(File.ReadAllBytes(args[0]));
            var module = asm.ManifestModule;

            var defAsm = AssemblyDefinition.ReadAssembly(args[0]);
            var defModule = defAsm.MainModule;

            var allTypes = defModule.GetTypes().Skip(1).ToArray();//<Module> Skip
            var allRefTypes = module.GetTypes();

            var defMethods = (
                from type in allTypes
                from method in type.Methods
                let fullName = GetFullName(method)
                where !method.IsConstructor
                select (fullName, method))
                .ToDictionary(t => t.Item1, t => t.Item2);

            var methods = (
                from type in allRefTypes
                from method in type.GetRuntimeMethods()
                let fullName = GetFullName(method)
                where defMethods.ContainsKey(fullName)
                select (fullName, method))
                .ToDictionary(t => t.Item1, t => t.Item2);


            Builder builder = new Builder(methods, defMethods);
            builder.Run();


            foreach(var m in methods)
            {
                var targetMethod = defMethods[m.Key];
                if (m.Value.IsDefined(typeof(TemporaryMethodAttribute)))
                {
                    targetMethod.DeclaringType.Methods.Remove(targetMethod);
                }
                else if (m.Value.IsDefined(typeof(TempAttrAttribute)))
                {
                    var tempAttributes = targetMethod.CustomAttributes
                        .Where(a => a.AttributeType.Resolve().IsSubclassof(typeof(TempAttrAttribute).FullName));

                    foreach (var t in tempAttributes)
                        targetMethod.CustomAttributes.Remove(t);

                }
            };

            defAsm.Write(args[0]);//Save modified assembly
        }

        internal static string GetFullName(MethodDefinition method) => method.DeclaringType.FullName + "." + method.Name;
        internal static string GetFullName(MethodInfo method) => method.DeclaringType.FullName + "." + method.Name;
    }
}
