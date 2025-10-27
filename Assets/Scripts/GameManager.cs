using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player Progression")]
    [HideInInspector]
    public int level = 1;
    [Tooltip("Player's max life when the game starts")]
    public int startHealth = 5;
    [HideInInspector]
    public int xp;
    public int xpThreshold = 1000;

    [Header("UI References - Menu Scene")]
    public TextMeshProUGUI xpTxt;
    public TextMeshProUGUI levelTxt;
    public Slider xpSlider;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadProgress();
    }

    public void RegisterUI(TextMeshProUGUI levelText, TextMeshProUGUI xpText, Slider slider)
    {
        levelTxt = levelText;
        xpTxt = xpText;
        xpSlider = slider;
        UpdateUI();
    }

    public void LevelUp()
    {
        level++;
        xp -= xpThreshold;
        xpThreshold += 1000;

        startHealth += level;

        UpdateUI();
    }

    public void AddXP(int amount)
    {
        xp += amount;
        
        if(xp >= xpThreshold)
        {
            LevelUp();
        }

        UpdateUI();
    }


    public void UpdateUI()
    {
        if (levelTxt != null) levelTxt.text = level.ToString();
        if (xpTxt != null) xpTxt.text = $"{xp}/{xpThreshold}";

        if (xpSlider != null)
        {
            xpSlider.maxValue = xpThreshold;
            xpSlider.value = xp;
        }
    }

    public void ResetProgression()
    {
        PlayerPrefs.DeleteAll();

        LoadProgress();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveProgress();
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("XP", xp);
        PlayerPrefs.SetInt("XPThreshold", xpThreshold);
        PlayerPrefs.SetInt("StartHealth", startHealth);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        level = PlayerPrefs.GetInt("Level", 1);
        xp = PlayerPrefs.GetInt("XP", 0);
        xpThreshold = PlayerPrefs.GetInt("XPThreshold", 1000);
        startHealth = PlayerPrefs.GetInt("StartHealth", 5);
        UpdateUI();
    }
}
