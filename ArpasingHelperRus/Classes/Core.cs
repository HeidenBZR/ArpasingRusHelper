using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AtlasConverter.Classes
{

    public class Core
    {
        public MainWindow MainWindow { get; set; }
        public Ust Ust { get; set; }
        public Ust InitUst { get; set; }
        public Ust ConvertedUst { get; set; }
        public Atlas Atlas { get; set; }
        public Dict Dict { get; set; }
        public Singer Singer { get; set; }

        public int TransLength { get; set; }
        public bool MakeEnds { get; set; }
        public string Text { get; set; }
        public string Dir { get; set; }
        public List<string> Errors { get; set; }

        public Core(string dir)
        {
            try
            {
                if (File.Exists("ust.tmp")) File.Delete("ust.tmp");
                File.Copy(dir, "ust.tmp");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            Dir = dir;
            TransLength = 20;
            MakeEnds = true;
            Errors = new List<string>();
            File.WriteAllText("log.txt", "");
        }

        public void SetWindow(MainWindow mainWindow)
        {
            Log("Core", "Initiating...");
            Log("Core", "Start reading files...");
            MainWindow = mainWindow;
            mainWindow.Core = this;
            Log("Atlas", "Reading atlas...");
            try
            {
                Atlas = new Atlas(Dir);
            }
            catch (Exception ex)
            {
                Log("Atlas", $"{ex.Message }\r\n{ex.Source}\r\n{ex.TargetSite}\r\n{ex.StackTrace}", true);
                return;
            }
            Log("Atlas", "Reading complited.");
            Log("Dict", "Start reading...");
            try
            {
                Dict = new Dict();
            }
            catch (Exception ex)
            {
                Log("Dict", $"{ex.Message }\r\n{ex.Source}\r\n{ex.TargetSite}\r\n{ex.StackTrace}", true);
                return;
            }
            Log("Dict", "Reading complited.");
            Log("Ust", "Start reading...");
            try
            {
                Ust = new Ust(Dir);
            }
            catch (Exception ex)
            {
                Log("Ust", $"{ex.Message }\r\n{ex.Source}\r\n{ex.TargetSite}\r\n{ex.StackTrace}", true);
                return;
            }
            Log("Ust", "Reading complited.");
            Log("Singer", "Start reading...");
            try
            {
                Singer = new Singer(Ust.VoiceDir);
            }
            catch (Exception ex)
            {
                Log("Singer", $"{ex.Message }\r\n{ex.Source}\r\n{ex.TargetSite}\r\n{ex.StackTrace}", true);
                return;
            }
            Log("Singer", "Reading complited.");
            Log("Ust", "Reading complited.");
            Log("Core", "Reading files complited.");
            Log("Core", "Initiating values...");
            try
            {
                mainWindow.NewLengthDefault.Text = TransLength.ToString();
                mainWindow.ChangeSingerButton.ToolTip = Singer.Name;
                InitUst = Ust.Clone();
                MainWindow.SetText(Ust.Notes);
                ConvertedUst = Process(Atlas, Ust);
                MainWindow.DrawNotes(Ust.Notes, ConvertedUst.Notes);
                MainWindow.OnNoteChanged += OnNoteChanged_Core;
                MainWindow.OnTextChanged += OnTextChanged_Core;
                MainWindow.OnSave += Core_OnSave;
                mainWindow.OnLengthChanged += delegate (int length) { TransLength = length; };
                mainWindow.OnSingerChanged += MainWindow_OnSingerChanged;
                TransLength = (int)Ust.Tempo * 180 / 120;
                mainWindow.NewLengthDefault.Text = TransLength.ToString();
            }
            catch (Exception ex)
            {
                Log("Core", $"{ex.Message }\r\n{ex.Source}\r\n{ex.TargetSite}\r\n{ex.StackTrace}", true);
                return;
            }
            Log("Core", "Initiated.");
        }

        void SetSinger(Singer singer)
        {
            Log("Singer", "Set singer...");
            Singer = singer;
            MainWindow.ChangeSingerButton.ToolTip = Singer.Name;
            Refresh();
        }

        public void MainWindow_OnSingerChanged(string path)
        {
            Log("Singer", "Singer changed...");
            Singer singer = new Singer(path);
            if (singer.IsEnabled) { SetSinger(singer); return; }
            string subpath = Directory.GetParent(path).FullName;
            singer = new Singer(subpath);
            if (singer.IsEnabled) { SetSinger(singer); return; }
            else
            {
                MessageBoxResult result = MessageBox.Show("Singer is not valid. Try again?", "Error on reading singer", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes) MainWindow.ShowSingerDialog();
            }
        }

        public void Core_OnSave()
        {
            Log("Ust", "Saving ust...");
            ConvertedUst.Save("savedust.tmp");
            ConvertedUst.Save();
        }

        public void Refresh()
        {
            Log("Core", "Refresh...");
            SetLyrics(Ust, MainWindow.GetText());
            ConvertedUst = Process(Atlas, Ust);
            MainWindow.DrawNotes(Ust.Notes, ConvertedUst.Notes);
        }

        public void OnNoteChanged_Core(UNote note)
        {
            Log("Core", "Note changed...");
            Refresh();
        }

        public void OnTextChanged_Core()
        {
            Log("Core", "Start reading...");
            Refresh();
        }

        Ust Process(Atlas atlas, Ust initUst)
        {
            Log("Converter", "Start processing...");
            Ust ust = initUst.Clone();
            MakeEnds = MainWindow.MakeEnds.IsChecked.Value;
            if (int.TryParse(MainWindow.NewLengthDefault.Text, out int result))
                TransLength = result;
            SeparateLyric(ust);
            Converting(ust);
            AdjustLength(ust);
            Log("Converter", "Processing completed.");
            return ust;
        }

        void SetLyrics(Ust ust, string[] lyrics)
        {
            Log("Set Lyric", "Starting...");
            int i = 0;
            foreach (UNote note in ust.Notes)
            {
                if (!note.IsRest && i < lyrics.Length)
                {
                    note.Lyric = lyrics[i];
                    note.Phonemes = Dict.Process(lyrics[i]);
                    i++;
                }
                else
                {
                    note.ReadLyric(" ");
                }

            }
            Log("Set Lyric", "Completed");
        }

        void SeparateLyric(Ust ust)
        {
            Log("Separate Lyric", "Starting...");
            List<string> prevphonemes = new List<string>();
            Ust.MainNotes = new List<UNote>();
            foreach (UNote note in ust.Notes)
            {
                note.Children = new List<UNote>();
                note.Parent = null;
                if (note.IsRest) continue;
                UNote prev = ust.GetPrevNote(note);
                UNote next = ust.GetNextNote(note);
                List<string> phonemes = note.Phonemes.Split(' ').ToList();
                Ust.MainNotes.Add(note);
                phonemes.InsertRange(0, prevphonemes);
                prevphonemes = new List<string>();
                int vowel = phonemes.FindIndex(n => Atlas.IsVowel(n) || Atlas.IsRest(n));
                if (vowel == -1)
                {
                    Error(this as object, $"Can't find vowel in note {note.Lyric} [{note.Phonemes}]");
                    note.ReadLyric(phonemes[0], keepRest:true);
                    vowel = 0;
                }
                if (phonemes.Count == 0) { }
                else if (vowel < phonemes.Count - 1)
                {
                    prevphonemes = phonemes.Skip(vowel + 1).ToList();
                    phonemes = phonemes.Take(vowel + 1).ToList();
                    note.ReadLyric(phonemes[vowel]);
                }
                else note.ReadLyric(phonemes[vowel]);
                for (int i = vowel; i > 0; i--)
                {
                    if (phonemes.Count > 0)
                    {
                        Console.WriteLine(phonemes[i - 1]);
                        if (prev.IsRest) prev.Children.Add(ust.InsertNote(prev, phonemes[i - 1], Insert.After, note));
                        else prev.Children.Add(ust.InsertNote(prev, phonemes[i - 1], Insert.After, prev));
                    }
                }
                if (next == null) ust.InsertNote(note, "R", Insert.Append, note);
                else if (next.IsRest) next.Children.Add(ust.InsertNote(next, "R", Insert.Before, note));
                if (prevphonemes.Count == 0) continue;
                for (int i = 0; i < prevphonemes.Count; i++)
                {
                    if (next == null) note.Children.Add(ust.InsertNote(note, prevphonemes[i], Insert.After, note));
                    else if (next.IsRest) note.Children.Add(ust.InsertNote(note, prevphonemes[i], Insert.After, note));
                }
                if (next == null || next.IsRest) prevphonemes = new List<string>();
            }
            Log("Separate Lyric", "Completed.");
        }

        void Converting(Ust ust)
        {
            Log("Converting to aliases", "Starting...");
            string prevlyr = ust.Notes[0].Phonemes;
            string currlyr;
            bool currIsRest;
            bool prevIsRest = ust.Notes[0].IsRest;
            for (int i = 1; i < ust.Notes.Length; i++)
            {
                UNote note = ust.Notes[i];
                UNote prev = ust.Notes[i - 1];
                if (note.IsRest && prevIsRest) continue;
                currlyr = note.IsRest ? "Rest" : note.Phonemes;
                currIsRest = note.IsRest;
                if (prevIsRest) note.Phonemes = "- " + note.Phonemes;
                else if (note.IsRest && (MakeEnds || Atlas.IsConsonant(prevlyr)))
                {
                    note.Phonemes = prevlyr + " -";
                    note.IsRest = false;
                    prevIsRest = true;
                }
                else note.Phonemes = prevlyr + " " + note.Phonemes;
                prevlyr = currlyr;
                prevIsRest = currIsRest;
                note.Lyric = "";
            }
            Log("Converting to aliases", "Completed.");
        }

        void AdjustLength(Ust ust)
        {
            Log("Adjusting length", "Starting...");
            foreach (UNote note in Ust.MainNotes)
            {
                note.Length = note.InitLength;
                foreach (UNote child in note.Children)
                {
                    if (child is null) continue;
                    double length = TransLength;
                    Oto oto = Singer.FindOto(child.Phonemes);
                    if (oto != null) length = oto.Length;
                    else Log("Converting to aliases", $"No alias found for {child.Phonemes}");
                    child.Length = MusicMath.MillisecondToTick(length, ust.Tempo);
                    if (note.Length - child.Length < 10)
                        child.Length = note.Length / 2;
                    note.Length -= child.Length;
                }
            }
            Log("Adjusting length", "Completed");
        }

        public void Error(object sender, string error)
        {
            Errors.Add($"Error on {sender.ToString()}:" + error);
        }

        public void Log(string sender, string message, bool isError=false)
        {
            if (isError)
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), $"\r\n=== Error on {sender} ===\r\n{message}\r\n", Encoding.Unicode);
                Environment.Exit(0);
            }
            else
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"), $"\r\nOn {sender}\r\n{message}\r\n", Encoding.Unicode);
            }

        }

    }
}
