using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatHub.Control.Model
{
    internal class DiskConsole
    {
        public string path;
        public string size;
        public string status;
        public string progress;
        public double interval;
        public DateTime update_at;
        public DiskConsole(string path, string size, string progress)
        {
            this.path = path;
            this.size = size;
            this.status = string.Empty;
            this.progress = progress;
            update_at = DateTime.Now;
        }
        public string ToIntervar(double seconds)
        {
            if (seconds == 0)
                return "计算中";
            double minutes = seconds / 60;
            double remainingSeconds = seconds % 60;
            if (minutes < 1)
                return $"{(int)remainingSeconds}秒";
            return $"{(int)minutes}分{(int)remainingSeconds}秒";
        }
    }
}
