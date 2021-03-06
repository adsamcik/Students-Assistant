﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Main {
    public class ClassWeekStyleSelector : DataTemplateSelector {

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
            var listItem = (DisplayClass)item;

            if (listItem.classInstance != null) {
                int currentWeek = System.Convert.ToInt32(Math.Ceiling((double)DateTime.Now.DayOfYear / 7)) % 2 != 0 ? 1 : 2;
                if (listItem.classInstance.weekType == 0 || (int)listItem.classInstance.weekType == currentWeek)
                    return WeekView.resources["standardClassTemplate"] as DataTemplate;
                else
                    return WeekView.resources["wrongWeekClassTemplate"] as DataTemplate;
            }
            else if (listItem.taskInstance != null) {
                return WeekView.resources["standardTaskTemplate"] as DataTemplate;
            }

            throw new ArgumentNullException("not task or classInstance");

        }
    }
}
