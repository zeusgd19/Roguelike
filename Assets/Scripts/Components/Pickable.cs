using DefaultNamespace.Interface;
using UnityEngine;

namespace DefaultNamespace.Components
{
    public class Pickable: MonoBehaviour, IPickable
    {
        public void PickUp(IPickable pickable)
        {
            GameManager.Instance.Player.Collect(pickable);
        }
    }
}