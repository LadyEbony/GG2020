using ExitGames.Client.Photon.LoadBalancing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeSelect : MonoBehaviour {

  public static int Mode;

  public GameObject buttons;
  public TextMeshProUGUI textMesh;

  private void Update() {
    textMesh.text = NetworkManager.net.State.ToString();
  }

  public void PlaySingleplayer(){
    gameObject.SetActive(false);
    Mode = 1;
  }

  public void PlayMultiplayer(){
    buttons.SetActive(false);
    StartCoroutine(Connect());
  }

  IEnumerator Connect(){
    bool request;

    if (NetworkManager.net.ConnectToNameServer()) {
      Debug.Log("Connecting to name server");
    } else {
      Debug.Log("Name Server connection failed");
      yield break;
    }

    while (!NetworkManager.onNameServer) yield return null;

    request = NetworkManager.net.OpGetRegions();
    if (request) {
      Debug.Log("Region request sent");
    } else {
      Debug.Log("Failed request regions");
      buttons.SetActive(true);
      yield break;
    }

    while (NetworkManager.net.AvailableRegions == null) yield return null;
    Debug.Log("Regions list recieved");

    request = NetworkManager.net.ConnectToRegionMaster(NetworkManager.net.AvailableRegions[0]);
    if (request) {
      Debug.Log("Connected to region master.");
    } else {
      Debug.Log("Couldn't connect to region master.");
      buttons.SetActive(true);
      yield break;
    }

    while (!NetworkManager.onMasterLobby) yield return null;

    var ro = new RoomOptions();
    ro.EmptyRoomTtl = 1000;
    ro.CleanupCacheOnLeave = true;
    ro.PlayerTtl = 500;
    ro.PublishUserId = false;
    ro.MaxPlayers = 20; // TODO: Expose this better

    request = NetworkManager.net.OpJoinOrCreateRoom("gamespawn", ro, ExitGames.Client.Photon.LoadBalancing.TypedLobby.Default);
    if (request) {
      Debug.Log("Room created");
    } else {
      Debug.Log("Couldn't create/join room");
      buttons.SetActive(true);
      yield break;
    }

    while (!NetworkManager.inRoom) yield return null;

    gameObject.SetActive(false);
    Mode = 2;

  }

}
