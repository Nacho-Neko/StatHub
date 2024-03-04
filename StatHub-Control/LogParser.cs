using System.Text.RegularExpressions;

namespace StatHub.Control
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
    }
    public class DiskEntry
    {
        public int Id { get; set; }
        public string Size { get; set; }
        public string DiskSize { get; set; }
        public string Directory { get; set; }
    }

    public class Sector
    {
        public string DiskFarmIndex;
        public string CompletionPercentage;
        public Sector(string DiskFarmIndex, string CompletionPercentage)
        {
            this.DiskFarmIndex = DiskFarmIndex;
            this.CompletionPercentage = CompletionPercentage;
        }
    }
    public class LogParser
    {
        private static Regex logregex = new Regex(@"(?<timestamp>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z)\s+(?<level>\w+)\s+(?<source>(.*)(::)[^:]+):\s+(?<message>.+)");
        public static LogEntry LogParse(string logLine)
        {
            // 正则表达式用于匹配日志的各个部分
            var match = logregex.Match(logLine);
            if (!match.Success)
            {
                return null;
            }
            return new LogEntry
            {
                Timestamp = DateTime.Parse(match.Groups["timestamp"].Value).ToUniversalTime(),
                Level = match.Groups["level"].Value,
                Source = match.Groups["source"].Value,
                Message = match.Groups["message"].Value
            };
        }

        public static int disk_farm_index(string Source)
        {
            string DiskFarmIndex;
            var diskFarmIndexMatch = Regex.Match(Source, @"disk_farm_index=(\d+)");
            if (diskFarmIndexMatch.Success)
            {
                DiskFarmIndex = diskFarmIndexMatch.Groups[1].Value;
                return int.Parse(DiskFarmIndex);
            }
            return -1;
        }

        public static string disk_piece_sync(string Source)
        {
            var diskFarmIndexMatch = Regex.Match(Source, @"sync\s+(\d+(\.\d+)?%)");
            if (diskFarmIndexMatch.Success)
            {
                return diskFarmIndexMatch.Groups[1].Value;
            }
            return string.Empty;
        }

        public static Sector CompleteParse(LogEntry logEntry)
        {
            string? DiskFarmIndex = null;
            string? CompletionPercentage = null;

            // 从source中提取disk_farm_index
            var diskFarmIndexMatch = Regex.Match(logEntry.Source, @"disk_farm_index=(\d+)");
            if (diskFarmIndexMatch.Success)
            {
                DiskFarmIndex = diskFarmIndexMatch.Groups[1].Value;
            }
            // 从message中提取完成百分比
            var completionPercentageMatch = Regex.Match(logEntry.Message, @"(\d+\.\d+)% complete");
            if (completionPercentageMatch.Success)
            {
                CompletionPercentage = completionPercentageMatch.Groups[1].Value + "%";
            }
            return new Sector(DiskFarmIndex, CompletionPercentage);
        }
    }

}
