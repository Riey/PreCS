using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    public static class Helper
    {
        private static Dictionary<string, object> _fields = new Dictionary<string, object>();

        public static void ClearField() => _fields.Clear();

        public static T LoadField<T>(string name)
        {
            return (T)_fields[name];
        }

        public static void SaveField<T>(string name, T value)
        {
            _fields.Add(name, value);
        }
    }
}
