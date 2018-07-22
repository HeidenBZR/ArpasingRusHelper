using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AtlasConverter.Classes
{

    public class Colormap
    {
        public List<UColor> Colors;
        string Dir;
        List<string> Suffixes
        {
            get
            {
                return Colors.Select(n => n.Suffix).ToList();
            }
        }

        public Colormap(string dir)
        {
            Open(dir);
            Dir = dir;
        }

        public void Open(string dir)
        {
            Colors = new List<UColor>();
            string filename = Path.Combine(dir, @"color.map");
            if (!File.Exists(filename)) return;
            var lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                if (line.Length == 0) continue;
                if (line.StartsWith(";")) continue;
                var param = line.Split('\t');
                if (!byte.TryParse(param[2], out byte r) || r < 0 || r > 255) continue;
                if (!byte.TryParse(param[3], out byte g) || g < 0 || g > 255) continue;
                if (!byte.TryParse(param[4], out byte b) || b < 0 || b > 255) continue;
                UColor color;
                if (param.Length == 5)
                    color = new UColor(param[0], param[1], r, g, b);
                else
                    color = new UColor(param[0], param[1], r, g, b, param[5]);
                AddColor(color);
            }
        }

        public void Reopen()
        {
            Open(Dir);
        }

        public void Save()
        {
            string text = ";Name\tSuffix\tR\tG\tB\tCustom Prefixmap (optional)\r\n";
            foreach (UColor color in Colors)
            {
                string pitchmap = color.Pitchmap is null ? "" : color.Pitchmap.Dir;
                text += $"{color.Name}\t{color.Suffix}\t{color.Color.R}\t{color.Color.G}\t{color.Color.B}\t{pitchmap}\r\n";
            }
            File.WriteAllText(Path.Combine(Dir, "color.map"), text);
        }

        public bool AddColor(UColor color)
        {
            if (!Suffixes.Contains(color.Suffix))
            {
                Colors.Add(color);
                return true;
            }
            return false;
        }

        public bool RemoveColor(string suffix)
        {
            UColor color = Colors.Find(n => n.Suffix == suffix);
            if (Suffixes.Contains(color.Suffix))
            {
                Colors.Remove(color);
                return true;
            }
            return false;

        }

        public bool RemoveColor(UColor color)
        {
            if (Suffixes.Contains(color.Suffix))
            {
                Colors.Remove(color);
                return true;
            }
            return false;
        }

        public Oto Substract(Oto oto)
        {
            foreach (var color in Colors)
            {
                if (oto.Alias.Contains(color.Suffix))
                {
                    oto.Color = color;
                    oto.Alias = oto.Alias.Replace(color.Suffix, "");
                    return oto;
                }
            }
            return oto;
        }
    }
}
