using System;
using static NCrontab.CrontabSchedule;

namespace TimeWheelDemo.Common
{
    /// <summary>
    /// CRON 帮助类
    /// </summary>
    public static class CronHelper
    {
        /// <summary>
        /// 尝试转换为Cron表达式
        /// </summary>
        /// <param name="cron"></param>
        /// <returns></returns>
        public static bool TryParse(string cron)
        {
            try
            {
                Parse(cron);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 尝试获取下一个CRON表达式的时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="cron"></param>
        /// <returns></returns>
        public static DateTime? GetNextTime(DateTime dateTime, string cron)
        {
            try
            {
                var ex = Parse(cron, new ParseOptions() { IncludingSeconds = true });
                return ex.GetNextOccurrence(dateTime);
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
