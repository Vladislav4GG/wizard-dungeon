using UnityEngine;
using UnityEngine.Advertisements;

public static class PlacementId
{
    public const string Video = "video";
    public const string RewardedVideo = "rewardedVideo";
    public const string Banner = "banner";
}

public class UnityAdsController : 
#if UNITY_ADS
    MonoBehaviour, IUnityAdsListener
#else
    MonoBehaviour
#endif
{
    public string iOSGameId;
    public string androidGameId;
    public string otherGameId;

    string GameId => 
#if UNITY_IOS
    iOSGameId;
#elif UNITY_ANDROID
    androidGameId;
#else
    otherGameId;
#endif

#if UNITY_ADS
    void Awake()
    {
        if (!Advertisement.isSupported)
            return;

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.AddListener(this);
        Advertisement.Initialize(GameId);
    }

    public void OnUnityAdsReady(string placementId)
    {
        if (!UserProgress.Current.IsItemPurchased("no_ads") && placementId == PlacementId.Banner)
            Advertisement.Banner.Show(placementId);
    }

    public void OnUnityAdsDidError(string message)
    {
    }

    public void OnUnityAdsDidStart(string placementId)
    {
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
    }
#endif
}
