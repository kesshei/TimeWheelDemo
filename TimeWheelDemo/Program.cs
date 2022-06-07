
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheelDemo
{
    class Program
    {
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
    }
}
