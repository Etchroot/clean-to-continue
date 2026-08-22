using CleanToContinue.Core;
using CleanToContinue.Input;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class CleaningCursorView : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform cursorRoot;
        [SerializeField] private Image halo;
        [SerializeField] private Image core;

        private StageInputController inputController;
        private ToolSelectionModel selectionModel;
        private Vector2 baseSize;
        private bool cleanHeld;
        private bool pointerOverUi;
        private float animationTime;

        public void Configure(
            RectTransform canvas,
            RectTransform root,
            Image haloImage,
            Image coreImage,
            StageInputController input,
            ToolSelectionModel selection)
        {
            Unbind();
            canvasRect = canvas;
            cursorRoot = root;
            halo = haloImage;
            core = coreImage;
            inputController = input;
            selectionModel = selection;

            if (inputController != null)
            {
                inputController.PointerPositionChanged += MoveToPointer;
                inputController.CleanContextChanged += SetCleanContext;
            }

            if (selectionModel != null)
            {
                selectionModel.SelectionChanged += RenderTool;
                RenderTool(selectionModel.Selected);
            }

            RenderVisibility();
        }

        private void Update()
        {
            if (cursorRoot == null || !cursorRoot.gameObject.activeSelf)
            {
                return;
            }

            animationTime += Time.unscaledDeltaTime;
            var pulse = 1f + Mathf.Sin(animationTime * 12f) * 0.08f;
            cursorRoot.sizeDelta = baseSize * pulse;
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void MoveToPointer(Vector2 screenPosition)
        {
            if (canvasRect == null || cursorRoot == null)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    null,
                    out var localPosition))
            {
                cursorRoot.anchoredPosition = localPosition;
            }
        }

        private void SetCleanContext(bool held, bool overUi)
        {
            cleanHeld = held;
            pointerOverUi = overUi;
            RenderVisibility();
        }

        private void RenderTool(CleaningTool tool)
        {
            switch (tool)
            {
                case CleaningTool.AirGun:
                    baseSize = new Vector2(92f, 92f);
                    SetColors(new Color(0.72f, 0.9f, 1f, 0.18f), new Color(0.85f, 0.96f, 1f, 0.8f));
                    break;
                default:
                    baseSize = new Vector2(68f, 68f);
                    SetColors(new Color(1f, 0.68f, 0.38f, 0.2f), new Color(1f, 0.84f, 0.6f, 0.88f));
                    break;
            }

            if (cursorRoot != null)
            {
                cursorRoot.sizeDelta = baseSize;
            }
        }

        private void SetColors(Color haloColor, Color coreColor)
        {
            if (halo != null)
            {
                halo.color = haloColor;
            }

            if (core != null)
            {
                core.color = coreColor;
            }
        }

        private void RenderVisibility()
        {
            if (cursorRoot != null)
            {
                cursorRoot.gameObject.SetActive(cleanHeld && !pointerOverUi);
            }
        }

        private void Unbind()
        {
            if (inputController != null)
            {
                inputController.PointerPositionChanged -= MoveToPointer;
                inputController.CleanContextChanged -= SetCleanContext;
            }

            if (selectionModel != null)
            {
                selectionModel.SelectionChanged -= RenderTool;
            }
        }
    }
}
