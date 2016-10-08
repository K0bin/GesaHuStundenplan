using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StundenplanImport.Model.GesaHu;
using StundenplanImport.Model;
using StundenplanImport.Model.ViewModel;
using StundenplanImport.Model.Export;

namespace StundenplanImport.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var classes = (string[])Names.Classes.Clone();
            Array.Sort(classes);
            ViewData["Classes"] = classes;

            var students = (string[])Names.Students.Clone();
            Array.Sort(students);
            ViewData["Students"] = students;

            var teacherShorts = (string[])Names.Teachers.Clone();
            Array.Sort(teacherShorts);
            var teachers = new Tuple<string, string>[teacherShorts.Length];
            for(int i = 0; i < teacherShorts.Length; i++)
            {
                teachers[i] = Tuple.Create(teacherShorts[i], Names.ResolveTeacher(teacherShorts[i]));
            }
            ViewData["Teachers"] = teachers;

            return View();
        }

        public async Task<IActionResult> Edit(string @class, string student, string teacher)
        {
            TimetableKind kind = TimetableKind.Class;
            string element = "";

            if (!string.IsNullOrWhiteSpace(@class))
            {
                kind = TimetableKind.Class;
                element = @class;
            }
            else if (!string.IsNullOrWhiteSpace(teacher))
            {
                kind = TimetableKind.Teacher;
                element = teacher;
            }
            else if (!string.IsNullOrWhiteSpace(student))
            {
                kind = TimetableKind.Student;
                element = student;
            }

            TimetableLoader timetable = new TimetableLoader(kind, element);
            var lessons = await timetable.LoadAsync();

            var lessonVMs = new List<LessonViewModel>();
            foreach(var lesson in lessons)
            {
                var classVMs = new List<ClassViewModel>();
                foreach(var _class in lesson.Classes)
                {
                    classVMs.Add(new ClassViewModel(_class.Name, _class.Teacher, _class.Room));
                }
                lessonVMs.Add(new LessonViewModel(lesson.DayOfWeek, lesson.Period, lesson.Duration, lesson.Name, kind == TimetableKind.Teacher ? lesson.SchoolClass : lesson.Teacher, lesson.Room, lesson.Color, lesson.Week, classVMs));
            }

            var viewModel = new EditViewModel(kind, element, lessonVMs);

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
