using StundenplanImport.Model.GesaHu;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.ViewModel
{
    public class LessonViewModel
    {
        public DayOfWeek Day
        { get; private set; }
        public int Period
        { get; private set; }

        public int Duration
        { get; private set; }

        public string Name
        { get; private set; }

        public string Room
        { get; private set; }

        public string TeacherOrSchoolClass
        { get; private set; }

        public ImmutableList<ClassViewModel> Classes
        { get; private set; }

        public string ShortName
        { get; private set; }

        public string Color
        { get; private set; }

        public string ShortTeacher
        { get; private set; }

        public string ShortRoom
        { get; private set; }

        public Week Week
        { get; private set; }

        public LessonViewModel(DayOfWeek day, int period, int duration, string name, string teacherOrSchoolClass, string room, string color, Week week = Week.Both, List<ClassViewModel> classes = null)
        {
            Day = day;
            Period = period;
            Duration = duration;
            Name = Names.ResolveSubject(name);
            Room = Names.ResolveRoom(room);
            TeacherOrSchoolClass = Names.ResolveTeacher(teacherOrSchoolClass);
            Color = color;
            Week = week;

            if (classes != null)
                Classes = classes.ToImmutableList();
            else
                Classes = new List<ClassViewModel>().ToImmutableList();
            
            ShortName = name;
            ShortTeacher = teacherOrSchoolClass;
            ShortRoom = room;
        }
    }
}
