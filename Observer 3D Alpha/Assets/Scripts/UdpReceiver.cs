using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class UdpReceiver:MonoBehaviour {

    public int Port = 55000;
    public Terrain terrain;
    public TerrainData tData;
    public Renderer skr;
    private object messageLock = new object();
    private List<string> messageList = new List<string>();

    private IPEndPoint endPoint;
    private UdpClient client;
    private Thread receiveThread;
    private bool isOn;

    private void Start(){
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

        //for (int i=0;i<tokens.Length;i++) print(i + ": " + tokens[i]);

        int size = Convert.ToInt16(tokens[0]);
        tData.size = new Vector3(size/16, 1, size/16);

        Texture2D texture = new Texture2D(size, size);
        skr.material.mainTexture = texture;

        for (int y = 0; y < texture.height; y++){
            for (int x = 0; x < texture.width; x++){
                Color color;
                if (y < x) color = Color.grey;
                else color = Color.green;
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        SplatPrototype[] terrainTexture = new SplatPrototype[1];
        terrainTexture[0] = new SplatPrototype();
        terrainTexture[0].texture = texture;
        tData.splatPrototypes = terrainTexture;

        /*
        int j = 2;
        string key;
        for (int i=0;i<size;i++){
            key = tokens[j];
            //int dataType = Convert.ToInt16(tokens[j]);
            switch(key){
                case("1"): //cell
                    print(tokens[j+1]);
                    int attNumber = Convert.ToInt32(tokens[j+1]);
                    for (int k=j;k<j+attNumber; k++){
                        string attribute = tokens[k+3];
                        int value = Convert.ToInt16(tokens[k+5]);
                        switch(attribute){
                            case("cover"):
                                print("cover");
                                print(value);
                                /*string txt = tokens[17];
                                SplatPrototype[] terrainTexture = new SplatPrototype[1];
                                terrainTexture[0] = new SplatPrototype();
                                terrainTexture[0].texture = (Texture2D)Resources.Load(txt, typeof(Texture2D));
                                tData.splatPrototypes = terrainTexture;
                            break;
                            case("height"):
                                print("height");
                                print(value);
                                /*float[,] heights = new float[1,1];
                                heights[0,0] = (float) Convert.ToDouble(tokens[19]);
                                tData.SetHeights(0,0,heights);
                            break;
                    }
                    }
                break;
                case("2"):
                break;

                default:
                break;
            }
            j++;
        }*/
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
                    if ((c >= 'A' && c <= 'Z')||(c=='_')||(c == '$')||(c >= '0' && c <= '9')||(c >= 'a' && c <= 'z')){
                        sb.Append(c);
                    }
                }

                result = sb.ToString();
                string[] aux = result.Split('$');
                if (!result.Equals("COMPLETE_STATE")){
                    int i = result.IndexOf("$$");
                    if (i >= 0) result = result.Substring(i+2);
                }
                result = aux[3] + '$' + result;
                Debug.Log(result);
                lock(messageLock){
                    messageList.Add(result);
                    result = "";
                }
            } catch (Exception err){
                Debug.Log("Nothing received");
            }
        }
    }
}
