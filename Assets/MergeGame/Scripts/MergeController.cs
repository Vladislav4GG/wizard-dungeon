using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BrickType
{
    Default,
    Random,
    Rewarded
}

public class MergeController
{
    static MergeController instance;

    readonly List<MergePreset> presets;
    MergePreset current;
    int maxOpenLevel;
    
    public static event Action<int, BrickType> Purchased = delegate { };
    public static event Action<int, BrickType> RewardUsed = delegate { };
    public static event Action<bool, int> UnlockedNewLevel = delegate {  };

    MergeController()
    {
        presets = Resources.Load<MergeCollection>("IconsCollection").objects;
        Current = presets[0];
    }
    
    public static MergeController Instance => instance ?? (instance = new MergeController());
    public MergePreset Current
    {
        get => current;
        set
        {
            Assert.IsTrue(presets.IndexOf(value) >= 0);
            current = value;
        }
    }
    public bool FreeSpace{ get; set; }
    public DateTime RewardTimer { get; set; }
    public int MaxOpenLevel
    {
        get => maxOpenLevel;
        private set => maxOpenLevel = Mathf.Clamp(value, 0, presets.Count);
    }

    public IEnumerable< MergePreset> GetPresets()
    {
        return presets;
    }

    public void CheckForRandomPreset(float randomProbability, out int level, ref BrickType type)
    {
        level = 0;
        if (!(Random.Range(0f, 1f) < randomProbability) || MaxOpenLevel - 1 == level) return;

        level = Random.Range(level + 1, MaxOpenLevel - 1);
        type = BrickType.Random;
    }

    public MergePreset GetPresset(int level)
    {
        return presets[level];
    }
    
    public void OnPurchaseCompleted(MergePreset preset)
    {
        int level = presets.IndexOf(preset);
        preset.UpdatePrice();
        Purchased.Invoke(level, BrickType.Default);

        if (level == maxOpenLevel)
            UnlockedNewLevel(false, 0);
    }

    public void OnRewardUsed()
    {
        int level = Random.Range(1, MaxOpenLevel - 1);
        RewardUsed.Invoke(level, BrickType.Rewarded);
    }

    public void UpdateState(int maxOpenLevel, long[] prices, DateTime timerTime)
    {
        RewardTimer = timerTime;
        UpdateMaxOpenLevel(maxOpenLevel);
        UpdatePresets(prices);
    }
    
    public void UpdateMaxOpenLevel(int level)
    {
        if (level <= MaxOpenLevel) return;
        MaxOpenLevel = level;
        UnlockedNewLevel(true, level);
    }

    void UpdatePresets(long[] prices)
    {
        for (int i = 0; i < prices.Length; i++)
            presets[i].Price = prices[i];
    }

    public void ResetPresets()
    {
        for (int i = 0; i < presets.Count; i++)
            presets[i].Price = 0;
    }

    public void UpdateRewardTimer(DateTime time)
    {
        RewardTimer = time;
    }
}
