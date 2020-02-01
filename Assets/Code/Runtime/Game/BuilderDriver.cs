using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game;
using Code.Runtime.Game.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using EntityNetwork;

public class BuilderDriver : EntityBase, IAutoSerialize, IAutoDeserialize, IAutoRegister, IMasterOwnsUnclaimed {

  // Any variable with [NetVar]
  // Will be sent to all clients based on the parameters
  // token = must be unique
  // alwaysReliable = if the message MUST be sent (not important for positions for example)
  // alwaysSend = if any network message is sent, this variable must be sent
  [Header("Networked Variables")]
  // updateMs = update timers
  [NetVar('p', false, false, 100)]
  public Vector3 position;

  [Header("Components")]
  public NavMeshAgent nva;

  [Header("Helds")]
  public Heldable item;

  public List<Heldable> Items;

  public int heldItemIndex;

  public ITargetable Target;

  public GameObject TestTarget;

  private void Start()
  {
    RepairItem hammer = gameObject.AddComponent<RepairItem>();
    hammer.name = "Hammer";
    hammer.baseRepairStength = 5;
    hammer.useCooldown = TimeSpan.FromMilliseconds(500);
    Items.Add(hammer);
    item = hammer;
    RepairItem mortar = gameObject.AddComponent<RepairItem>();
    mortar.name = "Mortar";
    mortar.baseRepairStength = 20;
    mortar.useCooldown = TimeSpan.FromMilliseconds(1000);
    Items.Add(mortar);
  }

  // Update is called once per frame
  void Update(){
    if (isMine){
      LocalUpdate();
    } else {
      RemoteUpdate();
    }

    if (TestTarget)
    {
      Target = TestTarget.GetComponent<Structure>();
    }
  }

  /// <summary>
  /// Called by the client that owns this object.
  /// </summary>
  void LocalUpdate(){
    // movement
    var steering = GameHelper.GetDirectionInput;
    nva.velocity = Vector3.MoveTowards(nva.velocity, nva.speed * steering, nva.acceleration * Time.deltaTime);

    position = transform.position;

    // items
    if (Input.GetMouseButtonDown(0))
      if (Target != null)
      {
        if (item is ITargeting)
        {
          (item as ITargeting).UseOn(Target);
        }
        else
        {
          item?.Use();
        }
      }
      else
      {
        item?.Use();
      }
    if (Input.mouseScrollDelta.y > 0)
    {
      if (Items.Any())
      {
        heldItemIndex = (heldItemIndex + 1) % Items.Count;
        item = Items[heldItemIndex];
      }
    }
    else if (Input.mouseScrollDelta.y < 0)
    {
      if (Items.Any())
      {
        heldItemIndex = (heldItemIndex - 1);
        if (heldItemIndex < 0)
        {
          heldItemIndex = Items.Count - 1;
        }
        item = Items[heldItemIndex];
      }
    }
  }

  /// <summary>
  /// Called by the client that doesn't own this object.
  /// </summary>
  void RemoteUpdate(){
    nva.destination = position;
  }
}
