using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StundenplanImport.Model.GesaHu;

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
            TimetableLoader.Kind kind = TimetableLoader.Kind.Class;
            string element = "";

            if (!string.IsNullOrWhiteSpace(@class))
            {
                kind = TimetableLoader.Kind.Class;
                element = @class;
            }
            else if (!string.IsNullOrWhiteSpace(teacher))
            {
                kind = TimetableLoader.Kind.Teacher;
                element = teacher;
            }
            else if (!string.IsNullOrWhiteSpace(student))
            {
                kind = TimetableLoader.Kind.Student;
                element = student;
            }

            TimetableLoader timetable = new TimetableLoader(kind, element);
            var data = await timetable.LoadAsync();
            var lessons = data.Item1;
            var classes = data.Item2;

            ViewData["Lessons"] = lessons;
            ViewData["Classes"] = classes;

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
