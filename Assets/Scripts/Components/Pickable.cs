using System;
using DefaultNamespace.Interface;
using UnityEngine;

namespace DefaultNamespace.Components
{
    public class Pickable: MonoBehaviour, IPickable
    {
        public void PickUp(Item item)
        {
            
            GameManager.Instance.Inventory.Add(item);
        }
    }
}