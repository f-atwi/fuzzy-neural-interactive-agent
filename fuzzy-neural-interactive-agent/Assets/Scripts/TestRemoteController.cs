// creation: Mathieu Quentel
// revision: 23-aug-2018 pierre.chevaillier@enib.fr data exchanged as bytes

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TestRemoteController : MonoBehaviour
{
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
        m_networkDataMutex = new Mutex();
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress broadcast = IPAddress.Parse(m_serverAdressString);
        m_endPoint = new IPEndPoint(broadcast, m_serverPort);
        m_receiver = new UdpClient(m_clientPort);
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

            m_socket.SendTo(dataToSend, m_endPoint);
            m_receiver.BeginReceive(UpdateAgent, m_receiver);

            m_src = m_target;
            m_networkDataMutex.WaitOne();
            m_gotAnswer = false;  
            m_target = m_newAgentPosition;
            m_networkDataMutex.ReleaseMutex();
        }

        gameObject.transform.position = Vector3.Lerp(m_src, m_target, m_samplesTimeCounter / m_recordingPeriod);
    }

    byte[] FormatDataToSend() {
        // agent's transform (2 vector3) and interactingObject's transform (2 vector3)
        const int nValues = 2 * 3;
        const int dataSize = nValues * nBytesFloat;

        Vector3 angles = gameObject.transform.eulerAngles;
        for (int j = 0; j < 3; j++)
        {
            if (angles[j] > 180f)
                angles[j] -= 360f;
        }
        angles *= Mathf.Deg2Rad;

        float[] values = {gameObject.transform.position.x, 
            gameObject.transform.position.y,
            gameObject.transform.position.z,
            angles.x, angles.y, angles.z};
        
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

        Vector3 position = Vector3.zero;
        for (int j = 0; j < 3; j++)
        {
            position[j] = BitConverter.ToSingle(receivedBytes, j * nBytesFloat);
        }

        m_networkDataMutex.WaitOne();
        m_newAgentPosition = position;
        m_gotAnswer = true;
        m_networkDataMutex.ReleaseMutex();
    }
}
