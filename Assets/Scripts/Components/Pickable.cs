using DefaultNamespace.Interface;
using UnityEngine;

namespace DefaultNamespace.Components
{
    public class Pickable: MonoBehaviour, IPickable
    {
        public void PickUp(Item item)
        {
            item.Stack++;
            GameManager.Instance.Inventory.Add(item);
        }
    }
}