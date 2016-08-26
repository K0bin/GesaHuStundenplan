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
        public int Number
        { get; private set; }

        public int Duration
        { get; private set; }

        public string Name
        { get; private set; }

        public string TeacherOrSchoolClass
        { get; private set; }

        public ImmutableList<ClassViewModel> Classes
        { get; private set; }

        public string ShortName
        { get; private set; }

        public string ShortTeacher
        { get; private set; }

        public LessonViewModel(DayOfWeek day, int number, int duration, string name, string teacherOrSchoolClass, List<ClassViewModel> classes = null)
        {
            Day = day;
            Number = number;
            Duration = duration;
            Name = Names.ResolveSubject(name);
            TeacherOrSchoolClass = Names.ResolveTeacher(teacherOrSchoolClass);

            if (classes != null)
                Classes = classes.ToImmutableList();
            else
                Classes = new List<ClassViewModel>().ToImmutableList();
            
            ShortName = name;
            ShortTeacher = teacherOrSchoolClass;
        }
    }
}
