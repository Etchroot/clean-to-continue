using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class UiButtonHoverBackground : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float FadeDuration = 0.1f;

        private Button button;
        private Graphic background;
        private Color normalColor;
        private Coroutine fadeRoutine;
        private bool initialized;

        private void Awake()
        {
            Initialize();
        }

        public void Configure(Graphic target)
        {
            button = GetComponent<Button>();
            background = target != null ? target : button.targetGraphic;
            CaptureNormalColor();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Initialize();
            if (button != null && button.interactable)
            {
                FadeTo(Color.black);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Initialize();
            FadeTo(normalColor);
        }

        private void OnDisable()
        {
            if (!initialized || background == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            background.color = normalColor;
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            button = GetComponent<Button>();
            background = button.targetGraphic;
            CaptureNormalColor();
        }

        private void CaptureNormalColor()
        {
            if (background == null)
            {
                initialized = false;
                return;
            }

            normalColor = background.color;
            initialized = true;
        }

        private void FadeTo(Color targetColor)
        {
            if (background == null)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeBackground(targetColor));
        }

        private IEnumerator FadeBackground(Color targetColor)
        {
            var startColor = background.color;
            var elapsed = 0f;

            while (elapsed < FadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                background.color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(elapsed / FadeDuration));
                yield return null;
            }

            background.color = targetColor;
            fadeRoutine = null;
        }
    }
}
