using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AtlasConverter.Classes
{
    public class UColor
    {
        public string Suffix { get; set; }
        public Color Color { get; set; }
        public string ColorString { get { return this.Color.ToString(); } }
        public int Index { get; set; }
        public string Name { get; set; }
        public Pitchmap Pitchmap { get; set; }
        public static int LastIndex = 0;


        public UColor(string name, string suffix, int r, int g, int b)
        {
            Index = LastIndex++;
            Name = name;
            Color = System.Windows.Media.Color.FromRgb((byte)r, (byte)g, (byte)b);
            Suffix = suffix;
        }

        public UColor(string name, string suffix, int r, int g, int b, string pitchmap) 
            : this(name, suffix, r, g, b)
        {
            Pitchmap = new Pitchmap(pitchmap);
        }
    }
}
