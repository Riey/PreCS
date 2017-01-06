using CodeHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
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

            StackAnalyser.ExternModule = module;

            var defAsm = AssemblyDefinition.ReadAssembly(new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var defModule = defAsm.MainModule;

            var allDefTypes = defModule.GetTypes().Skip(1).ToArray();//<Module> Skip
            var allTypes = module.GetTypes();


            var throughWorker = new Workers.ThroughWorker(allTypes, allDefTypes);
            throughWorker.Run();

            var builderWorker = new Workers.BuilderWorker(allTypes, allDefTypes);
            builderWorker.Run();


            foreach (var t in allDefTypes)
            {
                DeleteTemporaryMembers(t);
                DeleteTempAttributes(t);
            }

            defAsm.Write(new FileStream(args[0], FileMode.Open, FileAccess.Write, FileShare.ReadWrite));//Save modified assembly
        }

        static void DeleteTemporaryMembers(TypeDefinition type)
        {
            DeleteTempoaryMembers(type.Fields);
            DeleteTempoaryMembers(type.Methods);
            DeleteTempoaryMembers(type.Properties);
        }

        static void DeleteTempAttributes(TypeDefinition type)
        {
            DeleteTempAttribute(type);
            DeleteTempAttributes(type.Fields);
            DeleteTempAttributes(type.Properties);
            DeleteTempAttributes(type.Events);
            DeleteTempAttributes(type.GenericParameters);
            foreach (var m in type.Methods)
            {
                DeleteTempAttribute(m);
                DeleteTempAttribute(m.MethodReturnType);
                DeleteTempAttributes(m.Parameters);
                DeleteTempAttributes(m.GenericParameters);
            }
        }

        static bool IsTempoaryMember(IMemberDefinition m) => m.CustomAttributes.Where(a => a.AttributeType.FullName == typeof(TemporaryMemberAttribute).FullName).Any();

        static void DeleteTempoaryMembers<T>(Collection<T> c) where T : IMemberDefinition
        {
            var t = new Queue<T>();
            foreach (var m in c)
                if (IsTempoaryMember(m))
                    t.Enqueue(m);
            while (t.Count > 0)
                c.Remove(t.Dequeue());
        }

        static void DeleteTempAttributes<T>(Collection<T> e) where T : Mono.Cecil.ICustomAttributeProvider
        {
            foreach (var m in e) DeleteTempAttribute(m);
        }

        static void DeleteTempAttribute<T>(T c) where T : Mono.Cecil.ICustomAttributeProvider
        {
            try
            {
                int count = c.CustomAttributes.Count;

                for (int i = 0; i < count; i++)
                {
                    if (c.CustomAttributes[i].AttributeType.Resolve().IsSubclassof(typeof(TemporaryAttributeAttribute).FullName))
                    {
                        c.CustomAttributes.RemoveAt(i);
                        i -= 1;
                        count -= 1;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
        internal static string GetFullName(MethodDefinition method) => method.DeclaringType.FullName + "." + method.Name;
        internal static string GetFullName(MethodInfo method) => method.DeclaringType.FullName + "." + method.Name;

        internal static string GetFullName(IMemberDefinition member)
        {
            if (member is TypeDefinition type) return type.FullName;
            else return member.DeclaringType.FullName + "." + member.Name;
        }

        internal static string GetFullName(MemberInfo member)
        {
            switch (member)
            {
                case TypeInfo type: return type.FullName;
                default: return member.DeclaringType.FullName + "." + member.Name;
            }
        }
    }
}
