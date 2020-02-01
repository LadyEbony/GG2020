using System;
using System.Collections.Generic;
using UnityEngine;

public class Item : Heldable
{
    public string name;
    public TimeSpan useCooldown;
    protected TimeSpan timeSinceLastUse;
    
    public void Start()
    {
        timeSinceLastUse = useCooldown;
    }

    public void Update()
    {
        timeSinceLastUse += TimeSpan.FromSeconds(Time.deltaTime);
    }
    public override void Use()
    {
        
    }
}
