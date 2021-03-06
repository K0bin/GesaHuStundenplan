﻿using Ical.Net;
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
            var fileName = timetableName + "_" + DateTime.Now.ToString("dd-M-yyyy-h-mm-ss");
            var vacations = await Vacation.LoadForYearAsync(Bundesland.Hessen, DateTime.Now.Year.ToString());
            vacations.Insert(vacations.FindIndex(v => v.Title == "Osterferien 2017 Hessen"), new Vacation("HACKY", new DateTime(2017, 1, 1), new DateTime(2017, 2, 3)));
            var semesterDates = await Vacation.GetSemesterDatesAsync(Semester.Second, vacations);
            var semesterStart = semesterDates.Item1;
            var semesterEnd = semesterDates.Item2;

            var calendar = new Calendar();

            for(int i = 0; i < vacations.Count - 1; i++)
            {
                var yearPartStart = vacations[i].End.AddDays(3);
                var yearPartEnd = vacations[i+1].Begin;

                // Workaround for inconsistent vacation data by api, ensure start is monday
                if (yearPartStart.DayOfWeek != DayOfWeek.Monday && yearPartStart.DayOfWeek != DayOfWeek.Saturday && yearPartStart.DayOfWeek != DayOfWeek.Sunday)
                    yearPartStart = yearPartStart.AddDays((int)DayOfWeek.Monday - (int)yearPartStart.DayOfWeek);
                else if (yearPartStart.DayOfWeek == DayOfWeek.Saturday)
                    yearPartStart = yearPartStart.AddDays(2);
                else if (yearPartStart.DayOfWeek == DayOfWeek.Sunday)
                    yearPartStart = yearPartStart.AddDays(1);

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

                    var dateEnd = dateStart.AddMinutes(Periods.Duration * lesson.Duration + (lesson.Period != 8 ? lesson.Duration / 2 * 5 : 0));
                    var rrule = new RecurrencePattern(FrequencyType.Weekly, lesson.Week == Week.Both ? 1 : 2);
                    rrule.Until = yearPartEnd;

                    var description = (kind == TimetableKind.Teacher ? "Kurs: " : "Lehrer: ") + lesson.TeacherOrSchoolClass;
                    var room = Names.ResolveRoom(lesson.Room);

                    var calEvent = new Event()
                    {
                        Summary = Names.ResolveSubject(lesson.Name),
                        Description = description,
                        Location = "Gesamtschule Hungen" + (!string.IsNullOrWhiteSpace(room) ? ": " + Names.ResolveRoom(lesson.Room) : string.Empty),
                        DtStart = new CalDateTime(dateStart),
                        DtEnd = new CalDateTime(dateEnd),
                        RecurrenceRules = { rrule },
                    };

                    calendar.Events.Add(calEvent);
                }
            }

            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(calendar);

            string filePath = Path.Combine("Static", "ICalendar", fileName + ".ics");
            File.WriteAllText(filePath, serializedCalendar);

            return fileName;
        }
    }
}
