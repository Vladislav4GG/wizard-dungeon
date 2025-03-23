using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	class Brick
	{
		public readonly int index;
		public bool exist;
		public readonly GameObject emptyBrick;
		public BrickController brick;

		public Brick(int index, GameObject emptyBrick)
		{
			this.index = index;
			this.emptyBrick = emptyBrick;
		}
	}
	
	event Action OnLevelUp = delegate { };

	[Range(0f, 1f)]
	public float randomIconProbability;
	[Range(0f, 2f)]
	public float timerSpeed = 1f;
	public GameObject emptyBrickPrefab;
	public BrickController brickPrefab;
	public Animator fieldAnimator;

	[Header("UI")]
	public RectTransform fieldTransform;
	public Text levelText;
	public Text experienceText;
	public Text maxExperienceText;
	public Text boxTimer;
	public Image boxImage;
	public GameObject fullText;
	public Button boxButton;

	[Header("SFX")]
	public PlaySfx clickSfx;
	public PlaySfx landingSfx;
	public PlaySfx mergingSfx;

	[Header("VFX")]
	public ParticleSystem spawnEffect;
	public ParticleSystem openEffect;
	public ParticleSystem mergeEffect;

	/// General //
	int totalBricksCount = 6;
	Vector2Int minCoords;
	Vector2Int maxCoords;
	Vector2Int bricksCount = new Vector2Int(3,2);
	
	Brick[,] field;
	readonly List<Vector2Int> freeCoords = new List<Vector2Int>();
	
	/// Level Stats ///
	[SerializeField]
	readonly int[,] fieldIndexes =
	{
		{39,37,36,38,40},
		{29,27,26,28,30},
		{24,1,2,3,25},
		{20,4,5,6,17},
		{19,8,7,9,16},
		{21,11,10,12,18},
		{22,14,13,15,23},
		{34,32,31,33,35}
	};
	const int maxBricksCount = 40;
	readonly int[] startingLevelsStats = {40, 46, 74, 110};
	int currentExperienceLevel;
	int currentExperience;
	int prevLevelMaxExperience;
	int levelMaxExperience = 40;

	const float spawnVerticalOffset = 500f;
	const float spawnTime = 10f;
	float timer;
	float imageDelta = 0.1f;
	
	static readonly int BigField = Animator.StringToHash("Big");
	static readonly int SmallField = Animator.StringToHash("Small");

	Vector2Int BricksCount
	{
		get
		{
			bricksCount.x = maxCoords.y - minCoords.y + 1;
			bricksCount.y = maxCoords.x - minCoords.x + 1;
			return bricksCount;
		}
	}

	int CurrentExperience
	{
		get => currentExperience;
		set
		{
			currentExperience = value; 
			experienceText.text = currentExperience.ToString();
		}
	}

	int CurrentExperienceLevel
	{
		get => currentExperienceLevel;
		set
		{
			currentExperienceLevel = value;
			levelText.text = (currentExperienceLevel + 1).ToString();
		}
	}

	int LevelMaxExperience
	{
		get => levelMaxExperience;
		set
		{
			levelMaxExperience = value;
			maxExperienceText.text = levelMaxExperience.ToString();
		}
	}
	
	GameState gameState;

	void Awake()
	{
		timer = spawnTime;
		boxImage.fillAmount = 0;
		boxButton.onClick.AddListener(OnBoxClick);
		
		gameState = UserProgress.Current.GetGameState<GameState>(name);
		if (gameState == null)
		{
			gameState = new GameState();
			UserProgress.Current.SetGameState(name, gameState);
		}
		UserProgress.Current.CurrentGameId = name;

		InitField();

		if (!LoadGame())
		{
			CurrentExperience = 0;
			CurrentExperienceLevel = 0;
			LevelMaxExperience = startingLevelsStats[0];
			gameState.Score = 0;
			UpdateLevelExperience();
		}
		
		UpdateCoords();
		OnLevelUp += UpdateFieldSize;
		MergeController.Purchased += SpawnBrick;
		MergeController.RewardUsed += SpawnBrick;
		MergeController.Purchased += UpdateLevelExperience;
		MergeController.RewardUsed += UpdateLevelExperience;
	}
	
	void Update()
	{
		UpdateSpawnTimer(true);
	}

	bool LoadGame()
	{
		BrickState[] bricks = gameState.GetField();
		
		if (bricks == null || bricks.Length != field.GetLength(0) * field.GetLength(1))
			return false;
		
		totalBricksCount = gameState.BricksCount;
		CurrentExperience = gameState.Experience;
		CurrentExperienceLevel = gameState.ExperienceLevel;
		LevelMaxExperience = gameState.LevelMaxExperience;
		prevLevelMaxExperience = gameState.PreviousLevelStats;

		MergeController.Instance.UpdateState(gameState.MaxOpenLevel, gameState.GetPresetsPrices(), gameState.GetRewardTime());
		UpdateCoords();

		for (int i = 0; i < field.GetLength(0); i++)
		{
			for (int j = 0; j < field.GetLength(1); j++)
			{
				if (bricks[i * field.GetLength(1) + j].level < 0) continue;
				
				SpawnBrick(new Vector2Int(i, j), bricks[i * field.GetLength(1) + j].level, 
					(BrickType)bricks[i * field.GetLength(1) + j].type, bricks[i * field.GetLength(1) + j].open == 0);
			}
		}
		MergeController.Instance.FreeSpace = freeCoords.Count > 0;
		return true;
	}

	void SaveGame()
	{
		BrickState[] bricks = new BrickState[field.GetLength(0) * field.GetLength(1)];
		for (int i = 0; i < field.GetLength(0); i++)
		{
			for (int j = 0; j < field.GetLength(1); j++)
			{
				bricks[i * field.GetLength(1) + j] = field[i, j].brick != null ? 
					new BrickState(field[i, j].brick.Level, field[i, j].brick.Open, (int)field[i, j].brick.Type) : new BrickState(-1);
			}
		}

		IEnumerable<MergePreset> presets = MergeController.Instance.GetPresets();
		long[] presetPrices = new long[presets.Count()];
		for (int j = 0; j < presetPrices.Length; j++)
			presetPrices[j] = presets.ElementAt(j).Price;

		gameState.BricksCount = totalBricksCount;
		gameState.MaxOpenLevel = MergeController.Instance.MaxOpenLevel;
		gameState.ExperienceLevel = CurrentExperienceLevel;
		gameState.Experience = CurrentExperience;
		gameState.LevelMaxExperience = levelMaxExperience;
		gameState.PreviousLevelStats = prevLevelMaxExperience;
		gameState.SetField(bricks);
		gameState.SetPresetsPrices(presetPrices);
		gameState.SetRewardTimer(MergeController.Instance.RewardTimer);
		UserProgress.Current.SaveGameState(name);
	}
	
	void InitField()
	{
		field = new Brick[fieldIndexes.GetLength(0), fieldIndexes.GetLength(1)];

		for (int i = 0; i < fieldIndexes.GetLength(0); i++)
		{
			for (int j = 0; j < fieldIndexes.GetLength(1); j++)
				field[i,j] = new Brick(fieldIndexes[i,j], BrickObject(emptyBrickPrefab, false));
		}
		
		minCoords = new Vector2Int(field.GetLength(0), field.GetLength(1));
		maxCoords = new Vector2Int(0, 0);
	}

	void UpdateCoords()
	{
		freeCoords.Clear();
		
		for (int i = 0; i < field.GetLength(0); i++)
		{
			for (int j = 0; j < field.GetLength(1); j++)
			{
				if (field[i, j].index <= totalBricksCount)
				{
					field[i, j].exist = true;
					minCoords.x = Mathf.Min(i, minCoords.x);
					minCoords.y = Mathf.Min(j, minCoords.y);

					maxCoords.x = Mathf.Max(i, maxCoords.x);
					maxCoords.y = Mathf.Max(j, maxCoords.y);
				}

				if (field[i, j].brick == null && field[i, j].index <= totalBricksCount)
					freeCoords.Add(new Vector2Int(i, j));
			}
		}
		MergeController.Instance.FreeSpace = freeCoords.Count > 0;
		UpdateField();
	}
	
	void UpdateFieldSize()
	{
		if(totalBricksCount < maxBricksCount)
			totalBricksCount++;
		UpdateCoords();
	}
	
	void UpdateField()
	{
		for (int i = 0; i < field.GetLength(0); i++)
		{
			for (int j = 0; j < field.GetLength(1); j++)
			{
				if (!field[i, j].exist) continue;
				if (field[i, j].brick)
					SetBrick(field[i,j].brick.gameObject, new Vector2Int(i, j));
				SetBrick(field[i,j].emptyBrick, new Vector2Int(i, j));
			}
		}
	}
	
	void HighLightField(BrickController brick, bool highlight)
	{
		for (int i = 0; i < field.GetLength(0); i++)
		{
			for (int j = 0; j < field.GetLength(1); j++)
			{
				if (field[i, j].brick)
					field[i,j].brick.HighlightBrick(brick, highlight);
			}
		}
	}

	GameObject BrickObject(GameObject prefab, bool active = true)
	{
		GameObject brick = Instantiate(prefab, fieldTransform);
		brick.gameObject.SetActive(active);
		return brick;
	}

	void SpawnBrick(int level = 0, BrickType type = BrickType.Default)
	{
		MergeController.Instance.FreeSpace = freeCoords.Count > 0;
		if (freeCoords.Count <= 0)
			return;

		Vector2Int coords = freeCoords[Random.RandomRange(0, freeCoords.Count)];
		if(level == 0) 
			MergeController.Instance.CheckForRandomPreset(randomIconProbability, out level, ref type);

		if (type == BrickType.Random)
			UpdateLevelExperience(level, type);
		
		SpawnBrick(coords, level, type);
	}

	void SpawnBrick(Vector2Int coords, int level, BrickType type, bool open = false)
	{
		Vector2 position = GetBrickPosition(coords);
		Vector2 spawnPoint = new Vector2(position.x, position.y + spawnVerticalOffset);
		MergePreset preset = MergeController.Instance.GetPresset(level);
		
		field[coords.x,coords.y].brick = BrickObject(brickPrefab.gameObject).GetComponent<BrickController>();
		field[coords.x,coords.y].brick.RectTransform.sizeDelta = BrickSize(BricksCount);
		field[coords.x,coords.y].brick.SetBrick(preset, level, type, open);
		field[coords.x,coords.y].brick.DoLandingAnimation(spawnPoint, position);
		field[coords.x,coords.y].brick.OpenClick += BrickOnClick;
		field[coords.x,coords.y].brick.PointerUp += BrickOnPointerUp;
		field[coords.x,coords.y].brick.PointerDown += BrickOnPointerDown;

		freeCoords.Remove(coords);
		landingSfx.Play();
		SpawnEffect(spawnEffect, field[coords.x,coords.y].brick.gameObject);
		
		SaveGame();
	}
	
	void SetBrick(GameObject brick, Vector2Int coords)
	{
		brick.SetActive(true);
		Vector2 brickPosition = GetBrickPosition(coords);
		brick.GetComponent<RectTransform>().anchoredPosition = brickPosition;
		brick.GetComponent<RectTransform>().sizeDelta = BrickSize(BricksCount);
	}

	void SpawnEffect(ParticleSystem prefab, GameObject brick)
	{
		ParticleSystem effect = Instantiate(prefab, fieldTransform);
		Vector3 effectSpawnPosition = brick.transform.position;
		effectSpawnPosition.z = -1;
		effect.transform.position = effectSpawnPosition;
		effect.Play();
	}

	void BrickOnClick(BrickController brick)
	{
		clickSfx.Play();
		SpawnEffect(openEffect, brick.gameObject);
	}

	void BrickOnPointerDown(BrickController brick)
	{
		HighLightField(brick, true);
	}
	
	void BrickOnPointerUp(BrickController brick, Vector2 position)
	{
		HighLightField(brick, false);
		
		Vector2Int cachedCoords = BrickPositionToCoords(brick.CachedPosition, brick.RectTransform.pivot);
		Vector2Int coords = BrickPositionToCoords(position, brick.RectTransform.pivot);

		if(!field[coords.x, coords.y].exist)
		{
			brick.RectTransform.anchoredPosition = GetBrickPosition(cachedCoords);
		}
		else if (field[coords.x, coords.y].brick == null || coords == cachedCoords)
		{
			brick.RectTransform.anchoredPosition = GetBrickPosition(coords);
			if (coords != cachedCoords)
			{
				field[coords.x, coords.y].brick = brick;
				field[cachedCoords.x, cachedCoords.y].brick = null;

				freeCoords.Remove(freeCoords.Find(c => c == coords));
				freeCoords.Add(cachedCoords);
			}
		}
		else if (MergeBricks(brick, field[coords.x, coords.y].brick))
		{
			brick.RectTransform.anchoredPosition = GetBrickPosition(coords);
			field[coords.x, coords.y].brick = brick;
			field[cachedCoords.x, cachedCoords.y].brick = null;

			mergingSfx.Play();
			freeCoords.Add(cachedCoords);
			UpdateLevelExperience(brick.Level);
			SpawnEffect(mergeEffect, brick.gameObject);
		}
		else
		{
			BrickController targetBrick = field[coords.x, coords.y].brick;

			field[coords.x, coords.y].brick.IsLandingCheck();
			field[coords.x, coords.y].brick.RectTransform.anchoredPosition = GetBrickPosition(cachedCoords);
			field[coords.x, coords.y].brick = field[cachedCoords.x, cachedCoords.y].brick;

			field[cachedCoords.x, cachedCoords.y].brick = targetBrick;
			brick.RectTransform.anchoredPosition = GetBrickPosition(coords);
		}
		SaveGame();
	}

	Vector2 GetBrickPosition(Vector2 coords)
	{
		coords = new Vector2(coords.y - minCoords.y, coords.x - minCoords.x);
		Vector2 brickSize = BrickSize(BricksCount);

		RectTransform brickTransform = brickPrefab.GetComponent<RectTransform>();
		Vector2 brickPosition = Vector2.Scale(coords, brickSize);
		Vector2 offset = new Vector2(brickSize.x * BricksCount.x / 2, brickSize.y * BricksCount.y / 2);
		brickPosition += Vector2.Scale(brickSize, brickTransform.pivot) - offset;

		return brickPosition;
	}
	
	Vector2Int BrickPositionToCoords(Vector2 position, Vector2 pivot)
	{
		Vector2 brickSize = BrickSize(BricksCount);
		Vector2 offset = new Vector2(brickSize.x * BricksCount.x / 2, brickSize.y * BricksCount.y / 2);
		Vector2 coords = (position + offset - Vector2.Scale(brickSize, pivot)) / brickSize;
		
		coords.x = Mathf.Clamp(coords.x, 0 , BricksCount.x - 1);
		coords.y = Mathf.Clamp(coords.y, 0 , BricksCount.y - 1);

		Vector2Int result = Vector2Int.RoundToInt(coords);
		result = new Vector2Int(result.y  + minCoords.x , result.x + minCoords.y);

		return result;
	}

	Vector2Int BrickSize(Vector2Int count)
	{
		int size = (int)(fieldTransform.rect.width / count.x);
		
		if (size * count.y > fieldTransform.rect.height)
			size = (int) (fieldTransform.rect.height / count.y);
		return new Vector2Int(size, size);
	}

	static bool MergeBricks(BrickController brick, BrickController targetBrick)
	{
		if (brick.Level != targetBrick.Level || !brick.Open || !targetBrick.Open) return false;
		
		brick.LevelUp(MergeController.Instance.GetPresset(brick.Level + 1));
		MergeController.Instance.UpdateMaxOpenLevel(brick.Level);
		
		Destroy(targetBrick.gameObject);
		return true;
	}

	void UpdateLevelExperience(int value = 0, BrickType brickType = BrickType.Default)
	{
		gameState.Score += value;
		CurrentExperience += value;
		if (CurrentExperience < LevelMaxExperience) return;
		CurrentExperienceLevel++;
		CurrentExperience = 0;
		LevelMaxExperience = GetLevelMaxExperience();
		OnLevelUp.Invoke();
		SaveGame();
	}

	int GetLevelMaxExperience()
	{
		int result;
		if (CurrentExperienceLevel < startingLevelsStats.Length)
		{
			result = startingLevelsStats[CurrentExperienceLevel];
			if (CurrentExperienceLevel > 1)
				prevLevelMaxExperience = startingLevelsStats[CurrentExperienceLevel - 1];
		}
		else
		{
			result = LevelMaxExperience * 2 - prevLevelMaxExperience + totalBricksCount;
			prevLevelMaxExperience = LevelMaxExperience;
		}
		return result;
	}
	
	void UpdateSpawnTimer(bool smooth)
	{
		fullText.gameObject.SetActive(freeCoords.Count <= 0);
		boxTimer.transform.parent.gameObject.SetActive(freeCoords.Count > 0);
		
		timer = smooth ? timer - Time.deltaTime * timerSpeed : timer - 1;
		float seconds = Mathf.Max(1, Mathf.CeilToInt(timer % 60f));
		
		if (freeCoords.Count <= 0)
		{
			timer = spawnTime;
			boxImage.fillAmount = 0;
			return;
		}

		if (timer <= 0)
		{
			timer = spawnTime;
			SpawnBrick();
		}
		
		float value = timer < spawnTime ? boxImage.fillAmount : 0f;
		float targetValue = 1f + imageDelta - seconds / spawnTime;
		
		boxTimer.text = seconds.ToString();
		boxImage.fillAmount = smooth
			? Mathf.Lerp(value, targetValue, Time.deltaTime * timerSpeed)
			: targetValue;
	}

	public void OnBoxClick()
	{
		UpdateSpawnTimer(false);
	}
	
public void MinimizeCurrentGame(bool value)
{
    if (!value)
    {
        MaximizeCurrentGame();
        return;
    }

    ResetTriggers();
    // Time.timeScale = 0; ← Видаляємо або коментуємо
    fieldAnimator.SetTrigger(SmallField);
}

public void AddDebugBrick()
{
    SpawnBrick(0, BrickType.Default);
}
	
	void MaximizeCurrentGame()
	{
		Time.timeScale = 1;
		ResetTriggers();
		fieldAnimator.SetTrigger(BigField);
	}
	
	void ResetTriggers()
	{
		fieldAnimator.ResetTrigger(BigField);
		fieldAnimator.ResetTrigger(SmallField);
	}

public void AddBrickSlot()
{
    if (totalBricksCount < maxBricksCount)
    {
        totalBricksCount++;
        UpdateCoords();
        Debug.Log($"➕ Додано brick-ячейку. Поточна кількість: {totalBricksCount}");
    }
    else
    {
        Debug.LogWarning("🚫 Досягнуто максимуму brick-ячейок");
    }
}


}
