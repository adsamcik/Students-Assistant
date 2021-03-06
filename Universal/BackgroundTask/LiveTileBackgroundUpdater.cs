﻿using System;
using Windows.ApplicationModel.Background;
using SharedLib;

namespace BackgroundTasks {
    public sealed class LiveTileBackgroundUpdater : IBackgroundTask {
        public void Run(IBackgroundTaskInstance taskInstance) {           
            PrepareLiveTile();
        }

        const string updateBackroundTileTaskName = "BackgroundTileNotificationUpdate";
        const string taskEntryPoint = "BackgroundTasks.LiveTileBackgroundUpdater";

        public static void RegisterBackgroundTileUpdate(uint triggerIn) {
            if (triggerIn > 43200) return;
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == updateBackroundTileTaskName) {
                    task.Value.Unregister(true);
                }
            }

            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = updateBackroundTileTaskName;
            taskBuilder.TaskEntryPoint = taskEntryPoint;
            taskBuilder.SetTrigger(new TimeTrigger(triggerIn, true));
            var registration = taskBuilder.Register();
        }

        public static void PrepareLiveTile() {
            RegisterBackgroundTileUpdate((uint)Math.Ceiling((NotificationManager.PrepareLiveTile() - DateTime.Now).TotalMinutes));
        }
    }
}
