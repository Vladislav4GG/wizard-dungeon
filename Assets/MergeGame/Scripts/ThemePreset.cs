using UnityEngine;

[CreateAssetMenu(fileName = "ThemePreset", menuName = "Theme Preset")]
public class ThemePreset : ScriptableObject
{
    public Color[] labelColors;
    public Color[] spriteColors;
    public Color emptyColor;
    public Color fieldColor;
    public Color text;
    public Color background;
}
