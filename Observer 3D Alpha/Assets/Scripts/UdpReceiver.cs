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
    static string begin;
    private object messageLock = new object();
    private List<string> messageList = new List<string>();

    private IPEndPoint endPoint;
    private UdpClient client;
    private Thread receiveThread;
    private bool isOn;
    private int x=0, y=0;


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

    public Color findColor(string color){
        switch(color){
            case("green"):
                return Color.green;
            break;
            case("grey"):
                return Color.grey;
            break;
            default: return Color.black;
            break;
        }
    }

    public void Decode(string message){
        string[] tokens = message.Split('$');

        //for (int i=0;i<tokens.Length;i++) print(i + ": " + tokens[i]);
        int size = Convert.ToInt16(tokens[1]);
        tData.size = new Vector3(size/16, 2, size/16);
        int dimx = Convert.ToInt16(Math.Sqrt(size));
        int dimy = Convert.ToInt16(Math.Sqrt(size));
        Texture2D texture = new Texture2D(dimx, dimy);
        skr.material.mainTexture = texture;

        float[,] heights = tData.GetHeights(0, 0, tData.heightmapWidth, tData.heightmapHeight);

        int key = Convert.ToInt16(tokens[0]);
        switch(key){
            case(1):
            break;
            case(2): //CellularSpace
                string coverColor;
                Color color;
                int i=3;
                int attNumber;
                float h;
                for(int k=0;k<size;k++){
                    attNumber = Convert.ToInt16(tokens[i+1]);
                    int aux = i;
                    for(int j=0;j<attNumber;j++){
                        switch(tokens[aux+3]){
                            case("cover"):
                                Debug.Log(tokens[aux+5]);
                                coverColor = tokens[aux+5];
                                color = findColor(coverColor);
                                texture.SetPixel(x, y, color);
                                aux += 3;
                            break;
                            case("height"):
                                Debug.Log(tokens[aux+5]);
                                h = (float)(Convert.ToDouble(tokens[aux+5]));
                                //print("Altura "+ x + " " + y + " :" + h);
                                heights[y,x] = h;
                                aux += 3;
                            break;
                        }
                    }
                    y++;
                    if (y == texture.height){
                        y = 0;
                        x++;
                    }
                    i += 11;
                }
            break;
        }

        texture.Apply();
        SplatPrototype[] terrainTexture = new SplatPrototype[1];
        terrainTexture[0] = new SplatPrototype();
        terrainTexture[0].texture = texture;
        tData.splatPrototypes = terrainTexture;
        tData.SetHeights(0,0,heights);
    }

    private void OnApplicationQuit() {
        tData.splatPrototypes = null;
        /*float[,] heights = new float[1,1];
        heights[0,0] = 0.0f;
        tData.SetHeights(0,0, heights);*/
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
                int msgCount = 1;

                StringBuilder sb = new StringBuilder();
                foreach (char c in result) {
                    if ((c >= 'A' && c <= 'Z')||(c=='_')||(c == '$')||(c >= '0' && c <= '9')||(c >= 'a' && c <= 'z')){
                        sb.Append(c);
                    }
                }
                result = sb.ToString();
                string[] auxStr = result.Split('$');
                if (!result.Equals("COMPLETE_STATE")){
                    if (msgCount == 1){
                        int index = result.IndexOf("$$");
                        if (index >= 0) result = result.Substring(index+2);
                        result =  auxStr[1] + '$' + auxStr[3] + '$' + result;
                        msgCount = 0;
                    }
                    Debug.Log(result);
                    lock(messageLock){
                        messageList.Add(result);
                        result = "";
                    }
                }
            } catch (Exception err){
                Debug.Log("Nothing received");
            }
        }
    }
}
