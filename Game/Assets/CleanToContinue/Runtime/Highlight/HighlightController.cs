using System.Collections;
using CleanToContinue.Gap;
using CleanToContinue.Surface;
using UnityEngine;

namespace CleanToContinue.Highlight
{
    public sealed class HighlightController : MonoBehaviour
    {
        public const float PulseDurationSeconds = 1.2f;

        [SerializeField] private SurfaceMaskLayer[] surfaceLayers = new SurfaceMaskLayer[0];
        [SerializeField] private GapDirtSpot[] gapSpots = new GapDirtSpot[0];

        private Coroutine activePulse;

        public void Configure(SurfaceMaskLayer[] surfaces, GapDirtSpot[] spots)
        {
            surfaceLayers = surfaces ?? new SurfaceMaskLayer[0];
            gapSpots = spots ?? new GapDirtSpot[0];
            ClearHighlight();
        }

        public void Pulse()
        {
            if (activePulse != null)
            {
                StopCoroutine(activePulse);
            }

            ClearHighlight();
            activePulse = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            var elapsed = 0f;
            while (elapsed < PulseDurationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var normalizedTime = Mathf.Clamp01(elapsed / PulseDurationSeconds);
                SetHighlight(Mathf.Sin(normalizedTime * Mathf.PI));
                yield return null;
            }

            ClearHighlight();
            activePulse = null;
        }

        private void OnDisable()
        {
            if (activePulse != null)
            {
                StopCoroutine(activePulse);
                activePulse = null;
            }

            ClearHighlight();
        }

        private void SetHighlight(float intensity)
        {
            foreach (var surface in surfaceLayers)
            {
                if (surface != null)
                {
                    surface.SetHighlight(intensity);
                }
            }

            foreach (var spot in gapSpots)
            {
                if (spot != null && spot.Remaining01 > 0f)
                {
                    spot.SetHighlight(intensity);
                }
            }
        }

        private void ClearHighlight()
        {
            foreach (var surface in surfaceLayers)
            {
                if (surface != null)
                {
                    surface.SetHighlight(0f);
                }
            }

            foreach (var spot in gapSpots)
            {
                if (spot != null)
                {
                    spot.SetHighlight(0f);
                }
            }
        }
    }
}
