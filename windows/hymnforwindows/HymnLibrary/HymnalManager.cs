
using HymnLibrary;
using HymnLibrary.Helpers;
using HymnLibrary.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HymnLibrary
{
    public class HymnalManager
    {
        public static string SQLiteFilePath { get; private set; } = Path.Combine(DatabaseDirectory.Dir, "hymns.sqlite");
        static bool processing = false;
        public static async void InitData()
        {
            if (processing)
                return;
            processing = true;
            if (File.Exists(SQLiteFilePath) == false)
            {
                var input = DatabaseDirectory.GetEmbeddedResourceStreamAsync("HymnLibrary.Data.hymns.sqlite", typeof(HymnalManager));
                using (Stream file = File.Create(SQLiteFilePath))
                {
                    await input.CopyToAsync(file);
                }
            }
            processing = false;
        }
       
        public static SQLiteConnection GetHymnalConnection()
        {
            return new SQLite.SQLiteConnection(SQLiteFilePath);
        }
        public static async Task<List<object>> GetQuerySearchAsync(string query, int limit = 40)
        {
            List<object> listItems = new List<object>();
            List<Hymn> list = new List<Hymn>();
            if (string.IsNullOrEmpty(query))
                return listItems;
            while (processing)
            {
                await Task.Delay(500);
            }

            query = query.ToLower();
            using (var session = new SQLite.SQLiteConnection(SQLiteFilePath))
            {
                session.CreateTable<Hymn>();
                session.GetMapping<Hymn>();
                try
                {
                    list = session.Table<Hymn>().Where(p => p.no.ToLower().Contains(query)
                | p._id.ToLower().Contains(query)
                | p.hymn_group.ToLower().Contains(query)
                | p.first_stanza_line.ToLower().Contains(query)
                | p.sub_category.ToLower().Contains(query)
                | p.first_stanza_line.ToLower().Contains(query)
                | p.main_category.ToLower().Contains(query)).Take(limit).ToList();

                }
                catch { }
                if (list.Count == 0)
                {
                    var stanzas = session.Table<Stanza>().Where(p => p.text.ToLower().Contains(query)).ToList();
                    stanzas = stanzas.DistinctBy(p => p.parent_hymn).Take(limit).ToList();
                    foreach (var item in stanzas)
                    {
                        var hymn = session.Table<Hymn>().Where(p => p._id == item.parent_hymn).FirstOrDefault();
                        list.Add(hymn);
                    }
                }
                listItems.AddRange(list);
            }
           
            return listItems;
        }
        public static Hymn GetHymnById(string hymnalId)
        {
            using (var db = GetHymnalConnection())
            {
                return db.Table<Hymn>().Where(p => p._id == hymnalId).FirstOrDefault();
            }
        }
        public static List<Hymn> GetHymnsByNumber(string no)
        {
            using (var db = GetHymnalConnection())
            {
                return db.Table<Hymn>().Where(p => p.no == no).ToList();
            }
        }
        public static Hymn GetFirstHymn()
        {
            using (var db = GetHymnalConnection())
            {
                return db.Table<Hymn>().Where(p => p._id == "E789").FirstOrDefault();
            }
        }
        public static List<Stanza> GetHymnStanzaById(string hymnalId)
        {
            using (var db = GetHymnalConnection())
            {
                return db.Table<Stanza>().Where(p => p.parent_hymn == hymnalId).ToList();
            }
        }
        public static List<Tune> GetHymnTunesById(string hymnalId)
        {
            using (var db = GetHymnalConnection())
            {
                return db.Table<Tune>().Where(p => p._id == hymnalId).ToList();
            }
        }
        public static Midi GetHymnMidiByTune(string tuneId)
        {
            using (var db = GetHymnalConnection())
            {
                return db.Table<Midi>().Where(p => p.tune_id == tuneId).FirstOrDefault();
            }
        }
        public static string GetGuitarSVG(string hymnalId)
        {
            return $"https://raw.githubusercontent.com/lemuelinchrist/hymnsforandroid/master/app/src/guitarSvg/{hymnalId}.svg";
        }
        public static string GetPianoSVG(string hymnalId)
        {
            return $"https://raw.githubusercontent.com/lemuelinchrist/hymnsforandroid/master/app/src/pianoSvg/{hymnalId}.svg";
        }
        public static bool OrganizeChorus = false;
        public static string WrapStanzasToHtml(List<Stanza> stanzas_base, string footer)
        {
            string chorusColor = "#DAA520";
            StringBuilder stringBuilder = new StringBuilder();
            var stanzas = stanzas_base;
            if (OrganizeChorus)
            {
                stanzas = OrganizeStanzas(stanzas_base);
            }
            bool hasChorus = stanzas.Where(p => p.no == "chorus").Count() > 0;
            foreach (var stanza in stanzas)
            {
                string htm = "";
                if (stanza.no == "chorus")
                {
                    htm = $"<p style=\"color:{chorusColor}\"><br>{stanza.text}</p>";
                }
                else
                {
                    htm = $"<p><span style=\"color:{chorusColor}\">{stanza.no}</span><br>{stanza.text}</p>";
                }
                stringBuilder.AppendLine(htm);
            }
            if (!string.IsNullOrEmpty(footer))
            {
                stringBuilder.AppendLine($"<p><br>{footer}<br>");
                stringBuilder.AppendLine($"<hr style=\"border-top: 1px dashed #bbb;\"> </p>");
            }
            return stringBuilder.ToString();
        }
        public static List<Stanza> OrganizeStanzas(List<Stanza> stanzas_base)
        {
            List<Stanza> stanzas = stanzas_base.ToList();
            var chorus = stanzas.Where(p => p.no == "chorus").FirstOrDefault();
            if (chorus != null)
            {
                List<Stanza> stanzasFinal = new List<Stanza>();
                bool startChorus = false;
                foreach (var stanza in stanzas)
                {
                    if (startChorus)
                    {
                        if (stanza.no == "chorus")
                        {
                            stanzasFinal.Add(stanza);
                        }
                        else
                        {
                            stanzasFinal.Add(stanza);
                            stanzasFinal.Add(chorus);
                        }                       
                        
                    }
                    else
                    {
                        if (stanza.no == "chorus")
                        {
                            stanzasFinal.Add(stanza);
                            startChorus = true;
                        }
                        else
                        {
                            stanzasFinal.Add(stanza);
                        }

                    }
                }
                stanzas = stanzasFinal;
            }
            return stanzas;
        }
        public static string CreateHtmlHeader(Hymn hymn)
        {
            //onclick=\"window.external.notify(payload.value)\"
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<p>");

            //font-weight: 900;
            stringBuilder.AppendLine($"<span style=\"font-weight:900;\">{hymn.main_category}</span>");
            stringBuilder.AppendLine($"<br>{hymn.sub_category}");
            stringBuilder.AppendLine($"<br>Meter:  {hymn.meter}");
            stringBuilder.AppendLine($"<br>Time: {hymn.time} - {hymn.key}");
            stringBuilder.AppendLine($"<br>Related: {hymn.related}");
            stringBuilder.AppendLine($"<br>Number: {hymn._id}");
            stringBuilder.AppendLine(WrapButtons());
            stringBuilder.AppendLine("</p>");
            return stringBuilder.ToString();
        }
        public static string GetCopy(Hymn hymn)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Main: {hymn.main_category}");
            stringBuilder.AppendLine($"Title: {hymn.sub_category}");
            stringBuilder.AppendLine($"Meter:  {hymn.meter}");
            stringBuilder.AppendLine($"Time: {hymn.time} - {hymn.key}");
            stringBuilder.AppendLine($"Related: {hymn.related}");
            stringBuilder.AppendLine($"Number: {hymn._id}");
            stringBuilder.AppendLine("\n");

            foreach (var stanza in GetHymnStanzaById(hymn._id))
            {
                stringBuilder.AppendLine((stanza.no + "\n" + stanza.text.Replace("<br/>", "\n")).Trim());
                stringBuilder.AppendLine("\n");
            }
            stringBuilder.AppendLine($"Composer: {hymn.composer}");
            stringBuilder.AppendLine($"Author: {hymn.author}");
            stringBuilder.AppendLine($"Notes: {hymn.sheet_music_link}");

            var tunes = GetHymnTunesById(hymn._id);

            if (tunes?.Count > 0)
            {
                stringBuilder.AppendLine($"Youtube: https://youtu.be/{tunes[0].youtube_link}");
            }
            if (hymnTunes?.Count > 0)
            {
                var list = GetHymnTuneListById(hymn._id);
                if (list?.Count > 0)
                {
                    foreach (var yt in list)
                    {
                        stringBuilder.AppendLine($"Youtube: https://youtu.be/{yt.youtube_link}");
                    }
                }
            }
            stringBuilder.AppendLine("\n");

            return stringBuilder.ToString();
        }
        public static string GetHymnalUrl(Hymn hymn)
        {
            //https://www.hymnal.net/en/hymn/hs/373
            var number = GetNumbers(hymn._id);
            string letter = "h";
            if (hymn._id.StartsWith("E"))
            {
                letter = "h";
            }
            else
            {
                var letters = GetLetters(hymn._id);
                if (letters.Length > 1)
                {
                    letter = letters;
                }
                else
                {
                    letter = "h" + GetLetters(hymn._id);
                }

            }
            letter = letter.ToLower();
            return $"https://www.hymnal.net/en/hymn/{letter}/{number}";

        }

        private static string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
        private static string GetLetters(string input)
        {
            return new string(input.Where(c => !char.IsDigit(c)).ToArray());
        }
        public static string GetLanguageByCode(string _id)
        {
            if (string.IsNullOrEmpty(_id))
                return "";
            string language = "Language";
            var id = GetLetters(_id.Trim()).ToLower();
            switch (id)
            {
                case "e":
                    language = "English";
                    break;
                case "cb":
                    language = "Cebuano";
                    break;
                case "t":
                    language = "Tagalog";
                    break;
                case "k":
                    language = "Korean";
                    break;
                case "c":
                    language = "中文-繁";
                    break;
                case "cs":
                    language = "補充本-繁";
                    break;
                case "z":
                    language = "中文-简";
                    break;
                case "zs":
                    language = "補充本-简";
                    break;
                case "fr":
                    language = "French";
                    break;
                case "s":
                    language = "Spanish";
                    break;
                case "j":
                    language = "Japanese";
                    break;
                case "i":
                    language = "B.Indonesia";
                    break;
                case "bf":
                    language = "Be Filled";
                    break;
                case "ch":
                    language = "Children";
                    break;
                case "f":
                    language = "Farsi";
                    break;
                case "ns":
                    language = "New Songs";
                    break;
                case "g":
                    language = "German";
                    break;

            }
            return language;
        }
        static string WrapButtons()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<input hidden id=\"payload\" type=\"text\" value=\"play\" />");
            sb.AppendLine($"<input hidden id=\"payload2\" type=\"text\" value=\"notes\" />");
            sb.AppendLine($"<input hidden id=\"payload3\" type=\"text\" value=\"copy\" />");
            sb.AppendLine("<div>\r\n<style>\r\n.button {\r\n  background-color: #4CAF50; \r\n  border: none;\r\n  color: white;\r\n  padding: 15px 32px;\r\n  text-align: center;\r\n  text-decoration: none;\r\n  display: inline-block;\r\n  font-size: 16px;\r\n  margin: 4px 2px;\r\n  cursor: pointer;\r\n}\r\n.button2 {background-color: #008CBA;}\r\n.button4 {background-color: #e7e7e7; color: black;}\r\n</style>\r\n");
            sb.AppendLine($"<button onclick=\"window.external.notify(payload.value)\" class=\"button\">Play</button>");
            sb.AppendLine($"<button onclick=\"window.external.notify(payload2.value)\" class=\"button button2\">Notes</button>");
            sb.AppendLine($"<button onclick=\"window.external.notify(payload3.value)\" class=\"button button4\">Copy</button>");

            sb.Append("\r\n</div>");
            return sb.ToString();
        }
        const string TuneFileName = "tunes.json";
        static List<HymnTune> hymnTunes = new List<HymnTune>();
        public static List<HymnTune> GetHymnTuneListById(string _id)
        {
            if (hymnTunes == null)
                return null;
            if (hymnTunes.Count == 0)
                return null;
            var item = hymnTunes.Where(p => p.unique_id == _id).ToList();
            return item;
        }
        public static async void SaveTunes(bool refresh = false)
        {
            string url = "https://script.google.com/macros/s/AKfycbx9Udm5iO9hBNn8oY859ORa4RgR4GzqBSjiOtT75eBG6-JolhxT2sQ3Qp7NVOKyxpwe/exec";

            try
            {
                if (refresh == false)
                {
                    if (TextCache.IsFileExist(TuneFileName))
                    {
                        var json = await TextCache.ReadSavedStringAsync(TuneFileName);
                        hymnTunes = JsonConvert.DeserializeObject<List<HymnTune>>(json);
                    }
                    else
                    {
                        var json = await NetworkHelper.DownloadstringAsync(url);
                        if (!string.IsNullOrEmpty(json))
                        {
                            hymnTunes = JsonConvert.DeserializeObject<List<HymnTune>>(json);
                            TextCache.SaveStringToStorageFolder(TuneFileName, json);
                        }
                    }
                }
                else
                {
                    var json = await NetworkHelper.DownloadstringAsync(url);
                    if (!string.IsNullOrEmpty(json))
                    {
                        hymnTunes = JsonConvert.DeserializeObject<List<HymnTune>>(json);
                        TextCache.SaveStringToStorageFolder(TuneFileName, json);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static string GetOriginalSQLiteSource()
        {
            return "https://github.com/lemuelinchrist/hymnsforandroid/raw/master/app/src/main/assets/hymns.sqlite";
        }
        public static string AlignStanza(string stanza)
        {
            return stanza.Replace("<br/>", "\n");
        }
    }
    
}
