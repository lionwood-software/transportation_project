using System;
using TimeZoneConverter;

namespace Transport.Worker.DailyReports.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime? GetUkraineTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value.ToUniversalTime(), TZConvert.GetTimeZoneInfo("FLE Standard Time"));
        }
    }
}
