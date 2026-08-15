using UnityEngine;

namespace CleanToContinue.Gap
{
    public sealed class GapDirtSpot : MonoBehaviour
    {
        private static readonly int HighlightPulseId = Shader.PropertyToID("_HighlightPulse");

        [SerializeField] private Collider cleaningCollider;
        [SerializeField] private Transform dirtVisual;
        [SerializeField] private Renderer dirtRenderer;
        [SerializeField] private ParticleSystem completionParticles;

        private MaterialPropertyBlock propertyBlock;
        private Vector3 fullVisualScale = Vector3.one;

        public Collider CleaningCollider => cleaningCollider;
        public float Remaining01 { get; private set; } = 1f;
        public float Highlight01 { get; private set; }

        public void Configure(
            Collider targetCollider,
            Transform visual,
            Renderer visualRenderer,
            ParticleSystem particles = null)
        {
            cleaningCollider = targetCollider;
            dirtVisual = visual;
            dirtRenderer = visualRenderer;
            completionParticles = particles;
            Remaining01 = 1f;
            fullVisualScale = dirtVisual != null ? dirtVisual.localScale : Vector3.one;
            if (cleaningCollider != null)
            {
                cleaningCollider.enabled = true;
            }

            RefreshVisual();
            SetHighlight(0f);
        }

        public bool Apply(float cleaningAmount)
        {
            if (cleaningAmount <= 0f || Remaining01 <= 0f)
            {
                return false;
            }

            var previous = Remaining01;
            Remaining01 = Mathf.Clamp01(Remaining01 - cleaningAmount);
            if (Mathf.Approximately(previous, Remaining01))
            {
                return false;
            }

            RefreshVisual();
            if (Remaining01 <= 0f)
            {
                SetHighlight(0f);
                if (cleaningCollider != null)
                {
                    cleaningCollider.enabled = false;
                }

                if (completionParticles != null)
                {
                    completionParticles.Play();
                }
            }

            return true;
        }

        public void SetHighlight(float intensity)
        {
            Highlight01 = Remaining01 > 0f ? Mathf.Clamp01(intensity) : 0f;
            if (dirtRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            dirtRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(HighlightPulseId, Highlight01);
            dirtRenderer.SetPropertyBlock(propertyBlock);
        }

        private void Awake()
        {
            if (dirtVisual != null)
            {
                fullVisualScale = dirtVisual.localScale;
            }
        }

        private void RefreshVisual()
        {
            if (dirtVisual != null)
            {
                dirtVisual.localScale = fullVisualScale * Mathf.Lerp(0.25f, 1f, Remaining01);
            }
        }
    }
}
