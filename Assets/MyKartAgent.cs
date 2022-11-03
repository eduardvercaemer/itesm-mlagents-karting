using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using KartGame.KartSystems;

public class MyKartAgent : Agent, IInput
{
    public Transform sensores;
    public float speed = 1.0f;
    public float angularSpeed = 1.0f;
    float myHorizontal = 0;
    Vector3 initialPosition;
    Quaternion initialRotation;

    public override void Initialize() {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public override void OnEpisodeBegin() {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor) {
        for (int k = 0; k < sensores.childCount; k++) {
            Transform sensorK = sensores.GetChild(k);
            RaycastHit hit;
            if (Physics.Raycast(sensorK.position, sensorK.forward, out hit, 100.0f)) {
                sensor.AddObservation(hit.distance);
            } else {
                sensor.AddObservation(100.0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions) {
        float action = angularSpeed * Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);
        myHorizontal = action;
        SetReward(0.1f);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
    }

    public InputData GenerateInput() {
        return new InputData {
            Accelerate = true,
            Brake = false,
            TurnInput = myHorizontal
        };
    }

    private void OnCollisionEnter(Collision collision) {
        SetReward(-1.0f);
        EndEpisode();
    }
}
