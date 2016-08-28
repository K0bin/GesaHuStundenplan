using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using StundenplanImport.Model;
using StundenplanImport.Model.Export;
using System;
using System.Collections.Generic;
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
            var webFile = file.Replace('\\', '/');

            ViewData["FilePath"] = webFile;

            return View("SuccessICalendar");
        }
    }
}
