using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ParticipantManager : NetworkBehaviour {

    //fields
    public GameObject BotPrefab;
    //public GameObject PlayerPrefab;
    public Vector3 newSpawnPoint;

    private Transform blueSpawn;
    private Transform redSpawn;
    private BotDetectionSystem bds;
    private ScoreboardSystem ss;
    private bool fillWithBots;
    private WaitForSecondsRealtime fillBotWait;
    private List<ParticipantID> players;
    private List<ParticipantID> bots;

    private static bool movementAllowed;
    private static bool grabbingAndShootingAllowed;
    private static bool selfRespawnAllowed;
    

    //properties
    
    public static bool MovementAllowed { get { return movementAllowed; } }
    public static bool GrabbingAndShootingAllowed { get { return grabbingAndShootingAllowed; } }
    public static bool SelfRespawnAllowed { get { return selfRespawnAllowed; } }

    //methods
    public override void OnStartServer()
    {
        ss = this.GetComponent<ScoreboardSystem>();
        players = new List<ParticipantID>();
        bots = new List<ParticipantID>();
    }
    [Server]
    public void SetParticipantManager(bool FillWithBots, bool isSelfRespawnAllowed)
    {
        bds = this.GetComponent<BotDetectionSystem>();
        bds.GetData += this.sendBotsDataToBDS;
        bds.GetData += this.sendPlayersDataToBDS;
        fillWithBots = FillWithBots;
        setBlueSpawnAndRedSpawn();
        movementAllowed = true;
        grabbingAndShootingAllowed = true;
        selfRespawnAllowed = isSelfRespawnAllowed;
    }
    [Server]
    public void StartGame()
    {
        if (fillWithBots)
        {          
            for(int total = ss.TotalParticipants; total < ScoreboardSystem.TeamSize *2; total++)
            {
                CreateBot();
            }
            bds.DetectionOn();
            for(int i = 0; i < this.players.Count; i++)
            {
                ParticipantHelper.PH.GivePMSpawnPoint();
                players[i].MainObject.GetComponent<Player>().RpcRespawn(newSpawnPoint);
            }
            StartCoroutine("botFill");
        }
    }
    private IEnumerator botFill()
    {
        while (true)
        {
            fillBotWait = new WaitForSecondsRealtime(20f);
            yield return fillBotWait;
            while (ss.TotalParticipants < ScoreboardSystem.TeamSize * 2)
            {
                CreateBot();
            }              
        }
    }
    private void setBlueSpawnAndRedSpawn()
    {
        for(int i = 0; i < this.transform.childCount; i++)
        {
            Transform point = this.transform.GetChild(i);
            if (point.gameObject.name.Contains("BlueTeamSpawn"))
            {
                blueSpawn = point;
            }
            else if (point.gameObject.name.Contains("RedTeamSpawn"))
            {
                redSpawn = point;
            }
        }
        if(blueSpawn == null) { Debug.LogWarning("No BlueSpawn Found"); }
        if (redSpawn == null) { Debug.LogWarning("No RedSpawn Found"); }
    } //FINISHED
    [Server]
    private void sendBotsDataToBDS()
    {
        List<ParticipantID> data = new List<ParticipantID>();
        foreach (ParticipantID bot in bots)
        {
            if (bot.HealthStats.Lives > 0)
            {
                data.Add(bot);
            }
        }
        bds.GetBotData(data);
    } //FINISHED
    [Server]
    private void sendPlayersDataToBDS()
    {
        List<ParticipantID> data = new List<ParticipantID>();
        foreach (ParticipantID player in players)
        {
            if (player.HealthStats.Lives > 0)
            {
                data.Add(player);
            }
        }
        bds.GetPlayerData(data);
    } 
    [Server]
    public void ReportKill(ParticipantID attacker, ParticipantID destroyed)
    {
        ss.ReportKill(attacker, destroyed);
    } //checking is done by PH
    [Server]
    public ParticipantID RegisterPlayer(GameObject me, string Name, bool firstTrial) //player can register here!
    {
        if (getIDByBodyPart(me) == null) //this player has not been registered yet
        {
            if (ss.TotalParticipants < ScoreboardSystem.TeamSize * 2) //there is place for another player
            {

                int id;
                int spawnNumber;
                Team team;
                ParticipantID newID;
                ss.GetNextParticipantStats(out id, out team, out spawnNumber);

                if (id != -1) //player succesful registered!
                {
                    newID = new ParticipantID(id, Name, team, spawnNumber, me, new Health());
                }
                else //player register failed
                {
                    Debug.LogWarning("ParticipantManager: player " + Name + " failed to register (id was -1)");
                    return null;
                }

                players.Add(newID);
                ss.AddID(newID);
                Debug.Log("ParticipantManager: player " + newID.Name + " registered on " + team + " with id: " + id);
                return newID;
            }
            else //check if bot can be removed
            {
                if (firstTrial)
                {
                    if (bots.Count > 0) //remove bot
                    {
                        ParticipantID bot = bots[bots.Count - 1];
                        bot.MainObject.GetComponent<Bot>().BotIsActive = false;
                        Disconnect(bot);
                        bots.Remove(bot);
                        NetworkServer.Destroy(bot.MainObject);
                        Debug.Log("ParticipantManager: bot (id: " + bot.ID + ") has been removed");
                        return RegisterPlayer(me, Name, false); //try again
                    }
                }
                Debug.LogWarning("ParticipantManager: player " + Name + " failed to register (no slots available)");
                return null;
            }
        }
        Debug.LogWarning("ParticipantManager: player " + Name + " failed to register (already registered)");
        return null;
    } 
    [Server]
    private void CreateBot()
    {
        GameObject newBot = Instantiate(BotPrefab);
        ParticipantHelper.PH.GivePMSpawnPoint();
        
        if (ss.TotalBlueParticipants <= ss.TotalRedParticipants) //new blue team bot
        {
            newBot.GetComponent<Bot>().CheckMaterials(Team.Blue);       
        }
        else //new red team bot
        {
            newBot.GetComponent<Bot>().CheckMaterials(Team.Red);                     
        }

        ParticipantID newID = null;
        int id;
        int spawnNumber;
        Team team;
        ss.GetNextParticipantStats(out id, out team, out spawnNumber);

        if (id != -1) //bot succesful registered!
        {
            newID = new ParticipantID(id, (BotName)bots.Count, team, spawnNumber, newBot, new Health());
            NetworkServer.Spawn(newBot);
            Debug.Log("Bot has been added to " + team + " with id: " + id);
        }
        else
        {
            Debug.LogWarning("ParticipantManager: bot failed to register");
            Destroy(newBot);
            return;
        }

        ss.AddID(newID);
        bots.Add(newID);
        newBot.GetComponent<Bot>().SetBot(newSpawnPoint, newID);       
    }
    [Server]
    public void Disconnect(ParticipantID myID) //can be used for both players and bots (removes participant from scoreboard)
    {
        ss.RemoveID(myID.ID);
    } 
    [Server]
    public ParticipantID getIDByBodyPart(GameObject bodyPart)
    {
        //got to most upper level of participant
        if(bodyPart.transform.parent != null)
        {
            return getIDByBodyPart(bodyPart.transform.parent.gameObject);
        }

        //check if bodyPart is one of out participants
        for(int i = 0; i < bots.Count; i++)
        {
            if(bots[i].MainObject == bodyPart)
            {
                return bots[i];
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].MainObject == bodyPart)
            {
                return players[i];
            }
        }
        return null;
    }  
    [Server]
    public void AllowMovement(bool allowed)
    {
        RpcSetAllowMovement(allowed);
    }
    [Server]
    public void AllowGrabbingArrowsAndShooting(bool allowed)
    {
        RpcSetAllowGrabbingArrowsAndShooting(allowed);
        if (allowed) { bds.DetectionOn(); }
        else { bds.DetectionOff(); }
    }
    [ClientRpc]
    private void RpcSetAllowGrabbingArrowsAndShooting(bool allowed)
    {
        grabbingAndShootingAllowed = allowed;
    }
    [ClientRpc]
    private void RpcSetAllowMovement(bool allowed)
    {
        movementAllowed = allowed;
    }
}
