using LargeListViewTest.Enums;
using System;
using System.ComponentModel;

namespace LargeListViewTest.Classes
{
    public class LogEvent : INotifyPropertyChanged
    {
        private DateTime dateTime;
        private string level;
        private string machineName;
        private string source;
        private string message;
        private LogEventLevel eventType;

        /// <summary>
        /// 
        /// </summary>
        public LogEvent() { }

        /// <summary>
        /// Gets or sets the DateTime.
        /// </summary>
        public DateTime DateTime { get => dateTime; set { dateTime = value; NotifyPropertyChanged(nameof(DateTime)); } }

        /// <summary>
        /// Gets or sets the Level.
        /// </summary>
        public string Level { get => level; set { level = value; EventType = ParseEventType(value); NotifyPropertyChanged(nameof(Level)); } }

        /// <summary>
        /// Gets or sets the Machine Name.
        /// </summary>
        public string MachineName { get => machineName; set { machineName = value; NotifyPropertyChanged(nameof(MachineName)); } }

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        public string Source { get => source; set { source = value; NotifyPropertyChanged(nameof(Source)); } }

        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        public string Message { get => message; set { message = value; NotifyPropertyChanged(nameof(Message)); } }

        /// <summary>
        /// Gets or sets the EventType.
        /// </summary>
        public LogEventLevel EventType { get => eventType; set { eventType = value; NotifyPropertyChanged(nameof(EventType)); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private LogEventLevel ParseEventType(string value)
        {
            LogEventLevel eventType = LogEventLevel.Unknown;

            switch (value)
            {
                case "Verbose":
                    eventType = LogEventLevel.Verbose;
                    break;
                case "Debug":
                    eventType = LogEventLevel.Debug;
                    break;
                case "Information":
                    eventType = LogEventLevel.Information;
                    break;
                case "Warning":
                    eventType = LogEventLevel.Warning;
                    break;
                case "Error":
                    eventType = LogEventLevel.Error;
                    break;
                case "Fatal":
                    eventType = LogEventLevel.Fatal;
                    break;
            }

            return eventType;
        }

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        #endregion
    }
}