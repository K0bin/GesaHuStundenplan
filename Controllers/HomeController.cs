using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StundenplanImport.Model.GesaHu;
using StundenplanImport.Model;
using StundenplanImport.Model.ViewModel;

namespace StundenplanImport.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Classes"] = Names.Classes;
            ViewData["Students"] = Names.Students;
            ViewData["Teachers"] = Names.Teachers;

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
                lessonVMs.Add(new LessonViewModel(lesson.DayOfWeek, lesson.Number, lesson.Duration, lesson.Name, kind == TimetableKind.Teacher ? lesson.SchoolClass : lesson.Teacher, classVMs));
            }

            var viewModel = new EditViewModel(kind, lessonVMs);

            return View(viewModel);
        }

        public async Task<IActionResult> Publish(ICollection<Model.Lesson> lessons)
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
