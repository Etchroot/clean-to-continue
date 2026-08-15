using System;
using System.Collections.Generic;
using CleanToContinue.Core;
using CleanToContinue.Progress;
using UnityEngine;

namespace CleanToContinue.Gap
{
    public sealed class GapDirtGroup : MonoBehaviour, IProgressSource
    {
        [SerializeField] private GapDirtSpot[] spots = Array.Empty<GapDirtSpot>();

        public CleaningTool Tool => CleaningTool.CottonSwab;
        public IReadOnlyList<GapDirtSpot> Spots => spots;

        public float Progress01
        {
            get
            {
                if (spots == null || spots.Length == 0)
                {
                    return 1f;
                }

                var remainingTotal = 0f;
                var validSpotCount = 0;
                foreach (var spot in spots)
                {
                    if (spot == null)
                    {
                        continue;
                    }

                    remainingTotal += spot.Remaining01;
                    validSpotCount++;
                }

                return validSpotCount == 0
                    ? 1f
                    : 1f - remainingTotal / validSpotCount;
            }
        }

        public event Action ProgressChanged;

        public void Configure(GapDirtSpot[] dirtSpots)
        {
            spots = dirtSpots ?? Array.Empty<GapDirtSpot>();
        }

        public bool TryClean(CleaningTool selectedTool, Collider hitCollider, float cleaningAmount)
        {
            if (selectedTool != CleaningTool.CottonSwab || hitCollider == null || cleaningAmount <= 0f)
            {
                return false;
            }

            foreach (var spot in spots)
            {
                if (spot == null || spot.CleaningCollider != hitCollider)
                {
                    continue;
                }

                if (!spot.Apply(cleaningAmount))
                {
                    return false;
                }

                ProgressChanged?.Invoke();
                return true;
            }

            return false;
        }

        public void ForceFinish()
        {
            var changed = false;
            foreach (var spot in spots)
            {
                if (spot != null)
                {
                    changed |= spot.Apply(1f);
                }
            }

            if (changed)
            {
                ProgressChanged?.Invoke();
            }
        }
    }
}
