using System;
using UnityEngine;

public abstract class InputController : MonoBehaviour
{
    public static event Action Left;
    public static event Action Right;
    public static event Action Down;

    protected static void OnLeft()
    {
        if (Left != null)
            Left.Invoke();
    }

    protected static void OnRight()
    {
        if (Right != null)
            Right.Invoke();
    }

    protected static void OnDown()
    {
        if (Down != null)
            Down.Invoke();
    }
}