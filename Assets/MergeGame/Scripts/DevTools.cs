using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class DevTools : MonoBehaviour
{
    public void AddCoins()
    {
        UserProgress.Current.Coins += 50000;
        Debug.Log("🎁 Додано 50 000 монет");
    }

    public void UnlockAllMonsters()
    {
        string gameId = UserProgress.Current.CurrentGameId;
        GameState state = UserProgress.Current.GetGameState<GameState>(gameId);

        if (state == null)
        {
            Debug.LogWarning("❌ GameState не знайдено!");
            return;
        }

        var presets = new List<MergePreset>(MergeController.Instance.GetPresets());
        int allLevels = presets.Count;
        state.MaxOpenLevel = allLevels;

        UserProgress.Current.SetGameState(gameId, state);
        UserProgress.Current.SaveGameState(gameId);

        MergeController.Instance.UpdateMaxOpenLevel(allLevels);

        Debug.Log($"🧩 Відкрито всі монстри до рівня {allLevels}");
    }

public void ResetProgress()
{
    string gameId = UserProgress.Current.CurrentGameId;

    // 1. Створюємо новий, порожній GameState
    GameState state = new GameState();

    // 2. Зберігаємо новий стан гри
    UserProgress.Current.SetGameState(gameId, state);
    UserProgress.Current.SaveGameState(gameId);

    // 3. Скидаємо монети
    UserProgress.Current.Coins = 0;

    // 4. Повністю скидаємо MergeController
    var presets = new List<MergePreset>(MergeController.Instance.GetPresets());
    MergeController.Instance.UpdateState(
        0,                                // MaxOpenLevel = 0
        new long[presets.Count],          // Порожні ціни
        DateTime.UtcNow                  // Новий таймер
    );

    MergeController.Instance.ResetPresets(); // скидає ціни

    // 5. Примусово оновлюємо магазин
    StorePanel storePanel = GameObject.FindFirstObjectByType<StorePanel>();
    if (storePanel != null)
    {
        storePanel.RebuildStore();
        Debug.Log("🛒 Магазин оновлено після скидання прогресу");
    }
    else
    {
        Debug.LogWarning("⚠️ StorePanel не знайдено — магазин не оновлено");
    }

    Debug.Log("🔄 Прогрес повністю скинуто");

    // 6. Перезапуск сцени для повного оновлення всього UI і поля гри
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

public void AddBrickSlot()
{
    GameController gameController = GameObject.FindFirstObjectByType<GameController>();
    if (gameController != null)
    {
        gameController.AddBrickSlot();
    }
    else
    {
        Debug.LogWarning("❌ GameController не знайдено");
    }
}


}