using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticipantID : MonoBehaviour {

    private int id;
    private BotName botName;
    private string playerName;
    private Team team;
    private int spawnNumber;

    public int ID { get { return this.id; } }
    public string Name
    {
        get
        {
            if(playerName != null) { return this.playerName; }
            else { return "Bot " + botName; }
        }
    } 
    public Team Team { get { return this.team; } }
    public int SpawnNumber { get { return this.spawnNumber; } }
    public bool IsPlayer
    {
        get
        {
            if (playerName != null) { return true; }
            else { return false; }
        }
    }
    public void SetParticipantID(int ID, string Name, Team Team, int SpawnNumber)
    {
        if(this.id <= 0)
        {
            this.id = ID;
            this.playerName = Name;
            this.team = Team;
            this.spawnNumber = SpawnNumber;
        }        
    }
    public void SetParticipantID(int ID, BotName Name, Team Team, int SpawnNumber)
    {
        if (this.id <= 0)
        {
            this.id = ID;
            this.botName = Name;
            this.team = Team;
            this.spawnNumber = SpawnNumber;
            this.playerName = null;
        }
    }
}
