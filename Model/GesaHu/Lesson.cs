using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StundenplanImport.Model.GesaHu
{
    public class Lesson
    {
        /// <summary>
        /// Name thats found on the web timetable
        /// Might be wrong when theres multiple classes at the same spot (FR/LA)
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// Teacher thats found on the web timetable
        /// Might be wrong when theres multiple classes at the same spot (FR/LA; a,b,c Kurs)
        /// </summary>
        public string Teacher
        { get; set; }

        /// <summary>
        /// The nth lesson of the day
        /// </summary>
        public int Number
        { get; set; }

        public DayOfWeek DayOfWeek
        { get; set; }

        public int Duration
        { get; set; }

        /// <summary>
        /// The class (for teacher timetables)
        /// </summary>
        public string SchoolClass
        { get; set; }

        /// <summary>
        /// The tag found on the web timetable
        /// Used to assign a course from the second table
        /// </summary>
        public List<string> Tags
        { get; set; } = new List<string>();
        
        public Lesson(DayOfWeek dayOfWeek, int number, string name, int duration = 1)
        {
            DayOfWeek = dayOfWeek;
            Number = number;
            Name = name;
            Duration = duration;
        }

        public string ToGetString()
        {
            var builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(',');
            builder.Append(Teacher);
            builder.Append(',');
            builder.Append(Number);
            builder.Append(',');
            builder.Append((int)DayOfWeek);
            builder.Append(',');
            builder.Append(Duration);
            builder.Append(',');

            bool isFirstTag = true;
            foreach (var tag in Tags)
            {
                if (!isFirstTag)
                    builder.Append("-");

                builder.Append(tag);
                isFirstTag = false;
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            string tags = "";
            foreach(var tag in Tags)
            {
                tags += tag;
            }

            return string.Format("Lesson: Day={2}, Number={3}, Name={0}, Teacher={1}, Tag={4}", Name, Teacher, DayOfWeek, Number, tags);
        }

        public static Lesson FromGetString(string get)
        {
            var values = get.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return null;
        }
    }
}
