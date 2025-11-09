using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameTxt;
    public TextMeshProUGUI descriptionTxt;
    public TextMeshProUGUI levelTxt;
    public Button button;

    private PowerUpData currentPowerUp;
    private PowerUpUI uiManager;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(PowerUpData powerUp, PowerUpUI ui)
    {
        currentPowerUp = powerUp;
        uiManager = ui;

        if (iconImage != null)
            iconImage.sprite = powerUp.icon;

        if (nameTxt != null)
            nameTxt.text = powerUp.powerUpName;

        if (descriptionTxt != null)
            descriptionTxt.text = powerUp.description;

        if (levelTxt != null)
        {
            int currentLevel = PowerUpManager.instance.GetPowerUpLevel(powerUp.type);
            levelTxt.text = $"Lvl {currentLevel + 1}";
        }
    }

    void OnButtonClicked()
    {
        if (currentPowerUp != null && uiManager != null)
        {
            uiManager.OnPowerUpSelected(currentPowerUp);
        }
    }
}

