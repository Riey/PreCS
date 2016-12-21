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
    }
}
