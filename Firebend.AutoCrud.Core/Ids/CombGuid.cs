using System;

namespace Firebend.AutoCrud.Core.Ids;

public static class CombGuid
{
    public static Guid New(Guid guid, DateTimeOffset timestamp)
    {
        var dateTime = DateTimeOffset.UnixEpoch;
        var timeSpan = timestamp - dateTime;
        var timeSpanMs = (long)timeSpan.TotalMilliseconds;
        var timestampString = timeSpanMs.ToString("x8");
        var guidString = guid.ToString("N");

        var newGuidString = $"{timestampString[..11]}{guidString[11..]}";

        if (string.IsNullOrWhiteSpace(newGuidString))
        {
            throw new Exception("Could not get guid string");
        }

        var newGuid = Guid.Parse(newGuidString);

        return newGuid;
    }

    public static Guid New() => New(Guid.NewGuid(), DateTimeOffset.UtcNow);
}
