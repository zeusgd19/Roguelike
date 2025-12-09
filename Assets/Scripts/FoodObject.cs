using System;
using DefaultNamespace.Components;
using DefaultNamespace.ExtensionMethods;
using UnityEngine;

public class FoodObject : CellObject
{
    public string Name;
    public int AmountGranted = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public override void PlayerEntered()
    {
        
        //Add to Player Inventory
        Pickable pickable = gameObject.As<Pickable>();
        pickable.PickUp(pickable);
        
        //increase food
        GameManager.Instance.ChangeFood(AmountGranted);
        Destroy(gameObject);
    }
}
