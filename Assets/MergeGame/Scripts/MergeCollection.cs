using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MergeCollection", menuName = "Merge Collection")]
public class MergeCollection : ScriptableObject
{
    [SerializeField]
    public List<MergePreset> objects;
}