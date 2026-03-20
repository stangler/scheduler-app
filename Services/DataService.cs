using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SchedulerApp.Models;

namespace SchedulerApp.Services
{
    public class DataService
    {
        private readonly string _dataPath;

        public DataService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "SchedulerApp");
            Directory.CreateDirectory(folder);
            _dataPath = Path.Combine(folder, "tasks.json");
        }

        public List<ScheduleTask> LoadTasks()
        {
            if (!File.Exists(_dataPath))
                return new List<ScheduleTask>();

            try
            {
                var json = File.ReadAllText(_dataPath);
                return JsonConvert.DeserializeObject<List<ScheduleTask>>(json) ?? new List<ScheduleTask>();
            }
            catch
            {
                return new List<ScheduleTask>();
            }
        }

        public void SaveTasks(List<ScheduleTask> tasks)
        {
            var json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            File.WriteAllText(_dataPath, json);
        }
    }
}
