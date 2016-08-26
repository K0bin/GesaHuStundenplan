using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public class Course
    {
        public string Teacher
        { get; set; }

        public string Room
        { get; set; }

        public Course(string teacher, string room)
        {
            this.Teacher = teacher;
            this.Room = room;
        }
    }
}
