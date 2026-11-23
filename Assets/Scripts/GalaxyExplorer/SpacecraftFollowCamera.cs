using UnityEngine;

/// <summary>Smooth camera with switchable first-person cockpit and third-person chase views.</summary>
public sealed class SpacecraftFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5.5f, -13f);
    public Vector3 cockpitOffset = new Vector3(0f, 1.15f, 1.15f);
    [Range(1f, 20f)] public float positionDamping = 7f;
    [Range(1f, 20f)] public float rotationDamping = 8f;
    public float lookAhead = 12f;
    public bool FirstPerson { get; private set; }

    public void ToggleView()
    {
        FirstPerson = !FirstPerson;
    }

    public void SetFirstPerson(bool enabled)
    {
        FirstPerson = enabled;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.TransformPoint(FirstPerson ? cockpitOffset : offset);
        float positionBlend = 1f - Mathf.Exp(-positionDamping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionBlend);

        Quaternion desiredRotation;
        if (FirstPerson)
        {
            desiredRotation = target.rotation;
        }
        else
        {
            Vector3 lookPoint = target.position + target.forward * lookAhead;
            desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, target.up);
        }

        float rotationBlend = 1f - Mathf.Exp(-rotationDamping * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationBlend);
    }
}
