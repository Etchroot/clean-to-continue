using System;
using CleanToContinue.Core;

namespace CleanToContinue.Progress
{
    public interface IProgressSource
    {
        CleaningTool Tool { get; }
        float Progress01 { get; }

        event Action ProgressChanged;
    }
}
