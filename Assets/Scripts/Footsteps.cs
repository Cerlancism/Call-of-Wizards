using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour {
    public AudioSource rightFootAudioSource;
    public AudioSource leftFootAudioSource;
    public AudioClip[] footstepSounds;

    public CharacterPhysics characterPhysics;

    public void OnFootstepRight(float volume)
    {
        OnFootstep(rightFootAudioSource, volume);
    }

    public void OnFootstepLeft(float volume)
    {
        OnFootstep(leftFootAudioSource, volume);
    }

    private void OnFootstep(AudioSource source, float volume)
    {
        if (characterPhysics.IsGroundedLenient)
        {
            AudioClip randomFootstepSound = footstepSounds[Random.Range(0, footstepSounds.Length - 1)];
            source.PlayOneShot(randomFootstepSound, volume);
        }
    }
}
