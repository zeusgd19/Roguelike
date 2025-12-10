using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Scriptable Objects/Item/Food")]
public class Food : Item
{
    
    public override void Use()
    {
        if (Stack > 0)
        {
            Stack--;
            GameManager.Instance.ChangeFood(Amount);
        }
    }
}
