﻿using System;

namespace TickZoom.Api
{
    public interface TickFile : IDisposable
    {
        void Initialize(string folderOrfile, string symbolFile, BinaryFileMode mode);
        void Initialize(string fileName, BinaryFileMode mode);
        bool TryWriteTick(TickIO tickIO);
        void WriteTick(TickIO tickIO);
        void GetLastTick(TickIO lastTickIO);
        bool TryReadTick(TickIO tickIO);
        void Flush();
        long Length { get; }
        long Position { get; }
        int DataVersion { get; }
        int BlockVersion { get; }
        bool QuietMode { get; set; }
        string FileName { get; }
        SymbolInfo Symbol { get; }
        bool EraseFileToStart { get; set; }
        long WriteCounter { get; }
        long MaxCount { get; set; }
        TimeStamp StartTime { get; set; }
        TimeStamp EndTime { get; set; }
        long StartCount { get; set; }
        Action<Progress> ReportProgressCallback { get; set; }
        bool IsInitialized { get; }
    }
}