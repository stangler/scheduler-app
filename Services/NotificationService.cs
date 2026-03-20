using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using SchedulerApp.Models;

namespace SchedulerApp.Services
{
    public class NotificationService
    {
        private readonly DispatcherTimer _timer;
        private Func<List<ScheduleTask>>? _getTasksCallback;
        private Action<ScheduleTask>? _onReminderFired;

        public NotificationService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _timer.Tick += CheckReminders;
        }

        public void Start(Func<List<ScheduleTask>> getTasksCallback, Action<ScheduleTask> onReminderFired)
        {
            _getTasksCallback = getTasksCallback;
            _onReminderFired = onReminderFired;
            _timer.Start();
        }

        public void Stop() => _timer.Stop();

        private void CheckReminders(object? sender, EventArgs e)
        {
            if (_getTasksCallback == null) return;

            var now = DateTime.Now;
            var tasks = _getTasksCallback();

            foreach (var task in tasks.Where(t => t.ReminderEnabled && !t.ReminderFired && !t.IsCompleted))
            {
                var reminderTime = task.StartDateTime.AddMinutes(-task.ReminderMinutesBefore);

                if (now >= reminderTime && now < task.StartDateTime)
                {
                    ShowNotification(task);
                    task.ReminderFired = true;
                    _onReminderFired?.Invoke(task);
                }
            }
        }

        private void ShowNotification(ScheduleTask task)
        {
            var window = new Views.NotificationWindow(task);
            window.Show();
        }
    }
}
