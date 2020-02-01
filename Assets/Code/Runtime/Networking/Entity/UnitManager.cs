using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EntityNetwork;
using ExitGames.Client.Photon;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public interface IUnit {
  void Register(int entityId);
  void Destroy();
  int GetEntityId { get; }
  int GetTypeId { get; }
}

public class UnitManager : MonoBehaviour {

  public int entityCounter = 0;
  private Dictionary<int, IUnit> units;

  private void Start() {
    RoomProperties.CreateRoomHashtable();

    units = new Dictionary<int, IUnit>();

    while(entityCounter < 10){ 
      var u = CreateUnit(entityCounter++, 0);
      AddUnit(u);
    }
  }

  private void Update() {
    var prop = RoomProperties.entities.Get();
    var toRemove = new HashSet<int>(units.Keys);

    foreach(var itempairs in prop){
      var entityId = (int)itempairs.Key;
      var typeId = (int)itempairs.Value;

      if (!toRemove.Remove(entityId)) {  
        // couldn't remove entity, must be new
        var u = CreateUnit(entityId, typeId);
        units.Add(entityId, u);
      }
    }

    // remove
    foreach(var id in toRemove){
      units[id].Destroy();
      units.Remove(id);
    }

    if (Input.GetMouseButtonDown(0)){
      var u = CreateUnit(entityCounter++, 0);
      AddUnit(u);
    }
  }

  public IUnit CreateUnit(int entityId, int typeId){
    return null;
  }

  public void AddUnit(IUnit unit){
    units.Add(unit.GetEntityId, unit);
    UpdateRoomProperties();
  }

  public void RemoveUnit(IUnit unit){
    units.Remove(unit.GetEntityId);
    UpdateRoomProperties();
  }

  private void UpdateRoomProperties(){
    var h = new Hashtable();
    foreach(var u in units.Values){
      h.Add(u.GetEntityId, u.GetTypeId);
    }
    RoomProperties.entities.Set(h, true);
  }

}
