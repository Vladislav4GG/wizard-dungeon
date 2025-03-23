using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewItemPreview : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    GameObject panel;
    [SerializeField]
    Image icon;
    [SerializeField]
    Text title;

    Image raycastImage;
    
    void Start()
    {
        MergeController.UnlockedNewLevel += UpdatePreview;
        raycastImage = GetComponent<Image>();
    }

    void UpdatePreview(bool value, int level)
    {
        if (!value) return;
        
        MergePreset preset = MergeController.Instance.GetPresset(level);
        icon.sprite = preset.Icon;
        title.text = preset.Title;

        raycastImage.raycastTarget = true;
        panel.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        raycastImage.raycastTarget = false;
        panel.SetActive(false);
    }
}
