using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class SetThemeColor : MonoBehaviour
{
    enum ColorType
    {
        Text,
        Background,
        Field,
        Label,
        Sprite,
        Empty,
    }

    [SerializeField]
    ColorType colorType;
    [SerializeField]
    int index;

    void UpdateColor(ThemePreset theme)
    {
        Graphic graphic = GetComponent<Graphic>();
        switch (colorType)
        {
            case ColorType.Text:
                graphic.color = theme.text;
                break;
            case ColorType.Background:
                graphic.color = theme.background;
                break;
            case ColorType.Field:
                graphic.color = theme.fieldColor;
                break;
            case ColorType.Label:
                graphic.color = theme.labelColors[Mathf.Clamp(index, 0, theme.labelColors.Length - 1)];
                break;
            case ColorType.Sprite:
                graphic.color = theme.spriteColors[Mathf.Clamp(index, 0, theme.spriteColors.Length - 1)];
                break;
            case ColorType.Empty:
                graphic.color = theme.emptyColor;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void Start()
    {
        UpdateColor(ThemeController.Instance.CurrentTheme);
        ThemeController.Instance.ThemeChanged += UpdateColor;
    }

    void OnDestroy()
    {
        ThemeController.Instance.ThemeChanged -= UpdateColor;
    }
}
