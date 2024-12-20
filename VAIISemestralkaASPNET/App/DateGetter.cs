﻿


namespace VAIISemestralkaASPNET.App
{
    public class DateGetter
    {
        public static List<DateTime> NextDates()
        {
            List<DateTime> dates = new List<DateTime>();

            DateTime pointer = new (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

            while (dates.Count < 5) 
            {
                if (!IsWeekend(pointer))
                {
                    dates.Add(pointer);
                }

                pointer = pointer.AddDays(1);
            }

            return dates;
        }

        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
