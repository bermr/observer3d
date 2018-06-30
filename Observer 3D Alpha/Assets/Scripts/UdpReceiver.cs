﻿using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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

    double[] media = {0, 0, 0};
    int[] count = {0, 0};
    float deltaTime = 0.0f;
    float recoveryTime = 0.0f; //no terrame

    private void Start(){
        Init(Port);
        //Time.captureFramerate = 10;
    }

    public void FixedUpdate(){
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
            case("yellow"):
                return new Color(1.0f,1.0f,0.0f);
            break;
            default: return Color.black;
            break;
        }
    }

    public Color findColorW(long color){
        if (color < 1000) return Color.white;
        else if (color > 1000 && color < 10000) return new Color(0.80f, 0.90f, 1.00f);
        else if (color > 10000 && color < 20000) return new Color(0.70f, 0.80f, 0.90f);
        else if (color > 20000 && color < 30000) return new Color(0.60f, 0.70f, 0.80f);
        else if (color > 30000 && color < 50000) return new Color(0.50f, 0.60f, 0.70f);
        else if (color > 50000 && color < 60000) return new Color(0.40f, 0.50f, 0.60f);
        else if (color > 60000 && color < 70000) return new Color(0.30f, 0.40f, 0.50f);
        else if (color > 70000 && color < 80000) return new Color(0.20f, 0.30f, 0.40f);
        else if (color > 80000 && color < 90000) return new Color(0.10f, 0.20f, 0.30f);
        else return new Color(0.05f, 0.10f, 0.20f);
    }

    public void Decode(string messageReceived){
        Stopwatch renderingTime = Stopwatch.StartNew();
        string[] tokens = messageReceived.Split('$');

        Texture2D texture;
        int size = Convert.ToInt16(tokens[1]);
        int dimx = Convert.ToInt16(Math.Sqrt(size));
        int dimy = Convert.ToInt16(Math.Sqrt(size));
        tData.heightmapResolution = dimx;
        tData.size = new Vector3(15, 30, 15);
        texture = new Texture2D(dimx, dimy);
        //Debug.Log(tData.heightmapWidth + " " + tData.heightmapHeight + " " + texture.width + " " + texture.height +" " + tData.size);
        //float[,] heights = tData.GetHeights(0, 0, tData.heightmapWidth, tData.heightmapHeight);
        float[,] heights = new float[tData.heightmapWidth,tData.heightmapHeight];
        Color32[] pixels = new Color32[size];
        bool back = false;
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
                long water;
                float h;
                attNumber = Convert.ToInt16(tokens[i+1]);
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
                                        //pixels[Convert.ToInt16(tokens[aux-1]) - 3] = color;
                                        texture.SetPixel(xi, yi, color);
                                        aux += 3;
                                    break;
                                    case("water"):
                                        coverColor = tokens[aux+5];
                                        //Debug.Log(coverColor);
                                        coverColor = (coverColor.Length < 5) ? coverColor  : coverColor.Substring(0,5);
                                        water = Convert.ToInt64(coverColor);
                                        color = findColorW(water);
                                        //pixels[Convert.ToInt16(tokens[aux-1]) - 3] = color;
                                        texture.SetPixel(xi, yi, color);
                                        aux += 3;
                                    break;
                                    case("height"):
                                        try{
                                            hei = tokens[aux+5];
                                            hei = (hei.Length < 5) ? hei  : hei.Substring(0,5);
                                            hei = "0." + hei;
                                            h = ((float) Convert.ToDouble(hei));
                                            //UnityEngine.Debug.Log(h);
                                            if (h==1) h = 0.0f;
                                        } catch(Exception err){
                                            h = 0.0f;
                                        }
                                        heights[yi,xi] = h;
                                        //UnityEngine.Debug.Log(yi+" "+xi + " ");
                                        aux += 3;
                                    break;
                                    default:
                                    back = true;
                                    break;
                                }
                            }
                    }
                    } catch(Exception err){ i--;}
                    if (!back)  i += attNumber*3 + 5; //8 ou 11
                    else i--;
                }
            break;
        }
        SplatPrototype[] terrainTexture = new SplatPrototype[1];
        terrainTexture[0] = new SplatPrototype();
        //texture.SetPixels32(pixels);
        skr.material.mainTexture = texture;
        texture.Apply();
        terrainTexture[0].texture = texture;
        tData.splatPrototypes = terrainTexture;
        tData.SetHeightsDelayLOD(0,0,heights);
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        renderingTime.Stop();
        media[1] = media[1] + renderingTime.ElapsedMilliseconds;
        count[1]++;
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
        media[0] = media[0] / count[0];
        media[1] = media[1] / count[1];
        media[2] = totalFps / Time.frameCount;
        //string fileName = "/home/bernardo/Desktop/bernardo/UFOP/TerraLAB/Unity Project/Observer 3D Alpha/Assets/InternetSender/output/output" + texture.width + "x" + texture.height + ".txt";
        //System.IO.File.WriteAllText(fileName,
        //    "Decoding time: " + media[0] + "\n" +
        //    "Rendering time: " + media[1] + "\n" +
        //    "Average FPS: " + media[2]);
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
        UnityEngine.Debug.Log(sb.ToString());
    }

    private void ReceiveData(){
        string msg = "";
        while (isOn) {
            try {
                Stopwatch decodingTime = Stopwatch.StartNew();
                byte[] data = client.Receive(ref endPoint);
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                string result = iso.GetString(data);
                //PrintByteArray(data);
                StringBuilder sb = new StringBuilder();
                foreach (char c in result){
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
                    //Debug.Log(msg);
                    lock(messageLock){
                        messageList.Add(msg);
                        //UnityEngine.Debug.Log(msg);
                        decodingTime.Stop();
                        media[0] = media[0] + decodingTime.ElapsedMilliseconds;
                        count[0]++;
                        //UnityEngine.Debug.Log(decodingTime.ElapsedMilliseconds);
                    }
                    result = "";
                    msg = "";
                }
                else{
                    /*int index = result.IndexOf("$");
                    if (result[index+1] == '$') index++;
                    if (index >= 0) msg = msg + result.Substring(index+1);*/
                    msg = msg + result;
                }

            } catch (Exception err){
                //UnityEngine.Debug.Log("Nothing received");
            }
        }
    }

    float totalFps = 0;
    void OnGUI(){
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        if (fps < 50) totalFps += fps;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
