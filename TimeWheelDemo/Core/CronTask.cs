using NCrontab;
using System;

namespace TimeWheelDemo
{
    /// <summary>
    /// 定时器任务，CRON表达式任务
    /// </summary>
    public class CronTask : IScheduledTask
    {
        private CrontabSchedule expression;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="Cron"> */30 * * * * *</param>
        public CronTask(string Cron)
        {
            this.expression = CrontabSchedule.Parse(Cron, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
        }
        public DateTime? GetNextTime()
        {
            return expression.GetNextOccurrence(DateTime.Now);
        }
    }
}
