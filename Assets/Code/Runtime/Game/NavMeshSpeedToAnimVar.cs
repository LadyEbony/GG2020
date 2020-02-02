using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshSpeedToAnimVar : MonoBehaviour
{
  public NavMeshAgent navMeshAgent;
  public Animator animator;
  public string animatorVariable = "WalkSpeed";

  private void Update()
  {
    if(navMeshAgent && animator)
    {
      animator.SetFloat(animatorVariable, navMeshAgent.velocity.magnitude / navMeshAgent.speed);
    }
  }
}
