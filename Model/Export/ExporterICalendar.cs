using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.Export
{
    public class ExporterICalendar
    {
        public async Task<string> ExportAsync(string timetableName, ICollection<Lesson> lessons)
        {
            var fileName = timetableName + "_" + DateTime.Now.ToString("dd-M-yyyy-h-mm-ss.icf");
            var dates = SemesterDates.GetStartEndAsync(Semester.First, Bundesland.Hessen);

            using (FileStream stream = new FileStream(fileName, FileMode.CreateNew))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("BEGIN:VCALENDAR");
                    writer.WriteLine("VERSION:2.0");
                }
            }

            return "";
        }
    }
}
