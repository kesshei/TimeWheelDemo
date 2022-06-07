using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeWheelDemo
{
    /// <summary>
    /// 任务体
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// 任务ID,唯一
        /// </summary>
        /// <returns></returns>
        public string ID { get; }
        /// <summary>
        /// 脚本
        /// </summary>
        /// <returns></returns>
        public void Execute();
        /// <summary>
        /// 取消执行
        /// </summary>
        public void Cancel();
        /// <summary>
        /// 获取任务执行时间
        /// </summary>
        /// <returns></returns>
        public DateTime? GetNextTime();
    }
}
