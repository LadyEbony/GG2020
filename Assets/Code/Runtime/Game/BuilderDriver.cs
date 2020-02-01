using System.Collections;
using System.Collections.Generic;
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

  // Update is called once per frame
  void Update(){
    if (isMine){
      LocalUpdate();
    } else {
      RemoteUpdate();
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
      item?.Use();
  }

  /// <summary>
  /// Called by the client that doesn't own this object.
  /// </summary>
  void RemoteUpdate(){
    nva.destination = position;
  }
}
