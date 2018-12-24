using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreboardSystem : NetworkBehaviour {
    //fields
    private static bool enemiesAreTeamBased;
    private static int teamSize; 
    private static int redTeamScore;
    private static int blueTeamScore;
    private static ParticipantID[] ids;
    private static string[] names;
    private static int[] kills;
    private static int[] deads;

    private bool killsChanged;
    private bool deadsChanged;
    private bool namesChanged;
    private WaitForSeconds statsHoldUp;

    //properties
    public ParticipantID[] IDs { get { return ids; } }
    public static string[] Names { get { return names; } }
    public static int[] Kills { get { return kills; } }
    public static int[] Deads { get { return deads; } }
    public int RedTeamScore { get { return redTeamScore; } }
    public int BlueTeamScore { get { return blueTeamScore; } }
    public static bool EnemiesAreTeamBased { get { return enemiesAreTeamBased; } }
    public static int TeamSize { get { return teamSize; } }
    private int NextAvailableID(Team team)
    {
        if (team == Team.Blue)
        {
            for (int i = 0; i < teamSize; i++)
            {
                if(ids != null)
                { 
                    if (ids[i] == null) { return i + 1; }
                }
                else
                {
                    return -1;
                    
                }
            }
        }
        else
        {
            for (int i = teamSize; i < ids.Length; i++)
            {
                if (ids != null)
                {
                    if (ids[i] == null) { return i + 1; }
                }
                else
                {
                    return -1;
                }
            }
        }
        return -1;
    } 
    public int TotalParticipants
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] != null) { counter++; }
            }
            return counter;
        }                
    } 
    public int TotalBlueParticipants
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] != null)
                {
                    if (ids[i].Team == Team.Blue)
                    {
                        counter++;
                    }
                }
                
            }
            return counter;
        }
    }
    public int TotalRedParticipants
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] != null)
                {
                    if (ids[i].Team == Team.Red)
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }
    }
    //methods
    [Server]
    public void SetScoreboardSystem(bool areEnemiesTeambased, int TeamSize)
    {
        enemiesAreTeamBased = areEnemiesTeambased;
        teamSize = TeamSize;
        redTeamScore = 0;
        blueTeamScore = 0;
        ids = new ParticipantID[teamSize*2];
        kills = new int[teamSize*2];
        deads = new int[teamSize*2];
        names = new string[teamSize * 2];
        namesChanged = true;
        killsChanged = true;
        deadsChanged = true;
        StartCoroutine("sendStats");
    }
    [Server]
    public void GetNextParticipantStats(out int id, out Team team, out int spawnNumber)
    {
        if (TotalParticipants < ids.Length)
        {
            if (TotalBlueParticipants <= TotalRedParticipants) //give blue participant info
            {
                team = Team.Blue;
                id = NextAvailableID(team);
                spawnNumber = id - 1;
            }
            else //give red participant info
            {
                team = Team.Red;
                id = NextAvailableID(team);
                spawnNumber = id - teamSize - 1;
            }
        }
        else
        {
            team = Team.Red;
            id = -1;
            spawnNumber = -1;
        }
    }
    [Server]
    public void RedTeamScores() { redTeamScore++; }
    [Server]
    public void BlueTeamScores() { blueTeamScore++; }
    [Server]
    public void ReportKill(ParticipantID killer, ParticipantID destroyed)
    {
        if (ParticipantManager.GrabbingAndShootingAllowed)
        {
            if (killer == destroyed) { deads[killer.ID - 1]++; } //suicide
            else if (killer.Team == destroyed.Team)
            { kills[killer.ID - 1]--; deads[destroyed.ID - 1]++; } //team kill
            else //normal kill
            {
                kills[killer.ID - 1]++;
                deads[destroyed.ID - 1]++;              
            }
            killsChanged = true;
            deadsChanged = true;
        }
    } 
    [Server]
    public bool AddID(ParticipantID newID)
    {
        if (newID.ID <= ids.Length && newID.ID > 0)
        {
            kills[newID.ID - 1] = 0;
            deads[newID.ID - 1] = 0;
            ids[newID.ID - 1] = newID;
            names[newID.ID - 1] = newID.Name;
            namesChanged = true;
            killsChanged = true;
            deadsChanged = true;
            return true;
        }
        return false;
    } 
    [Server]
    public bool RemoveID(int id)
    {
        if(id <= ids.Length)
        {
            ids[id - 1] = null;
            names[id - 1] = null;
            kills[id - 1] = 0;
            deads[id - 1] = 0;
            namesChanged = true;
            killsChanged = true;
            deadsChanged = true;
            return true;
        }
        return false;
    }
    private IEnumerator sendStats()
    {
        while (true)
        {
            string[] newNames = null;
            int[] newKills = null;
            int[] newDeads = null;
            if (namesChanged) { newNames = names; }
            if (killsChanged) { newKills = kills; }
            if (deadsChanged) { newDeads = deads; }
            RpcSendStats(newNames, newKills, newDeads);
            namesChanged = false;
            killsChanged = false;
            deadsChanged = false;

            statsHoldUp = new WaitForSeconds(3);
            yield return statsHoldUp;
        }
    }
    [ClientRpc]
    private void RpcSendStats(string[] newNames, int[] newKills, int[] newDeads)
    {
        if (!isServer)
        {
            if(newNames != null)
            {
                names = newNames;
            }
            if(newKills != null)
            {
                kills = newKills;
            }
            if(newDeads != null)
            {
                deads = newDeads;
            }
        }
    }
}
