using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LargeListViewTest.Classes
{
    public class SerilogFileLog : INotifyPropertyChanged
    {
        private LogEvent lastLogEvent;
        private ObservableCollectionEx<LogEvent> logEvents;
        private string name;
        private string description;
        private string filePath;

        private Regex patternMatching;
        private string matchExpression = @"^(?<DateTime>[^|]+)\| (?<Level>[^|]+) \| (?<MachineName>[^|]+) \| (?<Source>[^|]+) \| (?<Message>[^$]*)$";

        public delegate void SerilogParserProgressHandler(int Percentage);
        public delegate void SerilogParserFinishedHandler();

        /// <summary>
        /// 
        /// </summary>
        public event SerilogParserProgressHandler OnSerilogParserProgressChanged;

        /// <summary>
        /// 
        /// </summary>
        public event SerilogParserFinishedHandler OnSerilogParserFinished;

        /// <summary>
        /// Gets or sets the LogEvents.
        /// </summary>
        public ObservableCollectionEx<LogEvent> LogEvents { get => logEvents; private set { logEvents = value; NotifyPropertyChanged(nameof(LogEvents)); } }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get => name; private set { name = value; NotifyPropertyChanged(nameof(Name)); } }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get => description; private set { description = value; NotifyPropertyChanged(nameof(Description)); } }

        /// <summary>
        /// Gets or sets the FilePath.
        /// </summary>
        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                Name = Path.GetFileNameWithoutExtension(value);
                Description = FilePath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SerilogFileLog()
        {
            LogEvents = new ObservableCollectionEx<LogEvent>();
            patternMatching = new Regex(matchExpression, RegexOptions.Singleline | RegexOptions.Compiled);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Parse()
        {
            Task task = Task.Factory.StartNew(() => { InternalParse(); });
        }

        /// <summary>
        /// 
        /// </summary>
        private void InternalParse()
        {
            OnSerilogParserProgressChanged?.Invoke(0);

            try
            {
                if (!string.IsNullOrWhiteSpace(FilePath))
                {
                    Console.WriteLine("Starting parse for {0}", FilePath);

                    long currentLength = 0;

                    FileInfo fi = new FileInfo(FilePath);

                    if (fi.Exists)
                    {
                        Console.WriteLine("Parsing Serilog file: {0}.", FilePath);

                        fi.Refresh();

                        List<LogEvent> parsedLogEvents = new List<LogEvent>();
                        StringBuilder sb = new StringBuilder();

                        using (FileStream fileStream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Write))
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            while (streamReader.Peek() != -1)
                            {
                                sb.Append(streamReader.ReadLine());
                                LogEvent newLogEvent = ParseLogEvent(sb.ToString());
                                if (newLogEvent != null)
                                {
                                    parsedLogEvents.Add(newLogEvent);
                                    lastLogEvent = newLogEvent;
                                }

                                OnSerilogParserProgressChanged?.Invoke((int)(currentLength * 100 / fi.Length));
                                currentLength = currentLength + sb.ToString().Length;
                                sb.Clear();
                            }
                        }

                        LogEvents.ReplaceContent(parsedLogEvents);
                    }

                    Console.WriteLine("Finished parsing Serilog {0}.", FilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing Serilog." + ex.Message);
            }

            OnSerilogParserProgressChanged?.Invoke(100);

            SerilogParserFinishedHandler onSerilogParserFinished = OnSerilogParserFinished;

            if (onSerilogParserFinished == null)
                return;

            OnSerilogParserFinished();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        private LogEvent ParseLogEvent(string mes)
        {
            LogEvent logEvent = new LogEvent();

            Match matcher = patternMatching.Match(mes);

            try
            {
                if (matcher.Success)
                {
                    logEvent.Message = matcher.Groups["Message"].Value;

                    DateTime dt;
                    if (!DateTime.TryParse(matcher.Groups["DateTime"].Value, out dt))
                    {
                        Console.WriteLine("Failed to parse date {Value}", matcher.Groups["DateTime"].Value);
                    }
                    logEvent.DateTime = dt;
                    logEvent.Level = matcher.Groups["Level"].Value;
                    logEvent.MachineName = matcher.Groups["MachineName"].Value;
                    logEvent.Source = matcher.Groups["Source"].Value;
                }
                else
                {
                    if ((string.IsNullOrEmpty(mes) || (!Char.IsDigit(mes[0])) || !Char.IsDigit(mes[1])) && lastLogEvent != null)
                    {
                        // seems to be a continuation of the previous line, add it to the last event.
                        lastLogEvent.Message += Environment.NewLine;
                        lastLogEvent.Message += mes;
                        logEvent = null;
                    }
                    else
                    {
                        Console.WriteLine("Message parsing failed.");
                    }
                    if (logEvent != null)
                        logEvent.Message = mes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ParseLogEvent exception." + ex.Message);
            }

            return logEvent;
        }

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        #endregion
    }
}
