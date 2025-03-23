using System;
using UnityEngine;

[Serializable]
public class GameState
{
    public event Action StateUpdate;

    [SerializeField]
    int score;
    [SerializeField]
    int topScore;
    
    [SerializeField]
    int bricksCount;
    [SerializeField]
    BrickState [] bricks = new BrickState[0];
    [SerializeField]
    long[] presetPrices = new long[0];
    
    [SerializeField]
    int maxOpenLevel;
    [SerializeField]
    int experience;
    [SerializeField]
    int experienceLevel;
    [SerializeField]
    int levelMaxExperience;
    [SerializeField]
    int prevLevelStats;
    [SerializeField]
    string rewardDateTime;

    public Int32 Score
    {
        get => score;
        set
        {
            score = value;

            if (score > topScore)
                topScore = score;

            StateUpdate?.Invoke();
        }
    }

    public int TopScore => topScore;

    public int BricksCount
    {
        get => bricksCount;
        set => bricksCount = value;
    }

    public int MaxOpenLevel
    {
        get => maxOpenLevel;
        set => maxOpenLevel = value;
    }

    public int Experience
    {
        get => experience;
        set => experience = value;
    }
    
    public int ExperienceLevel
    {
        get => experienceLevel;
        set => experienceLevel = value;
    }

    public int LevelMaxExperience
    {
        get => levelMaxExperience;
        set => levelMaxExperience = value;
    }

    public int PreviousLevelStats
    {
        get => prevLevelStats;
        set => prevLevelStats = value;
    }

    public void SetField(BrickState[] value)
    {
        bricks = (BrickState[]) value.Clone();
    }
    
    public BrickState[] GetField()
    {
        return (BrickState[]) bricks.Clone();
    }

    public void SetPresetsPrices(long[] value)
    {
        presetPrices = (long[])value.Clone();
    }

    public long[] GetPresetsPrices()
    {
        return presetPrices.Clone() as long[];
    }

    public void SetRewardTimer(DateTime time)
    {
        rewardDateTime = time.ToString (System.Globalization.CultureInfo.InvariantCulture);
    }

    public DateTime GetRewardTime()
    {
        DateTime dateTime;

        bool didParse = DateTime.TryParse (rewardDateTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime);
        return didParse ? dateTime : DateTime.UtcNow;
    }

    public void ResetState()
    {
        score = 0;
        bricks = new BrickState[0];
        bricksCount = 6;
        maxOpenLevel = 0;
        experience = 0;
        experienceLevel = 0;
        prevLevelStats = 0;
        for (int i = 0; i < presetPrices.Length; i++)
            presetPrices[i] = 0;
        MergeController.Instance.ResetPresets();
    }
}

[Serializable]
public class BrickState
{
    [SerializeField]
    public int level;
    [SerializeField]
    public int open;
    [SerializeField]
    public int type;

    public BrickState(int value)
    {
        level = value;
        open = value;
        type = value;
    }
    
    public BrickState(int level, bool open, int type)
    {
        this.level = level;
        this.open = open ? 0 : 1;
        this.type = type;
    }
}
