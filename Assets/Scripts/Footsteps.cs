using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FootstepSet
{
    public string name;
    public AudioClip[] footstepSounds;
}

public class Footsteps : MonoBehaviour {
    public AudioSource rightFootAudioSource;
    public AudioSource leftFootAudioSource;
    public FootstepSet[] footstepSets;
    public string defaultFootstepSet;

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
        Transform ground = characterPhysics.GroundLenient;
        if (ground != null)
        {
            FootstepMaterial footstepMaterial = ground.GetComponent<FootstepMaterial>();

            FootstepSet footstepSet;
            if (footstepMaterial != null)
            {
                footstepSet = GetFootstepSet(footstepMaterial.material);
            }
            else
            {
                footstepSet = GetFootstepSet(defaultFootstepSet);
            }

            AudioClip[] footstepSounds = footstepSet.footstepSounds;
            AudioClip randomFootstepSound = footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length - 1)];
            source.PlayOneShot(randomFootstepSound, volume);
        }
    }

    private FootstepSet GetFootstepSet(string name)
    {
        foreach (FootstepSet footstepSet in footstepSets)
        {
            if (footstepSet.name == name)
            {
                return footstepSet;
            }
        }

        return null;
    }
}
