using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AtlasConverter.Classes
{
    struct Format
    {
        public Format(string line)
        {
            /// line looks like -V[][0], i.e. AliasType[MembersPrev][Members]
            /// 

            if (line.Count(n => n == '[') != 2 || line.Count(n => n == ']') != 2) throw new Exception("Error reading rule format");
            string[] splitted = line.Split(new char[] { '[', ']' });
            AliasType = splitted[0];
            string membersPrev = splitted[1];
            string members = splitted[3];

            Members = new int[] { };
            MembersPrev = new int[] { };

            if (membersPrev.Length != 0)  MembersPrev = membersPrev.Split(':').Select(n => int.Parse(n)).ToArray();
            if (members.Length != 0)  Members = members.Split(':').Select(n => int.Parse(n)).ToArray();
        }
        public Atlas Atlas { get { return App.Core.Atlas; } }
        public string AliasType;
        public int[] Members;
        public int[] MembersPrev;
        public bool IsLastPhoneme(int[] phonemes) { return phonemes[0] == -1; }
        public bool IsEmptyPhoneme(int[] phonemes) { return phonemes.Length == 0; }

        public RuleResult GetResult(string lyricPrev, string lyric)
        {
            if (lyricPrev == "s" && lyric == "t")
            {
                int i = 0;
            }
            string[] phonemesPrev = Atlas.GetPhonemes(lyricPrev);
            string[] phonemes = Atlas.GetPhonemes(lyric);
            string[] phonemesNew = GetNewPhonemes(phonemesPrev, phonemes);
            string alias = Atlas.GetAlias(AliasType, phonemesNew);
            RuleResult ruleResult = new RuleResult(alias, AliasType);
            return ruleResult;
        }

        string[] GetNewPhonemes(string[] phonemesPrev, string[] phonemes)
        {
            List<string> phonemesNew = new List<string>();
            if (IsEmptyPhoneme(MembersPrev)) { }
            else if (IsLastPhoneme(MembersPrev)) phonemesNew.Add(phonemesPrev.Last());
            else foreach (int ind in MembersPrev) phonemesNew.Add(phonemesPrev[ind]);

            if (IsEmptyPhoneme(Members)) { }
            else if (IsLastPhoneme(Members)) phonemesNew.Add(phonemes.Last());
            else foreach (int ind in Members) phonemesNew.Add(phonemes[ind]);
            if (Members.Length + MembersPrev.Length != phonemesNew.Count) throw new Exception("Error formating phonemes");
            return phonemesNew.ToArray();
        }
    }

    struct RuleResult
    {
        public string Alias;
        public string AliasType;

        public RuleResult(string alias, string aliasType)
        {
            Alias = alias;
            AliasType = aliasType;
        }
    }

    class Rule
    {
        public bool MustConvert = false;
        public bool MustInsert = false;

        // for converting
        public Format FormatConvert;
        public Format FormatInsert;

        private static Dictionary<string, Rule> Links = new Dictionary<string, Rule>();

        public Rule(string ruleline)
        {
            if (ruleline.Contains(","))
            {
                MustConvert = true;
                MustInsert = true;
                var tl = ruleline.Split(',');
                string toconvert = tl[0].StartsWith("INSERT") ? tl[1] : tl[0];
                string toinsert = tl[0].StartsWith("INSERT") ? tl[0] : tl[1];
                toinsert = toinsert.Substring("INSERT(".Length);
                toinsert = toinsert.TrimEnd(')');
                FormatConvert = new Format( toconvert);
                FormatInsert = new Format(toinsert);
            }
            else if (ruleline.StartsWith("INSERT"))
            {
                MustInsert = true;
                string toinsert = ruleline.Substring("INSERT(".Length);
                toinsert = toinsert.TrimEnd(')');
                FormatInsert = new Format(toinsert);
            }
            else
            {
                MustConvert = true;
                FormatConvert = new Format(ruleline);
            }
        }

        public static Rule GetRule(string subject)
        {
            if (!Links.ContainsKey(subject)) return null;
            return Links[subject];
        }

        public static void Read(string line)
        {
            var t = line.Split('=');
            if (t.Length != 2) return;
            string subject = t[0];
            string rule = t[1];

            if (rule.StartsWith(">")) FindLinks(subject, rule.Substring(1));
            else Links[subject] = new Rule(rule);
        }

        static void FindLinks(string subject, string link)
        {
            if (Links.ContainsKey(link)) Links[subject] = Links[link];
        }

    }

}
