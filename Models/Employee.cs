using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFunction.Models
{
    public class Employee
    { 
        public string Name { get; set; } = null!;

        public string Occupation { get; set; } = null!;


        public Employee(string name, string occupation)
        {
            Name = name;
            Occupation = occupation;
        }

        public Employee()
        {
            
        }
    }
}
