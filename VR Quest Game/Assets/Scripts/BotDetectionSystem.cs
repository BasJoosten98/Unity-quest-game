using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotDetectionSystem : MonoBehaviour {
    //fields
    public delegate void SendData();
    public event SendData GetData;

    private List<ParticipantID> bots;
    private List<ParticipantID> players;
    private int prevBotCount;
    private int prevPlayerCount;
    private bool isOn;

    private static float warningDistance = 25;

    private float waitTime;
    private WaitForSecondsRealtime wait;
    private WaitForSecondsRealtime waitOutOfOrder;

    //properties
    public static float WarningDistance { get{ return warningDistance; } }
    public bool IsOn { get { return this.isOn; } }

    //methods
    private void Awake()
    {
        waitOutOfOrder = new WaitForSecondsRealtime(5f);
    }
    public void DetectionOn()
    {
        if (!isOn)
        {
            if (bots == null) { bots = new List<ParticipantID>(); }
            if (players == null) { players = new List<ParticipantID>(); }
            prevBotCount = -1;
            waitTime = 0;
            StartCoroutine("warningSystem");
            isOn = true;
        }
    } //FINISHED
    public void DetectionOff()
    {
        if (isOn)
        {
            StopCoroutine("warningSystem");
            bots.Clear();
            players.Clear();
            isOn = false;
        }       
    } //FINISHED
    public void GetBotData(List<ParticipantID> Bots)
    {
        if(Bots != null)
        {
            bots = Bots;
        }
    } //FINISHED
    public void GetPlayerData(List<ParticipantID> Players)
    {
        if (Players != null)
        {
            players = Players;
        }
    } //FINISHED
    private bool enemyCheck(ParticipantID participant1, ParticipantID participant2)
    {
        if (!ScoreboardSystem.EnemiesAreTeamBased) { return true; } //everyone is your enemy
        else                                       //only the other team is your enemy
        {
            if(participant1.Team == participant2.Team) { return false; }
            else { return true; }
        }
    }
    private float distanceBetween(Vector3 p1, Vector3 p2)
    {
        return Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.z - p2.z, 2));
    } //FINISHED
    private IEnumerator warningSystem()
    {
        while (true)
        {          
            if (GetData != null)
            {
                GetData();
                if(bots.Count >= 2 || (bots.Count >= 1 && players.Count >= 1))
                {
                    if(prevBotCount != bots.Count || prevPlayerCount != players.Count)
                    {
                        prevBotCount = bots.Count;
                        prevPlayerCount = players.Count;
                        waitTime = 1f / ((bots.Count * (bots.Count - 1) / 2) + bots.Count * players.Count); 
                        wait = new WaitForSecondsRealtime(waitTime);
                    }
                    for (int b1 = 0; b1 < bots.Count; b1++) 
                    {
                        if (bots[b1].MainObject != null)
                        {
                            for (int b2 = b1 + 1; b2 < bots.Count; b2++) //warning bots about enemy bots
                            {
                                if (bots[b1].MainObject != null)
                                {
                                    if (bots[b2].MainObject != null)
                                    {
                                        if (enemyCheck(bots[b1], bots[b2]))
                                        {
                                            if (distanceBetween(bots[b1].MainObject.transform.GetChild(0).position, bots[b2].MainObject.transform.GetChild(0).position) <= warningDistance)
                                            {
                                                bots[b1].MainObject.GetComponent<Bot>().EnemyWarning(bots[b2]);
                                                bots[b2].MainObject.GetComponent<Bot>().EnemyWarning(bots[b1]);
                                            }
                                        }
                                    }
                                    yield return wait;
                                    wait = new WaitForSecondsRealtime(waitTime);
                                }
                                else
                                {
                                    yield return wait;
                                    wait = new WaitForSecondsRealtime(waitTime);
                                    break;
                                }
                            }
                            for (int p = 0; p < players.Count; p++) //warning bots about enemy players
                            {
                                if (bots[b1].MainObject != null)
                                {
                                    if (players[p].MainObject != null)
                                    {
                                        if (enemyCheck(bots[b1], players[p]))
                                        {
                                            if (distanceBetween(bots[b1].MainObject.transform.GetChild(0).position, players[p].MainObject.transform.GetChild(0).transform.position) <= warningDistance)
                                            {
                                                bots[b1].MainObject.GetComponent<Bot>().EnemyWarning(players[p]);
                                            }
                                        }
                                    }
                                    yield return wait;
                                    wait = new WaitForSecondsRealtime(waitTime);
                                }
                                else
                                {
                                    yield return wait;
                                    wait = new WaitForSecondsRealtime(waitTime);
                                    break;
                                }
                            }
                        }
                    }
                    
                }
                else
                {
                    Debug.Log("bds is out of order: Too less players/bots");
                    yield return waitOutOfOrder;
                    waitOutOfOrder = new WaitForSecondsRealtime(5f);
                }
            }
            else
            {
                Debug.Log("bds is out of order: No data");
                yield return waitOutOfOrder;
                waitOutOfOrder = new WaitForSecondsRealtime(5f);
            }         
        }
    } //FINISHED
}
