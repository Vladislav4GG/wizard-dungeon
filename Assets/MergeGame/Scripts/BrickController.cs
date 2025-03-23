using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler 
{
    public event Action<BrickController, Vector2> PointerUp = delegate { };
    public event Action<BrickController> PointerDown = delegate { };
    public event Action<BrickController> OpenClick = delegate { };

    [SerializeField]
    GameObject highlight;
    [SerializeField]
    GameObject shadow;
    [SerializeField]
    Image boxImage;
    [SerializeField]
    Image iconImage;
    [SerializeField]
    Sprite defaultBox;
    [SerializeField]
    Sprite randomBox;
    
    [SerializeField]
    Animator coinAnimator;
    [SerializeField]
    Text coinsCounter;

    RectTransform rect;
    Coroutine getCoins;
    const int idleSortingOrder = 1;
    const int selectedSortingOrder = 100;
    float moveDuration;
    bool canDrag;
    bool open;
    bool landing;

    public RectTransform RectTransform => rect ?? (rect = GetComponent<RectTransform>());
    public Vector2 CachedPosition { get; private set; }
    public int Level { get; private set; }
    public BrickType Type { get; private set; }
    public bool Open
    {
        get => open;
        private set
        {
            open = value;
            if (getCoins != null || !value) return;
            GetComponent<Animator>().SetTrigger("Open");
            getCoins = StartCoroutine(GetCoins());
        }
    }

    void OnEnable()
    {
        CachedPosition = RectTransform.anchoredPosition;
    }

    public void SetBrick(MergePreset preset, int level, BrickType type, bool open = true)
    {
        Level = level;
        CachedPosition = RectTransform.anchoredPosition;
        Type = type;
        Open = open;
        SetBrick(preset);
    }
    
    void SetBrick(MergePreset preset)
    {
        iconImage.sprite = preset.Icon;
        iconImage.gameObject.SetActive(Open);
        boxImage.gameObject.SetActive(!Open);
        
        if(!Open)
            boxImage.sprite = Type == BrickType.Default ? defaultBox : randomBox;
    }

    public void LevelUp(MergePreset preset)
    {
        Level ++;

        DoMergingAnimation();
        SetBrick(preset);
    }

    public void HighlightBrick(BrickController brick, bool active)
    {
        if (brick == this || !Open || Level != brick.Level) return;
        
        highlight.SetActive(active);
        iconImage.gameObject.GetComponent<Canvas>().sortingOrder = idleSortingOrder;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(landing) return;
        
        if (!Open)
        {
            boxImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(true);
            
            OpenClick.Invoke(this);
            OpenClick = null;
            Open = true;
            return;
        }

        canDrag = true;
        CachedPosition = RectTransform.anchoredPosition;
        shadow.SetActive(true);
        PointerDown.Invoke(this);

        StartCoroutine(LocalMove(ScreenPointToAnchoredPosition(eventData.position), null));
        iconImage.gameObject.GetComponent<Canvas>().sortingOrder = selectedSortingOrder;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        
        PointerUp.Invoke(this, ScreenPointToAnchoredPosition(eventData.position));
        CachedPosition = RectTransform.anchoredPosition;
        iconImage.gameObject.GetComponent<Canvas>().sortingOrder = idleSortingOrder;

        shadow.SetActive(false);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        RectTransform.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
    }
    
    Vector2 ScreenPointToAnchoredPosition(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            screenPoint,
            Camera.main, 
            out Vector2 position
        );
        
        return position;
    }
   
    IEnumerator LocalMove(Vector2 position, Action onComplete)
    {
        Vector2 startPosition = RectTransform.anchoredPosition;
        float t = Time.deltaTime;
        while (t < moveDuration)
        {
            RectTransform.anchoredPosition = Vector2.Lerp(startPosition, position, t / moveDuration);
            yield return null;
            t += Time.deltaTime;
        }

        RectTransform.anchoredPosition = position;
        onComplete?.Invoke();
    }

    
    public void DoLandingAnimation(Vector2 startPosition, Vector2 targetPosition)
    {
        landing = true;
        RectTransform.anchoredPosition = startPosition;
        GetComponent<Animator>().SetTrigger("Spawn");
        StartCoroutine(DelayedCall(() => { StartCoroutine(Falling(targetPosition)); }, 0.25f));
    }

    public void IsLandingCheck()
    {
        if (!landing) return;
        
        landing = false;
        StopAllCoroutines();
        GetComponent<Animator>().SetTrigger("Land");
        StartCoroutine(DelayedCall(() =>
        {
            boxImage.gameObject.GetComponent<Canvas>().sortingOrder = idleSortingOrder;
        }, 1f));
    }

    void DoMergingAnimation()
    {
        GetComponent<Animator>().SetTrigger("Merge");
    }
    
    IEnumerator Falling(Vector2 targetPosition)
    {
        Vector2 startPosition = RectTransform.anchoredPosition;
        float t = Time.deltaTime;
        while (t < 0.1f && landing)
        {
            RectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t / 0.1f);
            yield return null;
            t += Time.deltaTime;
        }

        landing = false;
        RectTransform.anchoredPosition = targetPosition;
        GetComponent<Animator>().SetTrigger("Land");
        StartCoroutine(DelayedCall(() =>
        {
            boxImage.gameObject.GetComponent<Canvas>().sortingOrder = idleSortingOrder;
        }, 1f));
    }

    IEnumerator GetCoins()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.5f);

            GetComponent<Animator>().SetTrigger("Blink");
            StartCoroutine(
                DelayedCall(() =>
                {
                    int coins = Level + 1;
                    UserProgress.Current.Coins += coins;
                    coinsCounter.text = CounterText.UpdateText(coins);
                    coinAnimator.SetTrigger("Coin");
                }, 0.5f));
        }
    }
    
    IEnumerator DelayedCall(Action onComplete, float delay)
    {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }
}
