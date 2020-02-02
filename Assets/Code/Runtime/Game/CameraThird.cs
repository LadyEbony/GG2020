using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraThird : MonoBehaviour
{
  [Header("Player Tracking")]
  public Transform player;                      // player transform to track
  public float cameraDistance = 3f;             // distance from player
  public float smoothTime = 0.3f;               // camera position smoothing
  private Vector3 cameraVelocity = Vector3.zero;

  [Header("Wall Checking")]
  public float minDistanceFromWall = 0.0625f;   // minimum distance from the wall
  public LayerMask cameraLayerMask;             // objects that the camera ray collides with

  [Header("Rotation")]
  public float pitchClamp = 80f;                // the pitch of the camera will stay between +/- pitchClamp
  public float pitchSensitivity = 5;            // pitch multiplier
  public float yawSensitivity = 5;              // yaw multiplier
  private float rotX = 0f;                      // the current pitch
  private float rotY = 0f;                      // the current yaw


  private void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;

    Vector3 rot = transform.localRotation.eulerAngles;
    rotX = rot.x;
    rotY = rot.y;
  }


  private void LateUpdate()
  {
    transform.rotation = ComputeRotation();

    if (player)
    {
      transform.position = ComputePosition();
    }
  }


  private Quaternion ComputeRotation()
  {
    float horizontal = -Input.GetAxis("Mouse Y");
    float vertical = Input.GetAxis("Mouse X");

    rotX += horizontal * yawSensitivity;
    rotY += vertical * pitchSensitivity;

    rotX = Mathf.Clamp(rotX, -pitchClamp, pitchClamp);
    return Quaternion.Euler(rotX, rotY, 0f);
  }


  private Vector3 ComputePosition()
  {
    var playerPos = player.position;
    var dir = transform.rotation * Vector3.back;

    float dist;
    if (Physics.Raycast(playerPos, dir, out RaycastHit hit, cameraDistance, cameraLayerMask))
    {
      // stop right before a wall (or else, the wall doesn't render)
      dist = hit.distance - minDistanceFromWall;
    }
    else
    {
      dist = cameraDistance;
    }

    Vector3 camPos = playerPos + dir * dist;
    return camPos;
  }
}