using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;

using Player = ExitGames.Client.Photon.LoadBalancing.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPropertyEntry<T>{
  /// <summary>
  /// Unique key for <see cref="Room.CustomProperties"/>.
  /// </summary>
  public readonly string id;

  /// <summary>
  /// Make sure <paramref name="id"/> is unique.
  /// </summary>
  /// <param name="id"></param>
  public RoomPropertyEntry(string id){
    this.id = id;
  }

  /// <summary>
  /// Sets <paramref name="value"/> for the room.
  /// Performs a <see cref="NetworkManager.isMaster"/> check if <paramref name="masterOnly"/> is true.
  /// </summary>
  /// <param name="value"></param>
  /// <param name="masterOnly"></param>
  public void Set(T value, bool masterOnly = true){
    if (masterOnly && !NetworkManager.isMaster) return;

    var h = new Hashtable();
    h.Add(id, value);
    RoomProperties.UpdateRoomHashtable(h);
  }

  /// <summary>
  /// Gets value from room.
  /// </summary>
  /// <returns></returns>
  public T Get(){
    var prop = RoomProperties.GetRoomHashtable();
    return (T)prop[id];
  }

    /// <summary>
  /// Sets <paramref name="hashtable"/> with (<see cref="id"/>, <paramref name="value"/>).
  /// </summary>
  /// <param name="hashtable"></param>
  /// <param name="value"></param>
  public void Initialize(Hashtable hashtable, T value){
    hashtable.Add(id, value);
  }

}

public static class RoomProperties {

  private static Hashtable _localRoomProperties = new Hashtable();

  public static RoomPropertyEntry<bool> teamStatus = new RoomPropertyEntry<bool>("ts");

  public static RoomPropertyEntry<string> sceneLoaded = new RoomPropertyEntry<string>("sl");
  public static RoomPropertyEntry<Hashtable> entities = new RoomPropertyEntry<Hashtable>("et");
  public static RoomPropertyEntry<string> entities2 = new RoomPropertyEntry<string>("et2");

  /// <summary>
  /// Initalizes all custom properties of the room.
  /// </summary>
  /// <returns></returns>
  public static Hashtable CreateRoomHashtable(){
    _localRoomProperties.Clear();

    sceneLoaded.Initialize(_localRoomProperties, "RoomScene1");
    entities.Initialize(_localRoomProperties, new Hashtable());
    entities2.Initialize(_localRoomProperties, "");

    return _localRoomProperties;
  }

  public static Hashtable GetRoomHashtable(){
    var room = NetworkManager.net != null ? NetworkManager.net.CurrentRoom : null;
    if (room != null) {
      _localRoomProperties = room.CustomProperties;
    }
    return _localRoomProperties;
  }

  public static void UpdateRoomHashtable(Hashtable propertiesToSet){
    _localRoomProperties.SetHashtable(propertiesToSet);
    var room = NetworkManager.net != null ? NetworkManager.net.CurrentRoom : null;
    if (room != null){
      room.SetCustomProperties(propertiesToSet);
    }
  }

  public static void LoadRoomHashtable(){

    // Put anything here

    /*
    // load scene before doing anything else

    var roomscene = sceneLoaded.Get();

    for(var i = 0; i < SceneManager.sceneCount; i++){
      var scene = SceneManager.GetSceneAt(i);
      if (roomscene == scene.name) {
        Debug.Log("Current room scene loaded");
        goto LoadRoomProperties;
      }
    }

    // The room scene we need loaded isn't loaded
    // Force load it
    Debug.Log("Loading room scene");
    SceneManager.LoadScene(roomscene);
    return;

    // We have the current room loaded. Load other properties if needed
    LoadRoomProperties:

    return;
    */
  }

}
