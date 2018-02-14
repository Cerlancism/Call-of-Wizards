using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetProgressButton : MonoBehaviour {
    public CheckpointManager checkpointManager;

    public void ResetProgress()
    {
        checkpointManager.ResetProgress();
    }
}
