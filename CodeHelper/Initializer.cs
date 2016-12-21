using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class InitializerAttribute: TempAttrAttribute
    {
        public string TargetName { get; }
        public InitializerAttribute(string targetName)
        {
            TargetName = targetName;
        }
    }
}
