using System;
using CleanToContinue.Core;
using CleanToContinue.Highlight;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CleanToContinue.Input
{
    public interface IContinuousToolAudio
    {
        void StopContinuousToolAudio();
    }

    public sealed class StageInputController : MonoBehaviour
    {
        [SerializeField] private StageInteractionController interactionController;
        [SerializeField] private HighlightController highlightController;
        [SerializeField] private MonoBehaviour continuousToolAudio;

        private ToolSelectionModel toolSelection = new ToolSelectionModel();
        private InputAction pointAction;
        private InputAction cleanAction;
        private InputAction rotateAction;
        private InputAction highlightAction;
        private InputAction airGunAction;
        private InputAction cottonSwabAction;
        private InputAction clothAction;
        private Vector2 previousPointerPosition;
        private bool hasPreviousPointerPosition;
        private bool hasCleanContext;
        private bool previousCleanContextHeld;
        private bool previousCleanContextPointerOverUi;

        public Vector2 PointerPosition { get; private set; }
        public bool PointerOverUi { get; private set; }
        public bool IsCleanHeld { get; private set; }
        public bool IsRotateHeld { get; private set; }
        public ToolSelectionModel ToolSelection => toolSelection;

        public event Action<Vector2> PointerPositionChanged;
        public event Action<bool> CleanHeldChanged;
        public event Action<bool, bool> CleanContextChanged;
        public event Action<bool> RotateHeldChanged;
        public event Action HighlightPerformed;
        public event Action<CleaningTool> NumericToolSelected;

        public void Configure(
            StageInteractionController interaction,
            HighlightController highlight,
            ToolSelectionModel selection)
        {
            interactionController = interaction;
            highlightController = highlight;
            toolSelection = selection ?? new ToolSelectionModel();
            interactionController?.SetToolSelection(toolSelection);
            if (highlightController != null)
            {
                interactionController?.SetHighlightController(highlightController);
            }
        }

        private void OnEnable()
        {
            CreateActions();
            pointAction.Enable();
            cleanAction.Enable();
            rotateAction.Enable();
            highlightAction.Enable();
            airGunAction.Enable();
            cottonSwabAction.Enable();
            clothAction.Enable();
        }

        private void Update()
        {
            PointerPosition = pointAction.ReadValue<Vector2>();
            var pointerDelta = hasPreviousPointerPosition
                ? PointerPosition - previousPointerPosition
                : Vector2.zero;
            previousPointerPosition = PointerPosition;
            hasPreviousPointerPosition = true;
            PointerPositionChanged?.Invoke(PointerPosition);

            PointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            SetCleanHeld(cleanAction.IsPressed());
            PublishCleanContext();
            SetRotateHeld(rotateAction.IsPressed());
            interactionController?.ProcessFrame(
                PointerPosition,
                pointerDelta,
                IsCleanHeld,
                IsRotateHeld,
                PointerOverUi);
        }

        private void OnDisable()
        {
            StopContinuousToolAudio();
            DisposeActions();
            hasPreviousPointerPosition = false;
            hasCleanContext = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                StopContinuousToolAudio();
            }
        }

        private void CreateActions()
        {
            DisposeActions();
            pointAction = new InputAction("Point", InputActionType.Value, "<Pointer>/position");
            cleanAction = new InputAction("Clean", InputActionType.Button, "<Mouse>/leftButton");
            rotateAction = new InputAction("Rotate", InputActionType.Button, "<Mouse>/rightButton");
            highlightAction = new InputAction("Highlight", InputActionType.Button, "<Keyboard>/space");
            airGunAction = new InputAction("AirGun", InputActionType.Button, "<Keyboard>/1");
            cottonSwabAction = new InputAction("CottonSwab", InputActionType.Button, "<Keyboard>/2");
            clothAction = new InputAction("Cloth", InputActionType.Button, "<Keyboard>/3");

            highlightAction.performed += _ => PerformHighlight();
            airGunAction.performed += _ => SelectTool(CleaningTool.AirGun);
            cottonSwabAction.performed += _ => SelectTool(CleaningTool.CottonSwab);
            clothAction.performed += _ => SelectTool(CleaningTool.Cloth);
        }

        private void SetCleanHeld(bool held)
        {
            if (IsCleanHeld == held)
            {
                return;
            }

            IsCleanHeld = held;
            CleanHeldChanged?.Invoke(held);
        }

        private void SetRotateHeld(bool held)
        {
            if (IsRotateHeld == held)
            {
                return;
            }

            IsRotateHeld = held;
            RotateHeldChanged?.Invoke(held);
        }

        private void PublishCleanContext()
        {
            if (hasCleanContext
                && previousCleanContextHeld == IsCleanHeld
                && previousCleanContextPointerOverUi == PointerOverUi)
            {
                return;
            }

            hasCleanContext = true;
            previousCleanContextHeld = IsCleanHeld;
            previousCleanContextPointerOverUi = PointerOverUi;
            CleanContextChanged?.Invoke(IsCleanHeld, PointerOverUi);
        }

        private void PerformHighlight()
        {
            if (highlightController != null)
            {
                highlightController.Pulse();
            }
            else
            {
                interactionController?.PulseHighlight();
            }

            HighlightPerformed?.Invoke();
        }

        private void SelectTool(CleaningTool tool)
        {
            toolSelection.Select(tool);
            NumericToolSelected?.Invoke(tool);
        }

        private void DisposeActions()
        {
            DisposeAction(ref pointAction);
            DisposeAction(ref cleanAction);
            DisposeAction(ref rotateAction);
            DisposeAction(ref highlightAction);
            DisposeAction(ref airGunAction);
            DisposeAction(ref cottonSwabAction);
            DisposeAction(ref clothAction);
        }

        private void StopContinuousToolAudio()
        {
            if (continuousToolAudio is IContinuousToolAudio toolAudio)
            {
                toolAudio.StopContinuousToolAudio();
            }
        }

        private static void DisposeAction(ref InputAction action)
        {
            if (action == null)
            {
                return;
            }

            action.Disable();
            action.Dispose();
            action = null;
        }
    }
}
