namespace Machly.Web.Utils
{
    public static class DateRangeHelper
    {
        public static (DateTime? from, DateTime? to, string scope) Resolve(string? scope, DateTime? from, DateTime? to)
        {
            var now = DateTime.UtcNow;
            DateTime? start = null;
            DateTime? end = null;
            var normalizedScope = (scope ?? "month").ToLowerInvariant();

            switch (normalizedScope)
            {
                case "today":
                    start = now.Date;
                    end = start.Value.AddDays(1);
                    break;
                case "week":
                    start = now.Date.AddDays(-7);
                    end = now.Date.AddDays(1);
                    break;
                case "month":
                    start = now.Date.AddMonths(-1);
                    end = now.Date.AddDays(1);
                    break;
                case "3m":
                    start = now.Date.AddMonths(-3);
                    end = now.Date.AddDays(1);
                    break;
                case "6m":
                    start = now.Date.AddMonths(-6);
                    end = now.Date.AddDays(1);
                    break;
                case "year":
                    start = now.Date.AddYears(-1);
                    end = now.Date.AddDays(1);
                    break;
                case "custom":
                    start = from;
                    end = to;
                    break;
                default:
                    normalizedScope = "month";
                    start = now.Date.AddMonths(-1);
                    end = now.Date.AddDays(1);
                    break;
            }

            if (start.HasValue)
                start = DateTime.SpecifyKind(start.Value, DateTimeKind.Utc);
            if (end.HasValue)
                end = DateTime.SpecifyKind(end.Value, DateTimeKind.Utc);

            return (start, end, normalizedScope);
        }
    }
}

