using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    public enum TargetType
    {
        Property,
        Field,
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ThroughAttribute : TemporaryAttributeAttribute
    {
        public TargetType TargetType { get; }
        public string Name { get; }

        public ThroughAttribute(TargetType targetType, string targetName)
        {
            TargetType = targetType;
            Name = targetName;
        }
    }
}
