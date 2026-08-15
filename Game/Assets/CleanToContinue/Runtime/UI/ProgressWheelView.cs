using UnityEngine;
using UnityEngine.UI;

namespace CleanToContinue.UI
{
    public sealed class ProgressWheelView : MonoBehaviour
    {
        [SerializeField] private Image radialFill;
        [SerializeField] private Text percentText;

        public float DisplayedProgress01 { get; private set; }

        public void Configure(Image fillImage, Text percentageText)
        {
            radialFill = fillImage;
            percentText = percentageText;
            Render(DisplayedProgress01);
        }

        public void Render(float progress01)
        {
            DisplayedProgress01 = Mathf.Clamp01(progress01);
            if (radialFill != null)
            {
                radialFill.type = Image.Type.Filled;
                radialFill.fillMethod = Image.FillMethod.Radial360;
                radialFill.fillAmount = DisplayedProgress01;
            }

            if (percentText != null)
            {
                percentText.text = $"{Mathf.FloorToInt(DisplayedProgress01 * 100f)}%";
            }
        }
    }
}
