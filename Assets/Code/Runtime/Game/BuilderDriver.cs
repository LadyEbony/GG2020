﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game;
using Code.Runtime.Game.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using EntityNetwork;

public class BuilderDriver : EntityBase, IAutoSerialize, IAutoDeserialize {

  // Any variable with [NetVar]
  // Will be sent to all clients based on the parameters
  // token = must be unique
  // alwaysReliable = if the message MUST be sent (not important for positions for example)
  // alwaysSend = if any network message is sent, this variable must be sent
  [Header("Networked Variables")]
  // updateMs = update timers
  [NetVar('p', false, false, 100)]
  public Vector3 position;

  [Header("Components")]
  public NavMeshAgent nva;
  public Player player;

  private LineRenderer lineRenderer;
  private float currentRange;
  
  public GameManager gameManager;

  public enum PlayerTypes
  {
    Fixer,
    Kaiju
  }

  public PlayerTypes playerType;

  [Header("Camera")]
  public float cameraDistance;

  void Start()
  {
    gameManager = GameObject.FindObjectOfType<GameManager>();
    switch (playerType)
    {
      case PlayerTypes.Fixer:
        player = gameObject.AddComponent<Fixer>();
        break;
      case PlayerTypes.Kaiju:
        player = gameObject.AddComponent<Monster>();
        break;
    }
    player.driver = this;
    lineRenderer = gameObject.GetComponent<LineRenderer>();
    lineRenderer.widthMultiplier = 0.2f;
    lineRenderer.startColor = Color.white;
    lineRenderer.endColor = Color.red;
    lineRenderer.positionCount = 2;

    if (isMine){
      var c = CameraThird.Instance;
      c.player = transform.Find("Camera Target");
      c.cameraDistance = cameraDistance;
    }
    
  }
  // Update is called once per frame
  void Update(){
    Debug.Log(PlayerProperties.localPlayer.ID);
    if (isMine){
      LocalUpdate();
    } else {
      RemoteUpdate();
    }
  }

  /// <summary>
  /// Called by the client that owns this object.
  /// </summary>
  void LocalUpdate(){
    // movement
    switch (gameManager.currentGameState)
    {
      case GameManager.GameState.Playing:
        if (player.IsAlive)
        {
          var steering = GameHelper.GetDirectionInput;
          nva.velocity = Vector3.MoveTowards(nva.velocity, nva.speed * steering, nva.acceleration * Time.deltaTime);
          position = transform.position;
          lineRenderer.SetPositions(new []
          {
            gameObject.transform.position,
            gameObject.transform.position + gameObject.transform.forward * player.currentRange
          });
          RaycastHit hit;
          Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, player.currentRange);
          if (hit.rigidbody != null)
          {
            if (hit.rigidbody.CompareTag("Structure"))
            {
              player.Target = hit.rigidbody.gameObject.GetComponent<Structure>();
            }
          }
          else
          {
            player.Target = null;
          }
        }
        break;
    }
    
  }

  [NetEvent('a')]
  void DoSomething(){
    // do something
  }

  /// <summary>
  /// Called by the client that doesn't own this object.
  /// </summary>
  void RemoteUpdate(){
    nva.destination = position;
  }
}
