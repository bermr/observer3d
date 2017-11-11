using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class UdpReceiver:MonoBehaviour {

    public int Port = 55000;
    public GameObject ball;
    public BallController bc;
    public Terrain terrain;
    public TerrainData tData;
    private object messageLock = new object();
    private List<string> messageList = new List<string>();

    private IPEndPoint endPoint;
    private UdpClient client;
    private Thread receiveThread;
    private bool isOn;

    private void Start(){
        bc = ball.GetComponent<BallController>();
        tData = Terrain.activeTerrain.terrainData;
        Init(Port);
    }

    public void FixedUpdate(){
        if(messageList.Count > 0){
            lock(messageLock){
                Decode(messageList[0]);
                messageList.Clear();
            }
        }
    }

    public void Decode(string message){
        string[] tokens = message.Split('$');

        for (int i=0;i<tokens.Length;i++) print(i + tokens[i]);

        int paramNumber = Convert.ToInt16(tokens[2]);
        int type = Convert.ToInt16(tokens[3]);

        switch(type){
            case(1): //cell
                string attribute = tokens[7];
                switch(attribute){
                    case("cover"):
                        string txt = tokens[17];
                        SplatPrototype[] terrainTexture = new SplatPrototype[1];
                        terrainTexture[0] = new SplatPrototype();
                        terrainTexture[0].texture = (Texture2D)Resources.Load(txt, typeof(Texture2D));
                        tData.splatPrototypes = terrainTexture;
                    break;
                    case("height"):
                        float[,] heights = new float[1,1];
                        heights[0,0] = (float) Convert.ToDouble(tokens[19]);
                        tData.SetHeights(0,0,heights);
                    break;
                }
            break;
            case(2):
            break;
        }
    }

    private void OnApplicationQuit() {
        tData.splatPrototypes = null;
        float[,] heights = new float[1,1];
        heights[0,0] = 0.0f;
        tData.SetHeights(0,0, heights);
        Kill();
    }

    public void Init(int _port) {
        Port = _port;
        isOn = true;

        client = new UdpClient(55000);
        client.Client.Blocking = false;
        endPoint = new IPEndPoint(IPAddress.Any, Port);

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void Kill() {
        isOn = false;
        if (client != null) {
            client.Close();
            client = null;
        }
        if (receiveThread != null) {
            receiveThread.Abort();
            receiveThread = null;
        }
    }

    private void ReceiveData(){
        while (isOn) {
            try {
                byte[] data = client.Receive(ref endPoint);
                string result = Encoding.UTF8.GetString(data);

                StringBuilder sb = new StringBuilder();
                foreach (char c in result) {
                    if ( (c=='_') || (c == '$') || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')){
                        sb.Append(c);
                    }
                }

                result = sb.ToString();
                Debug.Log("ReceiveData: Data=" + result);

                if (result.Equals("COMPLETE_STATE"))    Kill();


                lock(messageLock){
                    messageList.Add(result);
                    result = "";
                }
            } catch (Exception err) {
                Debug.Log("ReceiveData: nothing");
            }
        }
    }
}
