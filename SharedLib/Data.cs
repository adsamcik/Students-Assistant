﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace SharedLib {
    public static class Data {
        static DataStore dataStore = new DataStore();

        public static List<Class> classes { get { return dataStore.classes; } }
        public static List<Teacher> teachers { get { return dataStore.teachers; } }
        public static List<ClassInstance> classInstances { get { return dataStore.classInstances; } }
        public static List<Task> tasks { get { return dataStore.tasks; } }

        public static Windows.ApplicationModel.Resources.ResourceLoader loader = new Windows.ApplicationModel.Resources.ResourceLoader();

        static StorageFolder roamingFolder;

        public static void Save() {
            dataStore.Save();
        }

        public static async void SaveToFile() {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Javascript object", new List<string>() { ".json" });
            savePicker.FileTypeChoices.Add("iCalendar", new List<string>() { ".ics" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Students_Assistent-data";

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null) {
                CachedFileManager.DeferUpdates(file);
                // write to file
                if(file.ContentType == ".ics")
                await FileIO.WriteTextAsync(file, dataStore.ExportToiCalendar());
                else if(file.ContentType == ".json")
                    await FileIO.WriteTextAsync(file, dataStore.ExportToJson());

                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete) {
                    //this.textBlock.Text = "File " + file.Name + " was saved.";
                }
                else {
                    //this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
                }
            }
        }

        public static void Initialize() {
            roamingFolder = ApplicationData.Current.RoamingFolder;
            dataStore.Load();
        }

        public static void AddClass(Class Class) {
            classes.Add(Class);
            dataStore.Save();
        }

        public static void AddTeacher(Teacher teacher) {
            teachers.Add(teacher);
            dataStore.Save();
        }

        public static void AddClassInstance(ClassInstance classInstance) {
            classInstances.Add(classInstance);
            dataStore.Save();
        }

        public static void AddTask(Task task) {
            tasks.Add(task);
            dataStore.Save();
        }

        public static void ArchiveTask(Task task) {
            int index = tasks.FindIndex(x => x.uid == task.uid);
            dataStore.archivedTasks.Add(tasks[index]);
            tasks.RemoveAt(index);
            dataStore.Save();
        }

        public static void DeleteTeacher(Teacher teacher) {
            dataStore.teachers.Remove(teacher);
            dataStore.Save();
        }

        public static void DeleteTask(Task task) {
            dataStore.tasks.Remove(task);
            dataStore.Save();
        }

        public static void DeleteClass(Class Class) {
            dataStore.classes.Remove(Class);
            dataStore.Save();
        }

        public static void DeleteClassInstance(ClassInstance classInstance) {
            dataStore.classInstances.Remove(classInstance);
            dataStore.Save();
        }

        class DataStore {
            public List<Teacher> teachers = new List<Teacher>();
            public List<Class> classes = new List<Class>();
            public List<ClassInstance> classInstances = new List<ClassInstance>();
            public List<Task> tasks = new List<Task>();
            public List<Task> archivedTasks = new List<Task>();

            public async void Save() {
                StorageFile dataFile = await roamingFolder.CreateFileAsync("dataFile", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(dataFile, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects }));
            }

            public async void Load() {
                System.Diagnostics.Debug.WriteLine(roamingFolder.Path);
                try {
                    StorageFile dataFile = await roamingFolder.GetFileAsync("dataFile");
                    dataStore = JsonConvert.DeserializeObject<DataStore>(await FileIO.ReadTextAsync(dataFile));
                }
                catch {
                    Save();
                }
            }

            public string ExportToJson() {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }

            public string ExportToiCalendar() {
                string iCal = WNLiCal("BEGIN:VCALENDAR");
                iCal += WNLiCal("VERSION:2.0");

                foreach (var item in Data.classInstances)
                    iCal += WriteiCalEvent(iCal, item);

                iCal += WNLiCal("END:VCALENDAR");
                return iCal;
            }

            string WriteiCalEvent(string iCal, ClassInstance cInstance) {
                string Event = WNLiCal("BEGIN:VEVENT");
                Event += WNLiCal("SUMMARY:" + cInstance.classData.ToString());
                Event += WNLiCal("DTSTART:" + ToICalDateFormat(Extensions.WhenIsNext(cInstance)));
                Event += WNLiCal("DTEND:" + ToICalDateFormat(Extensions.WhenIsNext(cInstance).AddMinutes((cInstance.from - cInstance.to).TotalMinutes)));
                Event += WNLiCal("LOCATION:" + cInstance.room);
                //Event += WNLiCal("SEQUENCE:" + 5);
                Event += WNLiCal("END:VEVENT");
                return Event;
            }

            string WNLiCal(string data) {
                return data + Environment.NewLine;
            }

            string ToICalDateFormat(DateTime dateTime) {
                dateTime = dateTime.ToUniversalTime();
                return dateTime.Year.ToString() + dateTime.Month.ToString("00") + dateTime.Day.ToString("00") + "T" + dateTime.Hour.ToString("00") + dateTime.Minute.ToString("00") + dateTime.Second.ToString("00") + "Z";
            }

            public void ClearAll() {
                classes.Clear();
                teachers.Clear();
                classInstances.Clear();
                Save();
            }

        }
    }
}
