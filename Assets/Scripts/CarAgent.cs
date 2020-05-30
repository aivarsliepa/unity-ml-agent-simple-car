using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarAgent : Agent
{
    private float throttle = 0f;
    private float steer = 0f;

    public List<WheelCollider> throttleWheels;
    public List<WheelCollider> steeringWheels;

    public float throttleStrenght = 20000f;
    public float maxSteer = 20f;

    //public Transform centerOfMass;
    public Transform target;

    Rigidbody rBody;

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach (WheelCollider wheel in throttleWheels)
        {
            wheel.motorTorque = throttle * throttleStrenght * Time.fixedDeltaTime;
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = maxSteer * steer;
        }

        // Fell off platform or upside down
        if (transform.localPosition.y < 0 || Vector3.Dot(transform.up, Vector3.down) > 0)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            SetReward(1f);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
        
        // Agent rotation
        sensor.AddObservation(transform.rotation.y);
        sensor.AddObservation(transform.rotation.z);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        steer = vectorAction[0];
        throttle = vectorAction[1];
    }

    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < 0)
        {
            // If the Agent fell, zero its momentum
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }

        MoveTarget();
    }

    void MoveTarget()
    {
        // Move the target to a new spot
        target.localPosition = new Vector3(Random.Range(-40, 40),
                                           0.5f,
                                           Random.Range(-40, 40));
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }
}
