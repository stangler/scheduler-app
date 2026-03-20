using System;
using System.Collections.Generic;
using SchedulerApp.Models;

namespace SchedulerApp.Models
{
    public class CalendarDay
    {
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsSelected { get; set; }
        public List<ScheduleTask> Tasks { get; set; } = new();
        public bool HasTasks => Tasks.Count > 0;
    }
}
