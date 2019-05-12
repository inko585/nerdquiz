﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NerdQuizWPF
{
    public class NerdQuizViewModel : INotifyPropertyChanged
    {

        // ---------- INotifyPropertyChanged Interface ----------
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Reset()
        {
            foreach (var p in Players)
            {
                p.Reset();
            }

            foreach (var c in Categories)
            {
                c.Reset();
            }
        }

        [XmlIgnore]
        public Screen[] Screens
        {
            get
            {
                return Screen.AllScreens;
            }
        }

        [XmlIgnore]
        public Screen ScoreBoardScreen { get; set; }

        //public ObservableCollection<Screen> Screens
        //{
        //    get
        //    {
        //        var screens = new ObservableCollection<Screen>();
        //        return  { }
        //    }
        //}

        [XmlIgnore]
        public List<Category> Categories
        {
            get
            {
                return new List<Category>() { Cat1, Cat2, Cat3, Cat4, Cat5 };
            }
        }

        [XmlIgnore]
        public List<Player> Players
        {
            get
            {
                return new List<Player>() { P1, P2, P3, P4, P5 };
            }
        }

        public NerdQuizViewModel()
        {
            Cat1 = new Category("Cat1", this);
            Cat2 = new Category("Cat2", this);
            Cat3 = new Category("Cat3", this);
            Cat4 = new Category("Cat4", this);
            Cat5 = new Category("Cat5", this);

            AddDefaultPlayers();

            CurrentQuestion = Cat1.Q1;
        }

        private void AddDefaultPlayers()
        {
            P1 = new Player("Player1");
            P1.Deselectable = false;
            P2 = new Player("Player2");
            P2.Deselectable = false;
            P3 = new Player("Player3");
            P4 = new Player("Player4");
            P5 = new Player("Player5");
        }

        private Question cq;

        [XmlIgnore]
        public Question CurrentQuestion
        {
            get
            {
                return cq;
            }
            set
            {
                if (value != null)
                {
                    cq = value;
                    NotifyPropertyChanged("CurrentQuestion");
                }
            }
        }

        [XmlIgnore]
        public Player P1 { get; set; }
        [XmlIgnore]
        public Player P2 { get; set; }
        [XmlIgnore]
        public Player P3 { get; set; }
        [XmlIgnore]
        public Player P4 { get; set; }
        [XmlIgnore]
        public Player P5 { get; set; }

        public Category Cat1 { get; set; }
        public Category Cat2 { get; set; }
        public Category Cat3 { get; set; }
        public Category Cat4 { get; set; }
        public Category Cat5 { get; set; }

    }

    public class Player : INotifyPropertyChanged
    {
        // ---------- INotifyPropertyChanged Interface ----------
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Reset()
        {
            Score = 0;
        }

        public Player() { }
        public Player(string name)
        {
            Name = name;
            Deselectable = true;
        }
        private bool deselectable;
        public bool Deselectable
        {
            get
            {
                return deselectable;
            }
            set
            {
                deselectable = value;
                NotifyPropertyChanged("Deselectable");
            }
        }
        private bool active = true;
        public bool IsActive
        {
            get
            {
                return active;
            }

            set
            {
                active = value;
                if (active)
                {
                    ScoreBoardWidth = "1*";
                }
                else
                {
                    ScoreBoardWidth = "0";
                }
                NotifyPropertyChanged("ScoreBoardWidth");
                NotifyPropertyChanged("IsActive");
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }
        private int score;

        public int Score
        {
            get { return score; }
            set { score = value; NotifyPropertyChanged("Score"); }
        }

        public string ScoreBoardWidth
        {
            get; private set;
        }
    }

    public class Category : INotifyPropertyChanged
    {
        // ---------- INotifyPropertyChanged Interface ----------
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public Category() { }
        public Category(string catName, NerdQuizViewModel vm)
        {
            Text = catName;
            Q1 = new Question(100, this);
            Q2 = new Question(200, this);
            Q3 = new Question(300, this);
            Q4 = new Question(600, this);
            Q5 = new Question(1000, this);

        }

        public void Reset()
        {
            Q1.Points = 100;
            Q2.Points = 200;
            Q3.Points = 300;
            Q4.Points = 600;
            Q5.Points = 1000;

            foreach (var q in Questions)
            {
                q.Open = true;
            }
        }

        [XmlIgnore]
        public List<Question> Questions
        {
            get
            {
                return new List<Question> { Q1, Q2, Q3, Q4, Q5 };
            }
        }

        private string txt;
        public string Text
        {
            get
            {
                return txt;
            }
            set
            {
                txt = value;
                NotifyPropertyChanged("Text");
                foreach (var q in Questions)
                {
                    if (q != null)
                    {
                        q.CategoryText = Text;
                    }
                }
            }
        }
        public Question Q1 { get; set; }
        public Question Q2 { get; set; }
        public Question Q3 { get; set; }
        public Question Q4 { get; set; }
        public Question Q5 { get; set; }
    }

    public class Question : INotifyPropertyChanged
    {
        // ---------- INotifyPropertyChanged Interface ----------
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Question() { }
        public Question(int points, Category cat)
        {
            Text = "";
            Link = "";
            Points = points;
            CategoryText = cat.Text;
            Description = CategoryText + " " + Points;

        }

        private int points;

        private string categoryText = "";
        [XmlIgnore]
        public string CategoryText
        {
            get
            {
                return categoryText;
            }
            set
            {
                categoryText = value;
                Description = value + " " + Points;
            }
        }

        public int Points
        {
            get
            {
                return points;
            }

            set
            {
                points = value;

                Description = CategoryText + " " + Points;
                SetScoreBoardText();
            }
        }
        private string desc;
        [XmlIgnore]
        public string Description
        {
            get
            {
                return desc;
            }
            set
            {
                desc = value;
                NotifyPropertyChanged("Description");
            }
        }
        public override string ToString()
        {
            return Description;
        }

        private string txt;
        public string Text
        {
            get
            {
                return txt;
            }
            set
            {
                txt = value;
                NotifyPropertyChanged("Text");
            }
        }

        public string Answer { get; set; }
        private string link;
        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                link = value;
            }
        }

        private string imagePath;

        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; NotifyPropertyChanged("ImagePath"); }
        }


        private bool open = true;
        [XmlIgnore]
        public bool Open
        {
            get
            {
                return open;
            }

            set
            {
                open = value;
                SetScoreBoardText();
                NotifyPropertyChanged("Open");

            }
        }

        private void SetScoreBoardText()
        {
            if (!open)
            {
                scoreBoardText = "";
            }
            else
            {
                scoreBoardText = Points.ToString();
            }

            NotifyPropertyChanged("ScoreBoardText");
        }

        private string scoreBoardText;
        public string ScoreBoardText
        {
            get
            {
                return scoreBoardText;
            }
        }
    }
}
