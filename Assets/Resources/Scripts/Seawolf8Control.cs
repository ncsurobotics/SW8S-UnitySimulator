using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.Seawolf8Simulation;

public class Seawolf8Control : MonoBehaviour
{

    public Camera realSense;
    public float simSpeed = 0.05f;
    public float simAngleSpeed = 0.05f;
    public float realWorldScale = 1.0f / 10.0f;
    ROSConnection ros;

    //rate of movement
    float forwardRate = 0.0f;
    float strafeRate = 0.0f;
    float verticalRate = 0.0f;

    //rate of rotation
    float pitchRate = 0.0f;
    float rollRate = 0.0f;
    float yawRate = 0.0f;

    
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<OverrideRCInMsg>("mavros/rc/override", RCCallback);
        ros.RegisterPublisher<Float64Msg>("mavros/global_position/rel_alt");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //update motion based on RC rates
        this.gameObject.GetComponent<Rigidbody>().velocity = ((this.transform.up * verticalRate) + (this.transform.forward * forwardRate) + (this.transform.right * strafeRate)) * simSpeed;
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(pitchRate, -yawRate, rollRate) * simAngleSpeed;


        //send state data
        ros.Publish("mavros/global_position/rel_alt", new Float64Msg(this.transform.position.y * realWorldScale));
    }


    async void RCCallback(OverrideRCInMsg rates)
    {
        pitchRate = Mathf.Lerp(-1.0f, 1.0f, (rates.channels[0] - 1000.0f) / 1000.0f);
        rollRate = Mathf.Lerp(-1.0f, 1.0f, (rates.channels[1] - 1000f) / 1000f);
        verticalRate = Mathf.Lerp(-1.0f, 1.0f, (rates.channels[2] - 1000f) / 1000f);
        yawRate = Mathf.Lerp(-1.0f, 1.0f, (rates.channels[3] - 1000f) / 1000f);
        forwardRate = Mathf.Lerp(-1.0f, 1.0f, (rates.channels[4] - 1000f) / 1000f);
        strafeRate = Mathf.Lerp(-1.0f, 1.0f, (rates.channels[5] - 1000f) / 1000f);
        //Debug.Log(rates.channels[0] + " " + rates.channels[1] + " " + rates.channels[2] + " " + rates.channels[3] + " " + rates.channels[4] + " " + rates.channels[5]);
        //Debug.Log(pitchRate + " " + rollRate + " " + verticalRate + " " + yawRate + " " + forwardRate + " " + strafeRate);
    }
}
