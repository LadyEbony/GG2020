using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using ExitGames.Client.Photon.LoadBalancing;

public class QuickJoin : MonoBehaviour {
  // Update is called once per frame
  public bool allowQuickJoin = true;

	void Update () {
    if (allowQuickJoin && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.J)) {
      if (NetworkManager.inRoom) return;

      var activeScene = "QUICKJOIN";

      var ro = new RoomOptions();
      ro.IsVisible = false;
      ro.IsOpen = true;
      ro.MaxPlayers = NetworkManager.instance.expectedMaxPlayers;

      NetworkManager.net.OpJoinOrCreateRoomWithProperties(activeScene, ro, null);
    }
  }

  IEnumerator Start() {
    while (!allowQuickJoin) yield return null;

    if (NetworkManager.net.ConnectToNameServer()){
      Debug.Log("Connecting to name server");
    } else {
      Debug.Log("Name Server connection failed");
      yield break;
    }

    while (!NetworkManager.onNameServer || !NetworkManager.isReady) yield return null;
    Debug.Log("Connected to name server");

    if (NetworkManager.net.OpGetRegions()){
      Debug.Log("Started region request");
    } else {
      Debug.Log("Failed region request");
      yield break;
    }

    while (NetworkManager.net.AvailableRegions == null) yield return null;
    Debug.Log("Received region list");

    if(NetworkManager.net.ConnectToRegionMaster("usw")){
      Debug.Log("Connecting to region master 'usw'");
    } else {
      Debug.Log("Failed to connect to region master 'usw'");
      yield break;
    }
    
    while (!NetworkManager.onMasterLobby) yield return null;
    Debug.Log("Connected to region master");
    Debug.Log("You can quick join now");
  }

}
