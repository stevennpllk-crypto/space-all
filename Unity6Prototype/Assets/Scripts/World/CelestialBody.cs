using UnityEngine;
namespace GalaxyExplorer.World {
 public sealed class CelestialBody:MonoBehaviour {
  [SerializeField] private string bodyName="Unknown"; [SerializeField] private float rotationRate=1f;
  public string BodyName=>bodyName; public void Configure(string n,float r){bodyName=n;rotationRate=r;}
  private void Update(){transform.Rotate(Vector3.up,rotationRate*Time.deltaTime,Space.Self);}
 }
}