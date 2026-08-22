using System.Linq;
using CleanToContinue.Audio;
using CleanToContinue.Core;
using CleanToContinue.Gap;
using CleanToContinue.Highlight;
using CleanToContinue.Input;
using CleanToContinue.Progress;
using CleanToContinue.Stage;
using CleanToContinue.Surface;
using CleanToContinue.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.Flow
{
    public sealed class EquipmentStageBootstrap : MonoBehaviour
    {
        private const float SurfaceBrushRadius = 0.096f;

        [SerializeField] private StageController stageController;
        [SerializeField] private StageInputController inputController;
        [SerializeField] private StageInteractionController interactionController;
        [SerializeField] private EquipmentRotator equipmentRotator;
        [SerializeField] private HighlightController highlightController;
        [SerializeField] private CleaningAudioController audioController;
        [SerializeField] private Camera stageCamera;
        [SerializeField] private SurfaceMaskLayer[] surfaceLayers;
        [SerializeField] private ProgressWheelView progressWheel;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressText;
        [SerializeField] private ToolSelectorView toolSelector;
        [SerializeField] private ToolSelectorView.ToolButtonBinding[] toolButtons;
        [SerializeField] private CleaningCursorView cleaningCursor;
        [SerializeField] private MemoryPanelView memoryPanel;
        [SerializeField] private GameObject memoryPanelRoot;
        [SerializeField] private Image memoryDimmer;
        [SerializeField] private Image memoryImage;
        [SerializeField] private Sprite memorySprite;
        [SerializeField] private Text memoryLine;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string memoryText;
        [SerializeField] private string nextSceneName;

        public void Configure(
            StageController stage,
            StageInputController input,
            StageInteractionController interaction,
            EquipmentRotator rotator,
            HighlightController highlight,
            CleaningAudioController audio,
            Camera camera,
            SurfaceMaskLayer[] surfaces,
            ProgressWheelView wheel,
            Image wheelFill,
            Text wheelText,
            ToolSelectorView selector,
            ToolSelectorView.ToolButtonBinding[] buttons,
            CleaningCursorView cursor,
            MemoryPanelView memory,
            GameObject memoryRoot,
            Image dimmer,
            Image image,
            Text line,
            Button nextButton,
            Button menuButton,
            string stageMemoryText,
            string stageNextScene,
            Sprite stageMemorySprite = null)
        {
            stageController = stage;
            inputController = input;
            interactionController = interaction;
            equipmentRotator = rotator;
            highlightController = highlight;
            audioController = audio;
            stageCamera = camera;
            surfaceLayers = surfaces;
            progressWheel = wheel;
            progressFill = wheelFill;
            progressText = wheelText;
            toolSelector = selector;
            toolButtons = buttons;
            cleaningCursor = cursor;
            memoryPanel = memory;
            memoryPanelRoot = memoryRoot;
            memoryDimmer = dimmer;
            memoryImage = image;
            memorySprite = stageMemorySprite;
            memoryLine = line;
            nextStageButton = nextButton;
            mainMenuButton = menuButton;
            memoryText = stageMemoryText;
            nextSceneName = stageNextScene;
        }

        private void Awake()
        {
            surfaceLayers ??= new SurfaceMaskLayer[0];
            var selection = new ToolSelectionModel();
            highlightController?.Configure(surfaceLayers, new GapDirtSpot[0]);
            interactionController?.Configure(
                stageCamera,
                selection,
                equipmentRotator,
                surfaceLayers,
                null,
                SurfaceBrushRadius,
                0.34f,
                highlightController);
            inputController?.Configure(interactionController, highlightController, selection);
            progressWheel?.Configure(progressFill, progressText);
            memoryPanel?.Configure(
                memoryPanelRoot,
                memoryDimmer,
                memoryImage,
                memoryLine,
                nextStageButton,
                mainMenuButton,
                nextSceneName,
                memorySprite,
                memoryText);

            var sources = surfaceLayers.Cast<IProgressSource>().ToArray();
            toolSelector?.Configure(selection, sources, toolButtons, audioController);
            cleaningCursor?.Configure(
                cleaningCursor.transform.parent as RectTransform,
                cleaningCursor.transform as RectTransform,
                cleaningCursor.transform.Find("Halo")?.GetComponent<Image>(),
                cleaningCursor.transform.Find("Core")?.GetComponent<Image>(),
                inputController,
                selection);
            stageController?.ConfigureScene(
                surfaceLayers,
                null,
                inputController,
                interactionController,
                equipmentRotator,
                progressWheel,
                toolSelector,
                memoryPanel,
                audioController);
            stageController?.Initialize();
        }
    }
}
