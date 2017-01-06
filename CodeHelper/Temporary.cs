using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    /// <summary>
    /// TemporaryMember that delete when complete preprocess
    /// </summary>
    /// <remarks>
    /// When you use this property, you must make sure there are no other references
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TemporaryMemberAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute that delete when complete preprocess
    /// </summary>
    public abstract class TemporaryAttributeAttribute : Attribute
    {

    }
}
