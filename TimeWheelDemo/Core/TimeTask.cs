using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheelDemo
{
    /// <summary>
    /// 定时器任务，普通任务
    /// 间隔指定的时间
    /// </summary>
    public class TimeTask : IScheduledTask
    {
        private TimeSpan timeSpan;
        public TimeTask(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
        }
        public DateTime? GetNextTime()
        {
            return DateTime.Now.AddSeconds(timeSpan.TotalSeconds);
        }
    }
}
