using System;

namespace QuoteFeed.Core.Tests
{
    public static class Utils
    {
        public static DateTime ParseUtc(string dateTime)
        {
            return DateTimeOffset.Parse(dateTime).UtcDateTime;
        }
    }
}
