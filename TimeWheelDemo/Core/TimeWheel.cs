using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheelDemo
{
    /// <summary>
    /// 时间轮算法(终极)实现
    /// 大部分都是支持秒级，所以，按照秒级进行实现
    /// 任务体得有它自己的任务唯一的ID
    /// </summary>
    public class TimeWheel
    {
        /// <summary>
        /// 时间调度列表
        /// </summary>
        private ConcurrentDictionary<long, HashSet<string>> TimeTasks { get; set; } = new();
        /// <summary>
        /// 任务列表
        /// </summary>
        private ConcurrentDictionary<string, IJob> ScheduledTasks { get; set; } = new();
        /// <summary>
        /// 是否运行中
        /// </summary>
        private bool isRuning = false;
        /// <summary>
        /// 运行核心
        /// </summary>
        public void Run()
        {
            isRuning = true;
            Task.Run(() =>
            {
                while (isRuning)
                {
                    var timeStamp = GenerateTimestamp(DateTime.Now);
                    Task.Run(() => { Trigger(timeStamp); });
                    var offset = 500 - DateTime.Now.Millisecond;
                    SpinWait.SpinUntil(() => false, 1000 + offset);
                }
            });
        }
        public void Stop()
        {
            isRuning = false;
        }
        /// <summary>
        /// 定时触发器
        /// </summary>
        /// <param name="timeStamp"></param>
        private void Trigger(long timeStamp)
        {
            var oldTimeStamp = timeStamp - 1;
            var list = TimeTasks.Keys.Where(t => t <= oldTimeStamp).ToList();
            foreach (var item in list)
            {
                TimeTasks.TryRemove(item, out var _);
            }
            TimeTasks.TryGetValue(timeStamp, out var result);
            if (result?.Any() == true)
            {
                var Now = DateTime.Now;
                foreach (var id in result)
                {
                    //找到指定的任务
                    if (ScheduledTasks.TryGetValue(id, out IJob job))
                    {
                        Task.Run(() => { job.Execute(); });
                        var NewTime = job.GetNextTime();
                        if (NewTime.HasValue && NewTime >= Now)
                        {
                            AddTask(NewTime.Value, id);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="scheduledTask"></param>
        private void AddTask(DateTime dateTime, string ID)
        {
            var timeStamp = GenerateTimestamp(dateTime);
            TimeTasks.AddOrUpdate(timeStamp, new HashSet<string>() { ID }, (k, v) =>
            {
                v.Add(ID);
                return v;
            });
        }
        /// <summary>
        /// 增加一个任务
        /// </summary>
        public void AddTask(IJob job)
        {
            if (ScheduledTasks.ContainsKey(job.ID))
            {
                throw new ArgumentException($"{nameof(job)} 参数 {nameof(job.ID)}重复!");
            }
            else
            {
                ScheduledTasks.TryAdd(job.ID, job);
            }
            var time = DateTime.Now;
            var NewTime = job.GetNextTime();
            if (NewTime.HasValue && NewTime >= time)
            {
                Console.WriteLine($"新增任务：{job.ID}");
                AddTask(NewTime.Value, job.ID);
            }
        }
        /// <summary>
        /// 移除某个任务的Task
        /// </summary>
        /// <param name="ID"></param>
        public void RemoveTask(string ID)
        {
            var ids = ScheduledTasks.Values.Where(t => t.ID == ID)?.Select(t => t.ID).ToList();
            if (ids?.Any() == true)
            {
                foreach (var id in ids)
                {
                    if (ScheduledTasks.TryGetValue(id, out var job))
                    {
                        job.Cancel();
                        ScheduledTasks.TryRemove(id, out _);
                    }
                }
            }
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        private long GenerateTimestamp(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
        }
    }
}
