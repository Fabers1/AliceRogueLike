using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public static Transform FindTopmostPlatform()
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");

        if(platforms.Length == 0)
        {
            Debug.LogError("No platforms found!");
            return null;
        }

        Transform topPlatform = platforms[0].transform;
        float highestY = topPlatform.position.y;

        foreach (GameObject platform in platforms) 
        {
            if (platform.transform.position.y > highestY) 
            {
                highestY = platform.transform.position.y;
                topPlatform = platform.transform;
            }
        }

        return topPlatform;
    }

    public static Vector2 GetPositionOnPlatform(Transform platform, float xOffset = 0f, float yOffset = 1f)
    {
        if(platform == null) return Vector2.zero;

        Collider2D platformCollider = platform.GetComponent<Collider2D>();

        Vector2 position;

        if (platformCollider != null) 
        { 
            Bounds bounds = platformCollider.bounds;

            float maxOffset = bounds.extents.x * 0.8f;
            xOffset = Mathf.Clamp(xOffset, -maxOffset, maxOffset);

            position = new Vector2(
                bounds.center.x + xOffset,
                bounds.max.y + yOffset
            );
        }
        else
        {
            position = new Vector2(
                platform.position.x + xOffset,
                platform.position.y + yOffset
            );
        }

        return position;
    }
}
