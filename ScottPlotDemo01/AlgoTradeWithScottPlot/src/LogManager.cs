using System.Drawing;

namespace AlgoTradeWithScottPlot
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    public class LogManager
    {
        private static LogManager? _instance;
        private static readonly object _lock = new object();
        private RichTextBox? _richTextBox;
        private readonly object _logLock = new object();
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();
        
        private LogManager() { }

        public static LogManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LogManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void AttachRichTextBox(RichTextBox richTextBox)
        {
            lock (_logLock)
            {
                _richTextBox = richTextBox;
                
                // DoubleClick event'ini ekle
                _richTextBox.DoubleClick += RichTextBox_DoubleClick;
                
                // Existing log entries'leri RichTextBox'a yaz
                foreach (var entry in _logEntries)
                {
                    AppendToRichTextBox(entry);
                }
            }
        }

        private void RichTextBox_DoubleClick(object? sender, EventArgs e)
        {
            Clear();
            LogInfo("Loglar temizlendi (DoubleClick ile)");
        }

        public void DetachRichTextBox()
        {
            lock (_logLock)
            {
                if (_richTextBox != null)
                {
                    _richTextBox.DoubleClick -= RichTextBox_DoubleClick;
                    _richTextBox = null;
                }
            }
        }

        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void LogCritical(string message)
        {
            Log(LogLevel.Critical, message);
        }

        public void Log(LogLevel level, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            };

            lock (_logLock)
            {
                _logEntries.Add(entry);
                
                if (_richTextBox != null)
                {
                    if (_richTextBox.InvokeRequired)
                    {
                        _richTextBox.Invoke(new Action(() => AppendToRichTextBox(entry)));
                    }
                    else
                    {
                        AppendToRichTextBox(entry);
                    }
                }
            }
        }

        private void AppendToRichTextBox(LogEntry entry)
        {
            if (_richTextBox == null) return;

            var color = GetLogLevelColor(entry.Level);
            var prefix = GetLogLevelPrefix(entry.Level);
            var formattedMessage = $"[{entry.Timestamp:HH:mm:ss}] {prefix} {entry.Message}\n";

            _richTextBox.SelectionStart = _richTextBox.TextLength;
            _richTextBox.SelectionLength = 0;
            _richTextBox.SelectionColor = color;
            _richTextBox.AppendText(formattedMessage);
            _richTextBox.SelectionColor = _richTextBox.ForeColor;
            
            // Auto-scroll to bottom
            _richTextBox.SelectionStart = _richTextBox.TextLength;
            _richTextBox.ScrollToCaret();
        }

        private Color GetLogLevelColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => Color.Gray,
                LogLevel.Info => Color.Blue,
                LogLevel.Warning => Color.Orange,
                LogLevel.Error => Color.Red,
                LogLevel.Critical => Color.DarkRed,
                _ => Color.Black
            };
        }

        private string GetLogLevelPrefix(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "[DEBUG]",
                LogLevel.Info => "[INFO]",
                LogLevel.Warning => "[WARN]",
                LogLevel.Error => "[ERROR]",
                LogLevel.Critical => "[CRITICAL]",
                _ => "[LOG]"
            };
        }

        public void Clear()
        {
            lock (_logLock)
            {
                _logEntries.Clear();
                
                if (_richTextBox != null)
                {
                    if (_richTextBox.InvokeRequired)
                    {
                        _richTextBox.Invoke(new Action(() => _richTextBox.Clear()));
                    }
                    else
                    {
                        _richTextBox.Clear();
                    }
                }
            }
        }

        public List<LogEntry> GetLogEntries()
        {
            lock (_logLock)
            {
                return new List<LogEntry>(_logEntries);
            }
        }

        public List<LogEntry> GetLogEntries(LogLevel minLevel)
        {
            lock (_logLock)
            {
                return _logEntries.Where(entry => entry.Level >= minLevel).ToList();
            }
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}";
        }
    }
}