using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ksiazkaZDanymi
{
    internal class Person
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }

        static public Person CreateUser(int Id, string name, string surname, string phoneNumber, string mail, string dateOfBirth)
        {
            return new Person { ID = Id, Name = name, Surname = surname, PhoneNumber = phoneNumber, Email = mail, DateOfBirth = dateOfBirth };
        }

    }
}
