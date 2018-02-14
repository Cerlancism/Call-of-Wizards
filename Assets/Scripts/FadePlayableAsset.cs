using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class FadePlayableAsset : PlayableAsset
{
    public ExposedReference<CanvasGroup> canvasGroup;
    public FadeMode fadeMode;

    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
		ScriptPlayable<FadePlayableBehavior> playable = ScriptPlayable<FadePlayableBehavior>.Create(graph);
        FadePlayableBehavior behavior = playable.GetBehaviour();
        behavior.canvasGroup = canvasGroup.Resolve(graph.GetResolver());
        behavior.fadeMode = fadeMode;
        return playable;
	}
}
