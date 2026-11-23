using UnityEngine;
namespace GalaxyExplorer.Cameras {
 public sealed class CameraRigController:MonoBehaviour {
  [SerializeField] private Transform target; [SerializeField] private Vector3 thirdOffset=new Vector3(0,3.2f,-9), firstOffset=new Vector3(0,.65f,.45f); [SerializeField] private float smooth=8f;
  public bool FirstPerson{get;private set;} public void SetTarget(Transform t)=>target=t; public void ToggleMode()=>FirstPerson=!FirstPerson;
  private void LateUpdate(){if(!target)return;Vector3 p=target.TransformPoint(FirstPerson?firstOffset:thirdOffset);transform.position=Vector3.Lerp(transform.position,p,smooth*Time.deltaTime);transform.rotation=Quaternion.Slerp(transform.rotation,target.rotation,smooth*Time.deltaTime);}
 }
}