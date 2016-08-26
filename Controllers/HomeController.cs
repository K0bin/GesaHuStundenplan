using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StundenplanImport.Model;

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

        public async Task<IActionResult> Publish(string @class, string student, string teacher)
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
            var lessons = await timetable.LoadAsync();

            ViewData["Lessons"] = lessons;

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
