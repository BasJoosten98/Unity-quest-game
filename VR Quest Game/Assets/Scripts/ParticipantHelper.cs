using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ParticipantHelper : NetworkBehaviour
{

    //fields
    public Material BlueTeam;
    public Material RedTeam;
    public Material BlueArrow;
    public Material RedArrow;
    private static ParticipantHelper ph;
    private static int botVisionAngle = 80;
    private ParticipantManager pm;
    private List<Transform> botWalkPoints; //needs to have atleast 3 points
    private List<Transform> spawnPoints;
    private int spawnCounter;
    private int respawnTime = 5;
    private WaitForSecondsRealtime respawnWait;

    //properties
    public static ParticipantHelper PH { get { return ph; } }
    public static int BotVisionAngle { get { return botVisionAngle; } }

    //methods
    public override void OnStartServer()
    {
        ph = this;
        pm = this.GetComponent<ParticipantManager>();
        setBotWalkPointsAndSpawnPoints();
    }
    [Server]
    public void ReportArrowHit(ParticipantID Shooter, ParticipantID Hit, int damageAmount)
    {
        if (ParticipantManager.GrabbingAndShootingAllowed)
        {
            if (Hit.HealthStats.takeDamage(damageAmount)) //damage has been accepted (participant was alive before arrow hit)
            {
                if (Hit.HealthStats.Lives <= 0)
                {
                    pm.ReportKill(Shooter, Hit);
                    if (Hit.IsPlayer)
                    {
                        Hit.MainObject.GetComponent<Player>().RpcDie();
                    }
                    else
                    {
                        Hit.MainObject.GetComponent<Bot>().RpcDie();
                    }
                    if (ParticipantManager.SelfRespawnAllowed) { StartCoroutine("respawnTimer", Hit); }
                }
                else if (Hit.MainObject.GetComponent<Bot>() && Shooter.Team != Hit.Team) //if the one that got shot is a bot, WARN HIM
                {
                    Hit.MainObject.GetComponent<Bot>().Attack(Shooter);
                }
            }
        }
    }
    [Server]
    public void GivePMSpawnPoint()
    {
        this.pm.newSpawnPoint = GetSpawnPoint();
    }
    [Server]
    public ParticipantID getIDByBodyPart(GameObject bodyPart)
    {
        return pm.getIDByBodyPart(bodyPart);
    }
    [Server]
    public Vector3 BotGetNewWalkPoint(Vector3 prevDestination, Vector3 curDestination)
    {
        if (botWalkPoints.Count >= 3)
        {
            int rand;
            Vector3 newDestination = Vector3.zero;

            while (newDestination == Vector3.zero)
            {
                rand = Random.Range(0, botWalkPoints.Count);
                if (botWalkPoints[rand].position != prevDestination && botWalkPoints[rand].position != curDestination)
                {
                    newDestination = botWalkPoints[rand].position;
                }
            }
            return newDestination;
        }
        return Vector3.zero;
    } //FINISHED
    [Server]
    private Vector3 GetSpawnPoint()
    {
            spawnCounter++;
            if (spawnCounter >= botWalkPoints.Count + spawnPoints.Count) { spawnCounter = 0; }

            if (spawnCounter < botWalkPoints.Count)
            {
                return botWalkPoints[spawnCounter].position;
            }
            else if (spawnCounter < botWalkPoints.Count + spawnPoints.Count)
            {
                return spawnPoints[spawnCounter - botWalkPoints.Count].position;
            }
            else
            {
                Debug.LogWarning("spawnCounter became too high!");
                return Vector3.zero;
            }
    }
    [Server]
    private void setBotWalkPointsAndSpawnPoints()
    {
        botWalkPoints = new List<Transform>();
        spawnPoints = new List<Transform>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform point = this.transform.GetChild(i);
            if (point.gameObject.name.Contains("BotWalkPoint"))
            {
                botWalkPoints.Add(point);
            }
            else if (point.gameObject.name.Contains("SpawnPoint"))
            {
                spawnPoints.Add(point);
            }
        }
        if (botWalkPoints.Count <= 0) { Debug.LogWarning("No BotWalkPoints Found!"); }
        if (spawnPoints.Count <= 0) { Debug.LogWarning("No SpawnPoints except BotWalkPoints Found!"); }
    }   
    [Command]
    public void CmdRegisterPlayer(NetworkInstanceId NetID, string Name)
    {
        GameObject me = NetworkServer.FindLocalObject(NetID);
        ParticipantID myID = pm.RegisterPlayer(me, Name, true);
        if (myID != null)
        {
            SpawnPlayerWithMaterials(myID);
        }       
    } 
    [Server]
    public void SpawnPlayerWithMaterials(ParticipantID myID)
    {
        myID.MainObject.GetComponent<Player>().RpcCheckMaterials(myID.Team);
        myID.MainObject.GetComponent<Player>().RpcSpawn(GetSpawnPoint());
    }
    private IEnumerator respawnTimer(ParticipantID hit)
    {
        respawnWait = new WaitForSecondsRealtime(respawnTime);
        yield return respawnWait;
        respawnParticipant(hit);
    }
    [Server]
    private void respawnParticipant(ParticipantID participant)
    {
        if (participant.MainObject != null)
        {
            participant.HealthStats.Revive();
            if (participant.IsPlayer) { participant.MainObject.GetComponent<Player>().RpcRespawn(GetSpawnPoint()); }
            else { participant.MainObject.GetComponent<Bot>().Respawn(GetSpawnPoint()); }
        }
    }
}
