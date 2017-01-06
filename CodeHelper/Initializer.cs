using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    /// <summary>
    /// Builder Initializer
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class InitializerAttribute: TemporaryAttributeAttribute
    {
        public string TargetName { get; }
        /// <summary>
        /// </summary>
        /// <param name="targetName">Its must be same with Builder's Name</param>
        public InitializerAttribute(string targetName)
        {
            TargetName = targetName;
        }
    }
}
