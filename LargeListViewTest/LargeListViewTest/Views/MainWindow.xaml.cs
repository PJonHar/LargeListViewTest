using LargeListViewTest.Classes;
using LargeListViewTest.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LargeListViewTest.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Stopwatch sw = new Stopwatch();

        private int parserProgress;
        private SerilogFileLog selectedSerilogFileLog = null;
        private LogEvent selectedLogEvent;

        /// <summary>
        /// 
        /// </summary>
        public int ParserProgress { get => parserProgress; set { parserProgress = value; NotifyPropertyChanged(nameof(ParserProgress)); } }

        /// <summary>
        /// 
        /// </summary>
        public SerilogFileLog SelectedSerilogFileLog
        {
            get => selectedSerilogFileLog; set
            {
                if (selectedSerilogFileLog != null)
                {
                    SelectedSerilogFileLog.OnSerilogParserFinished -= OnSerilogParserFinished;
                    SelectedSerilogFileLog.OnSerilogParserProgressChanged -= OnSerilogParserProgressChanged;
                }

                selectedSerilogFileLog = value;

                if (selectedSerilogFileLog != null)
                {
                    ParserProgress = 0;

                    SelectedSerilogFileLog.OnSerilogParserFinished += OnSerilogParserFinished;
                    SelectedSerilogFileLog.OnSerilogParserProgressChanged += OnSerilogParserProgressChanged;

                    sw.Start();
                    SelectedSerilogFileLog.Parse();
                }

                NotifyPropertyChanged(nameof(SelectedSerilogFileLog));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LogEvent SelectedLogEvent { get => selectedLogEvent; set { selectedLogEvent = value; NotifyPropertyChanged(nameof(SelectedLogEvent)); } }

        #region Commands

        public ICommand OKCommand { get; set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {

            InitializeComponent();

            this.DataContext = this;
        }

        #region Commands Method

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void OnSerilogParserFinished()
        {
            sw.Stop();
            Console.WriteLine("Parse took: {0}ms", sw.ElapsedMilliseconds);
        }

        private void OnSerilogParserProgressChanged(int Percentage) => ParserProgress = Percentage;

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string p)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedSerilogFileLog = null;
            SelectedSerilogFileLog = new SerilogFileLog() { FilePath = "Application20210216.log" };
        }
    }
}