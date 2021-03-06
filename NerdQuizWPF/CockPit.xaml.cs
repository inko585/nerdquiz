﻿using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
            DataContext = App.CurrentViewModel;
            vm = App.CurrentViewModel;
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

        private void WebPlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vm.CurrentQuestion.Link != "")
                {
                    wb = new WebBrowser();
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
                if (vm.PowerPointVersion != null)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Office\" + vm.PowerPointVersion + @"\PowerPoint\Options", "DisplayMonitor", vm.ScoreBoardScreen);
                }
                sb = new ScoreBoard();
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
                    dialog.Filter = "xml Dateien|*.xml";
                    dialog.InitialDirectory = @"C:\";
                    dialog.Title = "XML wählen";

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (sb != null)
                        {
                            sb.Close();
                            sb = null;
                        }

                        var serializer = new XmlSerializer(typeof(NerdQuizViewModel));
                        using (var sr = new StreamReader(dialog.FileName))
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            if (File.Exists(vm.CurrentQuestion.ImagePath))
            {
                var uri = new Uri("file://" + vm.CurrentQuestion.ImagePath);
                try
                {
                    var bitmap = new BitmapImage(uri);
                    iv.Image.Source = bitmap;
                    iv.Show();
                    WindowExt.MaximizeToSpecificMonitor(iv, vm.ScoreBoardScreen);
                }
                catch
                {
                    MessageBox.Show("Kein gültiges Bild Format!");
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.CurrentQuestion.ImagePath))
            {
                MessageBox.Show("Datei existiert nicht!");
            }
        }

        private void ImageSearchClick(object sender, RoutedEventArgs e)
        {

            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Bild Dateien|*.jpg;*.jpeg;*.png;";
            dialog.InitialDirectory = @"C:\";
            dialog.Title = "Bild wählen";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.CurrentQuestion.ImagePath = dialog.FileName;
            }

        }

        private void PPSearchClick(object sender, RoutedEventArgs e)
        {

            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "PowerPoint Dateien|*.pptx";
            dialog.InitialDirectory = @"C:\";
            dialog.Title = "Präsentation wählen";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.CurrentQuestion.PptxPath = dialog.FileName;
            }

        }

        private void PPOpenClick(object sender, RoutedEventArgs e)
        {
            if (vm.PowerPointVersion != null)
            {
                if (File.Exists(vm.CurrentQuestion.PptxPath))
                {
                    if (vm.CurrentQuestion.PptxPath.EndsWith(".pptx"))
                    {
                        var ppApp = new Microsoft.Office.Interop.PowerPoint.Application();
                        ppApp.Visible = MsoTriState.msoTrue;
                        var ppPresens = ppApp.Presentations;
                        var objPres = ppPresens.Open(vm.CurrentQuestion.PptxPath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
                        objPres.SlideShowSettings.Run();

                    }
                    else
                    {
                        MessageBox.Show("Keine gültige pptx Datei!");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(vm.CurrentQuestion.PptxPath))
                {
                    MessageBox.Show("Datei existiert nicht!");
                }
            } else
            {
                MessageBox.Show("Keine PowerPoint Installation gefunden");
            }
        }

        private void ImageCloseClick(object sender, RoutedEventArgs e)
        {
            iv.Hide();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "xml Dateien|*.xml";
            dialog.InitialDirectory = @"C:\";
            dialog.Title = "Speicherort wählen";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var writer = new StreamWriter(dialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof(NerdQuizViewModel));
                    serializer.Serialize(writer, vm);
                }

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
