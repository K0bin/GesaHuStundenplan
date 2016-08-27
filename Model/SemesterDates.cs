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
                        semesterStart = vacation.Begin;
                    else
                        semesterStart = vacation.End;
                }
                if (vacation.Title.StartsWith("Weihnachtsferien"))
                {
                    if (semester == Semester.First)
                        semesterEnd = vacation.Begin;
                    else
                        semesterStart = vacation.End;
                }
            }

            foreach (var vacation in currentYearVacations)
            {
                if (vacation.Title.StartsWith("Winterferien"))
                {
                    if (semester == Semester.First)
                        semesterEnd = vacation.Begin;
                    else
                        semesterStart = vacation.End;
                }
            }

            return Tuple.Create<DateTime,DateTime>(semesterStart, semesterEnd);
        }
    }
}
