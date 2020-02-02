using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateEntityIds : MonoBehaviour {

  public int idStart;

  [ContextMenu("Create")]
  public void Create(){
    var items = GetComponentsInChildren<EntityBase>();
    var counter = 0;
    foreach (var i in items) {
      i.EntityID = idStart + counter;
      counter++;
    }
    Debug.Log(items.Length);
  }

}
