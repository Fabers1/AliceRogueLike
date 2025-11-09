using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager instance;

    [Header("References")]
    public PlayerStats playerStats;
    public WeaponController weaponController;
    public PlayerMovement playerMovement;

    [Header("PowerUp Pool")]
    public List<PowerUpData> avaiablePowerUps = new List<PowerUpData>();

    [Header("Active PowerUps")]
    Dictionary<PowerUpType, int> activePowerUpLevels = new Dictionary<PowerUpType, int>();
    Dictionary<PowerUpType, PowerUpData> powerUpDataLookup = new Dictionary<PowerUpType, PowerUpData>();

    [HideInInspector] public float totalAttackRangeMultiplier = 1f;
    [HideInInspector] public float totalSpeedMultiplier = 1f;
    [HideInInspector] public int totalXPMultiplier = 1;
    [HideInInspector] public bool canChooseTransformation = false;

    public event Action<PowerUpData> OnPowerUpApplied;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        InitializePowerUpLookup();
    }

    void InitializePowerUpLookup()
    {
        powerUpDataLookup.Clear();
        foreach(var powerUp in avaiablePowerUps)
        {
            if (!powerUpDataLookup.ContainsKey(powerUp.type))
            {
                powerUpDataLookup.Add(powerUp.type, powerUp);
                activePowerUpLevels[powerUp.type] = 0;
            }
        }
    }

    public List<PowerUpData> GetRandomPowerUpOptions(int count = 3)
    {
        List<PowerUpData> options = new List<PowerUpData>();
        List<PowerUpData> eligiblePowerUps = avaiablePowerUps
            .Where(p => activePowerUpLevels[p.type] < p.maxLevel)
            .ToList();

        if(eligiblePowerUps.Count == 0)
        {
            eligiblePowerUps = avaiablePowerUps.ToList();
        }

        System.Random rng = new System.Random();
        eligiblePowerUps = eligiblePowerUps.OrderBy(x => rng.Next()).ToList();

        int optionsToTake = Mathf.Min(count, eligiblePowerUps.Count);
        for (int i = 0; i < optionsToTake; i++) 
        {
            options.Add(eligiblePowerUps[i]);
        }

        return options;
    }

    public void ApplyPowerUp(PowerUpData powerUp)
    {
        if (!activePowerUpLevels.ContainsKey(powerUp.type))
        {
            activePowerUpLevels[powerUp.type] = 0;
        }

        activePowerUpLevels[powerUp.type]++;

        switch (powerUp.type)
        {
            case PowerUpType.PirulitoCha:
                ApplyAttackRangeBoost(powerUp.attackRangeMultiplier);
                break;

            case PowerUpType.CartaEspadas:
                ApplyXPMultiplier(powerUp.xpMultiplier);
                break;

            case PowerUpType.BiscoitoLirio:
                canChooseTransformation = true;
                break;
        }

        OnPowerUpApplied?.Invoke(powerUp);

        Debug.Log($"Applied {powerUp.powerUpName} - Level {activePowerUpLevels[powerUp.type]}");
    }

    private void ApplyAttackRangeBoost(float multiplier)
    {
        totalAttackRangeMultiplier *= multiplier;

        if(weaponController != null)
        {
            BoxCollider2D weaponCollider = weaponController.GetComponent<BoxCollider2D>();
            if(weaponCollider != null)
            {
                Vector2 newSize = weaponCollider.size;
                newSize.x *= multiplier;
                weaponCollider.size = newSize;
            }
        }
    }

    private void ApplyXPMultiplier(int xpMultiplier)
    {
        totalXPMultiplier *= xpMultiplier;
    }

    public int GetPowerUpLevel(PowerUpType type)
    {
        return activePowerUpLevels.ContainsKey(type) ? activePowerUpLevels[type] : 0;
    }

    public void ResetPowerUps()
    {
        activePowerUpLevels.Clear();
        totalAttackRangeMultiplier = 1f;
        totalXPMultiplier = 1;
        canChooseTransformation = false;

        InitializePowerUpLookup();
    }
}
