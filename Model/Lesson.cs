using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public class Lesson
    {
        public int Number
        { get; set; }
        public DayOfWeek DayOfWeek
        { get; set; }

        public string Name
        { get; set; }

        public Course Course
        { get; set; }

        public int Duration
        { get; set; }

        public string Tag
        { get; set; }

        public Lesson(DayOfWeek dayOfWeek, int number, string name, int duration = 1, Course course = null)
        {
            this.DayOfWeek = dayOfWeek;
            this.Name = name;
            this.Number = number;
            this.Duration = duration;
            this.Course = course;
        }
    }
}
