using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AtlasConverter.Classes
{

    class Number
    {

        public const string Next = "[#NEXT]";
        public const string Prev = "[#PREV]";
        public const string Insert = "[#INSERT]";
        public const string Delete = "[#DELETE]";
        public const string Version = "[#VERSION]";
        public const string Setting = "[#SETTING]";
        public static string LastNumber;

        public static bool IsNote(string number)
        {
            if (number.Length < 6) return false;
            if (number == Next) return true;
            if (number == Prev) return true;
            return int.TryParse(number.Substring(2, 4), out int i);
        }

        public static string GetNumber(int i)
        {
            return $"[#{i.ToString().PadLeft(4, '0')}]";
        }

        public static int GetInt(string number)
        {
            //if (number == "[#NEXT]") return Ust.Notes.Count();
            return int.Parse(number.Substring(2, 4));
        }
    }

    public class UNote
    {
        public Atlas Atlas { get { return App.Core.Atlas; } }
        public Singer Singer { get { return App.Core.Singer; } }
        public object UI { get; set; }
        public int InitLength { get; set; }
        public int Preutterance { get; set; }
        public int NoteNum { get; set; }
        //public List<Subnote> Subnotes;
        public UNote Parent { get; set; }
        public List<UNote> Children { get; set; }
        public UColor Color { get; set; }

        private string _number;
        private string _phonemes;
        private string _forcedPhonemes;
        private string _lyric;
        private int _length;

        public bool IsPhonemesForced { get; set; } = false;

        public string Lyric { get => _lyric; set => _lyric = value; }
        public string Number { get => _number; set => _number = value; }
        public string Phonemes
        {
            get
            {
                if (IsPhonemesForced) return _forcedPhonemes;
                else return _phonemes ;
            }
            set { _phonemes = value; }
        }
        public bool IsRest { get; set; }
        public int Length
        {
            get { return _length; }
            set
            {
                if (value <= 0)
                {
                    App.Core.Log("Note", $"Got negative note length on note {Lyric} [{Phonemes}]", true);
                    Length = 10;
                }
                else _length = value;
            }
        }

        public UNote()
        {
            Children = new List<UNote>();
            
        }


        public UNote Clone()
        {
            UNote note = new UNote()
            {
                Number = Number,
                Length = Length,
                Preutterance = Preutterance,
                Lyric = Lyric,
                NoteNum = NoteNum,
                Color = Color,
                InitLength = InitLength,
                Phonemes = Phonemes,
                IsRest = IsRest
            };
            return note;
        }


        public void ForcePhonemes(string phonemes)
        {
            _forcedPhonemes = phonemes;
            IsPhonemesForced = true;
        }

        public void ResetPhonemes()
        {
            IsPhonemesForced = false;
        }

        public string[] GetText(bool last = false)
        {
            string lyric = Phonemes;
            if (Atlas.IsLoaded && Atlas.IsRest(lyric)) lyric = "R";
            if (lyric == "r") lyric = "rr";
            string color = Color == null ? "" : Color.Suffix;
            string number = Number == Classes.Number.LastNumber ? Classes.Number.Insert : Number;
            if (last) number = Classes.Number.LastNumber;
            List<string> text = new List<string>
            {
                number,
                $"Length={Length}",
                $"Lyric={lyric}{color}",
                $"NoteNum={NoteNum}"
            };
            if (Number == AtlasConverter.Classes.Number.Insert) text.Add("Modulation=0");
            return text.ToArray();
        }

        public void ReadLyric(string lyric, bool keepRest = false)
        {
            bool InitRest = IsRest;
            IsRest = false;
            if (Atlas.IsRest(lyric))
            {
                IsRest = true;
                Lyric = "Rest";
                Phonemes = "R";
            }
            else if (lyric == "rr")
            {
                Lyric = "r";
                Phonemes = "r";
            }
            else
            {
                Lyric = lyric.Replace(' ', '-');
                Phonemes = lyric;
            }
            if (keepRest) IsRest = InitRest;
        }

        public void ResetColor()
        {
            Color = null;
        }

        public void SetColor(int index)
        {
            Console.WriteLine("Set color " + index);
            if (index < 1 || index > 9) return;
            Color = Singer.GetColor(index-1);
        }
    }

}
