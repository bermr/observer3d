using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UdpReceiver:MonoBehaviour {

    public int Port = 55000;
    public GameObject ball;
    public BallController bc;

    private bool startBall = false;
    private IPEndPoint endPoint;
    private UdpClient client;
    private Thread receiveThread;
    private bool isOn;

    private void Start(){
        bc = ball.GetComponent<BallController>();
        Init(Port);
    }

    public void FixedUpdate(){
        if (startBall){
            bc.Move();
        }
    }

    private void OnApplicationQuit() {
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

    private void ReceiveData() {
        while (isOn) {
            try {
                byte[] data = client.Receive(ref endPoint);
                string text = Encoding.UTF8.GetString(data);
                if (String.Equals(text, "Start")){
                startBall = true;
                }
                Debug.Log("ReceiveData: data=" + text);
            }catch (Exception err) {
                //Debug.Log("ReceiveData: nothing");
            }
        }
    }
}
