using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgent2D : Agent
{
    private Rigidbody2D rBody;
    public Transform Target;

    public float forceMultiplier = 3f;           // Reduced to avoid over-acceleration
    public float maxSpeed = 5f;                  // Speed cap
    public float fallThresholdY = -1f;           // Y position considered "falling"

    void Start()
    {
        rBody = GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset velocity and position if agent fell
        if (this.transform.localPosition.y < fallThresholdY)
        {
            rBody.linearVelocity = Vector2.zero;
            this.transform.localPosition = new Vector2(0f, 0.5f);
        }

        // Move the target to a random position within bounds
        Target.localPosition = new Vector2(Random.Range(-4f, 4f), 0.5f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Add agent velocity
        sensor.AddObservation(rBody.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Get movement inputs
        Vector2 controlSignal = Vector2.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.y = actionBuffers.ContinuousActions[1];

        // Apply force
        rBody.AddForce(controlSignal * forceMultiplier);

        // Clamp speed to avoid spinning out of control
        rBody.linearVelocity = Vector2.ClampMagnitude(rBody.linearVelocity, maxSpeed);

        // Reward if agent gets close to target
        float distanceToTarget = Vector2.Distance(this.transform.localPosition, Target.localPosition);
        if (distanceToTarget < 1.0f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // End episode if agent falls
        if (this.transform.localPosition.y < fallThresholdY)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // Filter out tiny inputs to reduce jitter
        if (Mathf.Abs(x) < 0.1f) x = 0;
        if (Mathf.Abs(y) < 0.1f) y = 0;

        continuousActionsOut[0] = x;
        continuousActionsOut[1] = y;
    }
}
