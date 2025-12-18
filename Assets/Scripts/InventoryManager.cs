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
        private Dictionary<Item, int> itemStack; 
        public GameObject Panel;
        public GameObject Prefab;
        private TextMeshPro text;
        private void Start()
        {
            inventory = new List<Item>();
            itemToSlot = new Dictionary<Item, GameObject>();
            itemStack = new Dictionary<Item, int>();
        }
        

        public void Add(Item item)
        {
            GameObject newItem;
            int stack;
            if (!itemToSlot.TryGetValue(item, out newItem))
            {
                inventory.Add(item);
                itemStack.Add(item, 1);
                Prefab.GetComponent<Image>().sprite = item.ItemSprite;
                Prefab.GetComponent<UIObject>().item = item;
                newItem = Instantiate(Prefab, Panel.transform, true);
                itemToSlot[item] = newItem;
            }
            else
            {
                itemStack.TryGetValue(item, out stack);
                updateStack(item);
            }
            
            newItem.transform.GetChild(0).GetComponent<TMP_Text>().text = "x" + itemStack[item];


        }

        void updateStack(Item item)
        {
            itemStack[item]++;
        }

        public void Remove(Item item)
        {
            inventory.Remove(item);
            itemStack[item]--;
            GameObject newItem = itemToSlot.GetValueOrDefault(item);
            newItem.transform.GetChild(0).GetComponent<TMP_Text>().text = "x" + itemStack[item];
        }

        public void Clear()
        {
            for (int i = Panel.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(Panel.transform.GetChild(i).gameObject);
                inventory.Clear();
                itemToSlot.Clear();
                itemStack.Clear();
            }
        }

        public void Clear(Item item)
        {
            if (itemStack[item] == 0)
            {
                foreach (Transform child in Panel.transform)
                {
                    UIObject ui = child.GetComponent<UIObject>();

                    if (ui != null && ui.item == item)
                    {
                        Destroy(child.gameObject);
                        itemToSlot.Remove(item);
                        inventory.Remove(ui.item);
                        itemStack.Remove(ui.item);
                    }
                }
            }
        }
    }
}