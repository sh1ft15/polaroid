using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/InteractObject", order = 2)]
public class InteractObject : ScriptableObject
{
    public string characterName;
    public string condition = "";
    public List<CardObject> giveCards;
    public List<CardObject> receiveCards;
    public List<string> dialogs;
    public List<Sprite> sprites;
    public bool toggleEnemy;
}

