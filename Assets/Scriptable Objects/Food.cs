using DefaultNamespace.Components;
using DefaultNamespace.Interface;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Scriptable Objects/Item/Food")]
public class Food : Item
{
    
    public override void Use()
    {
        GameManager.Instance.Inventory.Clear(this);
        GameManager.Instance.ChangeFood(Amount);
        GameManager.Instance.Inventory.Remove(this);
    }
}
