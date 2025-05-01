using UnityEngine;
using Unity.MLAgents;

public class ForceNormalTimeScale : MonoBehaviour
{
    void Update()
    {
        if (Academy.Instance.IsCommunicatorOn) // Only when connected to training (mlagents-learn)
        {
            Time.timeScale = 1f; // Force normal time speed
        }
    }
}
