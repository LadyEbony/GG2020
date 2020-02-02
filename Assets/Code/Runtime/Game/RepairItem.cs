using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
  public class RepairItem : Item, ITargeting, IRanged
  {
    public int baseRepairStength;
    public float range;

    public float GetRange() => range;

    public void UseOn(ITargetable target)
    {
      if (timeSinceLastUse >= useCooldown)
      {
        timeSinceLastUse = TimeSpan.Zero;
        if (target is IRepairable)
        {
            (target as IRepairable).Repair(baseRepairStength);
            Debug.Log($"Repairing {target} for {baseRepairStength}");
        }
      }
    }

    //private bool InRange(IRepairable target)
    //{
    //  if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
    //  {
    //    if(hit.transform.CompareTag(""))
    //  }
    //}
  }


}