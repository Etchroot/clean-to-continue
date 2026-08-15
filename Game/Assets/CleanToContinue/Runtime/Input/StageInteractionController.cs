using CleanToContinue.Core;
using CleanToContinue.Gap;
using CleanToContinue.Highlight;
using CleanToContinue.Surface;
using UnityEngine;

namespace CleanToContinue.Input
{
    public sealed class StageInteractionController : MonoBehaviour
    {
        public const int CleanableLayer = 8;

        [SerializeField] private Camera stageCamera;
        [SerializeField] private EquipmentRotator equipmentRotator;
        [SerializeField] private SurfaceMaskLayer[] surfaceLayers = new SurfaceMaskLayer[0];
        [SerializeField] private GapDirtGroup gapDirtGroup;
        [SerializeField] private HighlightController highlightController;
        [SerializeField, Range(0.01f, 1f)] private float surfaceBrushRadius = 0.1f;
        [SerializeField, Min(0.01f)] private float gapCleaningAmount = 0.5f;

        private ToolSelectionModel toolSelection = new ToolSelectionModel();
        private bool cleanWasHeld;
        private bool rotateWasHeld;
        private bool cleanPressBlocked;
        private bool rotatePressBlocked;

        public ToolSelectionModel ToolSelection => toolSelection;

        public void Configure(
            Camera camera,
            ToolSelectionModel selection,
            EquipmentRotator rotator,
            SurfaceMaskLayer[] layers,
            GapDirtGroup dirtGroup,
            float brushRadius,
            float cleaningAmount,
            HighlightController highlight = null)
        {
            stageCamera = camera;
            toolSelection = selection ?? new ToolSelectionModel();
            equipmentRotator = rotator;
            surfaceLayers = layers ?? new SurfaceMaskLayer[0];
            gapDirtGroup = dirtGroup;
            surfaceBrushRadius = Mathf.Clamp01(brushRadius);
            gapCleaningAmount = Mathf.Max(0f, cleaningAmount);
            highlightController = highlight;
        }

        public void SetToolSelection(ToolSelectionModel selection)
        {
            toolSelection = selection ?? new ToolSelectionModel();
        }

        public void PulseHighlight()
        {
            highlightController?.Pulse();
        }

        public void ProcessFrame(
            Vector2 pointerPosition,
            Vector2 pointerDelta,
            bool cleanHeld,
            bool rotateHeld,
            bool pointerOverUi)
        {
            UpdateUiPressGuards(cleanHeld, rotateHeld, pointerOverUi);

            if (rotateHeld && !rotatePressBlocked)
            {
                equipmentRotator?.ApplyDrag(pointerDelta);
                return;
            }

            if (cleanHeld && !cleanPressBlocked)
            {
                RouteCleaning(pointerPosition);
            }
        }

        private void UpdateUiPressGuards(bool cleanHeld, bool rotateHeld, bool pointerOverUi)
        {
            if (cleanHeld && !cleanWasHeld)
            {
                cleanPressBlocked = pointerOverUi;
            }
            else if (!cleanHeld)
            {
                cleanPressBlocked = false;
            }

            if (rotateHeld && !rotateWasHeld)
            {
                rotatePressBlocked = pointerOverUi;
            }
            else if (!rotateHeld)
            {
                rotatePressBlocked = false;
            }

            cleanWasHeld = cleanHeld;
            rotateWasHeld = rotateHeld;
        }

        private void RouteCleaning(Vector2 pointerPosition)
        {
            if (stageCamera == null)
            {
                return;
            }

            var ray = stageCamera.ScreenPointToRay(pointerPosition);
            var cleanableMask = 1 << CleanableLayer;
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, cleanableMask))
            {
                return;
            }

            if (toolSelection.Selected == CleaningTool.CottonSwab)
            {
                gapDirtGroup?.TryClean(
                    CleaningTool.CottonSwab,
                    hit.collider,
                    gapCleaningAmount);
                return;
            }

            foreach (var layer in surfaceLayers)
            {
                if (layer == null || layer.Tool != toolSelection.Selected || !BelongsToHit(layer.transform, hit.collider.transform))
                {
                    continue;
                }

                layer.TryClean(toolSelection.Selected, hit, surfaceBrushRadius);
                return;
            }
        }

        private static bool BelongsToHit(Transform layerTransform, Transform hitTransform)
        {
            return layerTransform == hitTransform
                || layerTransform.IsChildOf(hitTransform)
                || hitTransform.IsChildOf(layerTransform);
        }
    }
}
