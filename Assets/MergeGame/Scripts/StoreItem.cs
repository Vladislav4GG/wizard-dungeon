using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class StoreItem :
#if UNITY_ADS
    MonoBehaviour, IUnityAdsListener
#else
    MonoBehaviour
#endif
{      
    [SerializeField]
    Image icon;
    [SerializeField]
    Text name;
    [SerializeField]
    Text price;
    [SerializeField]
    Button adsButton;
    [SerializeField]
    Button buyButton;
    [SerializeField]
    GameObject cannotBuy;
    [SerializeField]
    GameObject lockBg;
    [SerializeField]
    GameObject alert;

    [SerializeField]
    Sprite unKnownIcon;

    MergePreset preset;
    Int64 Price { get; set; }
    const string defaultName = "????";

    bool isWatchAdsClicked;
    bool isGetCoinsClicked;

    public event Action<MergePreset> OnPurchase = delegate { };

    public MergePreset Preset
    {
        get => preset;
        set
        {
            preset = value;
            UpdatePreview();
        }
    }

    void Start()
    {
        buyButton.onClick.AddListener(OnBuyClick);
        adsButton.onClick.AddListener(OnWatchAdsClick);
        MergeController.Purchased += OnPreviewUpdate;
    }

    void OnBuyClick()
    {
        alert.SetActive(false);
        UserProgress.Current.Coins -= Price;
        OnPurchase.Invoke(Preset);
    }

    void OnWatchAdsClick()
    {
        isWatchAdsClicked = true;
#if UNITY_ADS
        Advertisement.Show(PlacementId.RewardedVideo);
#endif
    }

    void OnPreviewUpdate(int level, BrickType type)
    {
        UpdatePreview();
    }

    void UpdatePreview()
    {
        if (preset == null)
        {
            icon.sprite = unKnownIcon;
            name.text = defaultName;
            SetItemUnlock(false);
            return;
        }

        icon.sprite = preset.Icon;
        name.text = preset.Title;
        price.text = CounterText.UpdateText(preset.Price);
        Price = preset.Price;

        SetItemUnlock(true);

        buyButton.interactable = MergeController.Instance.FreeSpace && Price < UserProgress.Current.Coins;
#if UNITY_ADS
        adsButton.interactable = MergeController.Instance.FreeSpace;
#else
        adsButton.interactable = false;
#endif
    }

    void SetItemUnlock(bool unlock)
    {
        cannotBuy.SetActive(!unlock);
        lockBg.SetActive(!unlock);
        buyButton.gameObject.SetActive(unlock);
    }

    public void HighlightItem()
    {
        alert.SetActive(true);
    }
#if UNITY_ADS
    public void OnUnityAdsReady(string placementId)
    { }

    public void OnUnityAdsDidError(string message)
    {
        Debug.LogError(message);
    }

    public void OnUnityAdsDidStart(string placementId)
    { }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (placementId != PlacementId.RewardedVideo || showResult != ShowResult.Finished)
            return;

        alert.SetActive(false);
        OnPurchase.Invoke(Preset);
    }
#endif
}