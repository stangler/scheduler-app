using System;
using System.Windows;
using SchedulerApp.Models;

namespace SchedulerApp.Views
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(ScheduleTask task)
        {
            InitializeComponent();
            
            TitleText.Text = task.Title;
            TimeText.Text = $"開始: {task.StartDateTime:yyyy/MM/dd HH:mm}";

            // Position at bottom-right of screen
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 16;
            Top = workArea.Bottom - Height - 16;

            // Auto-close after 10 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            timer.Tick += (s, e) => { timer.Stop(); Close(); };
            timer.Start();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
