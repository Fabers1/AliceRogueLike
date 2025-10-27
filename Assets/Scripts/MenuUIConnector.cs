using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIConnector : MonoBehaviour
{
    [Header("Drag your Menu UI elements here")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public Slider xpSlider;

    private void Start()
    {
        if (GameManager.instance != null)
        {
            // Register this scene's UI with the GameManager
            GameManager.instance.RegisterUI(levelText, xpText, xpSlider);
        }
        else
        {
            Debug.LogError("GameManager instance not found! Make sure it exists in the first scene.");
        }
    }
}
