using UnityEngine;

/// <summary>
/// Handles camera orthographic size to maintain consistent width across different aspect ratios.
/// Attach this script to your main camera.
/// For landscape games, this ensures the game width stays constant while height adapts.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAspectRatioHandler : MonoBehaviour
{
    [Header("Reference Settings")]
    [Tooltip("The aspect ratio you designed your game for (width / height). For 16:9 landscape = 1.777")]
    public float targetAspectRatio = 16f / 9f;

    [Tooltip("The orthographic size when at target aspect ratio")]
    public float targetOrthographicSize = 5f;

    [Header("Debug")]
    [Tooltip("Enable to see changes in Unity Editor while adjusting values")]
    public bool executeInUpdate = false;

    private Camera cam;
    private float lastAspect;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.LogWarning("Camera is not orthographic! This script is designed for orthographic cameras.");
        }
    }

    void Start()
    {
        AdjustCamera();
    }

    void Update()
    {
        // Only update if aspect ratio changed or if executeInUpdate is enabled for testing
        if (executeInUpdate || !Mathf.Approximately(cam.aspect, lastAspect))
        {
            AdjustCamera();
        }
    }

    void AdjustCamera()
    {
        if (cam == null) return;

        lastAspect = cam.aspect;

        // Calculate new orthographic size to maintain consistent width
        // Formula: newSize = targetSize * (targetAspect / currentAspect)
        cam.orthographicSize = targetOrthographicSize * (targetAspectRatio / cam.aspect);

#if UNITY_EDITOR
        if (executeInUpdate)
        {
            Debug.Log($"Camera adjusted - Aspect: {cam.aspect:F3}, Ortho Size: {cam.orthographicSize:F3}");
        }
#endif
    }

    // Helper method to calculate world units visible
    public Vector2 GetVisibleWorldSize()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        return new Vector2(width, height);
    }

    // Optional: Draw gizmos in editor to visualize the camera bounds
    void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (!cam.orthographic) return;

        Vector2 size = GetVisibleWorldSize();

        Gizmos.color = Color.yellow;
        Vector3 topLeft = transform.position + new Vector3(-size.x / 2, size.y / 2, 0);
        Vector3 topRight = transform.position + new Vector3(size.x / 2, size.y / 2, 0);
        Vector3 bottomLeft = transform.position + new Vector3(-size.x / 2, -size.y / 2, 0);
        Vector3 bottomRight = transform.position + new Vector3(size.x / 2, -size.y / 2, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}