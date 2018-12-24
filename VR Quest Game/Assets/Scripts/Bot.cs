using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bot : NetworkBehaviour
{
    //fields
    private ParticipantID huntTargetID; //contain scripts like ID and Health
    private GameObject huntTargetBody; //the actual body of the target
    private Vector3 huntTargetLastSeen;

    private bool resetLookDirection;
    [SyncVar]
    private bool playerInSight;
    private bool goToLastSeenDistanceCheck;
    private Vector3 curDestination;
    private Vector3 prevDestination;

    private float moveSpeed;
    private float t;
    private Vector3 moveTowardsEndLocation;
    private Vector3 moveTowardsStartLocation;

    private ParticipantID botID;
    private BotStatus status;
    private bool botHasBeenSet;
    private bool botIsActive;
    private Transform raycastPoint;
    private GameObject bow;
    private GameObject head;
    private GameObject body;
    private static Vector3 headPos;
    private static Vector3 bowPos;
    private UnityEngine.AI.NavMeshAgent agent;

    private WaitForSecondsRealtime waitHunt;
    private WaitForSecondsRealtime waitLastSeen;
    private WaitForSecondsRealtime waitDistanceChecker;

    //properties
    public BotStatus Status { get { return this.status; } }
    public bool BotIsActive { get { return this.botIsActive; } }

    //methods
    private void Awake()
    {
        setBowAndHeadAndRaycastPoint();
        if(headPos == Vector3.zero) { headPos = head.transform.localPosition; }
        if(bowPos == Vector3.zero) { bowPos = bow.transform.localPosition; }       
        status = BotStatus.Idle;
    }
    void FixedUpdate()
    {
        if (botIsActive)
        {
            if (status == BotStatus.Hunting)
            {
                if (huntTargetBody != null)
                {
                    if (playerInSight) //look at HuntTarget
                    {
                        Vector3 bodyDifference = huntTargetBody.transform.position - body.transform.position;
                        Vector3 bowDifference = huntTargetBody.transform.position - this.bow.transform.position;
                        float distance = Mathf.Sqrt(Mathf.Pow(bowDifference.x, 2) + Mathf.Pow(bowDifference.z, 2));
                        Quaternion end = Quaternion.LookRotation(new Vector3(bodyDifference.x, 0, bodyDifference.z));
                        Quaternion end2 = Quaternion.LookRotation(new Vector3(0, bowDifference.y, distance));

                        body.transform.localRotation = Quaternion.Slerp(body.transform.localRotation, end, Time.deltaTime * 2); //body rotation
                        head.transform.localRotation = Quaternion.Slerp(head.transform.localRotation, end2, Time.deltaTime * 2); //head rotation
                        bow.transform.localRotation = Quaternion.Slerp(bow.transform.localRotation, end2, Time.deltaTime * 2); //bow rotation
                    }
                    else //look at huntTargetLastSeen
                    {
                        Vector3 bodyDifference = huntTargetLastSeen - body.transform.position;
                        Vector3 bowDifference = huntTargetLastSeen - this.bow.transform.position;
                        float distance = Mathf.Sqrt(Mathf.Pow(bowDifference.x, 2) + Mathf.Pow(bowDifference.z, 2));
                        Quaternion end = Quaternion.LookRotation(new Vector3(bodyDifference.x, 0, bodyDifference.z));
                        Quaternion end2 = Quaternion.LookRotation(new Vector3(0, bowDifference.y, distance));

                        body.transform.localRotation = Quaternion.Slerp(body.transform.localRotation, end, Time.deltaTime * 2); //body rotation
                        head.transform.localRotation = Quaternion.Slerp(head.transform.localRotation, end2, Time.deltaTime * 2); //head rotation
                        bow.transform.localRotation = Quaternion.Slerp(bow.transform.localRotation, end2, Time.deltaTime * 2); //bow rotation
                    }
                }
            }
            else
            {
                if (resetLookDirection)
                {
                    Quaternion end = Quaternion.LookRotation(Vector3.forward);
                    head.GetComponent<Transform>().localRotation = Quaternion.Slerp(head.GetComponent<Transform>().localRotation, end, Time.deltaTime); //head rotation
                    bow.GetComponent<Transform>().localRotation = Quaternion.Slerp(bow.GetComponent<Transform>().localRotation, end, Time.deltaTime); //bow rotation
                    if (Vector3.Angle(bow.GetComponent<Transform>().forward, body.transform.forward) <= 2)
                    {
                        resetLookDirection = false;
                    }
                }
            } 
            if(moveTowardsStartLocation != Vector3.zero && moveTowardsEndLocation != Vector3.zero)
            {
                if (t < 0.5f)
                {
                    t += Time.deltaTime;
                    body.transform.position = Vector3.MoveTowards(moveTowardsStartLocation, moveTowardsEndLocation, t * moveSpeed);
                }
                else
                {
                    t = 0;
                    moveTowardsStartLocation = Vector3.zero;
                    moveTowardsEndLocation = Vector3.zero;
                }
            }
        }

    } //FINISHED
    private void setBowAndHeadAndRaycastPoint()
    {
        this.body = this.transform.GetChild(0).gameObject;
        this.head = body.transform.GetChild(0).gameObject;
        this.bow = body.transform.GetChild(1).gameObject;
        this.raycastPoint = head.transform.GetChild(1);
    }
    private IEnumerator hunt()
    {
        while (status == BotStatus.Hunting)
        {
            if (botIsActive)
            {
                if (huntTargetID != null && ParticipantManager.GrabbingAndShootingAllowed)
                {
                    if (huntTargetID.HealthStats.Lives > 0) //target is alive
                    {
                        bool newPlayerInSight = false;
                        if (objectInSight(huntTargetBody.GetComponent<Transform>())) //enemy is in eye angle 
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                RaycastHit hit;
                                Vector3 direction; 
                                direction = huntTargetBody.GetComponent<Transform>().position - this.raycastPoint.position; 

                                if (Physics.Raycast(raycastPoint.position, direction, out hit, BotDetectionSystem.WarningDistance))
                                {
                                    //If bot model and player model are the same
                                    if (hit.collider.gameObject == huntTargetBody) { newPlayerInSight = true; break; } //check body
                                }
                            }
                            if(newPlayerInSight != playerInSight) { playerInSight = newPlayerInSight; }
                            if (playerInSight)
                            {
                                agent.isStopped = true;
                                huntTargetLastSeen = huntTargetBody.GetComponent<Transform>().position;
                                botShot();
                                goToLastSeenDistanceCheck = false;
                                waitHunt = new WaitForSecondsRealtime(2f);
                                yield return waitHunt;                               
                            }
                            else //huntTargetID is NOT in eyesight, Go to last seen place
                            {
                                goToLastSeen();
                                waitLastSeen = new WaitForSecondsRealtime(2f);
                                yield return waitLastSeen;
                            }
                        }
                        else //Noting is in eyesight, Go to last seen place
                        {
                            if (playerInSight != false) { playerInSight = false; }
                            goToLastSeen();
                            waitLastSeen = new WaitForSecondsRealtime(2f);
                            yield return waitLastSeen;                           
                        }
                    }
                    else//target is dead
                    {
                        if (playerInSight != false) { playerInSight = false; }
                        stopHunt();
                    }
                }
                else //target disappeared or has been removed from game
                {
                    if (playerInSight != false) { playerInSight = false; }
                    stopHunt();
                }
            }
        }
    } //FINISHED
    private void stopHunt()
    {
        if (status == BotStatus.Hunting)
        {
            agent.isStopped = true;
            status = BotStatus.Idle;
            huntTargetLastSeen = Vector3.zero;
            huntTargetID = null;
            huntTargetBody = null;
            resetLookDirection = true;
            goToLastSeenDistanceCheck = false;
            RpcStopHunt();
            if (ParticipantManager.MovementAllowed) { goToNextDestination(); }           
        }
    }  //FINISHED
    [ClientRpc]
    private void RpcStopHunt()
    {
        huntTargetLastSeen = Vector3.zero;
        huntTargetID = null;
        huntTargetBody = null;
        resetLookDirection = true;
        goToLastSeenDistanceCheck = false;
    }
    private void goToLastSeen()
    {
        if (status == BotStatus.Hunting)
        {
            if (!goToLastSeenDistanceCheck)
            {
                goToLastSeenDistanceCheck = true;
                if (distanceBetween(agent.transform.position, huntTargetLastSeen) > distanceBetween(body.transform.position, huntTargetLastSeen))
                {
                    agent.Warp(body.transform.position);
                }
            }
            agent.SetDestination(huntTargetLastSeen);
            agent.isStopped = false;
            if (distanceBetween(body.GetComponent<Transform>().position, huntTargetLastSeen) <= 4)
            {
                stopHunt();
            }
        }
    } //FINISHED
    [Server]
    public void Attack(ParticipantID target)
    {
        if (botIsActive)
        {
            if (target.Team != botID.Team)
            {
                RpcSetTarget(target.MainObject.GetComponent<NetworkIdentity>().netId);
                agent.isStopped = true;
                huntTargetID = target;
                huntTargetBody = target.MainObject.transform.GetChild(0).gameObject;
                huntTargetLastSeen = huntTargetBody.transform.position;
                status = BotStatus.Hunting;
                resetLookDirection = false;
                StartCoroutine("hunt");
            }
        }
    } 
    [ClientRpc]
    private void RpcSetTarget(NetworkInstanceId target)
    {
        GameObject targetMain = ClientScene.FindLocalObject(target);
        //huntTargetID = targetMain;
        huntTargetBody = targetMain.transform.GetChild(0).gameObject;
        huntTargetLastSeen = huntTargetBody.transform.position;
        status = BotStatus.Hunting;
        resetLookDirection = false;
    }
    private bool objectInSight(Transform targetObject)
    {
        if(objectSightAngle(targetObject) <= ParticipantHelper.BotVisionAngle)
        {
            return true;
        }
        return false;
    } 
    private float objectSightAngle(Transform targetObject)
    {
        Vector2 worldSpaceDifference = new Vector2(targetObject.position.x - body.transform.position.x, targetObject.position.z - body.transform.position.z);
        Vector2 eyeDirection = new Vector2(body.transform.forward.x, body.transform.forward.z);
        return Vector2.Angle(eyeDirection, worldSpaceDifference);
    } //cheap method for calculating angle between eyeMidSight and objectDirection
    private float objectSightAngle(Vector3 relativeDirection)
    {
        Vector2 worldSpaceDifference = new Vector2(relativeDirection.x, relativeDirection.z);
        Vector2 eyeDirection = new Vector2(body.transform.forward.x, body.transform.forward.z);
        return Vector2.Angle(eyeDirection, worldSpaceDifference);
    } //cheap method for calculating angle between eyeMidSight and objectDirection
    [Server]
    public void EnemyWarning(ParticipantID target)
    {
        if (botIsActive)
        {
            if (status != BotStatus.Hunting)
            {
                if (objectInSight(target.MainObject.transform.GetChild(0))) //enemy is in eye angle 
                {
                    RaycastHit hit;
                    Vector3 direction = target.MainObject.transform.GetChild(0).position - raycastPoint.position;

                    Debug.DrawRay(raycastPoint.position, direction, Color.green, 1f);
                    if (Physics.Raycast(raycastPoint.position, direction, out hit, BotDetectionSystem.WarningDistance))
                    {
                        //If bot model and player model are the same
                        if (hit.collider.gameObject == target.MainObject.transform.GetChild(0).gameObject) { Attack(target); } //check body                     
                    }
                }
            }
        }
    } //FINISHED
    [ClientRpc]
    private void RpcSetMoveTowards(Vector3 start, Vector3 end)
    {
        moveTowardsStartLocation = start;
        moveTowardsEndLocation = end;
        moveSpeed = 2 * Vector3.Distance(moveTowardsStartLocation, moveTowardsEndLocation);
    }
    [Server]
    private void checkForNewDestination()
    {
        if (status == BotStatus.Walking && botIsActive && ParticipantManager.MovementAllowed)
        {
            if (distanceBetween(body.transform.position, agent.transform.position) >= 0.5f)
            {
                //first teleport to your agent               
                RpcSetMoveTowards(body.transform.position, agent.transform.position);
                body.transform.rotation = agent.transform.rotation;
            }                
            //then check if you are close enough to the destination
            if (distanceBetween(body.transform.position, curDestination) <= 1)
            {
                goToNextDestination();
            }            
        }
        else if (status == BotStatus.Hunting && huntTargetLastSeen != Vector3.zero && !agent.isStopped)
        {
            if (distanceBetween(body.transform.position, agent.transform.position) >= 0.5f)
            {
                //teleport to your agent
                body.transform.position = agent.transform.position;
            }
        }
        else if (!ParticipantManager.MovementAllowed)
        {
            this.agent.isStopped = true;
        }
    } 
    [Server]
    private void goToNextDestination()
    {
        if (status != BotStatus.Hunting)
        {
            Vector3 newDestination;
            newDestination = ParticipantHelper.PH.BotGetNewWalkPoint(prevDestination, curDestination);
            if(newDestination != Vector3.zero)
            {
                status = BotStatus.Walking;
                prevDestination = curDestination;
                curDestination = newDestination;
                agent.isStopped = false;
                agent.SetDestination(curDestination);
            }
            else
            {
                status = BotStatus.Idle;
                agent.isStopped = true;
                Debug.LogWarning("New Destination was vector3.zero!");
            }
        }
    } 
    private float distanceBetween(Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.z - p2.z, 2);
    } 
    private IEnumerator checkForNewDestinationTimer()
    {
        while (true)
        {
            waitDistanceChecker = new WaitForSecondsRealtime(2f);
            yield return waitDistanceChecker;
            checkForNewDestination();
        }
    } 
    [Server]
    public bool SetBot(Vector3 spawnLocation, ParticipantID myID)
    {
        if (!botHasBeenSet)
        {
            //fields
            this.botID = myID;
            this.agent = this.transform.GetChild(1).GetComponent<UnityEngine.AI.NavMeshAgent>();
            this.agent.isStopped = true;
            this.botHasBeenSet = true;
            this.botIsActive = true;
            this.status = BotStatus.Idle;
            this.bow.GetComponent<BotBow>().SetOwner(botID);

            Spawn(spawnLocation);
            goToNextDestination();
            StartCoroutine("checkForNewDestinationTimer");
            return true;
        }
        Debug.LogWarning("Bot has already been set!");
        return false;
    } 
    [ClientRpc]
    public void RpcDie()
    {
        Die();
    }
    [Server]
    private void botShot()
    {
        GameObject arrow = bow.GetComponent<BotBow>().createArrow();
        if (arrow != null)
        {
            NetworkServer.Spawn(arrow);
            arrow.GetComponent<Arrow>().SetShooter(botID);
            RpcBotShot(arrow.GetComponent<NetworkIdentity>().netId);
        }
    }
    [ClientRpc]
    public void RpcBotShot(NetworkInstanceId id) //must be called form botbow script!
    {
        GameObject arrow = ClientScene.FindLocalObject(id);
        bow.GetComponent<BotBow>().BotShotForClient(arrow);
    }

    private void Die()
    {
        botIsActive = false;
        status = BotStatus.Idle;
        agent.isStopped = true;
        bow.GetComponent<BotBow>().ResetBow();
        body.GetComponent<Rigidbody>().isKinematic = false;
        body.GetComponent<Rigidbody>().useGravity = true;
        head.GetComponent<Rigidbody>().isKinematic = false;
        head.GetComponent<Rigidbody>().useGravity = true;
        bow.GetComponent<Rigidbody>().isKinematic = false;
        bow.GetComponent<Rigidbody>().useGravity = true;
        bow.GetComponent<BoxCollider>().enabled = true;
        bow.transform.parent = null;
        head.transform.parent = null;
        curDestination = Vector3.zero;
        prevDestination = Vector3.zero;
        moveTowardsStartLocation = Vector3.zero;
        moveTowardsEndLocation = Vector3.zero;
        t = 0;
        moveSpeed = 0;
    }
    [Server]
    public void Respawn(Vector3 spawnLocation)
    {
        botIsActive = true;
        reBuildBotAfterDead();      
        Spawn(spawnLocation);
        goToNextDestination();
    }
    [Server]
    private void reBuildBotAfterDead()
    {
        body.GetComponent<Rigidbody>().isKinematic = true;
        body.GetComponent<Rigidbody>().useGravity = false;
        head.GetComponent<Rigidbody>().isKinematic = true;
        head.GetComponent<Rigidbody>().useGravity = false;
        bow.GetComponent<Rigidbody>().isKinematic = true;
        bow.GetComponent<Rigidbody>().useGravity = false;
        bow.GetComponent<BoxCollider>().enabled = false;
        body.transform.eulerAngles = Vector3.zero;
        head.transform.parent = body.transform;
        bow.transform.parent = body.transform;
        head.transform.localPosition = headPos;
        bow.transform.localPosition = bowPos;
        head.transform.rotation = body.transform.rotation;
        bow.transform.rotation = body.transform.rotation;
    }
    [Server]
    public void Spawn(Vector3 spawnLocation)
    {
        curDestination = spawnLocation;
        agent.Warp(spawnLocation);
        RpcSpawnBody(agent.transform.position);
    }
    [ClientRpc]
    private void RpcSpawnBody(Vector3 spawnLocation)
    {
        this.transform.position = spawnLocation;
    }
    //public bool IsMyBodyPart(GameObject part)
    //{
    //    if(body == part) { return true; }
    //    if (head == part) { return true; }
    //    return false;
    //}
    [ClientRpc]
    public void RpcCheckMaterials(Team team)
    {
        CheckMaterials(team);
    }
    public void CheckMaterials(Team team)
    {
        if(team == Team.Blue)
        {
            bow.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueTeam;
            head.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueTeam;
            body.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueTeam; //body
            head.transform.GetChild(0).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueArrow; //eyes
            bow.GetComponent<BotBow>().SetArrowMaterial(ParticipantHelper.PH.BlueArrow); //bow string and arrow
            body.transform.GetChild(2).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueArrow; // fake arrow
        }
        else
        {
            bow.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedTeam;
            head.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedTeam;
            body.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedTeam; //body
            head.transform.GetChild(0).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedArrow; //eyes
            bow.GetComponent<BotBow>().SetArrowMaterial(ParticipantHelper.PH.RedArrow); //bow string and arrow
            body.transform.GetChild(2).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedArrow; // fake arrow
        }
    }
}

