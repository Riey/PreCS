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
        public int Boolet { get; set; }
        public void Shoot(Person person)
        {
            Console.WriteLine("Shoot!");
        }
    }
    class Person
    {
        Gun gun;
        public bool HasGun => gun != null;

        [Through("gun")]
        public void Shoot(Person person) { }

        [Through("gun")]
        public int Boolet { get; set; }
    }
}
