using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SchedulerApp.Models;
using SchedulerApp.Services;
using SchedulerApp.Views;

namespace SchedulerApp
{
    public partial class MainWindow : Window
    {
        private List<ScheduleTask> _allTasks = new();
        private DateTime _currentMonth = DateTime.Today;
        private DateTime _selectedDate = DateTime.Today;
        private readonly DataService _dataService = new();
        private readonly NotificationService _notificationService = new();
        private string _currentFilter = "all";

        public MainWindow()
        {
            InitializeComponent();
            _allTasks = _dataService.LoadTasks();
            _notificationService.Start(() => _allTasks, OnReminderFired);
            
            TodayLabel.Text = $"今日: {DateTime.Today:yyyy年M月d日 (ddd)}";
            RefreshCalendar();
            RefreshTaskList();
        }

        protected override void OnClosed(EventArgs e)
        {
            _notificationService.Stop();
            _dataService.SaveTasks(_allTasks);
            base.OnClosed(e);
        }

        private void OnReminderFired(ScheduleTask task)
        {
            _dataService.SaveTasks(_allTasks);
        }

        // ===================== Calendar =====================

        private void RefreshCalendar()
        {
            MonthLabel.Text = _currentMonth.ToString("yyyy年M月");
            CalendarGrid.Children.Clear();

            var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            var startOffset = (int)firstDay.DayOfWeek; // 0=Sun

            var today = DateTime.Today;
            var totalDays = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);

            for (int i = 0; i < 42; i++)
            {
                var dayOffset = i - startOffset;
                var date = firstDay.AddDays(dayOffset);
                bool isCurrentMonth = date.Month == _currentMonth.Month;
                bool isToday = date == today;
                bool isSelected = date == _selectedDate;

                var tasksOnDay = GetTasksForDate(date);

                var dayCell = CreateDayCell(date, isCurrentMonth, isToday, isSelected, tasksOnDay);
                CalendarGrid.Children.Add(dayCell);
            }
        }

        private List<ScheduleTask> GetTasksForDate(DateTime date)
        {
            var result = new List<ScheduleTask>();
            foreach (var task in _allTasks)
            {
                if (TaskOccursOnDate(task, date))
                    result.Add(task);
            }
            return result;
        }

        private bool TaskOccursOnDate(ScheduleTask task, DateTime date)
        {
            var taskDate = task.StartDateTime.Date;
            if (task.Recurrence == RecurrenceType.None)
                return taskDate == date;
            if (date < taskDate) return false;

            return task.Recurrence switch
            {
                RecurrenceType.Daily => true,
                RecurrenceType.Weekly => (int)(date - taskDate).TotalDays % 7 == 0,
                RecurrenceType.Monthly => date.Day == taskDate.Day,
                RecurrenceType.Yearly => date.Day == taskDate.Day && date.Month == taskDate.Month,
                _ => false
            };
        }

        private Button CreateDayCell(DateTime date, bool isCurrentMonth, bool isToday, bool isSelected, List<ScheduleTask> tasks)
        {
            var btn = new Button { Style = (Style)Resources["DayCellStyle"] };

            // Background
            if (isSelected)
                btn.Background = new SolidColorBrush(Color.FromRgb(74, 144, 217));
            else if (isToday)
                btn.Background = new SolidColorBrush(Color.FromRgb(235, 245, 251));
            else
                btn.Background = Brushes.White;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Day number
            var dayText = new TextBlock
            {
                Text = date.Day.ToString(),
                FontSize = 13,
                FontWeight = isToday ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 2),
                Foreground = isSelected ? Brushes.White
                    : isToday ? new SolidColorBrush(Color.FromRgb(74, 144, 217))
                    : !isCurrentMonth ? new SolidColorBrush(Color.FromRgb(200, 200, 200))
                    : date.DayOfWeek == DayOfWeek.Sunday ? new SolidColorBrush(Color.FromRgb(231, 76, 60))
                    : date.DayOfWeek == DayOfWeek.Saturday ? new SolidColorBrush(Color.FromRgb(74, 144, 217))
                    : new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Grid.SetRow(dayText, 0);
            grid.Children.Add(dayText);

            // Task dots
            if (tasks.Count > 0 && isCurrentMonth)
            {
                var dotsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };

                int showCount = Math.Min(tasks.Count, 3);
                for (int i = 0; i < showCount; i++)
                {
                    var taskColor = tasks[i].Color;
                    Color c;
                    try { c = (Color)ColorConverter.ConvertFromString(taskColor); }
                    catch { c = Color.FromRgb(74, 144, 217); }

                    var dot = new System.Windows.Shapes.Ellipse
                    {
                        Width = 5, Height = 5,
                        Fill = isSelected ? Brushes.White : new SolidColorBrush(c),
                        Margin = new Thickness(1, 0, 1, 0)
                    };
                    dotsPanel.Children.Add(dot);
                }
                Grid.SetRow(dotsPanel, 2);
                grid.Children.Add(dotsPanel);
            }

            btn.Content = grid;
            var capturedDate = date;
            btn.Click += (s, e) =>
            {
                _selectedDate = capturedDate;
                _currentFilter = "day";
                RefreshCalendar();
                RefreshTaskList();
            };

            return btn;
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            RefreshCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            RefreshCalendar();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = DateTime.Today;
            _selectedDate = DateTime.Today;
            _currentFilter = "today";
            RefreshCalendar();
            RefreshTaskList();
        }

        // ===================== Task List =====================

        private void RefreshTaskList()
        {
            TaskListPanel.Children.Clear();

            List<ScheduleTask> filtered;
            string headerText;

            if (_currentFilter == "today")
            {
                filtered = _allTasks.Where(t => TaskOccursOnDate(t, DateTime.Today)).OrderBy(t => t.StartDateTime).ToList();
                headerText = $"今日のタスク";
            }
            else if (_currentFilter == "upcoming")
            {
                filtered = _allTasks.Where(t => t.StartDateTime.Date >= DateTime.Today)
                    .OrderBy(t => t.StartDateTime).ToList();
                headerText = "今後のタスク";
            }
            else if (_currentFilter == "day")
            {
                filtered = GetTasksForDate(_selectedDate).OrderBy(t => t.StartDateTime).ToList();
                headerText = _selectedDate.ToString("M月d日 (ddd)");
            }
            else
            {
                filtered = _allTasks.OrderBy(t => t.StartDateTime).ToList();
                headerText = "すべてのタスク";
            }

            SelectedDateLabel.Text = headerText;
            TaskCountLabel.Text = $"{filtered.Count}件";

            if (filtered.Count == 0)
            {
                var empty = new TextBlock
                {
                    Text = "タスクがありません",
                    FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                };
                TaskListPanel.Children.Add(empty);
                return;
            }

            foreach (var task in filtered)
                TaskListPanel.Children.Add(CreateTaskCard(task));
        }

        private Border CreateTaskCard(ScheduleTask task)
        {
            Color cardColor;
            try { cardColor = (Color)ColorConverter.ConvertFromString(task.Color); }
            catch { cardColor = Color.FromRgb(74, 144, 217); }

            var card = new Border
            {
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 0, 10),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, cardColor.R, cardColor.G, cardColor.B)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            card.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black, Opacity = 0.05, BlurRadius = 8, ShadowDepth = 1
            };

            var outerGrid = new Grid();
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            outerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var colorBar = new System.Windows.Shapes.Rectangle
            {
                Fill = new SolidColorBrush(cardColor),
                RadiusX = 10, RadiusY = 10
            };
            Grid.SetColumn(colorBar, 0);
            outerGrid.Children.Add(colorBar);

            var content = new Grid { Margin = new Thickness(12, 10, 10, 10) };
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Title row
            var titleRow = new Grid();
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = task.Title,
                FontSize = 14, FontWeight = FontWeights.SemiBold,
                Foreground = task.IsCompleted
                    ? new SolidColorBrush(Color.FromRgb(180, 180, 180))
                    : new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(titleBlock, 0);

            // Action buttons
            var actionPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var completeBtn = new Button
            {
                Content = task.IsCompleted ? "↩" : "✓",
                Width = 28, Height = 28,
                Background = task.IsCompleted
                    ? new SolidColorBrush(Color.FromRgb(230, 230, 230))
                    : new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Foreground = Brushes.White,
                FontSize = 13, BorderThickness = new Thickness(0),
                Margin = new Thickness(4, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = task.IsCompleted ? "未完了に戻す" : "完了にする"
            };
            completeBtn.Style = (Style)Resources["ActionButtonStyle"];
            var capturedTask = task;
            completeBtn.Click += (s, e) =>
            {
                capturedTask.IsCompleted = !capturedTask.IsCompleted;
                _dataService.SaveTasks(_allTasks);
                RefreshTaskList();
                RefreshCalendar();
            };

            var editBtn = new Button
            {
                Content = "✎", Width = 28, Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                Foreground = Brushes.White, FontSize = 13,
                BorderThickness = new Thickness(0), Margin = new Thickness(4, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand, ToolTip = "編集"
            };
            editBtn.Style = (Style)Resources["ActionButtonStyle"];
            editBtn.Click += (s, e) => EditTask(capturedTask);

            var deleteBtn = new Button
            {
                Content = "🗑", Width = 28, Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White, FontSize = 12,
                BorderThickness = new Thickness(0), Margin = new Thickness(4, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand, ToolTip = "削除"
            };
            deleteBtn.Style = (Style)Resources["ActionButtonStyle"];
            deleteBtn.Click += (s, e) => DeleteTask(capturedTask);

            actionPanel.Children.Add(completeBtn);
            actionPanel.Children.Add(editBtn);
            actionPanel.Children.Add(deleteBtn);
            Grid.SetColumn(actionPanel, 1);

            titleRow.Children.Add(titleBlock);
            titleRow.Children.Add(actionPanel);
            Grid.SetRow(titleRow, 0);
            content.Children.Add(titleRow);

            // Date/time info
            var timeText = FormatTaskTime(task);
            var timeBlock = new TextBlock
            {
                Text = timeText, FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                Margin = new Thickness(0, 4, 0, 0)
            };
            Grid.SetRow(timeBlock, 1);
            content.Children.Add(timeBlock);

            // Description
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                var descBlock = new TextBlock
                {
                    Text = task.Description, FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                Grid.SetRow(descBlock, 2);
                content.Children.Add(descBlock);
            }

            Grid.SetColumn(content, 1);
            outerGrid.Children.Add(content);
            card.Child = outerGrid;
            return card;
        }

        private string FormatTaskTime(ScheduleTask task)
        {
            var recurrenceLabel = task.Recurrence switch
            {
                RecurrenceType.Daily => " 🔁毎日",
                RecurrenceType.Weekly => " 🔁毎週",
                RecurrenceType.Monthly => " 🔁毎月",
                RecurrenceType.Yearly => " 🔁毎年",
                _ => ""
            };
            var reminder = task.ReminderEnabled ? " 🔔" : "";
            var end = task.EndDateTime.HasValue ? $" ～ {task.EndDateTime.Value:HH:mm}" : "";
            return $"📅 {task.StartDateTime:yyyy/MM/dd HH:mm}{end}{recurrenceLabel}{reminder}";
        }

        // ===================== Task Actions =====================

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskDialog(defaultDate: _selectedDate) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                _allTasks.Add(dialog.Task);
                _dataService.SaveTasks(_allTasks);
                RefreshCalendar();
                RefreshTaskList();
            }
        }

        private void EditTask(ScheduleTask task)
        {
            var dialog = new TaskDialog(task) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                var idx = _allTasks.FindIndex(t => t.Id == task.Id);
                if (idx >= 0) _allTasks[idx] = dialog.Task;
                _dataService.SaveTasks(_allTasks);
                RefreshCalendar();
                RefreshTaskList();
            }
        }

        private void DeleteTask(ScheduleTask task)
        {
            var result = MessageBox.Show(
                $"「{task.Title}」を削除しますか？",
                "タスクの削除", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _allTasks.RemoveAll(t => t.Id == task.Id);
                _dataService.SaveTasks(_allTasks);
                RefreshCalendar();
                RefreshTaskList();
            }
        }

        // ===================== Filters =====================

        private void SetFilterButtonStyles(Button active)
        {
            foreach (var btn in new[] { FilterAllBtn, FilterTodayBtn, FilterUpcomingBtn })
            {
                btn.Background = btn == active
                    ? new SolidColorBrush(Color.FromRgb(74, 144, 217))
                    : new SolidColorBrush(Color.FromRgb(240, 242, 245));
                btn.Foreground = btn == active ? Brushes.White : new SolidColorBrush(Color.FromRgb(102, 102, 102));
            }
        }

        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            _currentFilter = "all";
            SetFilterButtonStyles(FilterAllBtn);
            RefreshTaskList();
        }

        private void FilterToday_Click(object sender, RoutedEventArgs e)
        {
            _currentFilter = "today";
            SetFilterButtonStyles(FilterTodayBtn);
            RefreshTaskList();
        }

        private void FilterUpcoming_Click(object sender, RoutedEventArgs e)
        {
            _currentFilter = "upcoming";
            SetFilterButtonStyles(FilterUpcomingBtn);
            RefreshTaskList();
        }
    }
}
