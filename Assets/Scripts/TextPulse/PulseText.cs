using UnityEngine;

public class PulseText : MonoBehaviour
{
    public float pulseSpeed = 1f; // Velocidade da pulsação
    public float scaleAmount = 0.1f; // Quanto o botão aumenta
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scaleFactor = 1 + Mathf.Sin(Time.time * pulseSpeed) * scaleAmount;
        transform.localScale = originalScale * scaleFactor;
    }
}
