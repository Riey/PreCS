using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace PreCS
{
    static class MethodSearcher
    {
        internal static MethodDefinition SearchMethod(TypeDefinition type, string methodName, IList<ParameterDefinition> parameters)
        {
            var methods = type.Methods.Where(m => m.Name == methodName);
            foreach(var method in methods)
            {
                if (method.Parameters.Count != parameters.Count)
                    continue;

                for(int i = 0; i < parameters.Count; i++)
                {
                    if(method.Parameters[i].ParameterType.FullName != parameters[i].ParameterType.FullName)
                    {
                        continue;
                    }
                }

                return method;
            }

            return null;
        }
    }
}
