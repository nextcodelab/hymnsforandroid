using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace HymnLibrary.Models
{
    [Table("hymn_tune")]
    public class HymnTune
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; } = 0;
        [JsonProperty("hymnid")]
        public string unique_id { get; set; }
        [JsonProperty("link")]
        public string youtube_link { get; set; }

    }
}
