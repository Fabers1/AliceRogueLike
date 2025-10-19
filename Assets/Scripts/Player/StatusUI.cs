using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    public PlayerStats playerStats;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI insanityTxt;

    public SpawnManager enemyAmount;

    public TextMeshProUGUI enemyAmountTxt;

    private void Start()
    {
        healthText.text = $"{playerStats.curHealth:F0}";

        insanityTxt.text = $"{playerStats.curInsanity:F0}";

        playerStats.OnHealthChanged += UpdateHealth;

        playerStats.OnInsanityChanged += UpdateInsanity;

        enemyAmount = SpawnManager.instance;

        enemyAmountTxt.text = $"{enemyAmount.GetRemainingEnemies():F0}";

        enemyAmount.OnEnemyCountChanged += UpdateEnemyCount;
    }

    private void UpdateHealth(int curHealth)
    {
        healthText.text = $"{curHealth:F0}";
    }

    private void UpdateInsanity(int curInsanity)
    {
        insanityTxt.text = $"{curInsanity:F0}";
    }

    private void UpdateEnemyCount(int curEnemy, int remainingEnemy)
    {
        enemyAmountTxt.text = $"{enemyAmount.GetRemainingEnemies():F0}";
    }
}
