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
    float myHorizontal = 0;

    ArcadeKart kart;

    public override void Initialize() {
        kart = GetComponent<ArcadeKart>();
    }

    public override void OnEpisodeBegin() {
        Transform spawns = GameObject.Find("SpawnPoints").transform;
        int i = Random.Range(0, spawns.childCount);
        Transform spawn = spawns.GetChild(i).transform;

        transform.position = spawn.position;
        transform.rotation = spawn.rotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // switch up stats
        kart.baseStats.TopSpeed = Random.Range(10.0f, 50.0f);
        kart.baseStats.Acceleration = Random.Range(3.0f, 15.0f);
        kart.baseStats.Grip = Random.Range(.70f, 1.20f);
        kart.baseStats.Steer = Random.Range(3.0f, 7.0f);

        Debug.Log(System.String.Format("Track {0} CC {1}", i, kart.baseStats.TopSpeed));
    }

    public override void CollectObservations(VectorSensor sensor) {
        // ray casts
        for (int k = 0; k < sensores.childCount; k++) {
            Transform sensorK = sensores.GetChild(k);
            RaycastHit hit;
            if (Physics.Raycast(sensorK.position, sensorK.forward, out hit, 100.0f)) {
                sensor.AddObservation(hit.distance / 100.0f);
            } else {
                sensor.AddObservation(1.0f);
            }
        }

        // rigid body
        sensor.AddObservation(GetComponent<Rigidbody>().velocity.z);
        sensor.AddObservation(GetComponent<Rigidbody>().angularVelocity.y);

        // kart stats
        sensor.AddObservation(kart.baseStats.TopSpeed);
        sensor.AddObservation(kart.baseStats.Acceleration);
        sensor.AddObservation(kart.baseStats.Grip);
        sensor.AddObservation(kart.baseStats.Steer);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        float action = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);

        if (action < -0.5f)
        {
            myHorizontal = (action + 0.5f) * 2.0f;
        }
        else if (action > 0.5f)
        {
            myHorizontal = (action - 0.5f) * 2.0f;
        }
        else
        {
            myHorizontal = 0.0f;
            AddReward(0.1f);
        }

        AddReward(0.1f);
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
        // crash
        EndEpisode();
    }
}
