using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class InventoryManager: MonoBehaviour
    {
        private List<Item> inventory;
        private Dictionary<Item, GameObject> itemToSlot;
        public GameObject Panel;
        public GameObject Prefab;
        private TextMeshPro text;
        private void Start()
        {
            inventory = new List<Item>();
            itemToSlot = new Dictionary<Item, GameObject>();
        }
        

        public void Add(Item item)
        {
            GameObject newItem;
            if (!itemToSlot.TryGetValue(item, out newItem))
            {
                inventory.Add(item);
                Prefab.GetComponent<Image>().sprite = item.ItemSprite;
                Prefab.GetComponent<UIObject>().item = item;
                newItem = Instantiate(Prefab, Panel.transform, true);
                itemToSlot[item] = newItem;
            }
            
            newItem.transform.GetChild(0).GetComponent<TMP_Text>().text = "x" + item.Stack;


        }

        public void Remove(Item item)
        {
            inventory.Remove(item);
            GameObject newItem = itemToSlot.GetValueOrDefault(item);
            newItem.transform.GetChild(0).GetComponent<TMP_Text>().text = "x" + item.Stack;
        }

        public void Clear()
        {
            for (int i = Panel.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(Panel.transform.GetChild(i).gameObject);
                inventory.Clear();
                itemToSlot.Clear();
            }
        }

        public void Clear(Item item)
        {
            foreach (Transform child in Panel.transform)
            {
                UIObject ui = child.GetComponent<UIObject>();

                if (ui != null && ui.item == item)
                {
                    Destroy(child.gameObject);
                    itemToSlot.Remove(item);
                    inventory.Remove(ui.item);
                }
            }
        }
    }
}