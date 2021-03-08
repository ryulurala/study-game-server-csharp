using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : Player
{
    NetworkManager _networkManager;

    void Start()
    {
        StartCoroutine(CoSendPacket());
        _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            C_Move movePacket = new C_Move();
            movePacket.posX = UnityEngine.Random.Range(-50, 50);
            movePacket.posY = 1f;
            movePacket.posZ = UnityEngine.Random.Range(-50, 50);

            _networkManager.Send(movePacket.Write());
        }
    }
}
