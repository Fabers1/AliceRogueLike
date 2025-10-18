using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public PlayerStats playerHealth;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    private void Start()
    {
        healthText.text = $"{playerHealth.curHealth:F0}/{playerHealth.curMaxHealth:F0}";

        healthSlider.maxValue = playerHealth.curMaxHealth;

        healthSlider.value = playerHealth.curHealth;

        playerHealth.OnHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(int curHealth, int maxHealth)
    {
        Debug.Log(1);

        healthText.text = $"{curHealth:F0}/{maxHealth:F0}";
        healthSlider.maxValue = playerHealth.curMaxHealth;

        float oldValue = healthSlider.value;
        float newValue = playerHealth.curHealth;

        StartCoroutine(AnimateHealthBar(oldValue, newValue));
    }

    private IEnumerator AnimateHealthBar(float oldValue, float newValue)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            healthSlider.value = Mathf.Lerp(oldValue, newValue, t);
            yield return null;
        }

        healthSlider.value = newValue;
    }
}
