using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CharacterSelect : MonoBehaviour {

  public GameObject playerPrefab;
  public Transform playerTranformLeft, playerTransformRight;

  public Dictionary<int, CharacterSelectPortrait> playerDictionary;

  public static CharacterSelect Instance { get; private set; }

  private void Awake() {
    Instance = this;
  }

  private IEnumerator Start() {
    PlayerProperties.CreatePlayerHashtable();

    // initialize
    Transform t;
    t = transform.Find("Players");
    playerTranformLeft = t.Find("Clients_Left");
    playerTransformRight = t.Find("Clients_Right");

    playerPrefab = playerTranformLeft.GetChild(0).gameObject;
    playerPrefab.SetActive(false);

    playerDictionary = new Dictionary<int, CharacterSelectPortrait>();

    // wait for mode choice
    while (ModeSelect.Mode == 0) yield return null;

    // Online
    // Create portraits based on who's connected
    if (ModeSelect.Mode == 2){
      var players = NetworkManager.getSortedPlayers;
      foreach (var p in players) {
        AddPlayer(p.ID);
      }
    }
  }

  public void LoadGame(){
    SceneManager.LoadScene("Player Prefab Test");
  }

  private void OnEnable() {
    NetworkManager.onJoin += OnPlayerJoin;
    NetworkManager.onLeave += OnPlayerLeave;
  }

  private void OnDisable() {
    NetworkManager.onJoin -= OnPlayerJoin;
    NetworkManager.onLeave -= OnPlayerLeave;
  }

  private void OnPlayerJoin(EventData data) {
    var id = (int)data.Parameters[ParameterCode.ActorNr];
    AddPlayer(id);
  }

  private void OnPlayerLeave(EventData data) {
    var id = (int)data.Parameters[ParameterCode.ActorNr];

    if (playerDictionary.ContainsKey(id)) {
      Destroy(playerDictionary[id].gameObject);
      playerDictionary.Remove(id);

      ReadyCheck.Stop = true;
    }
  }

  private void AddPlayer(int id){
    if (!playerDictionary.ContainsKey(id) && id != PlayerProperties.localPlayer.ID) {
      var gobject = Instantiate(playerPrefab, playerTranformLeft.childCount <= playerTransformRight.childCount ? playerTranformLeft : playerTransformRight);
      gobject.SetActive(true);

      var comp = gobject.GetComponent<CharacterSelectPortrait>();
      var player = NetworkManager.net.CurrentRoom.GetPlayer(id);
      comp.SetPlayer(player);

      playerDictionary.Add(id, comp);

      ReadyCheck.Stop = true;
    }
  }

}
