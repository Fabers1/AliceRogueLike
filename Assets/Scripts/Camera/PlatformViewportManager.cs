using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages platform objects to fit perfectly in any device viewport.
/// Scales platforms horizontally to match screen width on any device.
/// Attach this to an empty GameObject in your scene.
/// </summary>
public class PlatformViewportManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The camera to fit platforms to. If null, uses Camera.main")]
    public Camera targetCamera;

    [Tooltip("Keep camera orthographic size fixed (recommended for consistent gameplay)")]
    public float fixedOrthographicSize = 5f;

    [Header("Main Platform")]
    [Tooltip("Main floor/ground platform that spans the full screen width")]
    public GameObject mainFloor;

    [Tooltip("Y position for the main floor (in world units from camera center)")]
    public float mainFloorYOffset = -4f;

    [Header("Additional Platforms")]
    [Tooltip("List of platforms that should scale with screen width")]
    public List<PlatformInfo> platforms = new List<PlatformInfo>();

    [Header("Platform Settings")]
    [Tooltip("Should platforms scale to full screen width or maintain relative size?")]
    public bool scaleToFullWidth = true;

    [Tooltip("If not scaling to full width, what percentage of screen width? (0.5 = 50%)")]
    [Range(0.1f, 1f)]
    public float platformWidthPercentage = 1f;

    [Tooltip("Add extra width to platforms to prevent gaps (in world units)")]
    public float extraWidth = 0.1f;

    [Header("Update Settings")]
    [Tooltip("Update continuously (for testing different aspect ratios)")]
    public bool updateContinuously = false;

    [Tooltip("Automatically add BoxCollider2D to platforms if missing")]
    public bool autoAddColliders = true;

    private Dictionary<GameObject, PlatformData> platformData = new Dictionary<GameObject, PlatformData>();
    private Vector2 lastScreenSize;

    [System.Serializable]
    public class PlatformInfo
    {
        [Tooltip("The platform GameObject")]
        public GameObject platform;

        [Tooltip("Should this platform scale to match screen width?")]
        public bool shouldScale = true;

        [Tooltip("Custom width percentage for this platform (if not using global setting)")]
        [Range(0.1f, 1f)]
        public float customWidthPercentage = 1f;

        [Tooltip("Use custom width percentage instead of global?")]
        public bool useCustomWidth = false;
    }

    private class PlatformData
    {
        public Vector3 originalScale;
        public Vector3 originalPosition;
        public float originalWidth;
    }

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("No camera found!");
            return;
        }

        // Set camera to fixed size
        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = fixedOrthographicSize;
        }

        // Store original platform data
        StorePlatformData();

        // Setup platforms
        SetupPlatforms();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        if (targetCamera == null) return;

        bool screenChanged = !Mathf.Approximately(lastScreenSize.x, Screen.width) ||
                           !Mathf.Approximately(lastScreenSize.y, Screen.height);

        if (updateContinuously || screenChanged)
        {
            SetupPlatforms();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }

    void StorePlatformData()
    {
        // Store main floor data
        if (mainFloor != null)
        {
            StorePlatform(mainFloor);
        }

        // Store additional platforms data
        foreach (PlatformInfo platformInfo in platforms)
        {
            if (platformInfo.platform != null)
            {
                StorePlatform(platformInfo.platform);
            }
        }
    }

    void StorePlatform(GameObject platform)
    {
        if (platform == null || platformData.ContainsKey(platform)) return;

        PlatformData data = new PlatformData
        {
            originalScale = platform.transform.localScale,
            originalPosition = platform.transform.position
        };

        // Calculate original width
        SpriteRenderer sprite = platform.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            data.originalWidth = sprite.bounds.size.x;
        }
        else
        {
            data.originalWidth = platform.transform.localScale.x;
        }

        platformData[platform] = data;
    }

    void SetupPlatforms()
    {
        if (targetCamera == null || !targetCamera.orthographic) return;

        // Calculate viewport dimensions
        float viewportHeight = targetCamera.orthographicSize * 2f;
        float viewportWidth = viewportHeight * targetCamera.aspect;
        Vector3 cameraPos = targetCamera.transform.position;

        // Setup main floor
        if (mainFloor != null)
        {
            SetupMainFloor(viewportWidth, viewportHeight, cameraPos);
        }

        // Setup additional platforms
        foreach (PlatformInfo platformInfo in platforms)
        {
            if (platformInfo.platform != null && platformInfo.shouldScale)
            {
                float widthPercentage = platformInfo.useCustomWidth ?
                    platformInfo.customWidthPercentage : platformWidthPercentage;

                SetupPlatform(platformInfo.platform, viewportWidth, widthPercentage);
            }
        }

        Debug.Log($"Platforms setup complete - Viewport: {viewportWidth:F2} x {viewportHeight:F2}");
    }

    void SetupMainFloor(float viewportWidth, float viewportHeight, Vector3 cameraPos)
    {
        if (!platformData.ContainsKey(mainFloor)) return;

        PlatformData data = platformData[mainFloor];

        // Calculate target width (full screen + extra width)
        float targetWidth = viewportWidth + extraWidth;

        // Calculate scale needed
        float scaleX = targetWidth / data.originalWidth;

        // Apply new scale
        mainFloor.transform.localScale = new Vector3(
            data.originalScale.x * scaleX,
            data.originalScale.y,
            data.originalScale.z
        );

        // Position at specified Y offset from camera center
        float posY = cameraPos.y + mainFloorYOffset;
        mainFloor.transform.position = new Vector3(
            cameraPos.x,
            posY,
            data.originalPosition.z
        );

        // Ensure collider
        if (autoAddColliders)
        {
            EnsureCollider(mainFloor);
        }
    }

    void SetupPlatform(GameObject platform, float viewportWidth, float widthPercentage)
    {
        if (!platformData.ContainsKey(platform)) return;

        PlatformData data = platformData[platform];

        // Calculate target width based on percentage
        float targetWidth = (viewportWidth * widthPercentage) + extraWidth;

        // Calculate scale needed
        float scaleX = targetWidth / data.originalWidth;

        // Apply new scale (only X axis, preserve Y and Z)
        platform.transform.localScale = new Vector3(
            data.originalScale.x * scaleX,
            data.originalScale.y,
            data.originalScale.z
        );

        // Ensure collider
        if (autoAddColliders)
        {
            EnsureCollider(platform);
        }
    }

    void EnsureCollider(GameObject obj)
    {
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<BoxCollider2D>();
        }
    }

    // Public method to add a platform at runtime
    public void AddPlatform(GameObject platform, bool shouldScale = true, float customWidth = 1f, bool useCustom = false)
    {
        PlatformInfo info = new PlatformInfo
        {
            platform = platform,
            shouldScale = shouldScale,
            customWidthPercentage = customWidth,
            useCustomWidth = useCustom
        };

        platforms.Add(info);
        StorePlatform(platform);
        SetupPlatforms();
    }

    // Public method to remove a platform
    public void RemovePlatform(GameObject platform)
    {
        platforms.RemoveAll(p => p.platform == platform);
        platformData.Remove(platform);
    }

    // Public method to manually update platforms
    public void UpdatePlatforms()
    {
        SetupPlatforms();
    }

    // Get current viewport width
    public float GetViewportWidth()
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return 0f;

        float height = targetCamera.orthographicSize * 2f;
        return height * targetCamera.aspect;
    }

    // Get current viewport height
    public float GetViewportHeight()
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return 0f;

        return targetCamera.orthographicSize * 2f;
    }

    void OnDrawGizmos()
    {
        if (targetCamera == null || !targetCamera.orthographic) return;

        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;
        Vector3 cameraPos = targetCamera.transform.position;

        // Draw viewport bounds
        Gizmos.color = Color.green;
        Vector3 topLeft = cameraPos + new Vector3(-width / 2, height / 2, 0);
        Vector3 topRight = cameraPos + new Vector3(width / 2, height / 2, 0);
        Vector3 bottomLeft = cameraPos + new Vector3(-width / 2, -height / 2, 0);
        Vector3 bottomRight = cameraPos + new Vector3(width / 2, -height / 2, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // Draw main floor position line
        if (mainFloor != null)
        {
            Gizmos.color = Color.yellow;
            float floorY = cameraPos.y + mainFloorYOffset;
            Vector3 floorLeft = new Vector3(cameraPos.x - width / 2, floorY, 0);
            Vector3 floorRight = new Vector3(cameraPos.x + width / 2, floorY, 0);
            Gizmos.DrawLine(floorLeft, floorRight);
        }

        // Draw platform centers
        Gizmos.color = Color.cyan;
        foreach (PlatformInfo info in platforms)
        {
            if (info.platform != null)
            {
                Gizmos.DrawWireSphere(info.platform.transform.position, 0.2f);
            }
        }
    }
}