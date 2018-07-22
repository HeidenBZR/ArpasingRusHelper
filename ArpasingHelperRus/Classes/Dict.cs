using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasConverter.Classes
{
    public class Dict
    {
        Core Core { get { return App.Core; } }
        Dictionary<string, string[]> Rules;
        string DictPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"atlas", @"Arpasing RUS.dict");

        public Dict()
        {
            Rules = new Dictionary<string, string[]>();
            foreach (string l in File.ReadAllLines(DictPath))
            {
                // source=value1,value2...
                string line = l.Trim(' ');
                if (line == "") continue;
                if (!line.Contains('=')) continue;
                string[] temp = line.Split('=');
                string[] values = temp[1].Split(',');
                Rules[temp[0]] = values;
            }
        }
        /// <summary>
        /// Input: "спеть", output: {"s", "p'", "e", "t'"}
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string Process(string line)
        {
            int i = 0;
            List<string> letters = new List<string>();
            string defaultphonemes = line.Replace('-', ' ');
            line = line.Replace("-", "");
            while (i < line.Length)
            {
                string letter;
                // checking for 2 letter phoneme or CV
                if (i + 1 < line.Length)
                {
                    letter = line[i].ToString() + line[i + 1].ToString();
                    if (Rules.ContainsKey(letter))
                    {
                        letters.AddRange(Rules[letter]);
                        i++;
                        i++;
                        continue;
                    }
                }
                // checking for 1 letter phoneme
                letter = line[i].ToString();
                if (Rules.ContainsKey(letter))
                {
                    letters.AddRange(Rules[letter]);
                    i++;
                }
                else
                {
                    Core.Error((object)this, $"Unknown letter: {letter}");
                    return defaultphonemes;
                }
            }
            return String.Join(" ", letters);
        }
    }
}
