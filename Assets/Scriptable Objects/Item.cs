using DefaultNamespace.Interface;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public abstract class Item : ScriptableObject
{
    public string ItemName;
    public int Amount;
    public Sprite ItemSprite;

    public abstract void Use();
}
