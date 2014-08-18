using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Mle.MusicPimp.Util {
    public class BackgroundTaskManager {
        private string name;
        private string entryPoint;
        public BackgroundTaskManager(string name, string entryPoint) {
            this.name = name;
            this.entryPoint = entryPoint;
        }
        //private const string taskName = "BackgroundTask";
        //private const string taskEntryPoint = "StoreBackgroundTask.BackgroundTask";
        // the task runs every ... minutes
        private const int runIntervalMinutes = 20;

        public Task RegisterBackgroundTask() {
            return RegisterBackgroundTask(name, entryPoint);
        }
        public async Task RegisterBackgroundTask(string name, string entryPoint) {
            try {
                var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
                if(backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                    backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity) {

                    foreach(var task in BackgroundTaskRegistration.AllTasks) {
                        if(task.Value.Name == name) {
                            task.Value.Unregister(true);
                        }
                    }

                    BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                    taskBuilder.Name = name;
                    taskBuilder.TaskEntryPoint = entryPoint;
                    // run once an hour
                    taskBuilder.SetTrigger(new TimeTrigger(runIntervalMinutes, oneShot: false));
                    var registration = taskBuilder.Register();
                }
            } catch(Exception) {
                // might throw an Exception with a "No such element" / "Element not found" message occasionally, what a piece of shit API
            }
        }
    }
}
