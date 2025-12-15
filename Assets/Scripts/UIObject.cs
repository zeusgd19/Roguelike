using DefaultNamespace.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class UIObject : MonoBehaviour
    {
        public Item item;

        public void Use()
        {
            item.Use();
        }
    }
}