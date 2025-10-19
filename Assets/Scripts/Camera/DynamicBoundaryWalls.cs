using UnityEngine;

/// <summary>
/// Automatically positions boundary walls at the edges of the camera view.
/// Works with any aspect ratio and screen size.
/// Attach this to an empty GameObject that will hold all boundary walls.
/// </summary>
public class DynamicBoundaryWalls : MonoBehaviour
{
    [Header("Wall References")]
    [Tooltip("Assign your left wall GameObject")]
    public GameObject leftWall;

    [Tooltip("Assign your right wall GameObject")]
    public GameObject rightWall;

    [Tooltip("Assign your top wall GameObject (optional)")]
    public GameObject topWall;

    [Tooltip("Assign your bottom wall GameObject (optional)")]
    public GameObject bottomWall;

    [Header("Settings")]
    [Tooltip("The camera to calculate bounds from. If null, uses Camera.main")]
    public Camera targetCamera;

    [Tooltip("Offset from the edge of the screen (positive = inward, negative = outward)")]
    public float edgeOffset = 0f;

    [Tooltip("Thickness of the walls (for BoxCollider2D)")]
    public float wallThickness = 1f;

    [Tooltip("Height of vertical walls (left/right)")]
    public float wallHeight = 20f;

    [Tooltip("Update walls in real-time (useful for testing different aspect ratios)")]
    public bool updateContinuously = false;

    private Vector2 lastScreenSize;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("No camera found! Assign a camera or ensure Camera.main exists.");
            return;
        }

        PositionWalls();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        // Check if screen size changed (aspect ratio change)
        if (updateContinuously || HasScreenSizeChanged())
        {
            PositionWalls();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }

    bool HasScreenSizeChanged()
    {
        return !Mathf.Approximately(lastScreenSize.x, Screen.width) ||
               !Mathf.Approximately(lastScreenSize.y, Screen.height);
    }

    void PositionWalls()
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            Debug.LogWarning("Camera is null or not orthographic!");
            return;
        }

        // Calculate camera bounds in world space
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        Vector3 cameraPos = targetCamera.transform.position;

        // Calculate edge positions
        float leftEdge = cameraPos.x - (cameraWidth / 2f) + edgeOffset;
        float rightEdge = cameraPos.x + (cameraWidth / 2f) - edgeOffset;
        float topEdge = cameraPos.y + (cameraHeight / 2f) - edgeOffset;
        float bottomEdge = cameraPos.y - (cameraHeight / 2f) + edgeOffset;

        // Position and configure left wall
        if (leftWall != null)
        {
            leftWall.transform.position = new Vector3(leftEdge, cameraPos.y, 0);
            ConfigureWallCollider(leftWall, wallThickness, wallHeight);
        }

        // Position and configure right wall
        if (rightWall != null)
        {
            rightWall.transform.position = new Vector3(rightEdge, cameraPos.y, 0);
            ConfigureWallCollider(rightWall, wallThickness, wallHeight);
        }

        // Position and configure top wall
        if (topWall != null)
        {
            topWall.transform.position = new Vector3(cameraPos.x, topEdge, 0);
            ConfigureWallCollider(topWall, cameraWidth + wallThickness * 2, wallThickness);
        }

        // Position and configure bottom wall
        if (bottomWall != null)
        {
            bottomWall.transform.position = new Vector3(cameraPos.x, bottomEdge, 0);
            ConfigureWallCollider(bottomWall, cameraWidth + wallThickness * 2, wallThickness);
        }

        Debug.Log($"Walls positioned - Camera Width: {cameraWidth:F2}, Height: {cameraHeight:F2}");
    }

    void ConfigureWallCollider(GameObject wall, float width, float height)
    {
        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = wall.AddComponent<BoxCollider2D>();
        }
        collider.size = new Vector2(width, height);
    }

    // Public method to manually update walls (useful for runtime aspect ratio changes)
    public void UpdateWalls()
    {
        PositionWalls();
    }

    // Helper method to get current camera bounds
    public Bounds GetCameraBounds()
    {
        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;

        Vector3 center = targetCamera.transform.position;
        return new Bounds(center, new Vector3(width, height, 0));
    }

    // Draw gizmos to visualize wall positions in the editor
    void OnDrawGizmos()
    {
        if (targetCamera == null) return;
        if (!targetCamera.orthographic) return;

        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;
        Vector3 pos = targetCamera.transform.position;

        Gizmos.color = Color.red;

        // Draw wall positions
        float leftEdge = pos.x - (width / 2f) + edgeOffset;
        float rightEdge = pos.x + (width / 2f) - edgeOffset;
        float topEdge = pos.y + (height / 2f) - edgeOffset;
        float bottomEdge = pos.y - (height / 2f) + edgeOffset;

        // Left wall
        Gizmos.DrawWireCube(new Vector3(leftEdge, pos.y, 0), new Vector3(wallThickness, wallHeight, 0));

        // Right wall
        Gizmos.DrawWireCube(new Vector3(rightEdge, pos.y, 0), new Vector3(wallThickness, wallHeight, 0));

        // Top wall (if exists)
        if (topWall != null)
            Gizmos.DrawWireCube(new Vector3(pos.x, topEdge, 0), new Vector3(width, wallThickness, 0));

        // Bottom wall (if exists)
        if (bottomWall != null)
            Gizmos.DrawWireCube(new Vector3(pos.x, bottomEdge, 0), new Vector3(width, wallThickness, 0));
    }
}