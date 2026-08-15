using System;
using UnityEngine;

namespace CleanToContinue.Progress
{
    public sealed class StageProgressModel
    {
        private readonly IProgressSource[] sources;
        private readonly float completionThreshold;

        public StageProgressModel(IProgressSource[] sources, float completionThreshold)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
            this.completionThreshold = Mathf.Clamp01(completionThreshold);
        }

        public float Progress01 { get; private set; }
        public bool IsComplete { get; private set; }

        public event Action Completed;

        public void Refresh()
        {
            if (sources.Length == 0)
            {
                Progress01 = 0f;
                return;
            }

            var total = 0d;
            foreach (var source in sources)
            {
                total += Mathf.Clamp01(source.Progress01);
            }

            Progress01 = Mathf.Clamp01((float)(total / sources.Length));
            if (IsComplete || Progress01 < completionThreshold)
            {
                return;
            }

            IsComplete = true;
            Completed?.Invoke();
        }
    }
}
