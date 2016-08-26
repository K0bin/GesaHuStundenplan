using StundenplanImport.Model.GesaHu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.ViewModel
{
    public class ClassViewModel
    {
        public string Name
        { get; private set; }

        public string Teacher
        { get; private set; }
        public string Room
        { get; private set; }

        public string ShortName
        { get; private set; }

        public string ShortTeacher
        { get; private set; }

        public ClassViewModel(string name, string teacher, string room)
        {
            Name = Names.ResolveSubject(name);
            Teacher = Names.ResolveTeacher(teacher);
            Room = room;

            ShortName = name;
            ShortTeacher = teacher;
        }
    }
}
