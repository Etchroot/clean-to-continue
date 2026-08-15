using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class MemoryPanelView : MonoBehaviour
    {
        public const string TemporaryContinueScene = "01.MainMenu";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image backgroundDimmer;
        [SerializeField] private Image memoryImage;
        [SerializeField] private Text memoryLine;
        [SerializeField] private Button continueButton;
        [SerializeField] private Sprite mouseMemorySprite;
        [SerializeField] private string mouseMemoryText = "그때는 바라보는 것만으로도 새로운 세계가 열렸다.";

        private bool isOpen;

        public bool IsOpen => isOpen;

        public event Action Opened;

        public void Configure(
            GameObject root,
            Image dimmer,
            Image image,
            Text line,
            Button continueAction,
            Sprite mouseSprite = null,
            string mouseLine = null)
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ContinueToMainMenu);
            }

            panelRoot = root;
            backgroundDimmer = dimmer;
            memoryImage = image;
            memoryLine = line;
            continueButton = continueAction;
            mouseMemorySprite = mouseSprite;
            if (!string.IsNullOrWhiteSpace(mouseLine))
            {
                mouseMemoryText = mouseLine;
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueToMainMenu);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void OpenMouseMemory()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            if (backgroundDimmer != null)
            {
                var color = backgroundDimmer.color;
                color.a = Mathf.Max(color.a, 0.78f);
                backgroundDimmer.color = color;
            }

            if (memoryImage != null)
            {
                memoryImage.sprite = mouseMemorySprite;
            }

            if (memoryLine != null)
            {
                memoryLine.text = mouseMemoryText;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            Opened?.Invoke();
        }

        public void ContinueToMainMenu()
        {
            SceneManager.LoadScene(TemporaryContinueScene);
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ContinueToMainMenu);
            }
        }
    }
}
