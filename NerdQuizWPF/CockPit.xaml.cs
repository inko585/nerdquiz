using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace NerdQuizWPF
{
    /// <summary>
    /// Interaction logic for CockPit.xaml
    /// </summary>
    public partial class CockPit : Window
    {
        private NerdQuizViewModel vm;
        private ScoreBoard sb;
        public CockPit()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                try
                {
                    LoadSaveFile(args[1]);
                }
                catch (Exception)
                {
                    DataContext = App.CurrentViewModel;
                    vm = App.CurrentViewModel;
                }
            }
            else
            {
                DataContext = App.CurrentViewModel;
                vm = App.CurrentViewModel;
            }


            listbox.SelectedItem = listbox.Items[0];
        }

        private WebBrowser wb = new WebBrowser();
        private ImageViewer iv = new ImageViewer();

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;
            var q = (listbox.SelectedItem as ListBoxItem).Tag as Question;
            vm.CurrentQuestion = q;
        }

        public static BitmapImage BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private BitmapImage Pixelate(int pixelSize, string original)
        {


            var bitmapSource = new Bitmap(original);
            var width = bitmapSource.Width;
            var height = bitmapSource.Height;

            var pixelated = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(pixelated))
            {
                for (int y = 0; y < height; y += pixelSize)
                {
                    for (int x = 0; x < width; x += pixelSize)
                    {
                        int offsetX = Math.Min(pixelSize, width - x);
                        int offsetY = Math.Min(pixelSize, height - y);

                        var pixelColor = bitmapSource.GetPixel(x, y);

                        graphics.FillRectangle(new SolidBrush(pixelColor), x, y, offsetX, offsetY);
                    }
                }
            }

            return ToBitmapImage(pixelated);
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void WebPlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vm.CurrentQuestion.Link != "")
                {
                    wb.Navigate(vm.CurrentQuestion.Link);
                    wb.Show();
                    WindowExt.MaximizeToSpecificMonitor(wb, vm.ScoreBoardScreen);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void WebPlayStopClick(object sender, RoutedEventArgs e)
        {
            try
            {
                wb.BrowserCore.Navigate("about:blank");
                wb.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RunClick(object sender, RoutedEventArgs e)
        {
            var dia = new ScreenSelection();

            if (dia.ShowDialog() == true)
            {
                sb = new ScoreBoard();
                iv = new ImageViewer();
                wb = new WebBrowser();
                sb.Show();
                WindowExt.MaximizeToSpecificMonitor(sb, vm.ScoreBoardScreen);
            }

        }

        private void LoadQuestionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sb == null || MessageBox.Show("Dadurch wird aktuelle Runde beendet, sicher?", "Achtung", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Filter = "nq Dateien|*.nq";
                    dialog.Title = "Datei wählen";

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (sb != null)
                        {
                            sb.Close();
                            sb = null;
                        }

                        LoadSaveFile(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void LoadSaveFile(string path)
        {
            Directory.Delete(App.SessionPath, true);

            ZipFile.ExtractToDirectory(path, App.SessionPath);

            var serializer = new XmlSerializer(typeof(NerdQuizViewModel));
            using (var sr = new StreamReader(App.QuizPath))
            {
                vm = (NerdQuizViewModel)serializer.Deserialize(sr);
                vm.P1 = App.CurrentViewModel.P1;
                vm.P2 = App.CurrentViewModel.P2;
                vm.P3 = App.CurrentViewModel.P3;
                vm.P4 = App.CurrentViewModel.P4;
                vm.P5 = App.CurrentViewModel.P5;
                foreach (var cat in vm.Categories)
                {
                    foreach (var q in cat.Questions)
                    {
                        q.CategoryText = cat.Text;
                    }
                }
                vm.CurrentQuestion = vm.Cat1.Q1;
                App.CurrentViewModel = vm;
                DataContext = vm;




            }
        }

        private void Answer_Plus_Click(object sender, RoutedEventArgs e)
        {
            if (sb != null)
            {
                var b = sender as Button;
                var player = b.Tag as Player;

                player.Score += vm.CurrentQuestion.Points;
                vm.CurrentQuestion.Open = false;
            }
        }

        private void Answer_Minus_Click(object sender, RoutedEventArgs e)
        {
            if (sb != null)
            {
                var b = sender as Button;
                var player = b.Tag as Player;

                player.Score -= vm.CurrentQuestion.Points;
                vm.CurrentQuestion.Open = false;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            System.Windows.Application.Current.Shutdown();
        }

        private void CloseGameClick(object sender, RoutedEventArgs e)
        {
            if (sb != null)
            {
                sb.Close();
                vm.Reset();
                sb = null;
            }
        }

        private void x2Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sb != null)
                {
                    foreach (var cat in vm.Categories)
                    {
                        foreach (var q in cat.Questions)
                        {
                            q.Points *= 2;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ImageShowClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(vm.CurrentQuestion.ImageSavePath))
            {
                var uri = new Uri("file://" + vm.CurrentQuestion.ImageSavePath);
                try
                {
                    var bitmap = BitmapFromUri(uri);
    
                    ShowImage(bitmap);
                }
                catch
                {
                    MessageBox.Show("Kein gültiges Bild Format!");
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.CurrentQuestion.ImageSavePath))
            {
                MessageBox.Show("Datei existiert nicht!");
            }
        }

        private void ShowImage(BitmapImage bitmap)
        {

            if (!iv.IsLoaded)
            {
                iv = new ImageViewer();
            }
            iv.Image.Source = bitmap;

            iv.Show();
            iv.Activate();
            WindowExt.MaximizeToSpecificMonitor(iv, vm.ScoreBoardScreen);
        }

        private void Pixel5Click(object sender, RoutedEventArgs e)
        {
            PixelX(5);
        }
        private void Pixel4Click(object sender, RoutedEventArgs e)
        {
            PixelX(4);
        }
        private void Pixel3Click(object sender, RoutedEventArgs e)
        {
            PixelX(3);
        }
        private void Pixel2Click(object sender, RoutedEventArgs e)
        {
            PixelX(2);
        }
        private void Pixel1Click(object sender, RoutedEventArgs e)
        {
            PixelX(1);
        }


        private void PixelX(int x)
        {

            if (File.Exists(vm.CurrentQuestion.ImageSavePath))
            {
                var filePath = vm.CurrentQuestion.ImageSavePath;

                try
                {
                    var pixelImage = Pixelate(5 + x * 5, filePath);
                    ShowImage(pixelImage);
                }
                catch
                {
                    MessageBox.Show("Kein gültiges Bild Format!");
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.CurrentQuestion.ImageSavePath))
            {
                MessageBox.Show("Datei existiert nicht!");
            }
        }

        private void ImageSearchClick(object sender, RoutedEventArgs e)
        {

            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Bild Dateien|*.jpg;*.jpeg;*.png;";
            dialog.Title = "Bild wählen";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.CurrentQuestion.ImageName = System.IO.Path.GetFileName(dialog.FileName);
                if (!File.Exists(vm.CurrentQuestion.ImageSavePath))
                {
                    File.Copy(dialog.FileName, vm.CurrentQuestion.ImageSavePath);
                }

            }

        }

       



        private void ImageCloseClick(object sender, RoutedEventArgs e)
        {
            iv.Image.Source = null;
            iv.Hide();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "nq Dateien|*.nq;";
            dialog.Title = "Speicherort wählen";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(dialog.FileName))
                {
                    File.Delete(dialog.FileName);
                }
                using (var writer = new StreamWriter(App.QuizPath))
                {
                    var serializer = new XmlSerializer(typeof(NerdQuizViewModel));
                    serializer.Serialize(writer, vm);
                }

                ZipFile.CreateFromDirectory(App.SessionPath, dialog.FileName);

                App.CurrentViewModel.Status = DateTime.Now.ToString() + " Gespeichert: " + dialog.FileName;

            }



        }

        private void WebPlayClickLocal(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vm.CurrentQuestion.Link != "")
                {
                    wb = new WebBrowser();
                    wb.Navigate(vm.CurrentQuestion.Link);
                    wb.Show();
                    wb.MaximizeToSpecificMonitor(GetScreenFrom(this), maximized: false);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static System.Windows.Forms.Screen GetScreenFrom(Window window)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            return System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);

        }

        private void CloseQuestionClick(object sender, RoutedEventArgs e)
        {
            if (sb != null)
            {
                vm.CurrentQuestion.Open = false;
            }
        }
    }
}
