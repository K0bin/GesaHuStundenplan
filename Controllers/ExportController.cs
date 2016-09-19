using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using StundenplanImport.Model;
using StundenplanImport.Model.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Controllers
{
    public class ExportController : Controller
    {
        /*
        public async Task<IActionResult> GoogleCalendar(TimetableKind kind, string timetableName, ICollection<Model.Lesson> lessons)
        {
            return View("Success");
        }
        */

        public async Task<IActionResult> ICalendar(TimetableKind kind, string timetableName, ICollection<Model.Lesson> lessons)
        {
            var exporter = new ExporterICalendar();
            
            var file = await exporter.ExportAsync(kind, timetableName, lessons);
            ViewData["File"] = file;

            return View("SuccessICalendar");
        }

        public FileResult Download(string fileName)
        {
            var path = Path.Combine("Static", "ICalendar", fileName + ".ics");
            
            var bytes = System.IO.File.ReadAllBytes(path);
            System.IO.File.Delete(path);
                
            return File(bytes, "text/calendar", "Stundenplan.ics");
        }
    }
}
