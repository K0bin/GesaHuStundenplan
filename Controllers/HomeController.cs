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
            if (kind == TimetableLoader.Kind.Class)
            {
                var elementParts = element.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (elementParts.Length >= 1)
                {
                    var schoolClass = elementParts[0][elementParts[0].Length - 1];
                    var schoolYear = elementParts[0].Substring(0, elementParts[0].Length);
                    ViewData["SchoolClass"] = schoolClass;
                    ViewData["SchoolYear"] = schoolYear;
                }
            }

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
