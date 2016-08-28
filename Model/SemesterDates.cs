using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public static class SemesterDates
    {
        public static async Task<Tuple<DateTime, DateTime>> GetStartEndAsync(Semester semester, Bundesland bundesland)
        {            
            var currentYearVacations = await Vacation.LoadForYearAsync(bundesland, DateTime.Now.Year.ToString());
            DateTime semesterStart = new DateTime();
            DateTime semesterEnd = new DateTime();

            foreach (var vacation in currentYearVacations)
            {
                if (vacation.Title.StartsWith("Sommerferien"))
                {
                    if (semester == Semester.First)
                        semesterStart = vacation.End.AddDays(3);
                    else
                        semesterEnd = vacation.Begin.AddDays(-3);
                }
                if (vacation.Title.StartsWith("Weihnachtsferien"))
                {
                    if (semester == Semester.First)
                        semesterEnd = vacation.Begin.AddDays(-3);
                    else
                        semesterStart = vacation.End.AddDays(3);
                }
            }

            foreach (var vacation in currentYearVacations)
            {
                if (vacation.Title.StartsWith("Winterferien"))
                {
                    if (semester == Semester.First)
                        semesterEnd = vacation.Begin.AddDays(-3);
                    else
                        semesterStart = vacation.End.AddDays(3);
                }
            }

            return Tuple.Create<DateTime,DateTime>(semesterStart, semesterEnd);
        }
    }
}
