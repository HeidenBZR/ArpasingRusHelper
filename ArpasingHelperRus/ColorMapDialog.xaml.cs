using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using AtlasConverter.Classes;

namespace AtlasConverter
{
    /// <summary>
    /// Логика взаимодействия для ColorMapDialog.xaml
    /// </summary>
    public partial class ColorMapDialog : Window
    {

        Colormap Colormap;
        UColor CurrentColor;

        public ColorMapDialog(Colormap colormap)
        {
            Colormap = colormap;
            App.Core.Log("Color map editor", "0");
            InitializeComponent();
            App.Core.Log("Color map editor", "1");
            ColorMapView.ItemsSource = colormap.Colors;
            App.Core.Log("Color map editor", "2");
        }

        void SaveColor()
        {
            App.Core.Log("Color map editor", "Saving color...");
            if (CurrentColor != null)
            {
                CurrentColor.Name = NameTextBox.Text;
                CurrentColor.Suffix = SuffixTextBox.Text;
                CurrentColor.Color = Color.FromRgb(ColorCanvas.R, ColorCanvas.G, ColorCanvas.B);
                ColorMapView.UpdateLayout();
                ColorMapView.InvalidateVisual();
                Refresh();
            }
            App.Core.Log("Color map editor", "Saved color.");
        }

        private void ColorMapView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveColor();
            ListView listView = (ListView)sender;
            UColor color = (UColor)listView.SelectedItem;
            if (color == null) return;
            NameTextBox.Text = color.Name;
            SuffixTextBox.Text = color.Suffix;
            ColorCanvas.R = color.Color.R;
            ColorCanvas.G = color.Color.G;
            ColorCanvas.B = color.Color.B;
            CurrentColor = color;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            UColor color = new UColor(NameTextBox.Text, SuffixTextBox.Text, ColorCanvas.R, ColorCanvas.G, ColorCanvas.B);
            if (Colormap.AddColor(color))
            {
                CurrentColor = color;
                Refresh();
            }
            else
            {
                MessageBox.Show("Suffix must be unique.", "Error on adding a Color", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentColor = null;
            }
        }

        void Refresh()
        {
            var ItemsSource = ColorMapView.ItemsSource;
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
            view.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Colormap.RemoveColor(((Button)sender).Tag.ToString());
            Refresh();
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NameTextBox.Text.Length > 2 && SuffixTextBox != null)
            {
                SuffixTextBox.Text = $"_{NameTextBox.Text[0].ToString().ToUpper()}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Colormap.Reopen();
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Colormap.Save();
            Close();
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentColor is null)
            {
                UColor color = new UColor("Color", "_", 0, 0, 0);
                if (Colormap.AddColor(color))
                {
                    CurrentColor = color;
                }
            }
            SaveColor();
        }
    }
}
