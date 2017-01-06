using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    /// <summary>
    /// Value Builder
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class BuilderAttribute: TemporaryAttributeAttribute
    {
        public string Name { get; }
        /// <summary>
        /// </summary>
        /// <param name="name">Builder's Name</param>
        public BuilderAttribute(string name)
        {
            Name = name;
        }
    }
}
