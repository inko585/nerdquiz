using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NerdQuizWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            CurrentViewModel = new NerdQuizViewModel();
            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }
        public static NerdQuizViewModel CurrentViewModel { get; set; }

        public static void OnExit(object sender, EventArgs e)
        {
            if (Directory.Exists(SessionPath))
            {
                try
                {
                    Directory.Delete(SessionPath, true);
                }
                catch (Exception) { }
            }
        }



        public static string SessionPath => Path.GetTempPath() + "/" + SessionKey;
        public static string ImagePath => Path.GetTempPath() + "/" + SessionKey + "/images";
        public static string PPTXPath => Path.GetTempPath() + "/" + SessionKey + "/pptx";
        public static string QuizPath => Path.GetTempPath() + "/" + SessionKey + "/quiz.xml";

        private static string sessionKey;
        public static string SessionKey
        {
            get
            {
                if (sessionKey == null)
                {
                    sessionKey = RandomString(10);
                    Directory.CreateDirectory(SessionPath);
                    Directory.CreateDirectory(ImagePath);
                    Directory.CreateDirectory(PPTXPath);
                }
                return sessionKey;
            }
        }


        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void DeleteFilesRecursively(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {                    
                }
            }
          
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (var subFolder in subFolders)
            {
                DeleteFilesRecursively(subFolder);
            }
        }
    }
}
