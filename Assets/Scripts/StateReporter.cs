using System.Collections;
using System.Collections.Generic;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

public class StateReporter : RunAbleThread
{
    private RLPLanner rlPlanner;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InjectRLPlanner(RLPLanner inRLPlanner)
    {
        rlPlanner = inRLPlanner;
    }

    protected override void Run()
    {
        ForceDotNet.Force();

        if (rlPlanner == null)
        {
            NetMQConfig.Cleanup();
        }

        using (RequestSocket client = new RequestSocket())
        {
            Debug.Log("State Reporter is Running");

            client.Connect("tcp://localhost:5555");

            // TODO - this is to be deprecated; position of 0 passed in to prevent compile error
            float[] X = rlPlanner.GetDNNStateRepresentation(Vector3.zero);

            foreach (float feature in X)
            {
                Debug.Log("Feature: " + feature.ToString());
                client.SendFrame(feature.ToString());
            }

            byte[] message = null;
            bool gotMessage = false;
            while (Running)
            {
                gotMessage = client.TryReceiveFrameBytes(out message); // this returns true if it's successful
                if (gotMessage) break;
            }

            if (gotMessage) Debug.Log("Received bytes");
        }

        NetMQConfig.Cleanup();
    }
}
