using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class GameInitializer : MonoBehaviour {

  public GameObject builderPrefab, kaijuPrefab;

  IEnumerator Start(){
    PlayerProperties.gameStatus.SetLocal(true);

    while (!PlayerProperties.GetAllGameStatus()) yield return null;

    if (!NetworkManager.inRoom){
      PlayerProperties.CreatePlayerHashtable();

      var item = PlayerProperties.playerTeam.GetLocal() == 0 ? builderPrefab : kaijuPrefab;
      var gobj = Instantiate(item);
      var eb = gobj.GetComponent<EntityBase>();
      eb.EntityID = 1;
      eb.authorityID = -1;
      eb.Register();

      yield break;
    }

    var i = 1;
    var counter = 5;
    var players = NetworkManager.getSortedPlayers;
    foreach(var p in players){
      var item = PlayerProperties.playerTeam.Get(p) == 0 ? builderPrefab : kaijuPrefab;
      var gobj = Instantiate(item);
      var eb = gobj.GetComponent<EntityBase>();
      eb.EntityID = i;
      eb.authorityID = p.ID;
      eb.Register();

      i += counter;
    }
  }

}
