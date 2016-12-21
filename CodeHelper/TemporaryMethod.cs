using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TemporaryMethodAttribute : Attribute
    {
    }

    public abstract class TempAttrAttribute : Attribute
    {

    }
}
