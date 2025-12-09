using DefaultNamespace.Interface;
using UnityEngine;

namespace DefaultNamespace.Components
{
    public class Usable: MonoBehaviour, IUsable
    {
        public void Use()
        {
            GameManager.Instance.ChangeFood(3);
        }
    }
}