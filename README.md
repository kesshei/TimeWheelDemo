# TimeWheelDemo
一个基于时间轮原理的定时器

## 对时间轮的理解
其实我是有一篇文章([.Net 之时间轮算法(终极版)](#https://blog.csdn.net/i2blue/article/details/123608471))针对时间轮的理论理解的，但是，我想，为啥我看完时间轮原理后，会采用这样的方式去实现。


可能只是一些小技巧不上大雅之堂吧，大佬看看就行了。

当然如果大佬有别的看法，也请不吝赐教，互相交流，一起进步。

## 项目是基于时间轮理解上的一个任务调度轻型框架

作用么，造个小轮子，顺便，对任务调度的实现多一些深度的思考和了解。

## 这个框架实现了啥子

实现了对方法的定时 循环执行。
大概样子是下面这样的
```
TimeWheel timeWheel = new TimeWheel();

timeWheel.AddTask(new Job("定时1", () => { Console.WriteLine($"定时每1秒 {DateTime.Now}"); }, new TimeTask(TimeSpan.FromSeconds(1))));

timeWheel.AddTask(new Job("定时2", () => { Console.WriteLine($"定时2每10执行 {DateTime.Now}"); }, new TimeTask(TimeSpan.FromSeconds(10))));

timeWheel.AddTask(new Job("CRON", () => { Console.WriteLine($"CRON 每5秒 {DateTime.Now}"); }, new CronTask("*/5 * * * * *")));

timeWheel.AddTask(new Job("死信", () => { Console.WriteLine($"死信执行 {DateTime.Now}"); }, new DelayTask(TimeSpan.FromSeconds(20))));

timeWheel.AddTask(new Job("死信1", () => { Console.WriteLine($"死信1执行 {DateTime.Now}"); }, new DelayTask(TimeSpan.FromSeconds(10))));

timeWheel.Run();
```
能实现，定时任务，死信任务，能支持CRON表达式
### 定时任务如下 （TimeTask）
```
timeWheel.AddTask(new Job("定时1", () => { Console.WriteLine($"定时每1秒 {DateTime.Now}"); }, new TimeTask(TimeSpan.FromSeconds(1))));

timeWheel.AddTask(new Job("定时2", () => { Console.WriteLine($"定时2每10执行 {DateTime.Now}"); }, new TimeTask(TimeSpan.FromSeconds(10))));
```
通过 TimeTask进行实现的

### CRON定时任务 （CronTask）
主要是基于 NCrontab 库，实现对CRON表达式的解析。省的自己从头解析了

```
timeWheel.AddTask(new Job("CRON", () => { Console.WriteLine($"CRON 每5秒 {DateTime.Now}"); }, new CronTask("*/5 * * * * *")));
```
这样就能实现对特定任务的执行
### 死信任务，延迟任务 （DelayTask）
很多死信都是基于消息队列的，但是应该也有一些实际应用中的应用场景吧。看具体了。

```
timeWheel.AddTask(new Job("死信", () => { Console.WriteLine($"死信执行 {DateTime.Now}"); }, new DelayTask(TimeSpan.FromSeconds(20))));

timeWheel.AddTask(new Job("死信1", () => { Console.WriteLine($"死信1执行 {DateTime.Now}"); }, new DelayTask(TimeSpan.FromSeconds(10))));
```
实现

### 能按照指定ID名，来实现对任务的移除
比如下边的，就能直接移除死信的任务。可以别的定时器执行了任务，然后，对此任务进行清除。
```
timeWheel.RemoveTask("死信");
```
基本上，只要没有被执行的任务，都会被取消执行的。

## 效果图

![](https://tupian.wanmeisys.com/markdown/1654596770108-ee5f9a64-63e6-4296-949a-108dc11a31ad.gif)

## 代码详解
先看看main函数的示例
```csharp
static void Main(string[] args)
{
            TimeWheel timeWheel = new TimeWheel();
            timeWheel.AddTask(new Job("定时1", () => { Console.WriteLine($"定时每1秒 {DateTime.Now}"); }, new TimeTask(TimeSpan.FromSeconds(1))));
            timeWheel.AddTask(new Job("定时2", () => { Console.WriteLine($"定时2每10执行 {DateTime.Now}"); }, new TimeTask(TimeSpan.FromSeconds(10))));
            timeWheel.AddTask(new Job("CRON", () => { Console.WriteLine($"CRON 每5秒 {DateTime.Now}"); }, new CronTask("*/5 * * * * *")));
            timeWheel.AddTask(new Job("死信", () => { Console.WriteLine($"死信执行 {DateTime.Now}"); }, new DelayTask(TimeSpan.FromSeconds(20))));
            timeWheel.AddTask(new Job("死信1", () => { Console.WriteLine($"死信1执行 {DateTime.Now}"); }, new DelayTask(TimeSpan.FromSeconds(10))));
            timeWheel.Run();
            Task.Run(() =>
            {
                Thread.Sleep(10 * 1000);
                timeWheel.RemoveTask("死信");
                Console.WriteLine("移除死信");
                Thread.Sleep(10 * 1000);
                timeWheel.RemoveTask("CRON");
                Console.WriteLine("移除任务CRON");
            });
            Console.WriteLine("开始运行时间轮!");
            Console.ReadLine();
}
```
### 时间调度
```csharp
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
```
### 核心的时间轮
```csharp
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
```
### 任务体 (IJob)
```csharp
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
```
## 框架特点是啥

只有一个字，轻。
用的舒服点。
有问题大家一起沟通

## 框架地址

https://github.com/kesshei/TimeWheelDemo
