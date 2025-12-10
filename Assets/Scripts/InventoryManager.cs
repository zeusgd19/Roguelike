using System;
using System.Collections.Generic;
using DefaultNamespace.Components;
using DefaultNamespace.ExtensionMethods;
using DefaultNamespace.Interface; 
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class InventoryManager: MonoBehaviour
    {
        private List<Item> inventory;
        public GameObject Panel;
        public GameObject Prefab;
        private Image image;
        
        private void Start()
        {
            inventory = new List<Item>();
        }
        

        public void Add(Item item)
        {
            if (!inventory.Contains(item))
            {
                inventory.Add(item);
                Prefab.GetComponent<Image>().sprite = item.ItemSprite;
                Prefab.GetComponent<UIObject>().item = item;
                Instantiate(Prefab, Panel.transform, true);
            }

        }

        public void Remove(Item item)
        {
            inventory.Remove(item);
        }
    }
}