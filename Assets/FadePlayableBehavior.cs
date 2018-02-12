using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum FadeMode { In, Out };

// A behaviour that is attached to a playable
public class FadePlayableBehavior : PlayableBehaviour
{
    public CanvasGroup canvasGroup;
    public FadeMode fadeMode;
    public double deadzone = 0.05;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) {
		
	}

	// Called when the owning graph stops playing
	public override void OnGraphStop(Playable playable) {
		
	}

	// Called when the state of the playable is set to Play
	public override void OnBehaviourPlay(Playable playable, FrameData info) {
		
	}

	// Called when the state of the playable is set to Paused
	public override void OnBehaviourPause(Playable playable, FrameData info) {
		
	}

	// Called each frame while the state is set to Play
	public override void PrepareFrame(Playable playable, FrameData info) {
        double alpha = playable.GetTime() / playable.GetDuration();
        if (playable.GetTime() > playable.GetDuration() - deadzone)
        {
            alpha = 1;
        }
        if (fadeMode == FadeMode.Out)
        {
            alpha = 1 - alpha;
        }
        canvasGroup.alpha = (float) alpha;
	}
}
