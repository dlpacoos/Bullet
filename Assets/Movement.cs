using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Movement : Agent
{
    public float speed = 0.5f;
    private Rigidbody2D rb;
    private Vector2 input;
    public override void OnEpisodeBegin(){

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input.Normalize();
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent positions
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.y);
    }
    private float rewardTimer = 0f;

    public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Actions, size = 2 (x and y forces in 2D)
            Vector2 controlSignal = Vector2.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.y = actionBuffers.ContinuousActions[1];

            // Reward every 1 second
            rewardTimer += Time.deltaTime;
            if (rewardTimer >= 1f)
            {
                SetReward(0.1f); // small positive reward
                rewardTimer = 0f;
            }
            

        }

}
