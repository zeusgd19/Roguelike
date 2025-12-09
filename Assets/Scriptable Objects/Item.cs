using DefaultNamespace.Interface;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public abstract class Item : ScriptableObject
{
    public string ItemName;
    public int Amount;
    public Sprite ItemSprite;
}
