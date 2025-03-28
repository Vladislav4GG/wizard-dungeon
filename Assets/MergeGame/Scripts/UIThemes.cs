using UnityEngine;

[CreateAssetMenu(fileName = "UITheme", menuName = "UI/UI Theme")]
public class UITheme : ScriptableObject
{
    public Color buttonBackgroundColor;
    public Color buttonTextColor;
    public Font textFont;
}