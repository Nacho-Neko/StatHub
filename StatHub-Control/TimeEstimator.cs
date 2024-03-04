namespace StatHub_Control
{
    internal class TimeEstimator
    {
        public static TimeSpan CalculateRemainingTime(double elapsedSeconds, double completedPercentage)
        {
            if (completedPercentage <= 0 || completedPercentage >= 1)
            {
                throw new ArgumentException("完成的百分比必须在0和1之间（不包含0和1）。");
            }
            // 计算总时间（秒）
            double totalTimeSeconds = elapsedSeconds / completedPercentage;
            // 计算剩余时间（秒）
            double remainingTimeSeconds = totalTimeSeconds - elapsedSeconds;
            // 将剩余时间转换为TimeSpan对象
            TimeSpan remainingTime = TimeSpan.FromSeconds(remainingTimeSeconds);
            return remainingTime;
        }
    }
}
