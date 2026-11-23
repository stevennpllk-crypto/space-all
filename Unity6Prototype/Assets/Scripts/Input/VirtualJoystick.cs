using UnityEngine;
using UnityEngine.EventSystems;

namespace GalaxyExplorer.Input
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField, Min(1f)] private float radius = 90f;

        private RectTransform rect;
        private int activePointerId = int.MinValue;

        public Vector2 Value { get; private set; }

        private void Awake()
        {
            rect = (RectTransform)transform;
        }

        public void ConfigureHandle(RectTransform value)
        {
            handle = value;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue)
                return;

            activePointerId = eventData.pointerId;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            Value = Vector2.ClampMagnitude(localPoint / Mathf.Max(1f, radius), 1f);
            if (handle != null)
                handle.anchoredPosition = Value * radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
                return;

            ResetStick();
        }

        private void OnDisable()
        {
            ResetStick();
        }

        private void ResetStick()
        {
            activePointerId = int.MinValue;
            Value = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }
    }
}