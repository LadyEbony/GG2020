using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;
using System.Linq;

using EntityNetwork;

namespace Code.Runtime.Game{
  public class Pickup : EntityBase, IAutoSerialize, IAutoDeserialize, IEarlyAutoRegister, IMasterOwnsUnclaimed {

    [Header("Item states")]
    public ItemFactory.ItemType itemType;
    public bool fixerCanPickUp;
    public bool monsterCanPickup;

    [Header("Networked Values")]
    [NetVar('o', true, false, 100)]
    public int ownerId;

    private void OnTriggerEnter(Collider other) {
      Debug.Log("Pickup Entered");
      var driver = other.gameObject.transform?.parent.GetComponent<BuilderDriver>();
      var player = driver?.player;
      if (player != null){
        Debug.Log(player);
        if (monsterCanPickup && player is Monster || fixerCanPickUp && player is Fixer) {
          RaiseEvent('p', true, driver.EntityID);
        }
      }
    }

    void Update(){
      if (ownerId != 0){
        gameObject.SetActive(false);
      }
    }

    [NetEvent('p')]
    public void PickupItem(int ownerClaimingPickup){
      // no owner
      // only master can decide who picks it up
      if (NetworkManager.isMaster && ownerId == 0){
        ownerId = ownerClaimingPickup;
        var driver = EntityManager.Entity<BuilderDriver>(ownerId);
        var item = ItemFactory.SelectByName[itemType](driver.gameObject);
        driver.player.Items.Add(item);
      }
    }
  }
}