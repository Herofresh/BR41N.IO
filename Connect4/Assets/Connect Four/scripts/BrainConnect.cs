using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading;

public class BrainConnect : MonoBehaviour
{
    static readonly System.Object lockObject = new System.Object();
    public int Port;
    public string IpAddress;
    public string Key;

    public bool keyRead = true;

    private bool hasData = false;
    private Thread thread;
    private string returnData = "";
    
    UdpClient connection;
    IPEndPoint RemoteIpEndPoint;

    void OnApplicationQuit() {
        connection.Close();
        thread.Abort();
    }


    void Start()
    {
        //Creates a UdpClient for reading incoming data.
        UdpClient connection = new UdpClient(Port);

        //Creates an IPEndPoint to record the IP Address and port number of the sender. 
        // The IPEndPoint will allow you to read datagrams sent from any source.
        IPAddress ip = System.Net.IPAddress.Parse(IpAddress);
        RemoteIpEndPoint = new IPEndPoint(ip, 0);
        connection.Connect(RemoteIpEndPoint);
        thread = new Thread(new ThreadStart(ReceiveThings));
        thread.Start();
    }

    void Update()
    {
        if(hasData) 
        {
            lock (lockObject)
            {
                hasData = false;

                var regex = new Regex(@"Key_[A-Z][a-z0-9]*");
                var match = regex.Match(returnData);

                Key = "";
                if (match.Value.Contains("_"))
                {
                    Key = match.Value.Split('_')[1];
                    keyRead = false;
                }
                returnData = "";
                Debug.Log(Key);
            }
        }
    }

    void ReceiveThings() 
    {
        Debug.Log("Entered function ReceiveThings...");
    	while (true) 
    	{
    	 	try
    	        {
    	            // Blocks until a message returns on this socket from a remote host.
    	            Debug.Log("Waiting for a message...");
    	            Byte[] receiveBytes = connection.Receive(ref RemoteIpEndPoint);

    	            Debug.Log("Message was received!");
    	            string returnData = System.Text.Encoding.ASCII.GetString(receiveBytes);
                    hasData = true;
    	        }
    	        catch (Exception)
    	        {
    	            // "error-handling"
    	        }
    	}
       
    }
}
