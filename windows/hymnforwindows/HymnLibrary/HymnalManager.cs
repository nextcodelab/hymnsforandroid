
using HymnLibrary;
using HymnLibrary.Helpers;
using HymnLibrary.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HymnLibrary
{
    public class HymnalManager
    {
        public const string HYMN_FILE = "hymns.sqlite";
        public static string SQLiteFilePath { get; private set; } = Path.Combine(DatabaseDirectory.Dir, HYMN_FILE);
        static bool processing = false;
        public static async void InitData()
        {
            if (processing)
                return;
            processing = true;
            if (File.Exists(SQLiteFilePath) == false)
            {
                var input = DatabaseDirectory.GetEmbeddedResourceStreamAsync("Engine.Data.hymns.sqlite", typeof(HymnalManager));
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
        public static async Task<List<Hymn>> GetQuerySearchAsync(string query, int limit = 40)
        {

            List<Hymn> list = new List<Hymn>();
            if (string.IsNullOrEmpty(query))
                return list;
            var noText = GetNumbers(query);
            int no = 0;
            if (!string.IsNullOrEmpty(noText))
            {
                no = Int32.Parse(noText.Trim());
            }
            while (processing)
            {
                await Task.Delay(500);
            }

            query = query.ToLower();
            using (var session = new SQLite.SQLiteConnection(SQLiteFilePath))
            {

                list = session.Table<Hymn>().Where(p => p.no == no
                | p._id.ToLower().Contains(query)
                | p.hymn_group.ToLower().Contains(query)
                | p.first_stanza_line.ToLower().Contains(query)
                | p.sub_category.ToLower().Contains(query)
                | p.first_stanza_line.ToLower().Contains(query)
                | p.main_category.ToLower().Contains(query)).Take(limit).ToList();
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

            }

            return list;
        }
        public static Hymn GetHymnById(string hymnalId)
        {

            using (var db = GetHymnalConnection())
            {

                return db.Table<Hymn>().Where(p => p._id == hymnalId).FirstOrDefault();
            }
        }
        public static List<Hymn> GetHymnsByNumber(int no)
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
        /// <summary>
        /// Example code for Cebuano = CB 
        /// </summary>
        /// <param name="startLanguageCode"></param>
        /// <returns></returns>
        public static Hymn CreateNewHymn(string startLanguageCode)
        {
            using (var db = GetHymnalConnection())
            {
                db.CreateTable<Hymn>();
                db.GetMapping<Hymn>();
                var hm = db.Table<Hymn>().LastOrDefault();
                int no = 0;
                if (hm != null)
                {
                    no = hm.no + 1;
                }
                return new Hymn() { no = no, _id = startLanguageCode + no, };
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
        void RepeatedChorusHtml(Hymn hymn, List<Stanza> stanzas_base)
        {
            string chorusText = "";
            var text = new StringBuilder();
            foreach (Stanza stanza in stanzas_base)
            {
               
                if (stanza.no.Equals("chorus"))
                {
                    if (!string.IsNullOrEmpty(stanza.note))
                        text.Append("<i>@@(" + stanza.note + ")@@</i>");
                    chorusText = "<i>@@" + stanza.text + "@@</i>";
                    text.Append(chorusText);
                   
                    text = new StringBuilder();
                }
                else if (stanza.no.Contains("note"))
                {
                    // notes do not have their own lyric view unlike normal stanzas and choruses
                    text.Append("<i>" + stanza.text + "</i><br/>");
                }

                else
                {
                    // append stanza
                    text.Append("<b>##" + stanza.no + "##</b><br/>");
                    text.Append(stanza.text);
                   
                    text = new StringBuilder();


                    // append chorus after every stanza
                    var chorusCount = stanzas_base.Where(p => p.no == "chorus").Count();
                    if (chorusCount == 1 && !string.IsNullOrEmpty(chorusText))
                    {
                        
                        text = new StringBuilder();
                    }
                }
            }
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
        public static string GetLetters(string input)
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
        public static string AlignStanza(string stanza)
        {
            return stanza.Replace("<br/>", "\n");
        }
        public static ColorBytes GetColorBytes(string code)
        {
            ColorBytes colorBytes = new ColorBytes();
            colorBytes.BorderColor = Color.FromArgb(0x3D, 0x57, 0x7A);
            colorBytes.BackgroundColor = Color.FromArgb(0xB0, 0xBE, 0xC5);
            code = code.ToLower();
            Debug.WriteLine(code);
            switch (code)
            {
                case "e":
                    colorBytes.BorderColor = Color.FromArgb(0x3D, 0x57, 0x7A);
                    colorBytes.BackgroundColor = Color.FromArgb(0xB0, 0xBE, 0xC5);
                    break;
                case "c":
                    colorBytes.BorderColor = Color.FromArgb(0x66, 0x99, 0x00);
                    colorBytes.BackgroundColor = Color.FromArgb(0xA5, 0xD6, 0xA7);
                    break;
                case "cs":
                    colorBytes.BorderColor = Color.FromArgb(0x99, 0x33, 0xCC);
                    colorBytes.BackgroundColor = Color.FromArgb(0xCE, 0x93, 0xD8);
                    break;
                case "z":
                    colorBytes.BorderColor = Color.FromArgb(0x66, 0x99, 0x00);
                    colorBytes.BackgroundColor = Color.FromArgb(0xA5, 0xD6, 0xA7);
                    break;
                case "zs":
                    colorBytes.BorderColor = Color.FromArgb(0x99, 0x33, 0xCC);
                    colorBytes.BackgroundColor = Color.FromArgb(0xCE, 0x93, 0xD8);
                    break;
                case "cb":
                    colorBytes.BorderColor = Color.FromArgb(0xFF, 0x88, 0x00);
                    colorBytes.BackgroundColor = Color.FromArgb(0xFF, 0xCC, 0x80);
                    break;
                case "t":
                    colorBytes.BorderColor = Color.FromArgb(0x00, 0x96, 0x88);
                    colorBytes.BackgroundColor = Color.FromArgb(0x80, 0xCB, 0xC4);
                    break;
                case "fr":
                    colorBytes.BorderColor = Color.FromArgb(0xE9, 0x1E, 0x63);
                    colorBytes.BackgroundColor = Color.FromArgb(0xF4, 0x8F, 0xB1);
                    break;
                case "s":
                    colorBytes.BorderColor = Color.FromArgb(0x3F, 0x51, 0xB5);
                    colorBytes.BackgroundColor = Color.FromArgb(0x9F, 0xA8, 0xDA);
                    break;
                case "k":
                    colorBytes.BorderColor = Color.FromArgb(0x79, 0x55, 0x48);
                    colorBytes.BackgroundColor = Color.FromArgb(0xBC, 0xAA, 0xA4);
                    break;
                case "g":
                    colorBytes.BorderColor = Color.FromArgb(0xFF, 0x57, 0x22);
                    colorBytes.BackgroundColor = Color.FromArgb(0xFF, 0xAB, 0x91);
                    break;
                case "j":
                    colorBytes.BorderColor = Color.FromArgb(0x8B, 0xC3, 0x4A);
                    colorBytes.BackgroundColor = Color.FromArgb(0xAE, 0xD5, 0x81);
                    break;
                case "i":
                    colorBytes.BorderColor = Color.FromArgb(0xF4, 0x43, 0x36);
                    colorBytes.BackgroundColor = Color.FromArgb(0xE5, 0x73, 0x73);
                    break;
                case "bf":
                    colorBytes.BorderColor = Color.FromArgb(0x00, 0x99, 0xCC);
                    colorBytes.BackgroundColor = Color.FromArgb(0x90, 0xCA, 0xF9);
                    break;
                case "ns":
                    colorBytes.BorderColor = Color.FromArgb(0xCC, 0x00, 0x00);
                    colorBytes.BackgroundColor = Color.FromArgb(0xEF, 0x9A, 0x9A);
                    break;
                case "ch":
                    colorBytes.BorderColor = Color.FromArgb(0xAF, 0xB4, 0x2B);
                    colorBytes.BackgroundColor = Color.FromArgb(0xE6, 0xEE, 0x9C);
                    break;
                case "f":
                    colorBytes.BorderColor = Color.FromArgb(0x67, 0x3A, 0xB7);
                    colorBytes.BackgroundColor = Color.FromArgb(0x95, 0x75, 0xCD);
                    break;

            }
            return colorBytes;
        }
    }
    public class ColorBytes
    {
        public Color BorderColor { get; set; }
        public Color BackgroundColor { get; set; }
    }
}
