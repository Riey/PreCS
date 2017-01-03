using CodeHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Gun
    {
        public int Boolet { get; }
        public void Shoot(Person person)
        {

        }
    }
    class Person
    {
        Gun gun;
        public bool HasGun => gun != null;

        [Inner("gun")]
        public void Shoot(Person person) { }

        [Inner("gun")]
        public int Boolet { get; }
    }
}
