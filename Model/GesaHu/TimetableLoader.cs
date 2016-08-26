using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StundenplanImport.Model.GesaHu
{
    public class TimetableLoader
    {
        public enum Kind
        {
            Student,
            Teacher,
            Class
        }

        public enum Week
        {
            Even = 0,
            Odd = 1
        }

        public enum Semester
        {
            First = 0,
            Second = 1
        }

        private bool isOberstufe = false;

        private Uri uri;

        public TimetableLoader(Kind kind, string element, Week week = Week.Even, Semester semester = Semester.First)
        {
            int table = 4 + (int)week + (int)semester * 2;
            int index = 0;
            char kindChar = 'c';

            switch(kind)
            {
                case Kind.Class:
                    index = Array.IndexOf(Names.Classes, element);
                    kindChar = 'c';
                    break;

                case Kind.Student:
                    index = Array.IndexOf(Names.Students, element);
                    kindChar = 's';
                    isOberstufe = true;
                    break;

                case Kind.Teacher:
                    index = Array.IndexOf(Names.Teachers, element);
                    kindChar = 't';
                    break;
            }

            //Non 0-based
            index += 1;

            string url = String.Format("http://gesahui.de/schueler/lsimon1344/stundenplan/stupla_untis/0{0}/{1}/{1}{2}.htm",table, kindChar, index.ToString("D5"));
            uri = new Uri(url);
        }

        public async Task<Tuple<List<Lesson>, List<Class>>> LoadAsync()
        {
            var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(uri);
            var html = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);

            return Parse(html);
        }

        private Tuple<List<Lesson>, List<Class>> Parse(string html)
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
                        if (columnIndex == 0 || !column.HasAttributes || column.Attributes["colspan"]?.Value?.Trim() != "12" || string.IsNullOrWhiteSpace(column.InnerText))
                        {
                            columnIndex++;
                        }
                        else
                        {
                            var dayIndex = (columnIndex - 1);
                            var dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + dayIndex);
                            var number = rowIndex / 2;
                            Lesson previousLesson = null;
                            foreach (var lesson in lessons)
                            {
                                if (lesson.DayOfWeek == dayOfWeek && lesson.Number == number - 1 && lesson.Duration > 1)
                                    dayIndex++;

                                if(lesson.DayOfWeek == (DayOfWeek)((int)DayOfWeek.Monday + dayIndex) && (lesson.Number == number - 1 || (lesson.Number == number - 2 && lesson.Duration > 2)))
                                    previousLesson = lesson;
                            }
                            dayOfWeek = (DayOfWeek)((int)DayOfWeek.Monday + dayIndex);

                            var parsedLesson = ParseLesson(column, (DayOfWeek)((int)DayOfWeek.Monday + dayIndex), number, previousLesson);
                            if(parsedLesson != null)
                                lessons.Add(parsedLesson);

                            columnIndex++;
                        }
                    }
                }

                rowIndex++;
            }

            List<Class> classes = null;
            if (tables.Count() > 1)
            {
                var classesTable = tables.ElementAt(1);

                if(!isOberstufe)
                    classes = ParseClassesTable(classesTable);

            }

            return Tuple.Create(lessons, classes);
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
            if (previousLesson != null && name == previousLesson.Name)
            {
                previousLesson.Duration += duration;
                return null;
            }

            Lesson lesson = new Lesson(dayOfWeek, number, name, duration);
            lesson.Name = name;
            lesson.Teacher = teacher;
            
            if (!isOberstufe)
            {
                if(rows.Count() > 1)
                    lesson.Tag = rows.ElementAt(1).InnerText.Trim();
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

            var tag = string.Empty;
            foreach(var row in rows)
            {
                // Skip first (header) row
                if (rowIndex == 0)
                    continue;

                var cells = row.Elements("td");
                var cellIndex = 0;

                var teacher = string.Empty;
                var schoolClass = string.Empty;
                var name = string.Empty;
                var room = string.Empty;

                foreach(var cell in cells)
                {
                    if (string.IsNullOrWhiteSpace(cell.InnerText))
                        continue;

                    var text = cell.InnerText.Trim();

                    switch (cellIndex % 6)
                    {
                        case 0:
                            if(name != string.Empty)                            
                                classes.Add(new Class(name, teacher, tag, room));

                            //Tag
                            tag = text;
                            break;

                        case 1:
                            text = text.Replace(" ", "");
                            var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if(parts.Length >= 1)                            
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

                    cellIndex++;
                }

                rowIndex++;
            }

            return classes;
        }
    }
}
