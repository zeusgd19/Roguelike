using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Scriptable Objects/Item/Food")]
public class Food : Item
{
    
    public override void Use()
    {
        if (Stack - 1 == 0)
        {
            GameManager.Instance.Inventory.Clear(this);
        }
        if (Stack > 0)
        {
            Stack--;
            GameManager.Instance.ChangeFood(Amount);
            GameManager.Instance.Inventory.Remove(this);
        }
    }
}
