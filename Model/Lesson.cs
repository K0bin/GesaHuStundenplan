using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public class Lesson
   {
        public Week Week
        { get; set; } = Week.Both;

        [Required]
        public DayOfWeek Day
        { get; set; }

        [Required]
        public int Number
        { get; set; }

        public int Duration
        { get; set; } = 1;

        [Required]
        public string Name
        { get; set; }

        public string TeacherOrSchoolClass
        { get; set; }

        public string Room
        { get; set; }

        public override string ToString()
        {
            return string.Format("Lesson: Day={2}, Number={3}, Duration={4}, Name={0}, Teacher={1}, Room={5}", Name, TeacherOrSchoolClass, Day, Number, Duration, Room);
        }
    }
}
