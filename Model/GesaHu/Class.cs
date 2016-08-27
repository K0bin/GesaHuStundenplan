using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.GesaHu
{
    public class Class : IEquatable<Class>
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != null)
                    name = value.Trim().ToLower();
                else
                    name = string.Empty;
            }
        }

        private string teacher;
        public string Teacher
        {
            get { return teacher; }
            set
            {
                if (value != null)
                    teacher = value.Trim().ToLower();
                else
                    teacher = string.Empty;
            }
        }

        private string room = string.Empty;
        public string Room
        {
            get { return room; }
            set
            {
                if (value != null)
                    room = value.Trim();
                else
                    room = string.Empty;
            }
        }

        private string schoolClass;
        public string SchoolClass
        {
            get
            {
                return schoolClass;
            }
            set
            {
                if (value != null)
                    schoolClass = value.Trim().ToLower();
                else
                    schoolClass = string.Empty;
            }
        }

        private string tag;
        /// <summary>
        /// The tag found in the second table
        /// Used to assign to a lesson
        /// </summary>
        public string Tag
        {
            get { return tag; }
            set
            {
                if (value != null)
                    tag = value.Trim().ToLower();
                else
                    tag = string.Empty;
            }
        }

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

        public bool Equals(Class other)
        {
            if (other == null)
                return false;

            bool isParsed = true;
            int tagNumber = 0;
            int otherTagNumber = 0;
            if (Tag.Length <= 1 || int.TryParse(Tag.Substring(0, Tag.Length - 1), out tagNumber))
                isParsed = false;

            if (other.Tag.Length <= 1 || int.TryParse(other.Tag.Substring(0, other.Tag.Length - 1), out otherTagNumber))
                isParsed = false;

            return other != null && Name.Trim() == other.Name.Trim() && Room.Trim() == other.Room.Trim() && Teacher.Trim() == other.Teacher.Trim() && SchoolClass.Trim() == other.SchoolClass.Trim() && (isParsed && Math.Abs(tagNumber - otherTagNumber) <= 1);
        }
    }
}
