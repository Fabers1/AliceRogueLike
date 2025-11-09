using System.Collections.Generic;
using UnityEngine;

public class PowerUpUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject powerUpPanel;
    public List<PowerUpButton> powerUpButtons = new List<PowerUpButton>();
    public LevelManager levelManager;

    [Header("Settings")]
    public int optionsCount = 3;

    private bool isShowing = false;

    private void Start()
    {
        HidePowerUpSelection();

        if (SpawnManager.instance != null)
        {
            SpawnManager.instance.OnStageCompleted.AddListener(ShowPowerUpSelection);
        }
    }

    public void ShowPowerUpSelection()
    {
        if (isShowing) return;

        isShowing = true;
        Time.timeScale = 0f;

        List<PowerUpData> options = PowerUpManager.instance.GetRandomPowerUpOptions(optionsCount);

        for (int i = 0; i < powerUpButtons.Count; i++)
        {
            if (i < optionsCount)
            {
                powerUpButtons[i].Setup(options[i], this);
                powerUpButtons[i].gameObject.SetActive(true);
            }
            else
            {
                powerUpButtons[i].gameObject.SetActive(false);
            }
        }

        powerUpPanel.SetActive(true);
    }

    public void OnPowerUpSelected(PowerUpData powerUp)
    {
        PowerUpManager.instance.ApplyPowerUp(powerUp);
        HidePowerUpSelection();

        if (levelManager != null) 
        {
            levelManager.OnPowerUpSelectedContinue();
        }
    }

    public void HidePowerUpSelection()
    {
        isShowing = false;
        powerUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}