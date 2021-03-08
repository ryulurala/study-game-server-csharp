using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();
    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach (S_PlayerList.Player p in packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if (p.isSelf)
            {
                // 자신
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                // 그 외
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    public void Move(S_BroadcastMove pkt)
    {
        if (_myPlayer.PlayerId == pkt.playerId)
        {
            // 자신
            _myPlayer.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
        }
        else
        {
            // 그 외
            Player player = null;
            if (_players.TryGetValue(pkt.playerId, out player))
            {
                player.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
            }
        }
    }

    public void EnterGame(S_BroadcastEnterGame pkt)
    {
        if (_myPlayer.PlayerId == pkt.playerId)
            return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
        _players.Add(pkt.playerId, player);
    }

    public void LeaveGame(S_BroadcastLeaveGame pkt)
    {
        if (_myPlayer.PlayerId == pkt.playerId)
        {
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            Player player = null;
            if (_players.TryGetValue(pkt.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                _players.Remove(pkt.playerId);
            }
        }
    }

}
