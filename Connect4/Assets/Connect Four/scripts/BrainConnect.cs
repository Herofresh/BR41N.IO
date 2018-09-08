using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading;
using System.Text;

public class BrainConnect : MonoBehaviour
{
    static readonly System.Object lockObject = new System.Object();
    public int Port;
    public string Key;

    public bool keyRead = true;

    private bool hasData = false;
    private Thread thread;
    private string returnData = "";

    static UdpClient connection;

    void OnApplicationQuit()
    {
        connection.Close();
        thread.Abort();
    }


    void Start()
    {
        //Creates a UdpClient for reading incoming data.

        //Creates an IPEndPoint to record the IP Address and port number of the sender. 
        // The IPEndPoint will allow you to read datagrams sent from any source.
        thread = new Thread(new ThreadStart(ReceiveThings));
        thread.Start();
    }

    void Update()
    {
        if (hasData)
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
        UdpClient connection = new UdpClient(Port);
        Debug.Log("Entered function ReceiveThings...");
        while (true)
        {

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                byte[] receiveBytes = connection.Receive(ref RemoteIpEndPoint);
                Debug.Log(receiveBytes.Length);
                returnData = Encoding.UTF8.GetString(receiveBytes);
                Debug.Log(returnData);
            }
            catch (ThreadAbortException e)
            {
                Debug.Log(e);
            }

            lock (lockObject)
            {
                //Done, notify the Update function
                hasData = true;
            }
        }

    }
}