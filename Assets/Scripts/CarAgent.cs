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

    private bool didFallOffPlatform = false;

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

        didFallOffPlatform = transform.localPosition.y < 0;
        var isUpsideDown = Vector3.Dot(transform.up, Vector3.down) > 0;
        if (didFallOffPlatform || isUpsideDown)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            AddReward(1f);
            EndEpisode();
        }
    }

    // debug observations
    //public Vector3 targetLocalPosition;
    //public Vector3 localPosition;
    //public float rotationY;
    //public float rotationZ;
    //public float velocityX;
    //public float velocityZ;
    //public Vector3 angularVelocity;

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        //sensor.AddObservation(target.localPosition);
        //sensor.AddObservation(transform.localPosition);
        
        // Agent rotation
        //sensor.AddObservation(transform.rotation.eulerAngles.y);
        //sensor.AddObservation(transform.rotation.eulerAngles.z);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        // Agent angular velocity (steering and rolling)
        //sensor.AddObservation(rBody.angularVelocity.y);
        //sensor.AddObservation(rBody.angularVelocity.z);

        //targetLocalPosition = target.localPosition;
        //localPosition = transform.localPosition;

        //rotationY = transform.rotation.eulerAngles.y;
        //rotationZ = transform.rotation.eulerAngles.z;

        //velocityX = rBody.velocity.x;
        //velocityZ = rBody.velocity.z;

        //angularVelocity = rBody.angularVelocity;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        steer = vectorAction[0];
        throttle = vectorAction[1];

        AddReward(-1f / MaxStep);
    }

    public override void OnEpisodeBegin()
    {
        if (didFallOffPlatform)
        {
            didFallOffPlatform = false;

            // If the Agent fell, zero its momentum
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }

        var currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        MoveTarget();
    }

    void MoveTarget()
    {
        // Move the target to a new spot
        target.localPosition = new Vector3(Random.Range(-30, 30),
                                           0.5f,
                                           Random.Range(-30, 30));
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }
}
