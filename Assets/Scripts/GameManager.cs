using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector]
    public int level = 1;
    public TextMeshProUGUI levelTxt;
    [HideInInspector]
    public int xp;
    public int xpThreshold = 1000;
    public Slider xpSlider;
    public TextMeshProUGUI xpTxt;

    public int startHealth = 5;

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
        }
    }

    private void Start()
    {
        levelTxt.text = level.ToString();
        xpTxt.text = $"{xp}/{xpThreshold}";
        xpSlider.value = xp;
        xpSlider.maxValue = xpThreshold;
    }

    public void WinGame()
    {
        Debug.Log("Level Won!");
    }


}
