using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public enum TimetableKind
    {
        Student,
        Teacher,
        Class
    }

    public enum Week
    {
        Even = 0,
        Odd = 1
    }

    public enum Semester
    {
        First = 0,
        Second = 1
    }
}
