using System;

namespace TimeWheelDemo
{
    public class Job : IJob
    {
        private Action Action;
        private IScheduledTask scheduledTask;
        private bool IsCancel = false;
        public Job(string Name, Action action, IScheduledTask scheduledTask)
        {
            this.ID = Name;
            this.Action = action;
            this.scheduledTask = scheduledTask;
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException(nameof(Name));
            }
        }
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }

        public void Execute()
        {
            if (IsCancel)
            {
                return;
            }
            Action?.Invoke();
        }

        public void Cancel()
        {
            IsCancel = true;
        }

        public DateTime? GetNextTime()
        {
            return this.scheduledTask.GetNextTime();
        }
    }
}
