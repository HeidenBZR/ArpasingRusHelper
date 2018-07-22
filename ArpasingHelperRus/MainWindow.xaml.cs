using AtlasConverter.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AtlasConverter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double width;
        double height;
        double xScale;
        double xScaleMin;
        double xScaleMax;
        double scrollSpeed;
        double zoomSpeed;
        double lenScale;
        List<Button> UIs;
        List<object> NewUIs;
        List<UNote> DrawnNotes;
        public Core Core;
        public delegate void NoteChangedEventHandler(UNote note);
        public event NoteChangedEventHandler OnNoteChanged;
        public delegate void TextChangedEventHandler();
        public event TextChangedEventHandler OnTextChanged;
        public delegate void WindowEventHandler();
        public event WindowEventHandler OnSave;
        public delegate void LengthEventHandler(int value);
        public event LengthEventHandler OnLengthChanged;
        public delegate void SingerEventHandler(string path);
        public event SingerEventHandler OnSingerChanged;

        public MainWindow()
        {
            InitializeComponent();
            xScale = 25;
            xScaleMin = 10;
            xScaleMax = 100;
            lenScale = 120;
            ScrollViewer.CanContentScroll = true;
            scrollSpeed = 1;
            zoomSpeed = 0.12;
            Loaded += Loaded_MainWindow;
            App.Core.SetWindow(this);
        }

        void Loaded_MainWindow(object sender, RoutedEventArgs e)
        {
            width = RootCanvas.ActualWidth;
            height = RootCanvas.ActualHeight;
            Init();
        }

        void Init()
        {
            DrawGrid();
        }

        public void SetText(UNote[] Notes)
        {
            LyricWindow.Text = "";
            foreach (UNote note in Notes)
            {
                if (note.IsRest) LyricWindow.Text += "\r\n";
                else LyricWindow.AppendText($" {note.Lyric}");
            }
            LyricWindow.Text = LyricWindow.Text.Replace("\r\n ", "\r\n");
        }

        public string[] GetText()
        {
            LyricWindow.Text = LyricWindow.Text.Replace("\r\n ", "\r\n");
            LyricWindow.Text = LyricWindow.Text.Replace(" \r\n", "\r\n");
            LyricWindow.Text = LyricWindow.Text.Trim(' ');
            string text = LyricWindow.Text.Replace("\r\n", " ");
            text = text.Trim(' ');
            text = text.Replace("  ", " ");
            text = text.Replace("  ", " ");
            Console.WriteLine(text);
            string[] lyrics = text.Split(' ');
            return lyrics;
        }

        #region Drawing

        void DrawGrid()
        {
            GridCanvas.Children.Clear();
            for (int i = 0; i < width / xScale; i++)
            {
                if (i % 16 == 0)
                {
                    Line line = new Line()
                    {
                        X1 = i * xScale - 0.5,
                        X2 = i * xScale + 0.5,
                        Y1 = 0,
                        Y2 = height,
                        StrokeThickness = 2,
                        Stroke = (Brush)Resources["BorderBrush"]
                    };
                    GridCanvas.Children.Add(line);
                }
                else if (i % 4 == 0)
                {
                    Line line = new Line()
                    {
                        X1 = i * xScale,
                        X2 = i * xScale,
                        Y1 = 0,
                        Y2 = height,
                        SnapsToDevicePixels = true,
                        Stroke = (Brush)Resources["GridBrush"]
                    };
                    GridCanvas.Children.Add(line);
                }
                else
                {
                    Line line = new Line()
                    {
                        X1 = i * xScale,
                        X2 = i * xScale,
                        Y1 = 0,
                        Y2 = height,
                        SnapsToDevicePixels = true,
                        Stroke = (Brush)Resources["GridBrush"]
                    };
                    line.StrokeDashArray.Add(4);
                    line.StrokeDashArray.Add(4);
                    GridCanvas.Children.Add(line);
                }
            }
        }

        public void DrawNotes(UNote[] Notes, UNote[] NewNotes)
        {
            double pos = DrawMainNotes(Notes);
            double newpos = DrawNewNotes(NewNotes);
            width = Math.Max(pos * xScale, newpos * xScale);
            width += xScale * 16 - width % (xScale * 16);
            width = width > RootCanvas.ActualWidth ? width : RootCanvas.ActualWidth;
            RootCanvas.Width = width;
            DrawGrid();
        }

        public double DrawMainNotes(UNote[] Notes)
        {
            NotesCanvas.Children.Clear();
            UIs = new List<Button>();
            DrawnNotes = new List<UNote>();
            double pos = 0;
            foreach (UNote note in Notes)
            {
                Button button = new Button()
                {
                    Style = (Style)Resources["NoteStyle"],
                    Width = note.Length / lenScale * xScale,
                    Content = $"{note.Lyric} [{note.Phonemes}]",
                    Margin = new Thickness(pos * xScale, 10, 0, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    SnapsToDevicePixels = true,
                    BorderThickness = note.IsPhonemesForced ? new Thickness(3) : new Thickness(1),
                    Foreground = (Brush)Resources["ForeBrush"],
                    Background = (Brush)Resources["NoteBrush"]
                };
                if (note.IsRest) button.Opacity = 0.5;
                NotesCanvas.Children.Add(button);
                note.UI = (object)button;
                UIs.Add(button);
                button.Click += delegate (object sender, RoutedEventArgs e)
                {
                    if (Keyboard.IsKeyDown(Key.D0)) note.ResetColor();
                    else if (Keyboard.IsKeyDown(Key.D1)) note.SetColor(1);
                    else if (Keyboard.IsKeyDown(Key.D2)) note.SetColor(2);
                    else if (Keyboard.IsKeyDown(Key.D3)) note.SetColor(3);
                    else if (Keyboard.IsKeyDown(Key.D4)) note.SetColor(4);
                    else if (Keyboard.IsKeyDown(Key.D5)) note.SetColor(5);
                    else if (Keyboard.IsKeyDown(Key.D6)) note.SetColor(6);
                    else if (Keyboard.IsKeyDown(Key.D7)) note.SetColor(7);
                    else if (Keyboard.IsKeyDown(Key.D8)) note.SetColor(8);
                    else if (Keyboard.IsKeyDown(Key.D9)) note.SetColor(9);
                    else { note.IsRest = !note.IsRest; note.ResetPhonemes(); }
                    OnNoteChanged(note);
                };
                button.MouseRightButtonUp += delegate
                {
                    TextBox block = new TextBox();
                    block.Text = note.Phonemes;
                    block.Width = 100;
                    block.Margin = button.Margin;
                    NotesCanvas.Children.Add(block);
                    block.LostFocus += delegate (object sender, RoutedEventArgs e)
                    {
                        note.ForcePhonemes(block.Text);
                        NotesCanvas.Children.Remove(block);
                        OnNoteChanged(note);
                    };
                };
                DrawnNotes.Add(note);
                pos += note.Length / lenScale;
            }
            return pos;
        }

        public double DrawNewNotes(UNote[] NewNotes)
        {
            AliasesCanvas.Children.Clear();
            NewUIs = new List<object>();
            double newpos = 0;
            foreach (UNote note in NewNotes)
            {
                Label label = new Label()
                {
                    //Style = (Style)Resources["NewNoteStyle"],
                    Width = note.Length / lenScale * xScale,
                    Content = note.Phonemes,
                    Margin = new Thickness(newpos * xScale, 30, 0, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    BorderBrush = (Brush)Resources["BorderBrush"],
                    BorderThickness = new Thickness(1),
                    SnapsToDevicePixels = true,
                    Padding = new Thickness(1),
                    Foreground = (Brush)Resources["ForeBrush"],
                    Background = (Brush)Resources["BackgroundBrush"]
                };
                if (note.Color != null)
                {
                    label.Background = new SolidColorBrush(note.Color.Color);
                    label.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    label.BorderBrush = (Brush)Resources["BackgroundBrush"];
                    Label colorlabel = new Label()
                    {
                        Width = label.Width,
                        HorizontalContentAlignment = label.HorizontalContentAlignment,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        BorderThickness = label.BorderThickness,
                        SnapsToDevicePixels  = true,
                        Padding = label.Padding,
                        Foreground = (Brush)Resources["ForeBrush"],
                        Content = note.Color.Suffix,
                        Margin = new Thickness(newpos * xScale, 50, 0, 0)
                    };
                    if (note.IsRest) colorlabel.Opacity = 0;
                    AliasesCanvas.Children.Add(colorlabel);
                }
                AliasesCanvas.Children.Add(label);
                if (note.IsRest) label.Opacity = 0;
                note.UI = (object)label;
                NewUIs.Add((object)label);
                newpos += note.Length / lenScale;
            }
            return newpos;
        }

        #endregion

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            OnTextChanged();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) { Console.WriteLine(e.Delta); return; }
            int delta = e.Delta;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) Zoom(delta);
            else Scroll(delta);
        }

        void Scroll(double delta)
        {
            double offset = delta * scrollSpeed;
            offset = Math.Truncate(offset / xScale) * xScale;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) offset *= 2;
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + offset);
            Console.WriteLine(offset);
        }

        void Zoom(double delta)
        {
            double xScaleNew = xScale + delta * zoomSpeed;
            if (xScaleNew > xScaleMax) xScaleNew = xScaleMax;
            if (xScaleNew < xScaleMin) xScaleNew = xScaleMin;
            xScale = xScaleNew;
            double mouse = Mouse.GetPosition(ScrollViewer).X * (delta / Math.Abs(delta));
            double horizontalOffset = (ScrollViewer.HorizontalOffset) / ScrollViewer.ScrollableWidth;
            Console.WriteLine(horizontalOffset);
            OnTextChanged();
            ScrollViewer.ScrollToHorizontalOffset((ScrollViewer.ScrollableWidth) * horizontalOffset + mouse);
            Console.WriteLine(ScrollViewer.ScrollableWidth * horizontalOffset);
        }

        private void NewLengthDefault_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(NewLengthDefault.Text, out int newLengthDefault))
                if (newLengthDefault > 1 && newLengthDefault < 10000)
                    if (OnLengthChanged != null) OnLengthChanged(newLengthDefault);
        }

        private void ChangeSingerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSingerDialog();
        }

        public void ShowSingerDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".wav";
            openFileDialog.InitialDirectory = Directory.GetParent(Core.Singer.Dir).FullName;
            openFileDialog.FileOk += delegate (object s, CancelEventArgs e2)
            {
                OnSingerChanged(openFileDialog.FileName);
            };
            openFileDialog.ShowDialog();
        }

        private void ChangeColorMap_Click(object sender, RoutedEventArgs e)
        {
            App.Core.Log("Color map editor", "Starting...");
            try
            {
                ColorMapDialog colorMapDialog = new ColorMapDialog(Core.Singer.Colormap);
                colorMapDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                App.Core.Log("Color map editor", $"{ex.Message }\r\n{ex.Source}\r\n{ex.TargetSite}\r\n{ex.StackTrace}", true);
                return;
            }
            App.Core.Log("Color map editor", "Completed.");

        }
    }
}
