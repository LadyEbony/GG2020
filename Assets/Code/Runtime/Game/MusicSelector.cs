using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatically transitions between AudioSources
/// </summary>
public class MusicSelector : MonoBehaviour
{
  public int currentAudioIndex = 0;
  private int prevAudioIndex = 0;
  public List<AudioSource> audioSources;

  private AudioSource currentAudio = null;

  // Update is called once per frame
  void Update()
  {
    if(currentAudioIndex != prevAudioIndex)
    {
      TransitionAudioSources();
    }
    prevAudioIndex = currentAudioIndex;
  }

  void TransitionAudioSources()
  {
    if (currentAudio != null)
    {
      currentAudio.Stop();
    }

    currentAudio = audioSources[currentAudioIndex];

    if (currentAudio != null)
    {
      currentAudio.Play();
    }
  }
}
