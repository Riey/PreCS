using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreCS
{
    static class TypeDefinitionExtension
    {
        internal static bool IsSubclassof(this TypeDefinition type, string targetTypeFullName)
        {
            if (type.FullName == targetTypeFullName)
                return true;
            else
                return type.BaseType?.Resolve().IsSubclassof(targetTypeFullName) ?? false;
        }

        internal static bool IsSubclassof(this TypeDefinition type, Type refType) => type.IsSubclassof(refType.FullName);

        internal static IMemberDefinition GetMember(this TypeDefinition type, string name)
        {
            return
                (IMemberDefinition)type.Properties.Where(p => p.Name == name).FirstOrDefault() ??
                (IMemberDefinition)type.Fields.Where(f => f.Name == name).FirstOrDefault() ??
                (IMemberDefinition)type.Methods.Where(m => m.Name == name).FirstOrDefault();
        }
    }
}
