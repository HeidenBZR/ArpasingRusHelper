using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AtlasConverter.Classes
{

    public class Oto
    {
        public string File;
        public string Alias;
        public string Pitch;
        public UColor Color;
        public double Offset;
        public double Consonant;
        public double Cutoff;
        public double Preutterance;
        public double Overlap;
        public double FullLength
        {
            get
            {
                if (Cutoff < 0) return Math.Abs(Cutoff);
                else return Cutoff - Offset;
            }
        }
        public double StraightPreutterance
        {
            get
            {
                return Math.Abs(Preutterance - Overlap);
            }
        }
        public double Length
        {
            get
            {
                return FullLength - Preutterance;
            }
        }
    }
}
