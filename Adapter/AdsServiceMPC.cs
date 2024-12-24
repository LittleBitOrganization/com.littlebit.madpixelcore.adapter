using System;
using System.Collections;
using System.Collections.Generic;
using LittleBitGames.Ads;
using LittleBitGames.Ads.AdUnits;
using LittleBitGames.Ads.Configs;
using LittleBitGames.Environment.Ads;
using MAXHelper;
using UnityEngine;


public class AdsServiceMPC : MonoBehaviour, IAdsService
{
    private readonly AdsManager _adsManager;
    private readonly MAXCustomSettings _madPixelSettings;

    public IMediationNetworkInitializer Initializer { get; }
    public IReadOnlyList<IAdUnit> AdUnits { get; }


    public string GetAdUnitId(AdType adType)
    {
#if UNITY_ANDROID
        switch (adType)
        {
            case AdType.Banner: return _madPixelSettings.BannerID;
            case AdType.Inter: return _madPixelSettings.InterstitialID;
            case AdType.Rewarded: return _madPixelSettings.RewardedID;
        }

#else
        switch (adType)
        {
            case AdType.Banner: return _madPixelSettings.BannerID_IOS;
            case AdType.Inter: return _madPixelSettings.InterstitialID_IOS;
            case AdType.Rewarded: return _madPixelSettings.RewardedID_IOS;
        }
#endif
        throw new NotImplementedException();
    }


    public AdsServiceMPC(AdsManager adsManager)
    {
        _madPixelSettings = Resources.Load<MAXCustomSettings>("MAXCustomSettings");
        _adsManager = adsManager;
    }

    public bool IsAdReady(AdType type)
    {
        return AdsManager.HasLoadedAd(Convert.Ad.ToMadPixel(type));
    }

    public void Run()
    {
    }

    public void ShowAd(AdType adType, IAdUnitPlace adUnitPlace, Action<AdShowInfo> callback)
    {
        AdUnitKey adUnitKey = new AdUnitKey(GetAdUnitId(adType));

        void OnAdDismissed(bool isSuccess)
        {
            callback?.Invoke(new AdShowInfo(adUnitKey, isSuccess, adUnitPlace));
        }

        if (adType == AdType.Inter)
        {
            AdsManager.ShowInter(_adsManager.gameObject, OnAdDismissed, adUnitPlace.StringValue);
        }
        else if (adType == AdType.Rewarded)
        {
            AdsManager.ShowRewarded(_adsManager.gameObject, OnAdDismissed, adUnitPlace.StringValue);
        }
        else
            throw new NotImplementedException();
    }
}