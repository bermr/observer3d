using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class UdpReceiver:MonoBehaviour {
    public int Port = 55000;
    public Terrain terrain;
    public TerrainData tData;
    public Renderer skr;
    private object messageLock = new object();
    private List<string> messageList = new List<string>();
    private volatile static bool noAction = true;

    private IPEndPoint endPoint;
    private UdpClient client;
    private Thread receiveThread;
    private bool isOn;

    float deltaTime = 0.0f;

    private void Start(){
        Init(Port);
    }

    public void Update(){
        lock(messageLock){
            if (messageList.Count > 0){
                Decode(messageList[0]);
                messageList.RemoveAt(0);
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
            case("blue"):
                return Color.blue;
            break;
            default: return Color.black;
            break;
        }
    }

    public Color findColorW(int color){
        if (color == 0) return Color.white;
        else if (color > 100) return Color.blue;
        else return Color.grey;
    }

    public void Decode(string messageReceived){
        string[] tokens = messageReceived.Split('$');

        //for (int i=0;i<tokens.Length;i++) print(i + ": " + tokens[i]);

        int size = Convert.ToInt16(tokens[1]);
        tData.size = new Vector3(Convert.ToSingle(Math.Sqrt(size/40)), 20, Convert.ToSingle(Math.Sqrt(size/40)));
        int dimx = Convert.ToInt16(Math.Sqrt(size));
        int dimy = Convert.ToInt16(Math.Sqrt(size));
        Texture2D texture = new Texture2D(dimx, dimy);
        skr.material.mainTexture = texture;
        tData.heightmapResolution = 33;
        //Debug.Log(tData.heightmapWidth + " " + tData.heightmapHeight + " " + texture.width + " " + texture.height);
        float[,] heights = tData.GetHeights(0, 0, tData.heightmapWidth, tData.heightmapHeight);
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
                int xi, yi;
                string hei;
                int water;
                attNumber = Convert.ToInt16(tokens[i+1]);
                float h;
                for(int k=0;k<size;k++){
                    //Debug.Log("x:" + x + " y: " + y);
                    int aux = i;
                    try{
                        xi = (Convert.ToInt16(tokens[aux-1]) - 3)/dimx;
                        yi = (Convert.ToInt16(tokens[aux-1]) - 3)%dimx;
                        n = Convert.ToInt16(tokens[i+1]);
                        if (n != attNumber) i--;
                        else{
                            for(int j=0;j<attNumber;j++){
                                switch(tokens[aux+3]){
                                    case("cover"):
                                        coverColor = tokens[aux+5];
                                        color = findColor(coverColor);
                                        texture.SetPixel(xi, yi, color);
                                        aux += 3;
                                    break;
                                    case("water"):
                                        coverColor = tokens[aux+5];
                                        water = Convert.ToInt16(coverColor);
                                        Debug.Log(water);
                                        color = findColorW(water);
                                        texture.SetPixel(xi, yi, color);
                                        aux += 3;
                                    break;
                                    case("height"):
                                        try{
                                            hei = tokens[aux+5];
                                            hei = (hei.Length < 5) ? hei  : hei.Substring(0,5);
                                            hei = "0." + hei;
                                            h = 10 * ((float) Convert.ToDouble(hei));
                                            if (h==1) h = 0.0f;
                                        } catch(Exception err){
                                            h = 0.0f;
                                        }
                                        heights[yi,xi] = h;
                                        aux += 3;
                                    break;
                                }
                            }
                    }
                    } catch(Exception err){ i--;}
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
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        //tData.ApplyDelayedHeightmapModification();
    }

    private void OnApplicationQuit() {
        tData.splatPrototypes = null;
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

    public void PrintByteArray(byte[] bytes){
        var sb = new StringBuilder("new byte[] { ");
        foreach (var b in bytes){
            sb.Append(b + ", ");
        }
        sb.Append("}");
        Debug.Log(sb.ToString());
    }

    private void ReceiveData(){
        string msg = "";
        while (isOn) {
            try {
                byte[] data = client.Receive(ref endPoint);
                Encoding iso = Encoding.GetEncoding("latin1");
                string result = iso.GetString(data);
                //PrintByteArray(data);
                //Debug.Log(result);
                StringBuilder sb = new StringBuilder();
                foreach (char c in result){
                    if ((c >= 'A' && c <= 'Z')||(c=='_')||(c == '$')||(c >= '0' && c <= '9')||(c >= 'a' && c <= 'z')){
                        sb.Append(c);
                    }
                }
                result = sb.ToString();
                //Debug.Log(result);
                if (result.Equals("COMPLETE_STATE")){
                    string[] auxStr = msg.Split('$');
                    int index = msg.IndexOf("$$");
                    if (index >= 0) msg = msg.Substring(index+2);
                    msg =  auxStr[1] + '$' + auxStr[3] + '$' + msg;
                    //lock(messageLock){
                        messageList.Add(msg);
                    //}
                    result = "";
                    msg = "";
                }
                else{
                    /*int index = result.IndexOf("$");
                    if (result[index+1] == '$') index++;
                    if (index >= 0) msg = msg + result.Substring(index+1);*/
                    msg = msg + result;
                    //Debug.Log(result);
                }

            } catch (Exception err){
                //Debug.Log("Nothing received");
            }
        }
    }

    void OnGUI(){
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
