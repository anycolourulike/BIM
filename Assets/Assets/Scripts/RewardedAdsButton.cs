using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yodo1.MAS;
using System;

public class RewardedAdBtns : MonoBehaviour
{
    public bool PrivacyDialog = false;
    private Yodo1U3dBannerAdView bannerAdView;
    private Yodo1U3dNativeAdView nativeAdView;

    public static RewardedAdBtns Instance { get; private set; }

    private void Awake()
    {
        // Check if the instance already exists
        if (Instance != null && Instance != this)
        {
            // If another instance exists, destroy the current GameObject and return
            Destroy(gameObject);
            return;
        }

        // If no instance exists, set the current instance and keep it alive throughout the game
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        //if (PrivacyDialog) // We need a neutral age screen if app rating includes ages 12 or below.
        //{
        //    Yodo1AdBuildConfig config = new Yodo1AdBuildConfig().enableUserPrivacyDialog(true);
        //    Yodo1U3dMas.SetAdBuildConfig(config);
        //}
        //else // If app rating is teens or above 13, we directly use this. 
        //{
        //    Yodo1U3dMas.SetCOPPA(false);
        //    Yodo1U3dMas.SetGDPR(true);
        //    Yodo1U3dMas.SetCCPA(false);
        //}


        Yodo1U3dInterstitialAd.GetInstance().autoDelayIfLoadFail = true;
        Yodo1U3dRewardAd.GetInstance().autoDelayIfLoadFail = true;


        // Initialize callbacks.
        Yodo1U3dMasCallback.OnSdkInitializedEvent += (success, error) =>
        {
            Debug.Log("[Yodo1 Mas] OnSdkInitializedEvent, success:" + success + ", error: " + error.ToString());
            if (success)
            {
                Debug.Log("[Yodo1 Mas] In if of init callback");
                RequestAppOpen();
                InitializeInterstitial();
                InitializeRewardedAds();
                RequestRewardedInterstitial();

            }
            else
            {
                Debug.Log("[Yodo1 Mas] The initialization has failed");
            }
        };

        //Initializing the SDK
        Yodo1U3dMas.InitializeMasSdk();
    }

    //private void RequestBanner()
    //{
    //    // Clean up banner before reusing
    //    if (bannerAdView != null)
    //    {
    //        bannerAdView.Destroy();
    //    }

    //    // Create a 320x50 banner at top of the screen
    //    bannerAdView = new Yodo1U3dBannerAdView(Yodo1U3dBannerAdSize.Banner, Yodo1U3dBannerAdPosition.BannerTop | Yodo1U3dBannerAdPosition.BannerHorizontalCenter);

    //    // Load banner ads, the banner ad will be displayed automatically after loaded
    //    bannerAdView.LoadAd();
    //}


    // this method is used to show banner again after it is hidden.
    public void showBanner()
    {
        if (bannerAdView != null)
        {
            bannerAdView.Show();
        }
    }

    // this method is used to hide the banner once it is loaded
    //public void hideBanner()
    //{
    //    if (bannerAdView != null)
    //    {
    //        bannerAdView.Hide();
    //    }
    //}

    //private void RequestNative()
    //{
    //    // Clean up native before reusing
    //    if (nativeAdView != null)
    //    {
    //        nativeAdView.Destroy();
    //    }

    //    // Create a 375x200 native at top of the screen
    //    nativeAdView = new Yodo1U3dNativeAdView(0, 0, 375, 200);

    //    // Ad Events
    //    nativeAdView.OnAdLoadedEvent += OnNativeAdLoadedEvent;
    //    nativeAdView.OnAdFailedToLoadEvent += OnNativeAdFailedToLoadEvent;

    //    // Load native ads, the native ad will be displayed automatically after loaded
    //    nativeAdView.LoadAd();
    //}

    //private void OnNativeAdLoadedEvent(Yodo1U3dNativeAdView adView)
    //{
    //    // Native ad is ready to be shown.
    //    Debug.Log("[Yodo1 Mas] OnNativeAdLoadedEvent event received");
    //}

    //private void OnNativeAdFailedToLoadEvent(Yodo1U3dNativeAdView adView, Yodo1U3dAdError adError)
    //{
    //    Debug.Log("[Yodo1 Mas] OnNativeAdFailedToLoadEvent event received with error: " + adError.ToString());
    //}


    private void InitializeInterstitial()
    {

        Yodo1U3dInterstitialAd.GetInstance().LoadAd();

        // Ad Events
        Yodo1U3dInterstitialAd.GetInstance().OnAdLoadedEvent += OnInterstitialAdLoadedEvent;
        Yodo1U3dInterstitialAd.GetInstance().OnAdLoadFailedEvent += OnInterstitialAdLoadFailedEvent;
        Yodo1U3dInterstitialAd.GetInstance().OnAdOpenedEvent += OnInterstitialAdOpenedEvent;
        Yodo1U3dInterstitialAd.GetInstance().OnAdOpenFailedEvent += OnInterstitialAdOpenFailedEvent;
        Yodo1U3dInterstitialAd.GetInstance().OnAdClosedEvent += OnInterstitialAdClosedEvent;


    }

    private void OnInterstitialAdLoadedEvent(Yodo1U3dInterstitialAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnInterstitialAdLoadedEvent event received");
    }

    private void OnInterstitialAdLoadFailedEvent(Yodo1U3dInterstitialAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnInterstitialAdLoadFailedEvent event received with error: " + adError.ToString());
    }

    private void OnInterstitialAdOpenedEvent(Yodo1U3dInterstitialAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnInterstitialAdOpenedEvent event received");
    }

    private void OnInterstitialAdOpenFailedEvent(Yodo1U3dInterstitialAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnInterstitialAdOpenFailedEvent event received with error: " + adError.ToString());
        // Load the next ad
        Yodo1U3dInterstitialAd.GetInstance().LoadAd();
    }

    private void OnInterstitialAdClosedEvent(Yodo1U3dInterstitialAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnInterstitialAdClosedEvent event received");
        // Load the next ad
        Yodo1U3dInterstitialAd.GetInstance().LoadAd();
    }

    public void showInterstitial()
    {
        bool isLoaded = Yodo1U3dInterstitialAd.GetInstance().IsLoaded();

        if (isLoaded) Yodo1U3dInterstitialAd.GetInstance().ShowAd();
    }

    private void InitializeRewardedAds()
    {

        Yodo1U3dRewardAd.GetInstance().LoadAd();

        // Ad Events
        Yodo1U3dRewardAd.GetInstance().OnAdLoadedEvent += OnRewardAdLoadedEvent;
        Yodo1U3dRewardAd.GetInstance().OnAdLoadFailedEvent += OnRewardAdLoadFailedEvent;
        Yodo1U3dRewardAd.GetInstance().OnAdOpenedEvent += OnRewardAdOpenedEvent;
        Yodo1U3dRewardAd.GetInstance().OnAdOpenFailedEvent += OnRewardAdOpenFailedEvent;
        Yodo1U3dRewardAd.GetInstance().OnAdClosedEvent += OnRewardAdClosedEvent;
        Yodo1U3dRewardAd.GetInstance().OnAdEarnedEvent += OnRewardAdEarnedEvent;
    }

    private void OnRewardAdLoadedEvent(Yodo1U3dRewardAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardAdLoadedEvent event received");
    }

    private void OnRewardAdLoadFailedEvent(Yodo1U3dRewardAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnRewardAdLoadFailedEvent event received with error: " + adError.ToString());
    }

    private void OnRewardAdOpenedEvent(Yodo1U3dRewardAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardAdOpenedEvent event received");
    }

    private void OnRewardAdOpenFailedEvent(Yodo1U3dRewardAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnRewardAdOpenFailedEvent event received with error: " + adError.ToString());
        // Load the next ad
        Yodo1U3dRewardAd.GetInstance().LoadAd();
    }

    private void OnRewardAdClosedEvent(Yodo1U3dRewardAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardAdClosedEvent event received");
        // Load the next ad
        Yodo1U3dRewardAd.GetInstance().LoadAd();
    }

    private void OnRewardAdEarnedEvent(Yodo1U3dRewardAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardAdEarnedEvent event received");
        Debug.Log("Three Extra Lives");
        GameManager.Instance.playerLives += 3;
        // Add your reward code here
    }

    public void showRewarded()
    {
        bool isLoaded = Yodo1U3dRewardAd.GetInstance().IsLoaded();

        if (isLoaded) Yodo1U3dRewardAd.GetInstance().ShowAd();
    }

    private void RequestAppOpen()
    {
        Yodo1U3dAppOpenAd.GetInstance().LoadAd();

        Yodo1U3dAppOpenAd.GetInstance().OnAdLoadedEvent += OnAppOpenAdLoadedEvent;
        Yodo1U3dAppOpenAd.GetInstance().OnAdLoadFailedEvent += OnAppOpenAdLoadFailedEvent;
        Yodo1U3dAppOpenAd.GetInstance().OnAdOpenedEvent += OnAppOpenAdOpenedEvent;
        Yodo1U3dAppOpenAd.GetInstance().OnAdOpenFailedEvent += OnAppOpenAdOpenFailedEvent;
        Yodo1U3dAppOpenAd.GetInstance().OnAdClosedEvent += OnAppOpenAdClosedEvent;

        // App open ad soft close placement
        // Yodo1U3dMasCallback.OnAppEnterForegroundEvent += () => {
        //Yodo1U3dAppOpenAd.GetInstance().LoadAd();
        //showAppOpen(); };

    }

    private void OnAppOpenAdLoadedEvent(Yodo1U3dAppOpenAd ad)
    {
        //showing app open ad on startup as soon as it is loade.
        showAppOpen();
        Debug.Log("[Yodo1 Mas] OnAppOpenAdLoadedEvent event received");
    }

    private void OnAppOpenAdLoadFailedEvent(Yodo1U3dAppOpenAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnAppOpenAdLoadFailedEvent event received with error: " + adError.ToString());
    }

    private void OnAppOpenAdOpenedEvent(Yodo1U3dAppOpenAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnAppOpenAdOpenedEvent event received");
    }

    private void OnAppOpenAdOpenFailedEvent(Yodo1U3dAppOpenAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnAppOpenAdOpenFailedEvent event received with error: " + adError.ToString());

    }

    private void OnAppOpenAdClosedEvent(Yodo1U3dAppOpenAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnAppOpenAdClosedEvent event received");
    }

    public void showAppOpen()
    {
        Yodo1U3dAppOpenAd.GetInstance().ShowAd();

    }

    private void RequestRewardedInterstitial()
    {
        Yodo1U3dRewardedInterstitialAd.GetInstance().LoadAd();

        Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdLoadedEvent += OnRewardedInterstitialAdLoadedEvent;
        Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdLoadFailedEvent += OnRewardedInterstitialAdLoadFailedEvent;
        Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdOpenedEvent += OnRewardedInterstitialAdOpenedEvent;
        //Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdOpeningEvent += OnRewardedInterstitialAdOpenedEvent;
        Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdOpenFailedEvent += OnRewardedInterstitialAdOpenFailedEvent;
        Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdClosedEvent += OnRewardedInterstitialAdClosedEvent;
        Yodo1U3dRewardedInterstitialAd.GetInstance().OnAdEarnedEvent += OnRewardedInterstitialAdEarnedEvent;
    }

    private void OnRewardedInterstitialAdLoadedEvent(Yodo1U3dRewardedInterstitialAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardedInterstitialAdLoadedEvent event received");
    }

    private void OnRewardedInterstitialAdLoadFailedEvent(Yodo1U3dRewardedInterstitialAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnRewardedInterstitialAdLoadFailedEvent event received with error: " + adError.ToString());
        ad.LoadAd();
    }

    private void OnRewardedInterstitialAdOpenedEvent(Yodo1U3dRewardedInterstitialAd ad)
    {
       
        Debug.Log("[Yodo1 Mas] OnRewardedInterstitialAdOpenedEvent event received");
    }

    private void OnRewardedInterstitialAdOpenFailedEvent(Yodo1U3dRewardedInterstitialAd ad, Yodo1U3dAdError adError)
    {
        Debug.Log("[Yodo1 Mas] OnRewardedInterstitialAdOpenFailedEvent event received with error: " + adError.ToString());
    }

    private void OnRewardedInterstitialAdClosedEvent(Yodo1U3dRewardedInterstitialAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardedInterstitialAdClosedEvent event received");
        ad.LoadAd();
    }

    private void OnRewardedInterstitialAdEarnedEvent(Yodo1U3dRewardedInterstitialAd ad)
    {
        Debug.Log("[Yodo1 Mas] OnRewardedInterstitialAdEarnedEvent event received");
    }

    public void showRIV()
    {
        if (Yodo1U3dRewardedInterstitialAd.GetInstance().IsLoaded())
            Yodo1U3dRewardedInterstitialAd.GetInstance().ShowAd();

    }
}