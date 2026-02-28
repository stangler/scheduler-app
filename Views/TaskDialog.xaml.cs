using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SchedulerApp.Models;

namespace SchedulerApp.Views
{
    public partial class TaskDialog : Window
    {
        public ScheduleTask Task { get; private set; }
        private bool _isEdit;

        public TaskDialog(ScheduleTask? existingTask = null, DateTime? defaultDate = null)
        {
            InitializeComponent();
            
            _isEdit = existingTask != null;
            Task = existingTask != null
                ? CloneTask(existingTask)
                : new ScheduleTask { StartDateTime = defaultDate ?? DateTime.Now };

            DialogTitle.Text = _isEdit ? "タスクを編集" : "タスクを追加";
            LoadTask();
        }

        private ScheduleTask CloneTask(ScheduleTask src) => new()
        {
            Id = src.Id,
            Title = src.Title,
            Description = src.Description,
            StartDateTime = src.StartDateTime,
            EndDateTime = src.EndDateTime,
            Recurrence = src.Recurrence,
            ReminderEnabled = src.ReminderEnabled,
            ReminderMinutesBefore = src.ReminderMinutesBefore,
            Color = src.Color,
            IsCompleted = src.IsCompleted,
            ReminderFired = src.ReminderFired
        };

        private void LoadTask()
        {
            TitleBox.Text = Task.Title;
            DescriptionBox.Text = Task.Description;
            StartDatePicker.SelectedDate = Task.StartDateTime.Date;
            StartTimeBox.Text = Task.StartDateTime.ToString("HH:mm");

            if (Task.EndDateTime.HasValue)
            {
                EndDatePicker.SelectedDate = Task.EndDateTime.Value.Date;
                EndTimeBox.Text = Task.EndDateTime.Value.ToString("HH:mm");
            }

            // Set color
            foreach (ComboBoxItem item in ColorCombo.Items)
            {
                if (item.Tag?.ToString() == Task.Color)
                {
                    ColorCombo.SelectedItem = item;
                    break;
                }
            }

            // Set recurrence
            var recurrenceStr = Task.Recurrence.ToString();
            foreach (ComboBoxItem item in RecurrenceCombo.Items)
            {
                if (item.Tag?.ToString() == recurrenceStr)
                {
                    RecurrenceCombo.SelectedItem = item;
                    break;
                }
            }

            // Set reminder
            ReminderCheck.IsChecked = Task.ReminderEnabled;
            ReminderPanel.Visibility = Task.ReminderEnabled ? Visibility.Visible : Visibility.Collapsed;

            foreach (ComboBoxItem item in ReminderCombo.Items)
            {
                if (item.Tag?.ToString() == Task.ReminderMinutesBefore.ToString())
                {
                    ReminderCombo.SelectedItem = item;
                    break;
                }
            }
        }

        private void ReminderCheck_Changed(object sender, RoutedEventArgs e)
        {
            ReminderPanel.Visibility = ReminderCheck.IsChecked == true 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("タイトルを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleBox.Focus();
                return;
            }

            if (!StartDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("開始日を選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseTime(StartTimeBox.Text, out var startTime))
            {
                MessageBox.Show("開始時刻の形式が正しくありません。（例: 09:00）", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Task.Title = TitleBox.Text.Trim();
            Task.Description = DescriptionBox.Text.Trim();
            Task.StartDateTime = StartDatePicker.SelectedDate.Value.Date + startTime;
            Task.Color = (ColorCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "#4A90D9";
            Task.Recurrence = Enum.Parse<RecurrenceType>(
                (RecurrenceCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "None");
            Task.ReminderEnabled = ReminderCheck.IsChecked == true;

            if (Task.ReminderEnabled)
            {
                var tag = (ReminderCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "15";
                Task.ReminderMinutesBefore = int.Parse(tag);
                Task.ReminderFired = false; // Reset reminder
            }

            if (EndDatePicker.SelectedDate.HasValue && !string.IsNullOrWhiteSpace(EndTimeBox.Text))
            {
                if (TryParseTime(EndTimeBox.Text, out var endTime))
                    Task.EndDateTime = EndDatePicker.SelectedDate.Value.Date + endTime;
            }
            else
            {
                Task.EndDateTime = null;
            }

            DialogResult = true;
            Close();
        }

        private bool TryParseTime(string text, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(text)) return false;
            var parts = text.Split(':');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out int h) || !int.TryParse(parts[1], out int m)) return false;
            if (h < 0 || h > 23 || m < 0 || m > 59) return false;
            result = new TimeSpan(h, m, 0);
            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
