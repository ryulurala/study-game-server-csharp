using System;
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

        StartCoroutine(CoSendPacket());
    }

    void Update()
    {
        IPacket packet = PacketQueue.Instance.Pop();
        if (packet != null)
            PacketManager.Instance.HandlePacket(_session, packet);
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            C_Chat chatPacket = new C_Chat();
            chatPacket.chat = "Hello Unity !";
            ArraySegment<byte> segment = chatPacket.Write();

            _session.Send(segment);
        }
    }
}
