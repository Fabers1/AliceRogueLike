using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all level objects to fit perfectly in any device viewport.
/// This ensures NO blank spaces on tablets, phones, or any screen size.
/// Attach this to an empty GameObject in your scene.
/// </summary>
public class LevelViewportManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The camera to fit level to. If null, uses Camera.main")]
    public Camera targetCamera;

    [Tooltip("Keep camera orthographic size fixed (recommended for consistent gameplay)")]
    public float fixedOrthographicSize = 5f;

    [Header("Level Objects")]
    [Tooltip("Background object that fills the entire screen")]
    public GameObject background;

    [Tooltip("Floor/ground platform that spans the screen width")]
    public GameObject floor;

    [Tooltip("Left boundary wall")]
    public GameObject leftWall;

    [Tooltip("Right boundary wall")]
    public GameObject rightWall;

    [Tooltip("Top boundary wall (optional)")]
    public GameObject topWall;

    [Tooltip("Ceiling/roof (optional)")]
    public GameObject ceiling;

    [Header("Wall Settings")]
    [Tooltip("Thickness of side walls")]
    public float wallThickness = 0.5f;

    [Tooltip("Should walls extend beyond visible area?")]
    public float wallExtension = 2f;

    [Header("Additional Platforms")]
    [Tooltip("List of platforms that should scale with screen width")]
    public List<GameObject> scalablePlatforms = new List<GameObject>();

    [Header("Update Settings")]
    [Tooltip("Update continuously (for testing different aspect ratios)")]
    public bool updateContinuously = false;

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
    private Vector2 lastScreenSize;

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

        // Store original transforms
        StoreOriginalTransforms();

        // Set camera to fixed size
        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = fixedOrthographicSize;
        }

        // Setup level
        SetupLevel();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        if (targetCamera == null) return;

        bool screenChanged = !Mathf.Approximately(lastScreenSize.x, Screen.width) ||
                           !Mathf.Approximately(lastScreenSize.y, Screen.height);

        if (updateContinuously || screenChanged)
        {
            SetupLevel();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }

    void StoreOriginalTransforms()
    {
        StoreTransform(background);
        StoreTransform(floor);
        StoreTransform(leftWall);
        StoreTransform(rightWall);
        StoreTransform(topWall);
        StoreTransform(ceiling);

        foreach (GameObject platform in scalablePlatforms)
        {
            StoreTransform(platform);
        }
    }

    void StoreTransform(GameObject obj)
    {
        if (obj != null)
        {
            originalScales[obj] = obj.transform.localScale;
            originalPositions[obj] = obj.transform.position;
        }
    }

    void SetupLevel()
    {
        if (targetCamera == null || !targetCamera.orthographic) return;

        // Calculate viewport dimensions
        float viewportHeight = targetCamera.orthographicSize * 2f;
        float viewportWidth = viewportHeight * targetCamera.aspect;
        Vector3 cameraPos = targetCamera.transform.position;

        // Setup background (fills entire screen)
        if (background != null)
        {
            SetupBackground(viewportWidth, viewportHeight, cameraPos);
        }

        // Setup floor (spans screen width at bottom)
        if (floor != null)
        {
            SetupFloor(viewportWidth, viewportHeight, cameraPos);
        }

        // Setup walls (at screen edges)
        if (leftWall != null)
        {
            SetupLeftWall(viewportWidth, viewportHeight, cameraPos);
        }

        if (rightWall != null)
        {
            SetupRightWall(viewportWidth, viewportHeight, cameraPos);
        }

        if (topWall != null)
        {
            SetupTopWall(viewportWidth, viewportHeight, cameraPos);
        }

        if (ceiling != null)
        {
            SetupCeiling(viewportWidth, viewportHeight, cameraPos);
        }

        // Setup scalable platforms
        foreach (GameObject platform in scalablePlatforms)
        {
            if (platform != null)
            {
                SetupScalablePlatform(platform, viewportWidth);
            }
        }

        Debug.Log($"Level setup complete - Viewport: {viewportWidth:F2} x {viewportHeight:F2}");
    }

    void SetupBackground(float width, float height, Vector3 cameraPos)
    {
        // Scale background to fill screen
        Vector3 originalScale = originalScales[background];
        SpriteRenderer sprite = background.GetComponent<SpriteRenderer>();

        if (sprite != null)
        {
            // Calculate scale needed to fill viewport
            float spriteWidth = sprite.bounds.size.x / originalScale.x;
            float spriteHeight = sprite.bounds.size.y / originalScale.y;

            float scaleX = width / spriteWidth;
            float scaleY = height / spriteHeight;

            // Use max to ensure no blank spaces
            float scale = Mathf.Max(scaleX, scaleY);
            background.transform.localScale = originalScale * scale;
        }
        else
        {
            // No sprite, just scale to viewport
            background.transform.localScale = new Vector3(width, height, originalScale.z);
        }

        // Center background
        background.transform.position = new Vector3(cameraPos.x, cameraPos.y, background.transform.position.z);
    }

    void SetupFloor(float width, float height, Vector3 cameraPos)
    {
        Vector3 originalScale = originalScales[floor];
        Vector3 originalPos = originalPositions[floor];

        // Scale floor to screen width
        SpriteRenderer sprite = floor.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            float spriteWidth = sprite.bounds.size.x / originalScale.x;
            float scaleX = width / spriteWidth;
            floor.transform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y, originalScale.z);
        }
        else
        {
            floor.transform.localScale = new Vector3(width, originalScale.y, originalScale.z);
        }

        // Position at bottom of screen
        float floorY = cameraPos.y - (height / 2f) + (GetObjectHeight(floor) / 2f);
        floor.transform.position = new Vector3(cameraPos.x, floorY, originalPos.z);

        // Add/update collider
        EnsureCollider(floor);
    }

    void SetupLeftWall(float width, float height, Vector3 cameraPos)
    {
        Vector3 originalScale = originalScales[leftWall];
        Vector3 originalPos = originalPositions[leftWall];

        // Scale wall height to cover screen + extension
        float wallHeight = height + (wallExtension * 2f);
        leftWall.transform.localScale = new Vector3(wallThickness, wallHeight, originalScale.z);

        // Position at left edge
        float wallX = cameraPos.x - (width / 2f) + (wallThickness / 2f);
        leftWall.transform.position = new Vector3(wallX, cameraPos.y, originalPos.z);

        EnsureCollider(leftWall);
    }

    void SetupRightWall(float width, float height, Vector3 cameraPos)
    {
        Vector3 originalScale = originalScales[rightWall];
        Vector3 originalPos = originalPositions[rightWall];

        // Scale wall height to cover screen + extension
        float wallHeight = height + (wallExtension * 2f);
        rightWall.transform.localScale = new Vector3(wallThickness, wallHeight, originalScale.z);

        // Position at right edge
        float wallX = cameraPos.x + (width / 2f) - (wallThickness / 2f);
        rightWall.transform.position = new Vector3(wallX, cameraPos.y, originalPos.z);

        EnsureCollider(rightWall);
    }

    void SetupTopWall(float width, float height, Vector3 cameraPos)
    {
        Vector3 originalScale = originalScales[topWall];
        Vector3 originalPos = originalPositions[topWall];

        // Scale wall width to cover screen
        topWall.transform.localScale = new Vector3(width + (wallThickness * 2f), wallThickness, originalScale.z);

        // Position at top edge
        float wallY = cameraPos.y + (height / 2f) - (wallThickness / 2f);
        topWall.transform.position = new Vector3(cameraPos.x, wallY, originalPos.z);

        EnsureCollider(topWall);
    }

    void SetupCeiling(float width, float height, Vector3 cameraPos)
    {
        Vector3 originalScale = originalScales[ceiling];
        Vector3 originalPos = originalPositions[ceiling];

        // Scale to screen width
        SpriteRenderer sprite = ceiling.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            float spriteWidth = sprite.bounds.size.x / originalScale.x;
            float scaleX = width / spriteWidth;
            ceiling.transform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y, originalScale.z);
        }
        else
        {
            ceiling.transform.localScale = new Vector3(width, originalScale.y, originalScale.z);
        }

        // Position at top
        float ceilingY = cameraPos.y + (height / 2f) - (GetObjectHeight(ceiling) / 2f);
        ceiling.transform.position = new Vector3(cameraPos.x, ceilingY, originalPos.z);

        EnsureCollider(ceiling);
    }

    void SetupScalablePlatform(GameObject platform, float width)
    {
        Vector3 originalScale = originalScales[platform];

        // Scale platform to percentage of screen width
        SpriteRenderer sprite = platform.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            float spriteWidth = sprite.bounds.size.x / originalScale.x;
            float scaleX = width / spriteWidth;
            platform.transform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y, originalScale.z);
        }

        EnsureCollider(platform);
    }

    void EnsureCollider(GameObject obj)
    {
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            obj.AddComponent<BoxCollider2D>();
        }
    }

    float GetObjectHeight(GameObject obj)
    {
        SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            return sprite.bounds.size.y;
        }
        return obj.transform.localScale.y;
    }

    // Public method to add a platform at runtime
    public void AddScalablePlatform(GameObject platform)
    {
        if (!scalablePlatforms.Contains(platform))
        {
            scalablePlatforms.Add(platform);
            StoreTransform(platform);
            SetupLevel();
        }
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
    }
}