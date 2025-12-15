using System;
using DefaultNamespace.Components;
using DefaultNamespace.ExtensionMethods;
using UnityEngine;

public class FoodObject : CellObject
{
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
        Debug.Log(item);
        pickable.PickUp(item);
        Destroy(gameObject);
    }
}
