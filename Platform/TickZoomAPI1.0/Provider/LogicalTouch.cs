﻿namespace TickZoom.Api
{
    public interface LogicalTouch
    {
        int OrderId { get; }
        long OrderSerialNumber { get; }
        long Recency { get; }
        TimeStamp UtcTime { get; }
    }
}