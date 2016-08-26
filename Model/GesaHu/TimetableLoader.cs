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

        private Uri uri;

        private bool hasSchoolClass = false;
        private string schoolYear;
        private char schoolClass;

        public TimetableLoader(TimetableKind kind, string element, Week week = Week.Even, Semester semester = Semester.First)
        {
            int table = 4 + (int)week + (int)semester * 2;
            int index = 0;
            char kindChar = 'c';
            this.kind = kind;

            switch (kind)
            {
                case TimetableKind.Class:
                    index = Array.IndexOf(Names.Classes, element);
                    kindChar = 'c';

                    var elementParts = element.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (elementParts.Length >= 1)
                    {
                        hasSchoolClass = true;
                        schoolClass = elementParts[0][elementParts[0].Length - 1];
                        schoolYear = elementParts[0].Substring(0, elementParts[0].Length);
                    }
                    break;

                case TimetableKind.Student:
                    index = Array.IndexOf(Names.Students, element);
                    kindChar = 's';
                    break;

                case TimetableKind.Teacher:
                    index = Array.IndexOf(Names.Teachers, element);
                    kindChar = 't';
                    break;
            }

            //Non 0-based
            index += 1;

            string url = String.Format("http://gesahui.de/schueler/lsimon1344/stundenplan/stupla_untis/0{0}/{1}/{1}{2}.htm",table, kindChar, index.ToString("D5"));
            uri = new Uri(url);
        }

        public async Task<List<Lesson>> LoadAsync()
        {
            var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(uri);
            var html = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);

            return Parse(html);
        }

        private List<Lesson> Parse(string html)
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
                        if (columnIndex != 0 && column.HasAttributes && column.Attributes["colspan"]?.Value?.Trim() == "12" && !string.IsNullOrWhiteSpace(column.InnerText))
                        {
                            var number = rowIndex / 2;
                            Lesson previousLesson = null;
                            var dayIndex = (columnIndex - 1);
                            var dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + dayIndex);
                            foreach (var lesson in lessons)
                            {
                                if (lesson.DayOfWeek == dayOfWeek && lesson.Number == number - 1 && lesson.Duration > 1)
                                {
                                    columnIndex++;
                                    dayIndex = (columnIndex - 1);
                                    dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + dayIndex);
                                }
                            }

                            // Find previous lesson to fix two lessons instead of a double
                            foreach (var lesson in lessons)
                            {
                                if (lesson.DayOfWeek == dayOfWeek && (lesson.Number == number - 1 || (lesson.Number == number - 2 && lesson.Duration > 2)))
                                    previousLesson = lesson;
                            }

                            var parsedLesson = ParseLesson(column, (DayOfWeek)((int)DayOfWeek.Monday + dayIndex), number, previousLesson);
                            if(parsedLesson != null)
                                lessons.Add(parsedLesson);
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
                        }
                        else
                            lesson.Classes = relevantClasses;
                    }
                }
            }

            return lessons;
        }

        private Lesson ParseLesson(HtmlNode td, DayOfWeek dayOfWeek, int number, Lesson previousLesson = null)
        {
            if (td == null)
                return null;

            var innerTable = td.Element("table");
            var rows = innerTable.Elements("tr");

            if (!rows.Any())
                return null;
            
            var rowColumns = rows.First().Elements("td");
            if (!rowColumns.Any())
                return null;

            var name = rowColumns.First().InnerText.Trim().Replace("*", "").Replace(".", "");
            var teacher = string.Empty;
            if(rowColumns.Count() > 1)
                teacher = rowColumns.ElementAt(1).InnerText.Trim();
            int duration = int.Parse(td.Attributes["rowspan"].Value) / 2;

            string tag = string.Empty;
            if (kind == TimetableKind.Class)
            {
                if (rows.Count() > 1)
                    tag = rows.ElementAt(1).InnerText.Trim();
            }

            if (previousLesson != null && name.ToLower() == previousLesson.Name.ToLower())
            {
                previousLesson.Duration += duration;
                if (!string.IsNullOrWhiteSpace(tag))
                    previousLesson.Tags.Add(tag);

                return null;
            }

            Lesson lesson = new Lesson(dayOfWeek, number, name, duration);
            lesson.Name = name;
            lesson.Teacher = teacher;

            if (!string.IsNullOrWhiteSpace(tag))
                lesson.Tags.Add(tag);

            if (kind == TimetableKind.Teacher)
            {
                if (rows.Count() > 1)
                    lesson.SchoolClass = rows.ElementAt(1).InnerText.Trim();
            }

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
            var schoolClass = string.Empty;
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

                foreach(var cell in cells)
                {
                    var text = WebUtility.HtmlDecode(cell.InnerText).Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        switch (cellIndex % 6)
                        {
                            case 0:
                                if (name != string.Empty)
                                {
                                    var _class = new Class(name, teacher, cellIndex > 5 ? leftTag : rightTag, schoolClass);

                                    if (!string.IsNullOrWhiteSpace(room))
                                        _class.Room = room;

                                    if(!classes.Contains(_class))
                                        classes.Add(_class);

                                    teacher = string.Empty;
                                    schoolClass = string.Empty;
                                    name = string.Empty;
                                    room = string.Empty;
                                }

                                //Tag
                                if (cellIndex > 5)
                                    rightTag = text;
                                else
                                    leftTag = text;
                                break;

                            case 1:
                                if (name != string.Empty)
                                {
                                    var _class = new Class(name, teacher, cellIndex > 5 ? leftTag : rightTag, schoolClass);

                                    if (!string.IsNullOrWhiteSpace(room))
                                        _class.Room = room;

                                    if (!classes.Contains(_class))
                                        classes.Add(_class);

                                    teacher = string.Empty;
                                    schoolClass = string.Empty;
                                    name = string.Empty;
                                    room = string.Empty;
                                }

                                text = text.Replace(" ", "");
                                var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 1)
                                    teacher = parts[0].Trim();
                                if (parts.Length >= 2)
                                    name = parts[1].Trim();
                                if (parts.Length >= 3)
                                    room = parts[2].Trim();
                                break;

                            case 2:
                                schoolClass = text;
                                break;
                        }
                    }

                    cellIndex++;
                }

                rowIndex++;
            }

            return classes;
        }
    }
}
