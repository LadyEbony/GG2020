using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject TeamSelectionPanel;
    List<GameObject> builders;
    GameObject destroyer;
    private List<SpawnPoint> builderSpawns;
    private List<SpawnPoint> kaijuSpawns;

    public void Start()
    {
        builderSpawns = new List<SpawnPoint>();
        kaijuSpawns = new List<SpawnPoint>();
        foreach (var spawnPoint in GameObject.FindObjectsOfType<SpawnPoint>())
        {
            if (spawnPoint.CompareTag("BuilderSpawn"))
            {
                builderSpawns.Add(spawnPoint);
            }
            else if (spawnPoint.CompareTag("KaijuSpawn"))
            {
                kaijuSpawns.Add(spawnPoint);
            }
        }
    }
    public void AddDestroyerPlayer()
    {
        Debug.Log("Joined kaiju team");
    }

    public void AddBuilderPlayer()
    {
        Debug.Log("Joined repair team");
    }
    
    public Vector3? GetAvailableSpawnPoint(Player player)
    {
        SpawnPoint[] availableSpawns = new SpawnPoint[0];
        if (player is Fixer)
        {
            availableSpawns = builderSpawns.Where(s => !s.inUse).ToArray();
        }
        else if(player is Monster)
        {
            availableSpawns = kaijuSpawns.Where(s => !s.inUse).ToArray();
        }

        if (availableSpawns.Length > 0)
        {
            return availableSpawns[Random.Range(0, availableSpawns.Length)].Position;
        }
        return null;
    }
}
