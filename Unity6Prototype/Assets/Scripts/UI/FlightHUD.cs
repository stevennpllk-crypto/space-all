using UnityEngine;
using UnityEngine.UI;
using GalaxyExplorer.Spacecraft;
using GalaxyExplorer.Cameras;
using GalaxyExplorer.Input;
namespace GalaxyExplorer.UI {
 public sealed class FlightHUD:MonoBehaviour {
  [SerializeField] private SpacecraftController ship; [SerializeField] private LandingAssist landing; [SerializeField] private Text statusText; [SerializeField] private CameraRigController rig; [SerializeField] private Text speedText,modeText; [SerializeField] private Slider throttle;
  public void Bind(SpacecraftController s,CameraRigController r,VirtualJoystick left,VirtualJoystick right,Text speed,Text mode,Text status,Slider slider){
   ship=s; landing=s.GetComponent<LandingAssist>();rig=r;speedText=speed;modeText=mode;statusText=status;throttle=slider;ship.SetMoveStick(left);ship.SetLookStick(right);if(throttle)throttle.onValueChanged.AddListener(ship.SetThrottle);
  }
  private void Update(){if(ship&&speedText)speedText.text="VELOCITY  "+ship.Speed.ToString("0.0")+" m/s";if(rig&&modeText)modeText.text=rig.FirstPerson?"CAM / COCKPIT":"CAM / EXTERNAL";if(landing&&statusText)statusText.text=landing.Status+"  |  ALT "+(landing.Altitude>=99999f?"--":landing.Altitude.ToString("0.0")+" m");}
  public void ToggleCamera(){if(rig)rig.ToggleMode();} public void SetBrake(bool v){if(ship)ship.SetBraking(v);}
 }
}