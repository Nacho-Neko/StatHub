namespace StatHub.Control
{
    public class FixedSizeLog
    {
        private Queue<string> logs = new Queue<string>();
        private int maxSize = 23;
        public void Log(string log)
        {
            if (logs.Count >= maxSize)
            {
                logs.Dequeue();
            }
            logs.Enqueue(log);
        }

        public IEnumerable<string> GetLogs()
        {
            return logs;
        }
    }
}
