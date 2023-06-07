using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace HymnLibrary.Models
{

    //https://products.aspose.app/cells/conversion/sql-to-json
    [Table("hymns")]
    public class Hymn
    {
        [PrimaryKey]
        public string _id { get; set; }
        public string author { get; set; }
        public string composer { get; set; }
        public string first_chorus_line { get; set; }
        public string first_stanza_line { get; set; }
        public string hymn_group { get; set; }
        public string key { get; set; }
        public string main_category { get; set; }
        public string meter { get; set; }

        public int no { get; set; }
        public string parent_hymn { get; set; }
        public string related { get; set; }
        public string sheet_music_link { get; set; }
        public string sub_category { get; set; }
        public string time { get; set; }
        public string tune { get; set; }
        public string verse { get; set; }
    }

    [Table("stanza")]
    public class Stanza
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string parent_hymn { get; set; }
        public string no { get; set; }
        public string text { get; set; }

        public int n_order { get; set; }
        public string note { get; set; }

        [JsonIgnore]
        [Ignore]
        public string OrderBy { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    [Table("hymns_stanza")]
    public class HymnStanza
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string _id { get; set; }
        public string author { get; set; }
        public string composer { get; set; }
        public string first_chorus_line { get; set; }
        public string first_stanza_line { get; set; }
        public string hymn_group { get; set; }

        public string key { get; set; }
        public string main_category { get; set; }
        public string meter { get; set; }
        public int n_order { get; set; }
        public string no { get; set; }

        [JsonProperty("no:1")]
        [Column("no:1")]
        public string no1 { get; set; }
        public string note { get; set; }
        public string parent_hymn { get; set; }

        [JsonProperty("parent_hymn:1")]
        [Column("parent_hymn:1")]
        public string parent_hymn1 { get; set; }
        public string related { get; set; }
        public string sheet_music_link { get; set; }
        public string sub_category { get; set; }
        public string text { get; set; }
        public string time { get; set; }
        public string tune { get; set; }
        public string verse { get; set; }
    }



    public class Tune
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string _id { get; set; }
        public string comment { get; set; }
        public string youtube_link { get; set; }
    }
    [Table("midi")]
    public class Midi
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string tune_id { get; set; }
        public byte[] tune { get; set; }
    }



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
