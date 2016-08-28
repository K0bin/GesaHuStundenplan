using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StundenplanImport.Model.GesaHu
{
    public class TimetableLoader
    {
        private TimetableKind kind;

        private int timetableIndex = 0;
        private char kindChar = 'c';

        private bool hasSchoolClass = false;
        private string schoolYear;
        private char schoolClass;

        public TimetableLoader(TimetableKind kind, string element)
        {
            this.kind = kind;

            switch (kind)
            {
                case TimetableKind.Class:
                    timetableIndex = Array.IndexOf(Names.Classes, element);
                    kindChar = 'c';

                    var elementParts = element.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (elementParts.Length >= 1)
                    {
                        hasSchoolClass = true;
                        schoolClass = elementParts[0][elementParts[0].Length - 1];
                        schoolYear = elementParts[0].Substring(0, elementParts[0].Length - 1);
                    }
                    break;

                case TimetableKind.Student:
                    timetableIndex = Array.IndexOf(Names.Students, element);
                    kindChar = 's';
                    break;

                case TimetableKind.Teacher:
                    timetableIndex = Array.IndexOf(Names.Teachers, element);
                    kindChar = 't';
                    break;
            }

            //Non 0-based
            timetableIndex += 1;
        }

        private Uri BuildUri(Week week = Week.Even)
        {
            if(week == Week.Both)
                throw new Exception("Can't build the uri for both weeks.");

            int table = 35 + (int)week;
            string url = String.Format("http://gesahui.de/schueler/lsimon1344/stundenplan/stupla_untis/{0}/{1}/{1}{2}.htm", table, kindChar, timetableIndex.ToString("D5"));
            return new Uri(url);
        }

        public async Task<List<Lesson>> LoadAsync()
        {
            var evenClient = new HttpClient();
            var evenUri = BuildUri(Week.Even);
            byte[] evenBytes = new byte[0];
            try
            {
                evenBytes = await evenClient.GetByteArrayAsync(evenUri);
            }
            catch(HttpRequestException e)
            {
                throw new HttpRequestException("Can't access the timetable for even weeks. (Uri: )"+ evenUri, e);
            }
            var evenHtml = Encoding.GetEncoding("ISO-8859-1").GetString(evenBytes);

            var evenLessons = Parse(evenHtml, Week.Even);

            var oddClient = new HttpClient();
            var oddUri = BuildUri(Week.Odd);
            byte[] oddBytes = new byte[0];
            try
            {
                oddBytes = await oddClient.GetByteArrayAsync(oddUri);
            }
            catch(HttpRequestException e)
            {
                throw new HttpRequestException("Can't access the timetable for odd weeks. (Uri: )" + oddUri, e);
            }
            var oddHtml = Encoding.GetEncoding("ISO-8859-1").GetString(oddBytes);

            var oddLessons = Parse(oddHtml, Week.Odd);

            var lessons = new List<Lesson>();
            foreach(var evenLesson in evenLessons)
            {
                lessons.Add(evenLesson);
            }
            foreach(var oddLesson in oddLessons)
            {
                Lesson existingLesson = null;
                foreach (var evenLesson in evenLessons)
                {
                    if (evenLesson.Equals(oddLesson))
                        existingLesson = evenLesson;
                }

                if (existingLesson == null)
                    lessons.Add(oddLesson);
                else
                    existingLesson.Week = Week.Both;
            }

            return lessons;
        }

        private List<Lesson> Parse(string html, Week week)
        {
            var lessons = new List<Lesson>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var body = doc.DocumentNode.Element("html").Element("body");
            var tables = body.Element("center").Elements("table");
            var table = tables.ElementAt(0);

            var rows = table.Elements("tr");

            int rowIndex = 0;
            foreach (var row in rows)
            {
                if (rowIndex % 2 == 1)
                {
                    int columnIndex = 0;
                    var columns = row.Elements("td");
                    foreach (var column in columns)
                    {
                        if(columnIndex == 0)
                        {
                            columnIndex++;
                            continue;
                        }

                        var period = rowIndex / 2;
                        Lesson previousLesson = null;
                        var dayIndex = (columnIndex - 1);
                        var dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + dayIndex);

                        foreach (var lesson in lessons)
                        {
                            if (lesson.DayOfWeek == dayOfWeek && lesson.Period == period - 1 && lesson.Duration > 1)
                            {
                                columnIndex++;
                                dayIndex = (columnIndex - 1);
                                dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + dayIndex);
                            }
                        }

                        if (column.HasAttributes && column.Attributes["colspan"]?.Value?.Trim() == "12" && !string.IsNullOrWhiteSpace(column.InnerText))
                        {
                            // Find previous lesson to fix two lessons instead of a double
                            foreach (var lesson in lessons)
                            {
                                if (lesson.DayOfWeek == dayOfWeek && (lesson.Period == period - 1 || (lesson.Period == period - 2 && lesson.Duration > 2)))
                                    previousLesson = lesson;
                            }

                            var parsedLesson = ParseLesson(column, (DayOfWeek)((int)DayOfWeek.Monday + dayIndex), period, previousLesson);
                            if (parsedLesson != null)
                            {
                                parsedLesson.Week = week;
                                lessons.Add(parsedLesson);
                            }
                        }
                        columnIndex++;
                    }
                }

                rowIndex++;
            }

            List<Class> classes = null;
            if (tables.Count() > 1)
            {
                var classesTable = tables.ElementAt(1);

                if (kind == TimetableKind.Class)
                {
                    classes = ParseClassesTable(classesTable);

                    foreach (var lesson in lessons)
                    {
                        List<Class> relevantClasses = new List<Class>();
                        if (lesson.Tags.Count > 0 && classes != null)
                        {
                            foreach (var _class in classes)
                            {
                                bool isForClass = true;
                                if (kind == TimetableKind.Class && hasSchoolClass && !string.IsNullOrWhiteSpace(_class.SchoolClass))
                                    isForClass = _class.SchoolClass.Contains(schoolClass) && _class.SchoolClass.Contains(schoolYear);

                                if (lesson.Tags.Contains(_class.Tag) && isForClass && !relevantClasses.Contains(_class))
                                    relevantClasses.Add(_class);
                            }
                        }

                        if(relevantClasses.Count == 1)
                        {
                            lesson.Name = relevantClasses[0].Name;
                            lesson.Teacher = relevantClasses[0].Teacher;
                            lesson.Room = relevantClasses[0].Room;
                        }
                        else
                            lesson.Classes = relevantClasses;
                    }
                }
            }

            return lessons;
        }

        private Lesson ParseLesson(HtmlNode td, DayOfWeek dayOfWeek, int period, Lesson previousLesson = null)
        {
            if (td == null)
                return null;

            string color = string.Empty;
            if (td.Attributes != null && td.Attributes.Contains("bgColor") && !string.IsNullOrWhiteSpace(td.Attributes["bgColor"]?.Value))
                color = td.Attributes["bgColor"].Value;

            var innerTable = td.Element("table");
            var rows = innerTable.Elements("tr");

            if (!rows.Any())
                return null;
            
            int rowsCount = rows.Count();

            string name = string.Empty;
            string teacher = string.Empty;
            string _schoolClass = string.Empty;
            string tag = string.Empty;
            string room = string.Empty;

            if (kind == TimetableKind.Student)
            {
                name = rows.First().InnerText.Trim().Replace("*", "").Replace(".", "");
                if (rowsCount > 1)
                    teacher = rows.ElementAt(1).InnerText.Trim();
                if (rowsCount > 2)
                    room = rows.ElementAt(2).InnerText.Trim();
            }
            else
            {
                var firstRowColumns = rows.First().Elements("td");
                if (!firstRowColumns.Any())
                    return null;
                int firstRowColumnsCount = firstRowColumns.Count();

                if (kind == TimetableKind.Class)
                {
                    name = firstRowColumns.First().InnerText.Trim();
                    if (firstRowColumnsCount > 1)
                        teacher = firstRowColumns.ElementAt(1).InnerText.Trim();

                    // Parse tag to match info from the second table
                    if (rows.Count() > 1)
                    {
                        var columns = rows.ElementAt(1).Elements("td");
                        if (columns.Count() > 1)
                            tag = columns.ElementAt(1).InnerText.Trim();
                        else if (columns.Any())
                            tag = columns.ElementAt(0).InnerText.Trim();
                    }
                }
                else
                {
                    _schoolClass = firstRowColumns.First().InnerText.Trim();
                    if (firstRowColumnsCount > 1)
                        name = firstRowColumns.ElementAt(1).InnerText.Trim();
                }

            }
            
            int duration = int.Parse(td.Attributes["rowspan"].Value) / 2;

            // Fix two 45min lessons instead of one 90min
            if (previousLesson != null && name.ToLower() == previousLesson.Name.ToLower())
            {
                previousLesson.Duration += duration;
                if (!string.IsNullOrWhiteSpace(tag))
                    previousLesson.Tags.Add(tag);

                return null;
            }

            Lesson lesson = new Lesson(dayOfWeek, period, name, duration);
            lesson.Name = name;
            lesson.Teacher = teacher;
            lesson.Room = room;
            lesson.Color = color;
            if (kind == TimetableKind.Teacher)
                lesson.SchoolClass = _schoolClass;

            if (!string.IsNullOrWhiteSpace(tag))
                lesson.Tags.Add(tag);            

            return lesson;
        }

        private List<Class> ParseClassesTable(HtmlNode table)
        {
            if (table == null)
                return null;

            var classes = new List<Class>();

            var rows = table.Elements("tr");
            var rowIndex = 0;

            var leftTag = string.Empty;
            var rightTag = string.Empty;
            var teacher = string.Empty;
            var _schoolClass = string.Empty;
            var name = string.Empty;
            var room = string.Empty;

            foreach (var row in rows)
            {
                // Skip first (header) row
                if (rowIndex == 0)
                {
                    rowIndex++;
                    continue;
                }

                var cells = row.Elements("td");
                var cellIndex = 0;

                foreach (var cell in cells)
                {
                    var text = WebUtility.HtmlDecode(cell.InnerText).Trim();
                    switch (cellIndex % 6)
                    {
                        case 0:
                            if (name != string.Empty)
                            {
                                var _class = new Class(name, teacher, cellIndex > 5 ? leftTag : rightTag, _schoolClass);

                                if (!string.IsNullOrWhiteSpace(room))
                                    _class.Room = room;

                                if (!classes.Contains(_class))
                                    classes.Add(_class);

                                teacher = string.Empty;
                                _schoolClass = string.Empty;
                                name = string.Empty;
                                room = string.Empty;
                            }

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                //Tag
                                if (cellIndex > 5)
                                    rightTag = text;
                                else
                                    leftTag = text;
                            }
                            break;

                        case 1:
                            if (name != string.Empty)
                            {
                                var _class = new Class(name, teacher, cellIndex > 5 ? leftTag : rightTag, _schoolClass);

                                if (!string.IsNullOrWhiteSpace(room))
                                    _class.Room = room;

                                if (!classes.Contains(_class))
                                    classes.Add(_class);

                                teacher = string.Empty;
                                _schoolClass = string.Empty;
                                name = string.Empty;
                                room = string.Empty;
                            }

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                text = text.Replace(" ", "");
                                var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 1)
                                    teacher = parts[0].Trim();
                                if (parts.Length >= 2)
                                    name = parts[1].Trim();
                                if (parts.Length >= 3)
                                    room = parts[2].Trim();
                            }
                            break;

                        case 2:
                            if (!string.IsNullOrWhiteSpace(text))
                                _schoolClass = text;
                            break;
                    }

                    cellIndex++;
                }

                rowIndex++;
            }

            return classes;
        }
    }
}
