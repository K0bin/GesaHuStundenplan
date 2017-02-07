using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StundenplanImport.Model
{
    public class Vacation : IComparable<Vacation>
    {
        [JsonProperty]
           public string Title
        {
            get; private set;
        }
        
        [JsonProperty("beginn")]
        [JsonConverter(typeof(EpochConverter))]
        public DateTime Begin
        { get; private set; }

        [JsonProperty("ende")]
        [JsonConverter(typeof(EpochConverter))]
        public DateTime End
        { get; private set; }

        public Vacation(string title, DateTime begin, DateTime end)
        {
            Title = title;
            Begin = begin;
            End = end;
        }

        public int CompareTo(Vacation other)
        {
            if (other == null || other.Begin > Begin)
                return -1;
            else
                return 1;
        }

        public static readonly Dictionary<Bundesland, string> BundeslandShorts = new Dictionary<Bundesland, string>()
        {
            { Bundesland.BadenWuerttemberg, "bw" },
            { Bundesland.Bayern, "by" },
            { Bundesland.Berlin, "be" },
            { Bundesland.Brandenburg, "bb" },
            { Bundesland.Bremen, "hb" },
            { Bundesland.Hamburg, "hh" },
            { Bundesland.Hessen, "he" },
            { Bundesland.MecklenburgVorpommern, "mv" },
            { Bundesland.Niedersachsen, "ni" },
            { Bundesland.NordrheinWestfalen, "nrw" },
            { Bundesland.RheinlandPfalz, "rp" },
            { Bundesland.Saarland, "sl" },
            { Bundesland.Sachsen, "sn" },
            { Bundesland.SachsenAnhalt, "st" },
            { Bundesland.SchleswigHolstein, "sh" },
            { Bundesland.Thueringen, "th" },
        };

        public static async Task<List<Vacation>> LoadForYearAsync(Bundesland bundesland, string year)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(string.Format("http://api.smartnoob.de/ferien/v1/ferien/?bundesland={0}&jahr={1}", BundeslandShorts[bundesland], year));
            var json = await response.Content.ReadAsStringAsync();

            JObject vacations = JObject.Parse(json);
            var data = vacations["daten"].Children().ToList();

            var output = new List<Vacation>();
            foreach(var vacation in data)
            {
                output.Add(JsonConvert.DeserializeObject<Vacation>(vacation.ToString()));
            }
            output.Sort();

            return output;
        }

        public static async Task<Tuple<DateTime, DateTime>> GetSemesterDatesAsync(Semester semester, List<Vacation> vacations)
        {
            DateTime semesterStart = new DateTime();
            DateTime semesterEnd = new DateTime();
            var previousYearVacation = await LoadForYearAsync(Bundesland.Hessen, (DateTime.Now.Year + 1).ToString());
            var nextYearVacations = await LoadForYearAsync(Bundesland.Hessen, (DateTime.Now.Year+1).ToString());

            if (semester == Semester.Second)
            {
                foreach (var vacation in previousYearVacation)
                {
                    if (vacation.Title.StartsWith("Weihnachtsferien", StringComparison.InvariantCultureIgnoreCase))
                    {
                        semesterStart = vacation.End.AddDays(3);
                    }
                }
            }

            foreach (var vacation in vacations)
            {
                if (vacation.Title.StartsWith("Winterferien", StringComparison.InvariantCultureIgnoreCase) && semester == Semester.Second)
                {
                    semesterStart = vacation.Begin.AddDays(-3);
                }
                if (vacation.Title.StartsWith("Sommerferien", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (semester == Semester.First)
                        semesterStart = vacation.End.AddDays(3);
                    else
                        semesterEnd = vacation.Begin.AddDays(-3);
                }
                if (vacation.Title.StartsWith("Weihnachtsferien", StringComparison.InvariantCultureIgnoreCase) && semester == Semester.First)
                {
                    semesterEnd = vacation.Begin.AddDays(-3);
                }
            }

            if (semester == Semester.First)
            {
                foreach (var vacation in nextYearVacations)
                {
                    if (vacation.Title.StartsWith("Winterferien", StringComparison.InvariantCultureIgnoreCase))
                    {
                        semesterEnd = vacation.Begin.AddDays(-3);
                    }
                }
            }
            else
            {
                //HACKY
                semesterStart = new DateTime(2017, 2, 6);
            }

            return Tuple.Create<DateTime, DateTime>(semesterStart, semesterEnd);
        }

        public static bool IsInVacation(DateTime date, List<Vacation> vacations)
        {
            foreach(var vacation in vacations)
            {
                if (vacation.Begin >= date && vacation.End <= date)
                    return true;
            }
            return false;
        }
    }

    public enum Bundesland
    {
        SchleswigHolstein,
        MecklenburgVorpommern,
        Bremen,
        Niedersachsen,
        Brandenburg,
        Berlin,
        Hamburg,
        SachsenAnhalt,
        NordrheinWestfalen,
        Thueringen,
        Sachsen,                    
        Hessen,
        RheinlandPfalz,
        Saarland,
        BadenWuerttemberg,
        Bayern,
    }
}
