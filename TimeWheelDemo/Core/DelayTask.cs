using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheelDemo
{
    /// <summary>
    /// 延时任务（死信任务），普通任务
    /// 间隔指定的时间
    /// </summary>
    public class DelayTask : IScheduledTask
    {
        private DateTime TimeOutTime;
        public DelayTask(TimeSpan timeSpan)
        {
            this.TimeOutTime = DateTime.Now.AddSeconds(timeSpan.TotalSeconds);
        }
        public DateTime? GetNextTime()
        {
            return TimeOutTime;
        }
    }
}
