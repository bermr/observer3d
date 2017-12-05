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
        Init(Port);
    }

    public void FixedUpdate(){
        if (messageList.Count > 0){
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
            case("white"):
                return Color.white;
            break;
            case("red"):
                return Color.red;
            break;
            case("black"):
                return Color.black;
            break;
            case("alive"):
                return Color.grey;
            break;
            case("dead"):
                return Color.black;
            break;
            default: return Color.black;
            break;
        }
    }

    public void Decode(string messageReceived){
        string[] tokens = messageReceived.Split('$');

        //for (int i=0;i<tokens.Length;i++) print(i + ": " + tokens[i]);

        int size = Convert.ToInt16(tokens[1]);
        tData.size = new Vector3(size/16, 20, size/16);
        int dimx = Convert.ToInt16(Math.Sqrt(size));
        int dimy = Convert.ToInt16(Math.Sqrt(size));
        Texture2D texture = new Texture2D(dimx - 1, dimy - 1);
        skr.material.mainTexture = texture;

        int x=0, y=0, x1=0, y1=0;
        tData.heightmapResolution = (dimx+dimy)/2 - 1;
        Debug.Log(tData.heightmapWidth + " " + tData.heightmapHeight + " " + texture.width + " " + texture.height);
        float[,] heights = tData.GetHeights(0, 0, tData.heightmapWidth, tData.heightmapHeight);
        int count=0;
        int key = Convert.ToInt16(tokens[0]);
        switch(key){
            case(1):
            break;
            case(2): //CellularSpace
                string coverColor;
                Color color;
                int i=3;
                int n;
                int attNumber;
                string hei;
                attNumber = Convert.ToInt16(tokens[i+1]);
                float h;
                for(int k=0;k<size;k++){
                    int aux = i;
                    try{
                        n = Convert.ToInt16(tokens[i+1]);
                        if (n != attNumber) i--;
                        else{
                            for(int j=0;j<attNumber;j++){
                                switch(tokens[aux+3]){
                                    case("cover"):
                                        coverColor = tokens[aux+5];
                                        color = findColor(coverColor);
                                        texture.SetPixel(y, x, color);
                                        aux += 3;
                                    break;
                                    case("height"):
                                        try{
                                            hei = tokens[aux+5];
                                            hei = (hei.Length < 5) ? hei  : hei.Substring(0,5);
                                            hei = "0." + hei;
                                            h = 10 * ((float) Convert.ToDouble(hei));
                                        } catch(Exception err){
                                            h = 0.0f;
                                        }
                                        heights[y1,x1] = h;
                                        aux += 3;
                                    break;
                                }
                            }
                        y++;
                        if (y == texture.height){
                            y = 0;
                            x++;
                        }
                        y1++;
                        if (y1 == tData.heightmapHeight){
                            y1 = 0;
                            x1++;
                        }
                    }
                    } catch(Exception err){ i--; }
                    i += attNumber*3 + 5; //8 ou 11
                }
            break;
        }
        texture.Apply();
        SplatPrototype[] terrainTexture = new SplatPrototype[1];
        terrainTexture[0] = new SplatPrototype();
        terrainTexture[0].texture = texture;
        tData.splatPrototypes = terrainTexture;
        tData.SetHeightsDelayLOD(0,0,heights);
        //tData.ApplyDelayedHeightmapModification();
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
        string msg = "";
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
                if (result.Equals("COMPLETE_STATE")){
                    string[] auxStr = msg.Split('$');
                    int index = msg.IndexOf("$$");
                    if (index >= 0) msg = msg.Substring(index+2);
                    msg =  auxStr[1] + '$' + auxStr[3] + '$' + msg;
                    lock(messageLock){
                        messageList.Add(msg);
                    }
                    Debug.Log(msg);
                    result = "";
                    msg = "";
                }
                else msg = msg + result;

            } catch (Exception err){
                Debug.Log("Nothing received");
            }
        }
    }
}
