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
    public class Vacation
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
            return output;
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
