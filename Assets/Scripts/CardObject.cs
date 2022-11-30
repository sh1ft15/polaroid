using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CardObject", order = 1)]
public class CardObject : ScriptableObject
{
    public string title;
    public string code;
    public Sprite sprite;
    public int count;
    public int limit = 1;
    public bool unlocked;
    [TextArea(3, 100)] public string description;
}
