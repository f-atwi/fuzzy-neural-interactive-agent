// creation: 22-jun-2018 pierre.chevaillier@enib.fr
// revision: 21-aug-2018 pierre.chevaillier@enib>.fr orientations in radians
// revision: 22-aug-2018 pierre.chevaillier@benib<.fr change eulerAngles
// revision: 30-sep-2018 pierre.chevailler@enib.fr name of the agents
// revision: 22-jan-2019 pierre.chevailler@enib.fr timestamp (not yet used)
// revision: 05-nov-2021 pierre.chevailler@enib.fr named of the logged agents

using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class AgentsTransformLogger: MonoBehaviour {
    public IList<GameObject> agentsToLog = new List<GameObject>();

    protected FileInfo fileInfo;
    protected StreamWriter fileWritter;

    private DateTime originOfTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private long timePrecision = 50;
    float elapsedTime; // = .0f;
    float samplingPeriod = 1.0f / 4.0f;
    float timeFromLastRecord; // = .0f;

    // Use this for initialization
    void Start() {

        DefineAgentsToLog();
        CreateLogFile();
        WriteHeader();
    }

    void DefineAgentsToLog() {
        this.agentsToLog.Add(GameObject.Find("Agent"));
        this.agentsToLog.Add(GameObject.Find("SensitiveAgent"));
    }

    void CreateLogFile() {
        string path = Application.dataPath
                                 + "/Logs/agentsPositions_"
                                 + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
                                 + ".csv";
        this.fileInfo = new FileInfo(path);
        this.fileWritter = this.fileInfo.CreateText();
    }

    void WriteHeader() {
        string header = "time";
        for (int i = 0; i < this.agentsToLog.Count; i++)
            header = header + ";pos_x;pos_y;pos_z;rot_x;rot_y;rot_z";
        for (int i = 0; i < this.agentsToLog.Count; i++)
            header = header + ";v_lin;v_ang";
        this.fileWritter.WriteLine(header);
    }

    // Update is called once per frame
    void Update() {
        timeStamp();
        elapsedTime += Time.deltaTime;
        timeFromLastRecord += Time.deltaTime;
        if (timeFromLastRecord > samplingPeriod) {
            WriteRecord();
            timeFromLastRecord = .0f;
        }
    }

    long timeStamp() {
        DateTime now = DateTime.UtcNow;
        TimeSpan timeSinceOrigin = now.Subtract(originOfTime);
        long millisSinceOrigin = (Int64)timeSinceOrigin.TotalMilliseconds;
        // controlled by comparison to https://currentmillis.com
        millisSinceOrigin = (millisSinceOrigin / this.timePrecision) * this.timePrecision;
        //Debug.Log("Unix time like : " + millisSinceOrigin.ToString());
        return millisSinceOrigin;
    }

    void WriteRecord() {
        string line = elapsedTime.ToString();
        foreach (GameObject agent in this.agentsToLog) {
            Vector3 angles = agent.transform.eulerAngles;
            for (int j = 0; j < 3; j++) {
                if (angles[j] > 180f)
                    angles[j] -= 360f;
            }
            angles *= Mathf.Deg2Rad;
            line = line
                + ";" + agent.transform.position.x.ToString()
                + ";" + agent.transform.position.y.ToString()
                + ";" + agent.transform.position.z.ToString()
                + ";" + angles.x.ToString()
                + ";" + angles.y.ToString()
                + ";" + angles.z.ToString();
        }

        foreach (GameObject agent in this.agentsToLog) {
            KinematicModel km = agent.GetComponent(typeof(KinematicModel)) as KinematicModel;
            if (km != null) {
                float angularVelocity = km.angularVelocity * Mathf.Deg2Rad;
                line = line
                    + ";" + km.linearVelocity.ToString() + ";" + angularVelocity.ToString();
            } else {
                line = line + ";;";
            }
        }
        line = line + ";" + Component.FindObjectOfType<SensitiveAgentBehavior>().currentState.value();
        this.fileWritter.WriteLine(line);
    }

    void OnApplicationQuit() {
        Debug.Log("Application ending after " + Time.time + " seconds");
        Debug.Log("File  " + this.fileInfo.FullName);
        this.fileWritter.Close();
    }

}
