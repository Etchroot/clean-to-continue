using UnityEngine;

namespace CleanToContinue.Input
{
    public sealed class EquipmentRotator : MonoBehaviour
    {
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float sensitivity = 1f;

        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        public void Configure(float minimumPitch, float maximumPitch, float dragSensitivity)
        {
            minPitch = Mathf.Min(minimumPitch, maximumPitch);
            maxPitch = Mathf.Max(minimumPitch, maximumPitch);
            sensitivity = Mathf.Abs(dragSensitivity);
            Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);
            ApplyRotation();
        }

        public void ApplyDrag(Vector2 delta)
        {
            Yaw += delta.x * sensitivity;
            Pitch = Mathf.Clamp(Pitch - delta.y * sensitivity, minPitch, maxPitch);
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            transform.localRotation = Quaternion.Euler(Pitch, Yaw, 0f);
        }
    }
}
