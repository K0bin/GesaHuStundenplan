using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public class Lesson
   { 
        public DayOfWeek Day
        { get; set; }

        public int Number
        { get; set; }

        public int Duration
        { get; set; }

        public string Name
        { get; set; }

        public string TeacherOrSchoolClass
        { get; set; }

        public string Room
        { get; set; }
    }
}
