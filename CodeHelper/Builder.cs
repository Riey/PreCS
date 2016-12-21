using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class BuilderAttribute: TempAttrAttribute
    {
        public string Name { get; }
        public BuilderAttribute(string name)
        {
            Name = name;
        }
    }
}
