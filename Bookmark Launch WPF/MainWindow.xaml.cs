using System;
using System.Collections.Generic;
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
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Bookmark_Launch_WPF
{
    public partial class MainWindow : Window
    {
        private const string FILE_NAME = "bookmark_data";
        private string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Bookmark Launcher/";
        public string FilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(fileDirectory))
                {
                    string path = fileDirectory + FILE_NAME;
                    new FileInfo(path).Directory.Create();
                    return path;
                }
                else
                    return null;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            TxtUrl.Focus();
            var data = Load();
            if (data.Count() > 0)
                foreach (var item in data)
                    Lv.Items.Add(new ListViewItem() { Content = item });
        }

        private List<string> Load()
        {
            var data = new List<string>();
            if (File.Exists(FilePath))
                using (var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(stream))
                {
                    int count = reader.ReadInt32();
                    if (count > 0)
                        for (int i = 0; i < count; i++)
                            data.Add(reader.ReadString());
                }
            return data;
        }

        private void Save()
        {
            using (var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Lv.Items.Count);
                foreach (var item in Lv.Items)
                    writer.Write(GetItemContent(item));
            }
        }

        private void AddItem()
        {
            Lv.Items.Add(new ListViewItem { Content = TxtUrl.Text });
            Save();
            TxtUrl.Text = string.Empty;
        }

        private void HandleButtonState()
        {
            BtnRemove.Visibility
                = BtnOpen.Visibility
                = Lv.SelectedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LaunchUrl(string url)
        {
            System.Diagnostics.Process.Start(string.Format("{0}{1}", url.Contains("http://www.") || url.Contains("http://") ? string.Empty : "http://", url));
        }

        private string GetItemContent(object lvi)
        {
            return lvi.ToString().Replace(lvi.GetType().FullName + ": ", "");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void Lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnCopy.Visibility = Lv.SelectedItems.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
            HandleButtonState();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var data = Load();
            while (Lv.SelectedItems.Count > 0)
            {
                data.Remove(GetItemContent(Lv.SelectedItem));
                Lv.Items.Remove(Lv.SelectedItem);
            }
            Save();
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Lv.Items)
                LaunchUrl(GetItemContent(item));
            Close();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            while (Lv.SelectedItems.Count > 0)
            {
                var item = Lv.SelectedItem;
                LaunchUrl(GetItemContent(item));
                Lv.SelectedItems.Remove(item);
            }
        }

        private void TxtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtUrl.Text))
            {
                BtnAdd.IsEnabled = true;
                if (e.Key.ToString() == "Return")
                {
                    AddItem();
                    BtnAdd.IsEnabled = false;
                }
            }
            else
                BtnAdd.IsEnabled = false;
        }

        private void TxtUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            BtnAdd.Visibility = Visibility.Visible;
        }

        private void TxtUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            BtnAdd.Visibility = string.IsNullOrWhiteSpace(TxtUrl.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetItemContent(Lv.SelectedItem));
        }
    }
}
