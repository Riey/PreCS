using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    /// <summary>
    /// invoke member through this target(Don't use initial value in property!)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ThroughAttribute : TemporaryAttributeAttribute
    {
        public string Name { get; }

        public ThroughAttribute(string targetName)
        {
            Name = targetName;
        }
    }
}
