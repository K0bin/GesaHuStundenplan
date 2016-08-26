using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.GesaHu
{
    public class Class
    {
        public string Name
        { get; set; }

        public string Teacher
        { get; set; }

        public string Room
        { get; set; }

        public string SchoolClass
        { get; set; }

        /// <summary>
        /// The tag found in the second table
        /// Used to assign to a lesson
        /// </summary>
        public string Tag
        { get; set; }

        public Class(string name, string teacher, string tag, string schoolClass)
        {
            Name = name;
            Teacher = teacher;
            Tag = tag;
            SchoolClass = schoolClass;
        }
    }
}
