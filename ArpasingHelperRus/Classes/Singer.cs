using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace AtlasConverter.Classes
{

    public class Singer
    {
        public static List<Singer> Singers = new List<Singer>();
        public static Dictionary<string, Singer> SingerNames = new Dictionary<string, Singer>();

        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Image { get; private set; }
        public string Sample { get; private set; }
        public string Dir { get; private set; }
        public string VoicebankType { get; private set; }
        public string Readme { get; private set; }
        public List<string> Subs { get; private set; }
        public List<Oto> Otos { get; private set; }
        public bool IsEnabled { get; private set; }
        public Pitchmap Pitchmap { get; private set; }
        public Colormap Colormap { get; private set; }
        
        public Singer(string dir)
        {
            Dir = dir;
            CheckVoicebank();
            if (!IsEnabled) return;
            CharLoad();
            Pitchmap = new Pitchmap(Dir);
            Colormap = new Colormap(Dir);
            Load();
        }

        void Add()
        {
            Singers.Add(this);
            SingerNames[Name] = this;
        }

        void CheckVoicebank()
        {
            if (!Directory.Exists(Dir))
            {
                IsEnabled = false;
                return;
            }
            Subs = Directory.EnumerateDirectories(Dir).Select(n => Path.GetFileName(n)).ToList();
            Subs.Add("");
            IsEnabled = false;
            foreach (string sub in Subs)
            {
                string subdir = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(subdir))
                {
                    IsEnabled = true;
                    return;
                }
            }
            IsEnabled = false;
            return;
        }

        private void CharLoad()
        {
            string charfile = Path.Combine(Dir, "character.txt");
            if (IsEnabled && File.Exists(charfile))
            {
                string[] charlines = File.ReadAllLines(charfile);
                foreach (string line in charlines)
                {
                    if (line.StartsWith("author=")) Author = line.Substring("author=".Length);
                    if (line.StartsWith("image=")) Image = line.Substring("image=".Length);
                    if (line.StartsWith("name=")) Name = line.Substring("name=".Length);
                    if (line.StartsWith("sample=")) Sample = line.Substring("sample=".Length);
                    if (line.StartsWith("VoicebankType=")) VoicebankType = line.Substring("VoicebankType=".Length);
                }
            }
            if (Name == null) Name = Path.GetFileName(Dir);
        }

        public void Load()
        {
            Otos = new List<Oto> { };
            foreach (string sub in Subs)
            {
                string filename = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        string pattern = "(.*)=(.*),(.*),(.*),(.*),(.*),(.*)";
                        var arr = Regex.Split(line, pattern);
                        double temp;
                        if (arr.Length == 1) continue;
                        Oto Oto = new Oto()
                        {
                            File = arr[1],
                            Alias = arr[2],
                            Offset = double.TryParse(arr[3], out temp) ? temp : 0,
                            Consonant = double.TryParse(arr[4], out temp) ? temp : 0,
                            Cutoff = double.TryParse(arr[5], out temp) ? temp : 0,
                            Preutterance = double.TryParse(arr[6], out temp) ? temp : 0,
                            Overlap = double.TryParse(arr[7], out temp) ? temp : 0,
                        };
                        Oto = Pitchmap.Substract(Oto);
                        Oto = Colormap.Substract(Oto);
                        Otos.Add(Oto);
                    }
                }
                else File.Create(filename);
            }
        }

        public Oto FindOto(string lyric)
        {
            if (IsEnabled)
            {
                foreach (Oto Oto in Otos)
                {
                    if (Oto.Alias == lyric)
                    {
                        return Oto;
                    }
                }
                return null;
            }
            return null;
        }

        public UColor GetColor(int index)
        {
            if (index < Colormap.Colors.Count) return Colormap.Colors[index];
            else return null;
        }
    }
}
