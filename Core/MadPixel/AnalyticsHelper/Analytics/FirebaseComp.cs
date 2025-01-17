using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class FirebaseComp : MonoBehaviour {
    private static bool bInitialized = false;



    #region Unity events
    void Start() {
#if UNITY_EDITOR
        return;
#endif
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                InnerInit();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            } else {
                Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    private void OnDestroy() {
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= LogAdPurchase;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= LogAdPurchase;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= LogAdPurchase;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= LogAdPurchase;
    }
    #endregion

    private void InnerInit() {
        bInitialized = true;

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += LogAdPurchase;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += LogAdPurchase;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += LogAdPurchase;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += LogAdPurchase;
    }

    public void LogAdPurchase(string AdUnitID, MaxSdkBase.AdInfo adInfo) {
        double revenue = adInfo.Revenue;
        if (revenue > 0 && bInitialized) {
            string countryCode = MaxSdk.GetSdkConfiguration()
                        .CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
            string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
            string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
            string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
            string networkPlacement = adInfo.NetworkPlacement; // The placement ID from the network that showed the ad

            var impressionParameters = new[] {
                new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);

            //Debug.Log($"[MadPixel] Revenue logged {adInfo}");
        }
    }
}