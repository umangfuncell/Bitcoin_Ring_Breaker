using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GameAnalyticsSDK;
using Coredian.Attribution;
using Coredian.Unity;
using Coredian.Advertisements;
using Coredian.Firebase;

public class HCSDKManager : HostMonoBehaviour
{
    public static HCSDKManager INSTANCE;
    public const string IS_LOAD_NAME = "level_ads";
    public const string RV_LOAD_NAME = "rv_video";

    const string NO_ADS_KEY = "NoAds";

    public GameObject dnd;

    public bool IsInitialized { get; private set; }

    private IAdvertisementsService _adService;
    private IInterstitialController _interstitialController;
    private IRewardedController _rewardedController;
    private IBannerController _bannerController;

    private IFirebaseService _firebase;
    private IAttributionService _attribution;

    private Action _rvCallback;
    private Action _isCallback;
    private string _placementRV;
    private string _placementInter;

    // Interstitial cooldown: timestamp-based instead of a 1s InvokeRepeating tick.
    int AD_WAIT_TIME = 20;
    float _lastAdTime;
    int TimeSeconds => Mathf.FloorToInt(Time.unscaledTime - _lastAdTime);

    private void Awake()
    {
        if (INSTANCE != null && INSTANCE != this)
        {
            Debug.LogWarning("[HCSDKManager] Duplicate instance found, destroying component.");
            Destroy(this);
            return;
        }
        INSTANCE = this;
    }

    private void Start()
    {
        if (IsInitialized)
            return;

        _lastAdTime = Time.unscaledTime;
        _adService = Core.GetService<IAdvertisementsService>();

        if (_adService == null)
        {
            Debug.LogWarning("[Boombit_AdsManager] IAdvertisementsService is null. Ads disabled.");
            IsInitialized = true;
            return;
        }

        _interstitialController = _adService.GetInterstitialController();
        _rewardedController = _adService.GetRewardedController();
        _bannerController = _adService.GetBannerController();

        if (_rewardedController != null)
            _rewardedController.Finished += HandleRewardedResult;

        if (_interstitialController != null)
            _interstitialController.Finished += HandleInterResult;

        IsInitialized = true;
    }

    private void OnDestroy()
    {
        if (_rewardedController != null)
            _rewardedController.Finished -= HandleRewardedResult;

        if (_interstitialController != null)
            _interstitialController.Finished -= HandleInterResult;

        if (INSTANCE == this)
            INSTANCE = null;
    }

    static bool IsNoAds => PlayerPrefs.GetInt(NO_ADS_KEY, 0) == 1;

    void ResetAdTimer() => _lastAdTime = Time.unscaledTime;

    #region Ads
    private void HandleRewardedResult(IRewardedAdResult result)
    {
        // Take ownership of the callback first so it can never fire twice / go stale.
        Action callback = _rvCallback;
        string placement = _placementRV;
        _rvCallback = null;
        _placementRV = null;

        if (result == null || !result.WasSuccess)
            return; // ad failed to show: no reward, don't punish the interstitial timer

        if (!result.WasRewarded)
            return;

        AdWatched();
        Event_AdWatched("rv_watched");
        if (!string.IsNullOrEmpty(placement))
            LogEvent("rv_" + placement);

        callback?.Invoke();
    }

    private void HandleInterResult(IInterstitialAdResult result)
    {
        Action callback = _isCallback;
        string placement = _placementInter;
        _isCallback = null;
        _placementInter = null;

        ResetAdTimer();
        AdWatched();
        Event_AdWatched("is_watched");
        if (!string.IsNullOrEmpty(placement))
            LogEvent("interstitial_" + placement);

        callback?.Invoke();
    }

    private void ShowRewardAds(string placement, Action onComplete)
    {
        if (!isRewardedVideoAvailable())
        {
            Debug.Log("[HCSDKManager] Rewarded video not available.");
            return;
        }

        _placementRV = placement;
        _rvCallback = onComplete;
        _rewardedController.Show(placement);
    }

    public bool IsInterstitialReady()
    {
        return _interstitialController != null && _interstitialController.IsReady;
    }

    public bool isRewardedVideoAvailable()
    {
        return _rewardedController != null && _rewardedController.IsReady;
    }

    bool IsInterstitalAdTimeMatch() => TimeSeconds > AD_WAIT_TIME;

    /// <summary>
    /// Shows an interstitial if allowed. onComplete is ALWAYS invoked exactly once:
    /// after the ad closes, or immediately if the ad is skipped for any reason.
    /// </summary>
    public void ShowInterstitialAd(string placement, Action onComplete) // == levelComplete
    {
        if (IsNoAds)
        {
            onComplete?.Invoke();
            return;
        }

#if UNITY_IOS
        if (ios_in_reviewValue)
        {
            onComplete?.Invoke();
            return;
        }
#endif

        if (!IsInterstitalAdTimeMatch())
        {
            Debug.Log("Ad Wait time not completed : " + TimeSeconds);
            onComplete?.Invoke();
            return;
        }

        if (!IsInterstitialReady())
        {
            Debug.Log("Interstitial not ready !!!");
            onComplete?.Invoke();
            return;
        }

        _placementInter = placement;
        _isCallback = onComplete;
        _interstitialController.Show(placement);
    }

    public void ShowBanner(bool showBanner = true)
    {
        if (_bannerController == null)
            return;

        // Hiding is always allowed (e.g. right after a No Ads purchase).
        if (!showBanner)
        {
            _bannerController.Hide();
            return;
        }

        if (IsNoAds)
        {
            Debug.Log("Remove Ads Activated => Banner");
            _bannerController.Hide();
            return;
        }

#if UNITY_IOS
        if (ios_in_reviewValue)
            return;
#endif
        _bannerController.Show();
    }

    public void DisplayRV(string placement, Action onComplete)
    {
        ShowRewardAds(placement, onComplete);
    }
    #endregion

    #region Remote Config Setup
    private bool isFirebaseInitialized = false;

    const string Is_Is_Rv_is_delay_secsKey = "is_is_rv_is_delay_secs";
    const int Is_Is_Rv_is_delay_secs_Default = 60;
    public int is_is_rv_is_delay_secsValue;

    const string local_push_notification_activeKey = "local_push_notification_active";
    const bool local_push_notification_active_Default = true;
    public bool local_push_notification_activeValue;

    const string ios_in_review_Key = "ios_in_review";
    const bool ios_in_review_Default = false;
    public bool ios_in_reviewValue;

    const string interstitialAd_timer_key = "interstitial_frequency";
    const int interstitialAd_timer_Default = 600;
    public int interstitialAd_timer;

    const string multiple_Interstitial_frequency_key = "multiple_Interstitial_frequency";
    public List<int> multiple_Interstitial_frequency_Default;
    public List<int> multiple_Interstitial_frequency;

    const string LocalNotificationIntervalTimeHoursKey = "local_notification_interval_time_hours";
    const int LocalNotificationIntervalTimeHoursValue_Default = 16;
    public int localNotificationIntervalTimeHoursValue;
    public IReadOnlyDictionary<string, string> configDictionary;

    // NOTE: these must be hooked up to your remote-config fetch callbacks.
    public void OnFetchFail()
    {
        Debug.Log("RC Fetch Failed...!!!");
        SetRemoteOrLocalValues(true);
    }

    public void OnFetchSuccess(IReadOnlyDictionary<string, string> config)
    {
        configDictionary = config;
        Debug.Log("RC Fetch Sucessfully");
        isFirebaseInitialized = config != null;
        SetRemoteOrLocalValues(!isFirebaseInitialized);
    }

    void SetRemoteOrLocalValues(bool isLocal)
    {
        Debug.Log("RC Fetch isLocal: " + isLocal);
        GetUniversalData();
    }

    void GetUniversalData()
    {
        is_is_rv_is_delay_secsValue = GetIntData(Is_Is_Rv_is_delay_secsKey, Is_Is_Rv_is_delay_secs_Default);
        AD_WAIT_TIME = is_is_rv_is_delay_secsValue;
        ios_in_reviewValue = GetBoolData(ios_in_review_Key, ios_in_review_Default);
        interstitialAd_timer = GetIntData(interstitialAd_timer_key, interstitialAd_timer_Default);
        local_push_notification_activeValue = GetBoolData(local_push_notification_activeKey, local_push_notification_active_Default);
        localNotificationIntervalTimeHoursValue = GetIntData(LocalNotificationIntervalTimeHoursKey, LocalNotificationIntervalTimeHoursValue_Default);

        string freq = GetValueForKey(multiple_Interstitial_frequency_key);
        multiple_Interstitial_frequency = string.IsNullOrEmpty(freq)
            ? new List<int>(multiple_Interstitial_frequency_Default ?? new List<int>())
            : GetUtilitiesValues(freq);
    }

    int GetIntData(string key, int defaultValue)
    {
        string raw = GetValueForKey(key);
        if (string.IsNullOrEmpty(raw))
            return defaultValue;

        if (int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            return value;

        Debug.LogWarning($"[RC] Failed to parse int for '{key}': {raw}");
        return defaultValue;
    }

    bool GetBoolData(string key, bool defaultValue)
    {
        string raw = GetValueForKey(key);
        if (string.IsNullOrEmpty(raw))
            return defaultValue;

        raw = raw.Trim();
        if (bool.TryParse(raw, out bool value))
            return value;
        if (raw == "1") return true;
        if (raw == "0") return false;

        Debug.LogWarning($"[RC] Failed to parse bool for '{key}': {raw}");
        return defaultValue;
    }

    string GetStringData(string key, string defaultValue)
    {
        string raw = GetValueForKey(key);
        return string.IsNullOrEmpty(raw) ? defaultValue : raw;
    }

    string GetValueForKey(string key)
    {
        if (!isFirebaseInitialized || configDictionary == null)
            return null;

        return configDictionary.TryGetValue(key, out var value) ? value : null;
    }

    List<int> GetUtilitiesValues(string rcValue)
    {
        var result = new List<int>();
        if (string.IsNullOrEmpty(rcValue))
            return result;

        foreach (var part in rcValue.Split(','))
        {
            if (int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                result.Add(v);
        }
        return result;
    }

    #endregion

    #region Events
    IFirebaseService FirebaseSvc => _firebase ?? (_firebase = Core.GetService<IFirebaseService>());
    IAttributionService AttributionSvc => _attribution ?? (_attribution = Core.GetService<IAttributionService>());

    public void LogEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        FirebaseSvc?.LogEvent(eventName);
        GameAnalytics.NewDesignEvent(eventName);
    }

    public void LogEvent_Custom(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        FirebaseSvc?.LogEvent(eventName);
        AttributionSvc?.SendEvent(eventName);
    }

    public void Event_AdWatched(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        Debug.Log("Ad Watched event: " + eventName);
        LogEvent_Custom(eventName);
    }

    int counter = 0;
    void AdWatched()
    {
        counter++;
        Debug.Log("Ad Watched Counter: " + counter);
        if (counter == 10 || counter == 20 || counter == 30 || counter == 50)
            Event_AdWatched("ads_watched_" + counter);
    }

    //Firebase and AF
    public void CustomEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;
        Debug.Log("Custom Event: " + eventName);
        LogEvent_Custom(eventName);
    }

    #endregion
}
