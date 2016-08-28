using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;
using StundenplanImport.Model.GesaHu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StundenplanImport.Model.Export
{
    public class ExporterICalendar
    {
        public async Task<string> ExportAsync(TimetableKind kind, string timetableName, ICollection<Lesson> lessons)
        {
            var fileName = timetableName + "_" + DateTime.Now.ToString("dd-M-yyyy-h-mm-ss") + ".icf";
            var vacations = await Vacation.LoadForYearAsync(Bundesland.Hessen, DateTime.Now.Year.ToString());
            var semesterDates = await Vacation.GetSemesterDatesAsync(Semester.First, vacations);
            var semesterStart = semesterDates.Item1;
            var semesterEnd = semesterDates.Item2;

            var calendar = new Calendar();

            for(int i = 0; i < vacations.Count -1; i++)
            {
                var yearPartStart = vacations[i].End.AddDays(3);
                var yearPartEnd = vacations[i+1].Begin.AddDays(-2);

                if (yearPartStart >= semesterEnd || yearPartStart < semesterStart)
                    continue;

                foreach (var lesson in lessons)
                {
                    var dateStart = new DateTime(yearPartStart.Year, yearPartStart.Month, yearPartStart.Day, Periods.Hours[lesson.Period], Periods.Minutes[lesson.Period], 0);
                    dateStart = dateStart.AddDays((int)lesson.Day - (int)DayOfWeek.Monday);
                    int week = dateStart.GetIso8601WeekOfYear();

                    if (Vacation.IsInVacation(dateStart, vacations))
                        continue;

                    if (lesson.Week == Week.Odd && week % 2 == 0 || lesson.Week == Week.Even && week % 2 == 1)
                        dateStart = dateStart.AddDays(7);

                    var dateEnd = dateStart.AddMinutes(Periods.Duration * lesson.Duration);
                    var rrule = new RecurrencePattern(FrequencyType.Weekly, lesson.Week == Week.Both ? 1 : 2);
                    rrule.Until = yearPartEnd;

                    var description = (kind == TimetableKind.Teacher ? "Kurs: " : "Lehrer: ") + lesson.TeacherOrSchoolClass;

                    var calEvent = new Event()
                    {
                        Summary = Names.ResolveSubject(lesson.Name),
                        Description = description,
                        Location = "Gesamtschule Hungen " + Names.ResolveRoom(lesson.Room),
                        DtStart = new CalDateTime(dateStart),
                        DtEnd = new CalDateTime(dateEnd),
                        RecurrenceRules = { rrule },
                    };

                    calendar.Events.Add(calEvent);
                }
            }

            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(calendar);

            string filePath = Path.Combine("ICalendar", fileName);
            File.WriteAllText(filePath, serializedCalendar);

            return filePath;
        }
    }
}
