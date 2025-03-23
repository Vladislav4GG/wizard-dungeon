using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CoinsCounter : MonoBehaviour
{
    Text label;

    void OnProgressUpdate()
    {
        label.text = CounterText.UpdateText(UserProgress.Current.Coins);
    }

    void Start()
    {
        label = GetComponent<Text>();

        OnProgressUpdate();
        UserProgress.Current.ProgressUpdate += OnProgressUpdate;
    }

    void OnDestroy()
    {
        UserProgress.Current.ProgressUpdate -= OnProgressUpdate;
    }
}
