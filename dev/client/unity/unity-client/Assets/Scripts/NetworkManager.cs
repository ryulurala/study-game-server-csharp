using System.Collections;
using System.Collections.Generic;
using System.Net;
using core;
using dummy;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    void Start()
    {
        // IP 주소
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];

        // Port 번호
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint, () => _session);

    }

    void Update()
    {

    }
}
