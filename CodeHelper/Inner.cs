using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InnerAttribute : Attribute
    {
        public string FieldName { get; }
        public InnerAttribute(string targetFieldName)
        {
            FieldName = targetFieldName;
        }
    }
}
