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
        { get; set; } = string.Empty;

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

        public override string ToString()
        {
            return string.Format("Class: Name={0}, Teacher={1}, Room={2}, SchoolClass={3}, Tag={4}", Name, Teacher, Room, SchoolClass, Tag);
        }

        public override bool Equals(object obj)
        {
            if (obj is Class)
                return Equals((Class)obj);

            return base.Equals(obj);
        }

        private bool Equals(Class other)
        {
            if (other == null)
                return false;

            return Name == other.Name && Room == other.Room && Teacher == other.Teacher && SchoolClass == other.SchoolClass && Tag == other.Tag;
        }
    }
}
