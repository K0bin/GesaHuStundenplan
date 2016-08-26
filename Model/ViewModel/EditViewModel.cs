using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.ViewModel
{
    public class EditViewModel
    {
        public TimetableKind Kind
        { get; private set; }

        public ImmutableList<LessonViewModel> Lessons
        { get; private set; }

        public EditViewModel(TimetableKind kind, List<LessonViewModel> lessons)
        {
            Kind = kind;
            Lessons = lessons.ToImmutableList();
        }
    }
}
