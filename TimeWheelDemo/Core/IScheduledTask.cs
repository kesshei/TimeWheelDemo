using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheelDemo
{
    /// <summary>
    /// 时间调度方式
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>
        /// 获取下一个时间
        /// </summary>
        /// <returns></returns>
        public DateTime? GetNextTime();
    }
}
