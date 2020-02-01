using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Player = ExitGames.Client.Photon.LoadBalancing.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerPropertyEntry<T> {
  /// <summary>
  /// Unique key for <see cref="Player.CustomProperties"/>.
  /// </summary>
  public readonly string id;

  /// <summary>
  /// Make sure <paramref name="id"/> is unique.
  /// </summary>
  /// <param name="id"></param>
  public PlayerPropertyEntry(string id){
    this.id = id;
  }

  /// <summary>
  /// Gets value from local player.
  /// </summary>
  /// <returns></returns>
  public T GetLocal(){
    return Get(NetworkManager.net.LocalPlayer);
  }

  /// <summary>
  /// Gets value from <paramref name="player"/>.
  /// </summary>
  /// <param name="player"></param>
  /// <returns></returns>
  public T Get(Player player){
    return (T)player.CustomProperties[id];
  }

  /// <summary>
  /// Sets <paramref name="value"/> for local player.
  /// </summary>
  /// <param name="value"></param>
  public void SetLocal(T value){
    Set(NetworkManager.net.LocalPlayer, value);
  }

  /// <summary>
  /// Sets <paramref name="value"/> for <paramref name="player"/>.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="value"></param>
  public void Set(Player player, T value){
    var h = new Hashtable();
    h.Add(id, value);
    player.SetCustomProperties(h);
  }

  /// <summary>
  /// Sets <paramref name="hashtable"/> with (<see cref="id"/>, <paramref name="value"/>).
  /// </summary>
  /// <param name="hashtable"></param>
  /// <param name="value"></param>
  public void Initialilze(Hashtable hashtable, T value){
    hashtable.Add(id, value);
  }

  // ----------------------------------------------------
  // Seems like the best place to put this
  // dunno
  // ----------------------------------------------------

  /// <summary>
  /// Gets value of <see cref="id"/> in <see cref="PlayerPrefs"/>.
  /// </summary>
  /// <returns></returns>
  public int GetPlayerPrefInt() => PlayerPrefs.GetInt(id, 0);

  /// <summary>
  /// Sets <paramref name="value"/> using <see cref="id"/> into <see cref="PlayerPrefs"/>. 
  /// </summary>
  /// <param name="value"></param>
  public void SetPlayerPrefInt(int value) => PlayerPrefs.SetInt(id, value);
}

public static class PlayerProperties  {

  /// <summary>
  /// A huge list of a bunch of touhous.
  /// </summary>
  public static readonly string[] touhous = new[]{
    "Reimu", "Marsia",
    "Rumia", "Daiyousei", "Cirno", "Hong", "Koakuma", "Patchouli", "Sakuya", "Flandre",
    "Letty", "Chen", "Alice", "Lily", "Lyrica", "Lunasa", "Merlin", "Youmu", "Yuyuko", "Ran", "Yakari",
    "Suika",
    "Wriggle", "Mystia", "Keine", "Tewi", "Reisen", "Eirin", "Kaguya", "Mokou",
    "Aya", "Medicine", "Yuuka", "Komachi", "Eiki",
    "Shizuha", "Minoriko", "Hina", "Nitori", "Momiji", "Sanae", "Kanako", "Suwako",
    "Iku", "Tenshi", "Hatate", "Kokoro",
    "Kisume", "Yamame", "Parsee", "Yuugi", "Satori", "Rin", "Utsuho", "Koishi",
    "Kyouko", "Yoshika", "Seiga", "Tojiko", "Futo", "Miko", "Mamizou",
    "Wakasagihime", "Sekibanki", "Kagerou", "Benben", "Yatsuhashi", "Shinmyoumaru", "Raiko",
    "Sumireko",
    "Joon", "Shion",
    "Seiran", "Ringo", "Doremy", "Sagume", "Clownpiece", "Junko", "Hecatia",
    "Eternity", "Nemuno", "Auun", "Narumi", "Satono", "Mai", "Okina",
    "Eika", "Urumi", "Kutaka", "Yachie", "Mayumi", "Keiki", "Saki",
    "Rinnosuke", "Sunny", "Luna", "Star", "Chang'e", "Kasen", "Kosuzu"
  };

  /// <summary>
  /// As name implies, gives a random touhou.
  /// </summary>
  public static string getRandomTouhou => touhous[Random.Range(0, touhous.Length)];

  public static readonly string playerNickname = "nn";

  /// <summary>
  /// Player's status in lobby. (ready or not)
  /// </summary>
  public static readonly PlayerPropertyEntry<bool> lobbyStatus = new PlayerPropertyEntry<bool>("ls");
  /// <summary>
  /// Player's status in loading a scene. (ready or not)
  /// </summary>
  public static readonly PlayerPropertyEntry<bool> gameStatus = new PlayerPropertyEntry<bool>("gs");
  
  /// <summary>
  /// Player's selected character.
  /// </summary>
  public static readonly PlayerPropertyEntry<int> playerCharacter = new PlayerPropertyEntry<int>("pc");
  /// <summary>
  /// Player's selected team.
  /// </summary>
  public static readonly PlayerPropertyEntry<int> playerTeam = new PlayerPropertyEntry<int>("pt");

  /// <summary>
  /// Player's selected response.
  /// </summary>
  public static readonly PlayerPropertyEntry<int> playerResponse = new PlayerPropertyEntry<int>("pr");

  public static Player localPlayer {
     get {
        return NetworkManager.net.LocalPlayer;
     }
  }

  /// <summary>
  /// Initializes all custom properties for the local player.
  /// </summary>
  public static void CreatePlayerHashtable(){
    var h = new Hashtable();

    localPlayer.NickName = GetPlayerNickname();

    lobbyStatus.Initialilze(h, false);
    gameStatus.Initialilze(h, false);

    playerCharacter.Initialilze(h, playerCharacter.GetPlayerPrefInt());
    playerTeam.Initialilze(h, playerCharacter.GetPlayerPrefInt());

    playerResponse.Initialilze(h, -1);

    localPlayer.SetCustomProperties(h);
  }

  /// <summary>
  /// Resets a select few custom properties for the local player.
  /// </summary>
  public static void ResetPlayerHashtable(){
    var h = new Hashtable();

    lobbyStatus.Initialilze(h, false);
    playerResponse.Initialilze(h, -1);

    localPlayer.SetCustomProperties(h);
  }
  
  #region Ready Status

  /// <summary>
  /// If inside a room, returns if all players are lobby ready.
  /// If not, returns if the local player is lobby ready.
  /// </summary>
  /// <returns></returns>
  public static bool GetAllLobbyStatus(){
    if (!NetworkManager.inRoom) return lobbyStatus.GetLocal();

    var players = NetworkManager.net.CurrentRoom.Players.Values;

    if (players.Count < 2) return false;

    foreach(var p in players){
      if (!lobbyStatus.Get(p)) return false;
    }
    return true;
  }

  /// <summary>
  /// If inside a room, returns if all players are game ready.
  /// If not, returns if the local player is game ready.
  /// </summary>
  /// <returns></returns>
  public static bool GetAllGameStatus() {
    if (!NetworkManager.inRoom) return gameStatus.GetLocal();

    var players = NetworkManager.net.CurrentRoom.Players.Values;

    if (players.Count < 2) return false;

    foreach (var p in players) {
      if (!gameStatus.Get(p)) return false;
    }
    return true;
  }

  /// <summary>
  /// Returns the highest value player response from all players.
  /// Returns -3 if no room is active.
  /// Returns -2 if there is less than 2 players in the room.
  /// </summary>
  /// <returns></returns>
  public static int GetAllResponse(){
    if (!NetworkManager.inRoom) return -3;

    var players = NetworkManager.net.CurrentRoom.Players.Values;

    if (players.Count < 2) return -2;

    var response = int.MaxValue;
    foreach (var p in players) {
      response = Mathf.Min(playerResponse.Get(p), response);
    }
    return response;
  }

  /// <summary>
  /// If inside a room, returns true if team mode is inactive OR if team mode is active and there is 2+ unique teams.
  /// If not inside a room, returns true.
  /// </summary>
  /// <returns></returns>
  public static bool GetAllTeamDifferent(){
    if (!NetworkManager.inRoom) return true;
    if (!RoomProperties.teamStatus.Get()) return true;

    var uniqueTeams = NetworkManager.net.CurrentRoom.Players.Values.Select(p => playerTeam.Get(p)).Distinct();
    
    return uniqueTeams.Count() > 1;
  }

  #endregion

  #region Nickname 

  /// <summary>
  /// Gets local player's nickname from <see cref="PlayerPrefs"/>.
  /// Returns a random touhou if no nickname is found.
  /// </summary>
  /// <returns></returns>
  public static string GetPlayerNickname(){
    var key = playerNickname;
    if (PlayerPrefs.HasKey(key)){
      return PlayerPrefs.GetString(key);
    } else {
      var value = getRandomTouhou;
      PlayerPrefs.SetString(key, value);
      return value;
    }
  }

  /// <summary>
  /// Sets <paramref name="nickname"/> into <see cref="PlayerPrefs"/> and <see cref="NetworkManager.net.LocalPlayer"/>.
  /// </summary>
  /// <param name="nickname"></param>
  public static void SetPlayerNickname(string nickname){
    var key = playerNickname;
    PlayerPrefs.SetString(key, nickname);
    localPlayer.NickName = key;
  }

  #endregion

}
