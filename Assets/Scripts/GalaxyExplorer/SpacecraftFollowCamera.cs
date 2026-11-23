using UnityEngine;

/// <summary>Smooth third-person chase camera for the player spacecraft.</summary>
public sealed class SpacecraftFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5.5f, -13f);
    [Range(1f, 20f)] public float positionDamping = 7f;
    [Range(1f, 20f)] public float rotationDamping = 8f;
    public float lookAhead = 12f;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.TransformPoint(offset);
        float positionBlend = 1f - Mathf.Exp(-positionDamping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionBlend);

        Vector3 lookPoint = target.position + target.forward * lookAhead;
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, target.up);
        float rotationBlend = 1f - Mathf.Exp(-rotationDamping * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationBlend);
    }
}
