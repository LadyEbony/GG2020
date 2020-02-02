using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game;
using Code.Runtime.Game.Interfaces;
using UnityEditor.UIElements;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject TeamSelectionPanel;
    List<GameObject> builders;
    GameObject destroyer;
    private List<SpawnPoint> builderSpawns;
    private List<SpawnPoint> kaijuSpawns;
    [HideInInspector]
    public List<Structure> structures;
    public GameState currentGameState;

    public float waitingTime;
    public float playingTime;
    public float finishTime;
    public float buildingHealthPercentageForBuilderWin;

    private float elapsedStateTime;
    private float OverallBuildingHealth;

    public enum GameState
    {
        Waiting,
        Playing,
        Finish
    }
    
    public void Start()
    {
        builderSpawns = new List<SpawnPoint>();
        kaijuSpawns = new List<SpawnPoint>();
        currentGameState = GameState.Waiting;
        elapsedStateTime = 0;
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

        structures = FindObjectsOfType<Structure>().ToList();
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
    
    public void Update()
    {
        elapsedStateTime += Time.deltaTime;
        switch (currentGameState)
        {
            case GameState.Waiting:
                if (elapsedStateTime > waitingTime)
                {
                    currentGameState = GameState.Playing;
                    elapsedStateTime = 0;
                    Debug.Log("Wait time is over");
                }
                break;
            case GameState.Playing:
                if (elapsedStateTime > playingTime)
                {
                    Debug.Log("Play time is over");
                    DetermineWinner();
                    currentGameState = GameState.Finish;
                    elapsedStateTime = 0;
                }
                break;
            case GameState.Finish:
                if (elapsedStateTime > finishTime)
                {
                    //whatever happens after displaying the score
                }
                break;
        }
    }

    void DetermineWinner()
    {
        var structuresCurrentHealth = structures.Sum(s => s.currentHealth);
        var structuresTotalHealth = structures.Sum(s => s.maxHealth);
        var overallBuildingHealth = 100 * (float)structuresCurrentHealth / structuresTotalHealth;
        if (overallBuildingHealth > buildingHealthPercentageForBuilderWin)
        {
            Debug.Log($"The builders won, structures overall health is at " +
                      $"{(int)overallBuildingHealth}");
        }
        else
        {
            Debug.Log($"The Kaiju won, structures overall health is at " +
                      $"{(int)overallBuildingHealth}");
        }
    }
    
}
