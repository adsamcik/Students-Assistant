﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozvrh {
    class Extensions {
        public static int GetIso8601WeekOfYear(DateTime date) {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
                date = date.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static int GetWeekOfYear(DateTime date) {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

        public static DateTime WhenIsNext(ClassInstance classInstance) {
            DateTime now = DateTime.Now;
            int currentDay = (int)now.DayOfWeek - 1;
            if (currentDay == -1) currentDay = 6;

            int expireIn;
            if (currentDay == (int)classInstance.day)
                expireIn = classInstance.from > now.TimeOfDay ? 0 : 7;
            else
                expireIn = currentDay <= (int)classInstance.day ?
                    (int)classInstance.day - currentDay :
                    6 - currentDay + (int)classInstance.day;

            now = now.AddDays(expireIn);

            if (classInstance.weekType != WeekType.EveryWeek)
                if (GetWeekOfYear(now) % 2 == 0 && classInstance.weekType == WeekType.OddWeek)
                    now = now.AddDays(7);
                else if (classInstance.weekType == WeekType.EveryWeek)
                    now = now.AddDays(7);

            return new DateTime(now.Year, now.Month, now.Day, classInstance.from.Hours, classInstance.from.Minutes, classInstance.from.Seconds);
        }
    }
}
