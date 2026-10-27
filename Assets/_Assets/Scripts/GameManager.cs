using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject loading;
    [SerializeField] private Ring[] rings;
    [SerializeField] private Color[] ringColors;
    [SerializeField] private BallSpawner[] ballSpawners;
    [SerializeField] private GameObject healthUi;
    [SerializeField] TMP_Text _healthText, ringLeveltext, nextRingUnlockLevel;
    [SerializeField] private GameObject nextRingLevelUi;
    [SerializeField] Image _fillBar;

    [SerializeField] private ParticleSystem ringDestroyEffect;
    [SerializeField] private Material[] ringMaterials;

    [Header("Background")]
    [SerializeField] Image background;
    [SerializeField] Sprite[] backgroundSprites;

    [Header("Interstitial Pacing Settings")]
    [SerializeField] private float _interstitialCooldown = 120f;

    [Header("Level Setup Settings")]
    [SerializeField] private int _startingRings = 1;
    [SerializeField] private int _increaseRingEveryXLevels = 1;

    private int bgIndex;

    private int _currentRingIndex = 0;
    private int _totalActiavtedRings;

    private List<Double> healthList = new List<Double>();
    private Camera _cam;

    private void OnEnable()
    {
        HoleSkinsManager.OnSkinChanged += ChangeSkin;
        Ring.OnHealthChanged += UpdateHealthUi;
        UpgradeManager.OnFirstTimeUpgrade += EnableBallSpawner;
    }

    private void OnDisable()
    {
        HoleSkinsManager.OnSkinChanged -= ChangeSkin;
        Ring.OnHealthChanged -= UpdateHealthUi;
        UpgradeManager.OnFirstTimeUpgrade -= EnableBallSpawner;
    }

    public void ShowShop()
    {
        ShopManager.instance.OpenShopPanle();
        GameAnalyticsController.InfiniteGameProgressionRelated.LogInfiniteGameStartEvent("ring_level", 5);
    }

    void ChangeSkin()
    {
        for (int i = 0; i < rings.Length; i++)
        {
            rings[i].SwitchMaterial(ringMaterials[HoleSkinsManager.currentSkinIndex]);
        }
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        _cam = Camera.main;
    }

    private void Start()
    {
        Setup();
        EnableAllBallSpawner();
        //StartCoroutine(ShowInterstitial());
    }

    IEnumerator ShowInterstitial()
    {
        while (true)
        {
            yield return new WaitForSeconds(_interstitialCooldown);
            HCSDKManager.INSTANCE.ShowInterstitialAd(HCSDKManager.IS_LOAD_NAME, null);
        }
    }

    private void Setup()
    {
        _currentRingIndex = 0;
        healthList.Clear();
        Shuffle(ringColors);
        CalculateRingData();
        for (int i = 0; i < ballSpawners.Length; i++)
        {
            ballSpawners[i].SetBallIndex(i);
        }
        UpdateHealthUi();
        healthUi.SetActive(true);
    }

    public static void Shuffle<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }

    private int ringSet = 0;
    private void CalculateRingData()
    {
        ringSet = PlayerPrefs.GetInt(MyConstants.RING_LEVEL, 1);

        ShowLevelProgressionEvent(ringSet, ProgressionType.Start);

        ringLeveltext.text = $"(Lv. {ringSet})";

        // Ring count calculation
        _totalActiavtedRings = _startingRings + ((ringSet - 1) / _increaseRingEveryXLevels);
        _totalActiavtedRings = Mathf.Clamp(_totalActiavtedRings, _startingRings, rings.Length);

        // Next unlock UI
        if (_totalActiavtedRings >= rings.Length)
        {
            nextRingLevelUi.SetActive(false);
        }
        else
        {
            int nextUnlockLevel = (((ringSet - 1) / _increaseRingEveryXLevels) + 1) * _increaseRingEveryXLevels + 1;

            nextRingUnlockLevel.text = $"Lv.{nextUnlockLevel}";
            nextRingLevelUi.SetActive(true);
        }

        // Health scaling
        int x = ringSet - 1;

        double incrementVal = 5 * Math.Pow(4f, x);
        double initialVal = 50;

        if (ringSet > 1)
        {
            initialVal = (37 * Math.Pow(x, 4)) - (170 * Math.Pow(x, 2)) + (295 * x) - 104;
        }

        healthList.Clear();

        for (int i = 0; i < rings.Length; i++)
        {
            rings[i].gameObject.SetActive(i < _totalActiavtedRings);
        }

        for (int i = 0; i < _totalActiavtedRings; i++)
        {
            healthList.Add(initialVal);

            float t = _totalActiavtedRings > 1
                ? (float)i / (_totalActiavtedRings - 1)
                : 0f;

            rings[i].SetParameters(
                Mathf.Lerp(1f, 3f, t),
                ringColors[i],
                initialVal);

            initialVal += incrementVal;
        }

        ChangeBg();
    }

    void ChangeBg()
    {
        if (ringSet % 5 == 0)
        {
            background.sprite = backgroundSprites[bgIndex];
            bgIndex++;
            if (bgIndex == backgroundSprites.Length)
            {
                bgIndex = 0;
            }
        }
    }

    private void UpdateHealthUi()
    {
        double currentHealth = rings[_currentRingIndex].GetHealth();
        if (currentHealth <= 0)
        {
            PlayRingDestroyParticle(ringColors[_currentRingIndex], rings[_currentRingIndex].GetRadius());
            _currentRingIndex++;
            if (_currentRingIndex == _totalActiavtedRings)
            {
                for (int i = 0; i < ballSpawners.Length; i++)
                {
                    ballSpawners[i].StopSpawning();
                }

                ShowLevelProgressionEvent(ringSet, ProgressionType.Finish);
                ringSet++;
                PlayerPrefs.SetInt(MyConstants.RING_LEVEL, ringSet);
                healthUi.SetActive(false);

                loading.SetActive(true);

                // EVERY LEVEL IS
                HCSDKManager.INSTANCE.ShowInterstitialAd(HCSDKManager.IS_LOAD_NAME, () =>
                {
                    loading.SetActive(false);
                });

                Invoke(nameof(Setup), 2f);
                Invoke(nameof(EnableAllBallSpawner), 2f);
            }
        }

        if (_currentRingIndex < _totalActiavtedRings)
        {
            _healthText.text = NumberFormatter.FormatNumberSmall(rings[_currentRingIndex].GetHealth());
            _fillBar.color = ringColors[_currentRingIndex];
            _fillBar.fillAmount = (float)(rings[_currentRingIndex].GetHealth() / healthList[_currentRingIndex]);
        }
    }

    void PlayRingDestroyParticle(Color newColor, float newRadius)
    {
        ParticleSystem[] systems = ringDestroyEffect.GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in systems)
        {
            var main = ps.main;
            main.startColor = newColor;

            var shape = ps.shape;
            shape.radius = newRadius;
        }

        ringDestroyEffect.Play();
    }

    void EnableAllBallSpawner()
    {
        for (int i = 0; i < 8; i++)
        {
            EnableBallSpawner(i);
        }
    }

    void EnableBallSpawner(int ballSpawnerIndex)
    {
        ballSpawners[ballSpawnerIndex].Spawn();
    }

    readonly Color CriticalOrange = new Color(1f, 0.5f, 0f);
    public void AddMoneyOnCollide(double money, Vector3 pos, bool IsCriticalHit)
    {
        // =======================================
        // if related Rv or IAP Active
        if (UpgradeManager.IncomeMultiplierActive)
        {
            money *= UpgradeManager.IncomeMultiplier;
        }
        // ========================================
        Vector3 dir = (pos - rings[0].transform.position).normalized;

        EconomyManager.instance.IncreaseEconomy(money);
        Vector3 spawnPos = _cam.WorldToScreenPoint(pos + dir * 0.6f);
        GameObject obj = ObjectPooling.Instance.Get("float_text", spawnPos);
        string moneyText = NumberFormatter.FormatNumberSmall(money);
        Color textColor = IsCriticalHit ? CriticalOrange : Color.white;
        obj.GetComponent<FloatingText>().Show(moneyText, textColor);
    }

    private static float LastLevelStartTime;
    public static void ShowLevelProgressionEvent(int levelNumber, ProgressionType progressionType)
    {
        if (progressionType == ProgressionType.Start)
        {
            LastLevelStartTime = Time.realtimeSinceStartup;
            GameAnalyticsController.LevelBasedProgressionRelated.LogLevelStartEventWithTime(levelNumber);
        }
        else if (progressionType == ProgressionType.Fail)
        {
            var levelData = new GameAnalyticsController.LevelProgressTimeData(levelNumber, LastLevelStartTime);
            GameAnalyticsController.LevelBasedProgressionRelated.LogLevelFailEventWithTime(levelData);
        }
        else if (progressionType == ProgressionType.Finish)
        {
            var levelData = new GameAnalyticsController.LevelProgressTimeData(levelNumber, LastLevelStartTime);
            GameAnalyticsController.LevelBasedProgressionRelated.LogLevelEndEventWithTime(levelData);
        }
    }

    public enum ProgressionType
    {
        Start,
        Finish,
        Fail
    }
}
