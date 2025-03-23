using UnityEngine;

public class ThemeButton : MonoBehaviour
{
    public ThemePreset lightTheme;
    public ThemePreset darkTheme;
    
    
    static bool IsOn
    {
        get => PlayerPrefs.GetInt("Theme", 1) == 1;
        set => PlayerPrefs.SetInt("Theme", value ? 1 : 0);
    }
    
    public void ChangeState()
    {
        IsOn = !IsOn;
        UpdateState();
    }

    void UpdateState()
    {
        ThemePreset currentTheme = IsOn ? lightTheme : darkTheme;
        ThemeController.Instance.CurrentTheme = currentTheme;
        ThemeController.Instance.SaveCurrentTheme();
    }
}
