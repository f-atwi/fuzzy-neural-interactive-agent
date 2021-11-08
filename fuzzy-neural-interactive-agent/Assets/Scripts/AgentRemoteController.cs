// creation: Mathieu Quentel
// revision: 23-aug-2018 pierre.chevaillier@enib.fr data exchanged as bytes
// revision: 24-aug-2018 pierre.chevaillier@enib.fr interactiveObject - agent's architecture
// todos:
// some obfuscated code ... Need to be clean up

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class AgentRemoteController : MonoBehaviour
{

    KinematicModel kinematicModel;
    InteractiveObject interactingObject;

    float m_samplesTimeCounter = 0.0f;
    public float m_recordingPeriod = 0.25f;

    public string m_serverAdressString = "127.0.0.1";
    public int m_serverPort = 5005;
    public int m_clientPort = 5006;

    Socket m_socket;
    IPEndPoint m_endPoint;
    UdpClient m_receiver;

    private const int nBytesFloat = 4;
 
    Vector3 m_src = Vector3.zero;
    Vector3 m_target = Vector3.zero;

    Vector3 m_newAgentPosition = Vector3.zero;
    bool m_gotAnswer = true;
    Mutex m_networkDataMutex;

    // Use this for initialization
    void Start ()
    {
        interactingObject = GetComponent(typeof(InteractiveObject)) as InteractiveObject;
        DefineKinematicModel();

        m_networkDataMutex = new Mutex();
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress broadcast = IPAddress.Parse(m_serverAdressString);
        m_endPoint = new IPEndPoint(broadcast, m_serverPort);
        m_receiver = new UdpClient(m_clientPort);
    }
	
    protected void DefineKinematicModel()
    {
        kinematicModel = gameObject.AddComponent(typeof(KinematicModel)) as KinematicModel;
        kinematicModel.DefineLinearVelocity(0.0f, 0.0f, 1.0f); // (min, cur, max)
        kinematicModel.DefineAngularVelocity(-50.0f, 0.0f, 50.0f); // (min, cur, max)
        kinematicModel.currentTransform = gameObject.transform;
    }

	// Update is called once per frame
	void Update ()
    {
        m_samplesTimeCounter += Time.deltaTime;

        bool gotAnswer;
        m_networkDataMutex.WaitOne();
        gotAnswer = m_gotAnswer;
        m_networkDataMutex.ReleaseMutex();

        if (m_samplesTimeCounter >= m_recordingPeriod && gotAnswer)
        {
            byte[] dataToSend = FormatDataToSend();
            print(dataToSend);

            m_socket.SendTo(dataToSend, m_endPoint);
            m_receiver.BeginReceive(UpdateAgent, m_receiver);

            m_src = m_target;
            m_networkDataMutex.WaitOne();
            m_gotAnswer = false;  
            m_target = m_newAgentPosition;
            m_networkDataMutex.ReleaseMutex();
        }

        // --- Action
        kinematicModel.Turn();
        kinematicModel.MoveForward();

        // --- Internal state update
        kinematicModel.UpdateState();

        //gameObject.transform.position = Vector3.Lerp(m_src, m_target, m_samplesTimeCounter / m_recordingPeriod);
    }

    byte[] FormatDataToSend() {
        // agent's transform (2 vector3) and interactingObject's transform (2 vector3)
        const int nValues = 2 * 2 * 3;
        const int dataSize = nValues * nBytesFloat;

        // Agent
        Vector3 agentOrientation = gameObject.transform.eulerAngles;
        for (int j = 0; j < 3; j++)
        {
            if (agentOrientation[j] > 180f)
                agentOrientation[j] -= 360f;
        }
        agentOrientation *= Mathf.Deg2Rad;

        // InteractiveObject
        Vector3 interactingObjectOrientation = interactingObject.objectCurrentTransform.eulerAngles;
        for (int j = 0; j < 3; j++)
        {
            if (interactingObjectOrientation[j] > 180f)
                interactingObjectOrientation[j] -= 360f;
        }
        interactingObjectOrientation *= Mathf.Deg2Rad;

        float[] values = {
            gameObject.transform.position.x, 
            gameObject.transform.position.y,
            gameObject.transform.position.z,
            agentOrientation.x, 
            agentOrientation.y, 
            agentOrientation.z,
            interactingObject.objectCurrentTransform.position.x,
            interactingObject.objectCurrentTransform.position.y,
            interactingObject.objectCurrentTransform.position.z,
            interactingObjectOrientation.x,
            interactingObjectOrientation.y,
            interactingObjectOrientation.z
        };
        
        Byte[] result = new Byte[dataSize];
        Byte[] buffer;
        for (int i = 0; i < nValues; i++) {
            buffer = BitConverter.GetBytes(values[i]);
            for (int j = 0; j < nBytesFloat; j++)
                result[(i * nBytesFloat) + j] = buffer[j];
        }
        return result;
    }


    void UpdateAgent(IAsyncResult ar)
    {
        UdpClient c = (UdpClient)ar.AsyncState;
        IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);

        m_networkDataMutex.WaitOne();

        kinematicModel.linearVelocity = BitConverter.ToSingle(receivedBytes, 0);
        kinematicModel.angularVelocity = BitConverter.ToSingle(receivedBytes, nBytesFloat) * Mathf.Rad2Deg;
        Debug.Log("Vlin: " + kinematicModel.linearVelocity);
        Debug.Log("Vang: " + kinematicModel.angularVelocity);
        m_gotAnswer = true;

        m_networkDataMutex.ReleaseMutex();
    }
}
