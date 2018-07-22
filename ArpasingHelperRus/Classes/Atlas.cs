using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Windows;

namespace AtlasConverter.Classes
{

    public class Atlas
    {
        #region Variables
        public string Version;
        public string[] Vowels;
        public string[] Consonants;
        public string[] Rests;
        public List<string> AliasTypes;
        public Dictionary<string, string> Format;
        public List<string[]> Replaces;
        public Core Core { get { return App.Core; } }

        public bool IsLoaded = false;
        public bool HasDict = true;
        public bool IsDefault = false;
        public string DefaultVoicebankType { get { return "Arpasing RUS"; } }
        public string VoicebankType;
        public string AtlasPath
        {   get
            {
                if (IsDefault) return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"atlas", DefaultVoicebankType + ".atlas");
                else return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"atlas", VoicebankType + ".atlas");
            }
        }

        private string VowelPattern;
        private string ConsonantPattern;
        private string RestPattern;

        #endregion

        public Atlas(string dir)
        {
            VoicebankType = DefaultVoicebankType;
            IsDefault = true;
            ReadAtlas();
        }

        #region Reading

        void ReadVersion(string line) { Version = line.Substring("Version".Length + 1); }
        void ReadVowels(string line) { Vowels = line.Substring("Vowels".Length + 1).Split(','); }
        void ReadConsonants(string line) { Consonants = line.Substring("Consonants".Length + 1).Split(','); }
        void ReadRest(string line) { Rests = line.Substring("Rests".Length + 1).Split(','); }
        void ReadFormat(string line)
        {
            string[] format = line.Split('=');
            if (AliasTypes.Contains(format[0])) return;
            Format[format[0]] = format[1];
            AliasTypes.Add(format[0]);
        }
        void ReadRule(string line)
        {
            Rule.Read(line);
        }
        void ReadReplace(string line)
        {
            string[] replace = line.Split('=');
            if (replace.Length != 2) return;
            Replaces.Add(replace);
        }
        void ReadAtlas()
        {
            string[] atlas = File.ReadAllLines(AtlasPath);
            int i = 0;
            if (atlas[0] != "[MAIN]") throw new Exception("Can't find main section"); ;
            i++;

            while (atlas[i] != "[FORMAT]")
            {
                if (atlas[i].StartsWith("Version")) ReadVersion(atlas[i]);
                if (atlas[i].StartsWith("Vowels")) ReadVowels(atlas[i]);
                if (atlas[i].StartsWith("Consonants")) ReadConsonants(atlas[i]);
                if (atlas[i].StartsWith("Rests")) ReadRest(atlas[i]);
                i++;
            }

            if (Rests == null) throw new Exception("Can't find Rests definition");
            if (Consonants == null) throw new Exception("Can't find Consonants definition");
            if (Vowels == null) throw new Exception("Can't find Vowels definition");

            VowelPattern = $"({String.Join("|", Vowels)})";
            ConsonantPattern = $"({String.Join("|", Consonants)})";
            RestPattern = $"({String.Join("|", Rests)})";

            AliasTypes = new List<string>();
            Format = new Dictionary<string, string>();
            i++;
            while (atlas[i] != "[RULES]")
            {
                ReadFormat(atlas[i]);
                i++;
            }
            i++;
            while (atlas[i] != "[REPLACE]")
            {
                ReadRule(atlas[i]);
                i++;
            }
            i++;
            Replaces = new List<string[]>();
            while (i < atlas.Length)
            {
                ReadReplace(atlas[i]);
                i++;
            }
            IsLoaded = true;
            //Ust.ValidateLyrics();
        }


        #endregion

        public bool IsRest(string phoneme) { return phoneme.Trim(' ') == "" || Rests.Contains(phoneme) || phoneme == "Rest"; }
        public bool IsConsonant(string phoneme) { return Consonants.Contains(phoneme); }
        public bool IsVowel(string phoneme) { return Vowels.Contains(phoneme); }

        public string GetAliasType(string alias)
        {
            if (IsRest(alias)) return "R";
            if (IsVowel(alias)) return "V";
            if (IsConsonant(alias)) return "C";
            foreach (string alias_type in AliasTypes)
            {
                string pattern = Format[alias_type].Replace("%V%", VowelPattern);
                pattern = pattern.Replace("%C%", ConsonantPattern);
                pattern = pattern.Replace("%R%", RestPattern);
                if (Regex.IsMatch(alias, pattern) && Regex.Match(alias, pattern).Value == alias)
                {
                    string attempt = Regex.Match(alias, pattern).Value;
                    return alias_type;
                }
            }
            throw new KeyNotFoundException($"Can't detect alias type for [{alias}]. Check your atlas");
        }

        public string[] GetPhonemes(string alias)
        {
            if (IsRest(alias)) return new string[] {};
            if (IsVowel(alias) || IsConsonant(alias)) return new string[] { alias };
            foreach (string alias_type in AliasTypes)
            {

                string pattern = Format[alias_type].Replace("%V%", VowelPattern);
                pattern = pattern.Replace("%C%", ConsonantPattern);
                pattern = pattern.Replace("%R%", RestPattern);
                if (Regex.IsMatch(alias, pattern))
                {
                    string attempt = Regex.Match(alias, pattern).Value;
                    if (attempt == alias)
                    {
                        List<string> st = Regex.Split(alias, pattern).ToList();
                        st.Remove(st.First());
                        st.Remove(st.Last());
                        return st.ToArray();
                    }
                }
            }
            throw new Exception($"Can't extract phonemes from [{alias}]");
        }

        public string[] GetPhonemes(string alias, string alias_type)
        {
            if (IsRest(alias)) return new string[] { };
            string pattern = Format[alias_type];
            pattern = pattern.Replace("%V%", VowelPattern);
            pattern = pattern.Replace("%C%", ConsonantPattern);
            pattern = pattern.Replace("%R%", RestPattern);
            if (Regex.IsMatch(alias, pattern))
            {
                string attempt = Regex.Match(alias, pattern).Value;
                if (attempt == alias)
                {
                    List<string> st = Regex.Split(alias, pattern).ToList();
                    st.Remove(st.First());
                    st.Remove(st.Last());
                    return st.ToArray();
                }
            }
            throw new Exception($"Can't extract phonemes from {alias}");
        }

        public bool MatchPhonemeType(string PhonemeType, string phoneme)
        {
            switch (PhonemeType)
            {
                case "%C%":
                    if (Consonants.Contains(phoneme)) return true;
                    else return false;
                case "%V%":
                    if (Vowels.Contains(phoneme)) return true;
                    else return false;
                default:
                    throw new Exception($"Unknown phoneme: {phoneme}");
            }
        }

        public string GetAlias(string alias_type, string[] phonemes)
        {
            if (alias_type == "R") return " ";
            string ph = "%.%";
            if (!Format.ContainsKey(alias_type))
                throw new Exception($"Cannot found alias type format for alias type: {alias_type}");
            string format = Format[alias_type];
            int i = 0;
            while (Regex.IsMatch(format, ph))
            {
                if (i >= phonemes.Length) throw new Exception($"Not enough phonemes to format alias {alias_type}");
                string ph_type = Regex.Match(format, ph).Value;
                if (MatchPhonemeType(ph_type, phonemes[i]))
                {
                    var f = new Regex(ph_type);
                    format = f.Replace(format, phonemes[i], 1);
                    i++;
                }
                else throw new Exception($"Wrong phonemes to format alias {alias_type}");
            }

            return Replace(format);
        }

        string Replace(string line)
        {
            foreach (string[] pair in Replaces)
            {
                if (line.Contains(pair[0])) line = line.Replace(pair[0], pair[1]);
            }
            return line;
        }

        public int VowelsCount(string phonemes)
        {
            int count = 0;
            var phs = phonemes.Split(' ');
            foreach (var vowel in Vowels)
            {
                count += phs.Count(n => n == vowel);
            }
            return count;
        }
    }
}
