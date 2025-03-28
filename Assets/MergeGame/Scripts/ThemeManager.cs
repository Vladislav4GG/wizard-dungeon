using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    public ThemePreset[] themes;           // Масив тем
    public Image backgroundImage;          // UI-об'єкт фону

    private int currentThemeIndex = -1;

    public void ApplyThemeByLevel(int level)
    {
        int index = level / 2; // Кожні 2 рівня

        if (themes.Length == 0 || index >= themes.Length)
            index = themes.Length - 1;

        if (index == currentThemeIndex) return; // Щоб не оновлювати зайвий раз

        currentThemeIndex = index;
        ThemePreset theme = themes[index];

        backgroundImage.color = theme.background;

        Debug.Log($"🎨 Тема #{index} застосована для рівня {level}");
    }
}