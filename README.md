# TimeWheelDemo
一个基于时间轮原理的定时器

## 对时间轮的理解
其实我是有一篇文章针对时间轮的理论理解的，但是，我想，为啥我看完原理后，会采用这样的方式去实现。

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


## 框架特点是啥

只有一个字，轻。
用的舒服点。
