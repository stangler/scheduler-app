using System;

namespace SchedulerApp.Models
{
    public enum RecurrenceType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public class ScheduleTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; } = DateTime.Now;
        public DateTime? EndDateTime { get; set; }
        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
        public bool ReminderEnabled { get; set; } = false;
        public int ReminderMinutesBefore { get; set; } = 15;
        public string Color { get; set; } = "#4A90D9";
        public bool IsCompleted { get; set; } = false;
        public bool ReminderFired { get; set; } = false;

        public override string ToString() => Title;
    }
}
