using UnityEngine;

public class EnemySway : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;
    public float rotationSpeed = 2f;

    [Header("Body Sway (upper body only)")]
    public float maxBodyAngle = 8f;
    public float bodySpeed = 1f;

    [Header("Arm Raise")]
    public float maxArmAngle = 25f;
    public float armSpeed = 1f;

    Transform spine;
    Transform leftArm;
    Transform rightArm;

    Quaternion initialSpineRotation;
    Quaternion initialLeftArmRotation;
    Quaternion initialRightArmRotation;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        spine = FindChild(transform, "mixamorig:Spine1");
        leftArm = FindChild(transform, "mixamorig:LeftArm");
        rightArm = FindChild(transform, "mixamorig:RightArm");

        if (spine != null) initialSpineRotation = spine.localRotation;
        if (leftArm != null) initialLeftArmRotation = leftArm.localRotation;
        if (rightArm != null) initialRightArmRotation = rightArm.localRotation;
    }

    void Update()
    {
        // Face the player (yaw only) - feet stay planted, no tilt here
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // Sway applied only to the spine, so legs don't move
        if (spine != null)
        {
            float bodyAngle = Mathf.Sin(Time.time * bodySpeed) * maxBodyAngle;
            spine.localRotation = initialSpineRotation * Quaternion.Euler(0f, 0f, bodyAngle);
        }

        // Arms slowly raise and lower
        float armAngle = (Mathf.Sin(Time.time * armSpeed) * 0.5f + 0.5f) * maxArmAngle;

        if (leftArm != null)
            leftArm.localRotation = initialLeftArmRotation * Quaternion.Euler(0f, 0f, armAngle);

        if (rightArm != null)
            rightArm.localRotation = initialRightArmRotation * Quaternion.Euler(0f, 0f, -armAngle);
    }

    Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name == childName) return child;
        }
        return null;
    }
}