using CleanToContinue.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class EndingView : MonoBehaviour
    {
        public const string MainMenuSceneName = "01.MainMenu";

        [SerializeField] private Button restartButton;
        [SerializeField] private string destinationScene = MainMenuSceneName;
        private bool leaving;

        public string DestinationScene => destinationScene;

        public void Configure(Button restart, string destination = MainMenuSceneName)
        {
            Unbind();
            restartButton = restart;
            destinationScene = string.IsNullOrWhiteSpace(destination) ? MainMenuSceneName : destination;
            Bind();
        }

        public void Restart()
        {
            if (leaving)
            {
                return;
            }

            leaving = true;
            SceneFlow.Load(destinationScene);
        }

        private void Awake() => Bind();
        private void OnDestroy() => Unbind();

        private void Bind()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
                restartButton.onClick.AddListener(Restart);
            }
        }

        private void Unbind()
        {
            restartButton?.onClick.RemoveListener(Restart);
        }
    }
}
