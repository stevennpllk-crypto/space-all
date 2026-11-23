using UnityEngine;
using GalaxyExplorer.Input;
namespace GalaxyExplorer.Spacecraft {
 [RequireComponent(typeof(Rigidbody))]
 public sealed class SpacecraftController : MonoBehaviour {
  [SerializeField] private float thrust=18f, rotationSpeed=65f, maxSpeed=85f, brakeStrength=2.5f;
  [SerializeField] private VirtualJoystick moveStick, lookStick;
  private Rigidbody body; private float throttle=0f; private bool braking;
  public float Speed=>body==null?0f:body.linearVelocity.magnitude;
  public void SetMoveStick(VirtualJoystick v)=>moveStick=v; public void SetLookStick(VirtualJoystick v)=>lookStick=v;
  public void SetThrottle(float v)=>throttle=Mathf.Clamp01(v); public void SetBraking(bool v)=>braking=v;
  private void Awake(){body=GetComponent<Rigidbody>();body.useGravity=false;body.linearDamping=.08f;body.angularDamping=1.2f;body.interpolation=RigidbodyInterpolation.Interpolate;}
  private void FixedUpdate(){
   Vector2 m=moveStick?moveStick.Value:Vector2.zero, l=lookStick?lookStick.Value:Vector2.zero;
   body.AddRelativeTorque(new Vector3(-l.y,l.x,-m.x)*rotationSpeed*Mathf.Deg2Rad,ForceMode.Acceleration);
   body.AddRelativeForce(Vector3.forward*(m.y+throttle)*thrust,ForceMode.Acceleration);
   if(braking)body.linearVelocity=Vector3.Lerp(body.linearVelocity,Vector3.zero,brakeStrength*Time.fixedDeltaTime);
   if(body.linearVelocity.magnitude>maxSpeed)body.linearVelocity=body.linearVelocity.normalized*maxSpeed;
  }
 }
}