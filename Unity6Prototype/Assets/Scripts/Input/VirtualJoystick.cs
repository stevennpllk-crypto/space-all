using UnityEngine;
using UnityEngine.EventSystems;
namespace GalaxyExplorer.Input {
 public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
  [SerializeField] private RectTransform handle; [SerializeField] private float radius=90f;
  public Vector2 Value { get; private set; } private RectTransform rect;
  private void Awake(){rect=(RectTransform)transform;}
  public void OnPointerDown(PointerEventData e){OnDrag(e);}
  public void OnDrag(PointerEventData e){if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var p))return; Value=Vector2.ClampMagnitude(p/radius,1f); if(handle)handle.anchoredPosition=Value*radius;}
  public void OnPointerUp(PointerEventData e){Value=Vector2.zero;if(handle)handle.anchoredPosition=Vector2.zero;}
 }
}