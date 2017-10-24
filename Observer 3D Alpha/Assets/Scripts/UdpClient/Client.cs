using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class Client{

private static void Main(){
var client = new UdpClient();
IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55000); // endpoint where server is listening
client.Connect(ep);

// send data
while(true){
    string text = "Start";
    byte[] send_buffer = Encoding.ASCII.GetBytes(text);
    client.Send(send_buffer, send_buffer.Length);
}

}
}