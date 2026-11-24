using UnityEngine;
using System;

#if MES_HCSDK
using BoomBit.HyperCasual;
#else
using Coredian.Advertisements;
#endif

namespace MES.Crypto.Ads
{
    public class CryptoAdsListener
    {
        public event Action VideoDisplayEvent;
        public event Action VideoSuccessEvent;
        public event Action InterstitialDisplayEvent;
        public event Action InterstitialCloseEvent;
        public event Action BannerDisplayEvent;

        private bool _initialized;
        private bool _disposed;

#if !MES_HCSDK
        private IInterstitialController _interstitial;
        private IRewardedController _rewarded;
#endif

        public CryptoAdsListener()
        {
            if (_initialized)
                return;

#if MES_HCSDK
            HCSDK.VideoDisplayEvent += OnVideoDisplay;
            HCSDK.InterstitialDisplayEvent += OnInterstitialDisplay;
            HCSDK.VideoSuccessEvent += OnVideoSuccess;
            HCSDK.InterstitialCloseEvent += OnInterstitialClose;
            _initialized = true;
            return;
#else
            var adService = Core.GetService<IAdvertisementsService>();
            if (adService == null)
            {
                Debug.LogWarning("[CryptoAdsListener] IAdvertisementsService is null. Listener inactive.");
                return;
            }

            _interstitial = adService.GetInterstitialController();
            _rewarded = adService.GetRewardedController();

            if (_interstitial != null)
            {
                _interstitial.Showing += OnInterstitialDisplay;
                _interstitial.Finished += OnInterstitialFinished;
            }

            if (_rewarded != null)
            {
                _rewarded.Showing += OnVideoDisplay;
                _rewarded.Finished += OnRewardedFinished;
            }

            _initialized = true;
#endif
        }

        public void OnDestroy()
        {
            if (_disposed)
                return;

            if (_initialized)
            {
#if MES_HCSDK
                HCSDK.VideoDisplayEvent -= OnVideoDisplay;
                HCSDK.InterstitialDisplayEvent -= OnInterstitialDisplay;
                HCSDK.VideoSuccessEvent -= OnVideoSuccess;
                HCSDK.InterstitialCloseEvent -= OnInterstitialClose;
#else
                if (_interstitial != null)
                {
                    _interstitial.Showing -= OnInterstitialDisplay;
                    _interstitial.Finished -= OnInterstitialFinished;
                    _interstitial = null;
                }

                if (_rewarded != null)
                {
                    _rewarded.Showing -= OnVideoDisplay;
                    _rewarded.Finished -= OnRewardedFinished;
                    _rewarded = null;
                }
#endif
            }

            VideoDisplayEvent = null;
            VideoSuccessEvent = null;
            InterstitialDisplayEvent = null;
            InterstitialCloseEvent = null;
            BannerDisplayEvent = null;

            _disposed = true;
        }

        public void HideBanner()
        {
#if MES_HCSDK
            HCSDK.HideBanner();
#endif
        }

        private void OnVideoDisplay() => SafeInvoke(VideoDisplayEvent);
        private void OnInterstitialDisplay() => SafeInvoke(InterstitialDisplayEvent);

#if MES_HCSDK
        private void OnVideoSuccess() => SafeInvoke(VideoSuccessEvent);
        private void OnInterstitialClose() => SafeInvoke(InterstitialCloseEvent);
#else
        private void OnInterstitialFinished(IInterstitialAdResult result)
        {
            SafeInvoke(InterstitialCloseEvent);
        }

        private void OnRewardedFinished(IRewardedAdResult result)
        {
            if (result != null && result.WasSuccess && result.WasRewarded)
                SafeInvoke(VideoSuccessEvent);
        }
#endif

        private void SafeInvoke(Action evt)
        {
            if (evt == null) return;
            try
            {
                evt.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}