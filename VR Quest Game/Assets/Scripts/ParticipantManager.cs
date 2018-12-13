using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ParticipantManager : NetworkBehaviour {

    //fields
    public GameObject BotPrefab;
    public GameObject PlayerPrefab;
    public Vector3 newSpawnPoint;

    private Transform blueSpawn;
    private Transform redSpawn;
    private List<GameObject> players;
    private List<GameObject> bots;
    private BotDetectionSystem bds;
    private ScoreboardSystem ss;
    private bool fillWithBots;
    private WaitForSecondsRealtime fillBotWait;

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
        players = new List<GameObject>();
        bots = new List<GameObject>();
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
                players[i].GetComponent<Player>().RpcRespawn(newSpawnPoint);
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
        List<GameObject> data = new List<GameObject>();
        foreach (GameObject bot in bots)
        {
            if (bot.GetComponent<Health>().Lives > 0)
            {
                data.Add(bot);
            }
        }
        bds.GetBotData(data);
    } //FINISHED
    [Server]
    private void sendPlayersDataToBDS()
    {
        List<GameObject> data = new List<GameObject>();
        foreach (GameObject player in players)
        {
            if (player.GetComponent<Health>().Lives > 0)
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
    public void RegisterPlayer(GameObject me, string Name) //player can register here!
    {
        me.AddComponent<Health>();
        me.AddComponent<ParticipantID>();

        int id;
        int spawnNumber;
        Team team;
        ss.GetNextParticipantStats(out id, out team, out spawnNumber);

        if(id != -1) { me.GetComponent<ParticipantID>().SetParticipantID(id, Name, team, spawnNumber); }
        else { Debug.LogWarning("ID was -1! PM.RegisterPlayer"); }

        ss.AddID(me.GetComponent<ParticipantID>());
        players.Add(me);
    } 
    [Server]
    private void CreateBot()
    {
        GameObject newBot = Instantiate(BotPrefab);
        ParticipantHelper.PH.GivePMSpawnPoint();
        
        if (ss.TotalBlueParticipants <= ss.TotalRedParticipants) //new blue team bot
        {
            newBot.GetComponent<Bot>().CheckMaterials(Team.Blue);
            NetworkServer.Spawn(newBot);        
        }
        else //new red team bot
        {
            newBot.GetComponent<Bot>().CheckMaterials(Team.Red);
            NetworkServer.Spawn(newBot);                      
        }
        newBot.AddComponent<ParticipantID>();
        int id;
        int spawnNumber;
        Team team;
        ss.GetNextParticipantStats(out id, out team, out spawnNumber);
        if(id != -1) { newBot.GetComponent<ParticipantID>().SetParticipantID(id, (BotName)bots.Count, team, spawnNumber); }
        else { Debug.LogWarning("ID was -1! PM.CreateBot"); }

        ss.AddID(newBot.GetComponent<ParticipantID>());
        newBot.AddComponent<Health>();        
        newBot.GetComponent<Bot>().SetBot(newSpawnPoint);
        bots.Add(newBot);
    }
    [Server]
    public void Disconnect(GameObject me) //can be used for both players and bots (removes participant from scoreboard)
    {
        ss.RemoveID(me.GetComponent<ParticipantID>().ID);
    } 
    [Server]
    public ParticipantID getIDByBodyPart(GameObject bodyPart)
    {
        if(bodyPart.transform.parent != null)
        {
            return getIDByBodyPart(bodyPart.transform.parent.gameObject);
        }
        if (bodyPart.GetComponent<ParticipantID>())
        {
            return bodyPart.GetComponent<ParticipantID>();
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
