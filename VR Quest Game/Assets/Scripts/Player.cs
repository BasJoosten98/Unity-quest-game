using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.Networking;

public class Player : NetworkBehaviour 
{

    //fields
    public GameObject ArrowPrefab;
    private GameObject bow;
    private GameObject arrow;
    private GameObject head;
    private GameObject body;
    private GameObject deadFilter;
    private VRRig myVRRig;
    private bool trackVRRig;

    //fields for faster calculations
    private static float halfArrowLenght;
    private static float bodyBottomYCalculationPart;
    private static Vector3 newBodyPosCalculationPart;
    private static Vector3 bodyBowPositionCalculationPart;
    private static float bodyBottomToFloorCalculationPart;

    //properties
    public Transform bowTransform { get { return bow.transform; } }

    //methods
    private void Start()
    {
        setVRRigAndBodyParts();

        //pre calculations
        if (halfArrowLenght <= 0)
        {
            halfArrowLenght = ArrowPrefab.GetComponent<BoxCollider>().size.z / 2 * ArrowPrefab.transform.localScale.z;
            newBodyPosCalculationPart = Vector3.up * body.GetComponent<BoxCollider>().size.z * 0.7f;
            bodyBottomYCalculationPart = body.GetComponent<BoxCollider>().size.y / 2 * body.transform.localScale.z;
            bodyBowPositionCalculationPart = body.transform.up * body.GetComponent<BoxCollider>().size.z * 0.6f;
            bodyBottomToFloorCalculationPart = body.GetComponent<BoxCollider>().size.y * body.transform.localScale.z;
        }

        if (isServer)
        {
            if(!GameStartManager.GSM.SetGame(120, 2, true, GameMode.Team_deathmatch))
            {
                Debug.LogWarning("GSM failed to set game");
            }
            
        }

        if (isLocalPlayer)
        {
            if (myVRRig != null) { myVRRig.ConInput.SetPlayer(this); }
            bow.GetComponent<PlayerBow>().ThisIsMyBow();
            ParticipantHelper.PH.CmdRegisterPlayer(this.gameObject.GetComponent<NetworkIdentity>().netId, "Bas Joosten");
            //bow.GetComponent<PlayerBow>().SetOwner(this.GetComponent<ParticipantID>()); //clients don't own an ID, the sever does!
            //checkMaterials(); //will be done by the server in CmdRegisterPlayer
            //myVRRig.Head.GetComponent<BoxCollider>().enabled = false; //is not possible
            head.GetComponent<MeshRenderer>().enabled = false;
            head.GetComponent<BoxCollider>().enabled = false;
        }
    }
    public void SetOrTriggerAbility(Ability a)
    {
        if (isLocalPlayer)
        {
            GameObject arrowWithAbility = bow.GetComponent<PlayerBow>().getFlyingArrowWithAbility();
            if (arrow != null) //you have a arrow in hand
            {
                if (arrowWithAbility == null) //there is no exisiting arrow (in air) with an ability
                {
                    arrow.GetComponent<Arrow>().SetAbility(a);
                }
                else //there is a flying arrow with an ability, TRIGGER IT
                {
                    triggerAbility(arrowWithAbility);
                }
            }
            else if (arrowWithAbility != null) //there is a flying arrow with an ability, TRIGGER IT
            {
                triggerAbility(arrowWithAbility);
            }
        }
    }
    private void triggerAbility(GameObject arrowWithAbility)
    {
        if (arrowWithAbility.GetComponent<Arrow>().Ability == Ability.Teleport)
        {
            if (myVRRig.ConInput.TeleportTo(arrowWithAbility.transform.position, false, true, true, 6)) //teleporting succeeded!
            {
                Destroy(arrowWithAbility);
            }
        }
        else
        {
            arrowWithAbility.GetComponent<Arrow>().TriggerAbility();
        }
    } 
    private void Update()
    {
        if(trackVRRig)
        {
            if (myVRRig != null)
            {
                if (isLocalPlayer)
                {
                    //head tracking
                    head.transform.position = myVRRig.Head.transform.position - myVRRig.Head.transform.forward.normalized * 0.2f;
                    head.transform.rotation = myVRRig.Head.transform.rotation;

                    //bow trackking
                    if (!bow.GetComponent<PlayerBow>().BowIsBeingUsed)
                    {
                        bow.transform.rotation = myVRRig.LeftHand.transform.rotation;
                        bow.transform.Rotate(90, 0, 0, Space.Self);
                        bow.transform.position = myVRRig.LeftHand.transform.position;
                    }
                    else if (arrow != null)
                    {
                        bow.transform.position = myVRRig.LeftHand.transform.position;
                        bow.GetComponent<PlayerBow>().midPoint.position = myVRRig.RightHand.transform.position;
                        bow.transform.localRotation = Quaternion.LookRotation(bow.transform.position - bow.GetComponent<PlayerBow>().midPoint.position, Vector3.up);
                    }

                    //arrow tracking
                    if (arrow != null)
                    {
                        if (!bow.GetComponent<PlayerBow>().BowIsBeingUsed)
                        {
                            arrow.transform.rotation = myVRRig.RightHand.transform.rotation;
                            arrow.transform.position = myVRRig.RightHand.transform.position + arrow.transform.forward.normalized * halfArrowLenght;
                        }
                    }

                    //dead filter
                    if (deadFilter.activeInHierarchy)
                    {
                        deadFilter.transform.position = myVRRig.Head.transform.position + myVRRig.Head.transform.forward.normalized * 0.3f;
                        deadFilter.transform.rotation = myVRRig.Head.transform.rotation;
                        deadFilter.transform.Rotate(-90, 0, 0);
                    }
                }
            }

            //body tracking
            Vector3 newBodyPos = head.transform.position - newBodyPosCalculationPart;
            float bodyBottomY = newBodyPos.y - bodyBottomYCalculationPart;
            if (bodyBottomY >= myVRRig.transform.position.y) //body is above floor
            {
                body.transform.position = newBodyPos;
                body.transform.eulerAngles = new Vector3(0, head.transform.eulerAngles.y, 0);
            }
            else //prevent letting body go trough the floor
            {
                Vector3 headDirection = head.transform.forward.normalized;
                float bodyBottomToFloorPercentage = (myVRRig.transform.position.y - bodyBottomY) / bodyBottomToFloorCalculationPart;
                if (bodyBottomToFloorPercentage > 1) { bodyBottomToFloorPercentage = 1; }
                if (headDirection.y <= 0) //looking down, bring body to the back
                {
                    body.transform.eulerAngles = new Vector3(0, head.transform.eulerAngles.y, 0);
                    body.transform.localEulerAngles = new Vector3(bodyBottomToFloorPercentage * 90, body.transform.localEulerAngles.y, body.transform.localEulerAngles.z);
                    body.transform.position = head.transform.position - bodyBowPositionCalculationPart;
                }
                else //looking up, bring body to the front
                {
                    body.transform.eulerAngles = new Vector3(0, head.transform.eulerAngles.y, 0);
                    body.transform.localEulerAngles = new Vector3(-bodyBottomToFloorPercentage * 90, body.transform.localEulerAngles.y, body.transform.localEulerAngles.z);
                    body.transform.position = head.transform.position - bodyBowPositionCalculationPart;
                }
            }
        }
    } 
    private void setVRRigAndBodyParts()
    {
        trackVRRig = true;
        if (isLocalPlayer)
        {
            GameObject VRRig = GameObject.Find("[CameraRig]");
            if (VRRig != null)
            {
                myVRRig = VRRig.GetComponent<VRRig>();
                trackVRRig = true;
            }
            else { Debug.LogWarning("BodyAttachment Failed: No CameraRig Found!"); trackVRRig = false; }
        }
        head = this.transform.GetChild(1).gameObject;
        body = this.transform.GetChild(0).gameObject;
        bow = this.transform.GetChild(2).gameObject;
        deadFilter = this.transform.GetChild(3).gameObject;
        
    } 
    public void GrabOrReleaseArrow()
    {
        if (isLocalPlayer)
        {
            if (arrow == null)
            {
                Vector2 relativeHeadToHand = new Vector2(myVRRig.RightHand.transform.position.x - myVRRig.Head.transform.position.x, myVRRig.RightHand.transform.position.z - myVRRig.Head.transform.position.z);
                Vector2 headDirection = new Vector2(myVRRig.Head.transform.forward.x, myVRRig.Head.transform.forward.z);
                float angle = Vector2.Angle(headDirection, relativeHeadToHand);
                if (angle >= 110f && Mathf.Abs(myVRRig.Head.transform.position.y - myVRRig.RightHand.transform.position.y) <= 0.3f)
                {
                    CmdSpawnArrowInHand();
                }
            }
            else
            {
                CmdRemoveArrowInHand();
            }
        }
        else { Debug.LogWarning("Non local player: Player.GrabOrReleaseArrow " + Time.frameCount); }
    }
    [Command]
    private void CmdSpawnArrowInHand()
    {
        //GameObject newArrow = GameObject.Instantiate(ArrowPrefab, myVRRig.RightHand.transform.position, myVRRig.RightHand.transform.rotation); not possible(myVRRig is unknown on server)
        GameObject newArrow = GameObject.Instantiate(ArrowPrefab, transform.position  + transform.forward.normalized, transform.rotation);
        newArrow.GetComponent<MeshRenderer>().material = body.transform.GetChild(0).GetComponent<MeshRenderer>().material; //PropArrow Material
        newArrow.GetComponent<Arrow>().SetShooter(ParticipantHelper.PH.getIDByBodyPart(this.gameObject)); 
        NetworkServer.SpawnWithClientAuthority(newArrow, this.GetComponent<NetworkIdentity>().connectionToClient);

        RpcSetArrow(newArrow.GetComponent<NetworkIdentity>().netId);
    }
    [ClientRpc]
    private void RpcSetArrow(NetworkInstanceId netId)
    {
        if (isLocalPlayer)
        {
            GameObject newArrow = ClientScene.FindLocalObject(netId);
            arrow = newArrow;
            arrow.GetComponent<Arrow>().ThisIsMyArrow();
        }
    }
    [Command]
    private void CmdRemoveArrowInHand()
    {
        if (arrow != null)
        {
            NetworkServer.Destroy(arrow);        
        }
    } 
    public void AttachArrowToBowString()
    {
        if (isLocalPlayer)
        {
            if (arrow != null)
            {
                bow.GetComponent<PlayerBow>().AttachArrowToString(arrow, myVRRig.RightHand, halfArrowLenght);
            }
        }
    }
    public void ReleaseArrowFromBow()
    {
        if (isLocalPlayer)
        {
            if (arrow != null && bow.GetComponent<PlayerBow>().BowIsBeingUsed)
            {
                GameObject temp = arrow; //needed for startCoroutine being faster than ReleaseString() ERROR
                arrow = null;

                float distance;
                if (!bow.GetComponent<PlayerBow>().ReleaseString(out distance)) //arrow can not be shot away
                {
                    arrow = temp;
                }
                else //arrow will be shot away
                {
                    CmdstartShootArrow(distance);
                }
            }
        }
        else { Debug.LogWarning("Non local player: Player.ReleaseArrowFromBow " + Time.frameCount); }
    }
    [Command]
    private void CmdstartShootArrow(float distance)
    {
        RpcstartShootArrow(distance);
    }
    [ClientRpc]
    private void RpcstartShootArrow(float distance)
    {
        if (!isLocalPlayer)
        {
            StartCoroutine("ShootArrow", distance);
        }
    }
    public void AttachArrowToHand(GameObject Arrow)
    {
        if (isLocalPlayer)
        {
            if (Arrow != null) { this.arrow = Arrow; }
        }
        else { Debug.LogWarning("Non local player: Player.AttachArrowToHand " + Time.frameCount); }
    }  
    public void SmallTeleport()
    {
        if (isLocalPlayer)
        {
            RaycastHit hit;
            int layerMask = 1;
            Vector3 bowDirection = bow.transform.position - myVRRig.Head.transform.position;
            float bowDistance = Vector3.Distance(bow.transform.position, myVRRig.Head.transform.position);

            //if (!Physics.Raycast(myVRRig.Head.transform.position, bowDirection, out hit, bowDistance, layerMask)) //noting is in the way between bow and head
            if (!Physics.BoxCast(myVRRig.Head.transform.position, new Vector3(0.05f, 0.05f, 0.05f), bowDirection, out hit, Quaternion.Euler(0, 0, 0), bowDistance, layerMask))
            {
                Vector3 spawnLocation;
                float rayDistance = 1.5f; //max teleporting distance
                if (Physics.Raycast(bow.transform.position, bow.transform.forward, out hit, rayDistance))
                {
                    spawnLocation = hit.point + hit.normal.normalized * 0.2f;
                }
                else
                {
                    spawnLocation = bow.transform.position + bow.transform.forward.normalized * rayDistance;
                }
                for (int i = 3; i > 0; i--)
                {
                    if (myVRRig.ConInput.TeleportTo(bow.transform.position + (spawnLocation - bow.transform.position) * i / 3, false, true, true, 3))
                    {
                        break;
                    }
                }
            }
        }
        else { Debug.LogWarning("Non local player: PlayerBow.SmallTeleport " + Time.frameCount); }
    }
    [ClientRpc]
    public void RpcSpawn(Vector3 location)
    {
        spawn(location);
    }
    private void spawn(Vector3 location)
    {
        if (isLocalPlayer)
        {
            myVRRig.ConInput.TeleportTo(location, false, false, false, 6);
        }
    }
    [ClientRpc]
    public void RpcCheckMaterials(Team team)
    {
        CheckMaterials(team);
    }
    private void CheckMaterials(Team team)
    {
        if (team == Team.Blue)
        {
            bow.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueTeam;
            head.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueTeam;
            body.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueTeam; //body
            head.transform.GetChild(0).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueArrow; //eyes
            bow.GetComponent<PlayerBow>().SetStringMaterial(ParticipantHelper.PH.BlueArrow); //bow string and arrow
            body.transform.GetChild(0).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.BlueArrow; // fake arrow
        }
        else
        {
            bow.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedTeam;
            head.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedTeam;
            body.GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedTeam; //body
            head.transform.GetChild(0).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedArrow; //eyes
            bow.GetComponent<PlayerBow>().SetStringMaterial(ParticipantHelper.PH.RedArrow); //bow string and arrow
            body.transform.GetChild(0).GetComponent<MeshRenderer>().material = ParticipantHelper.PH.RedArrow; // fake arrow
        }
    }
    [ClientRpc]
    public void RpcDie()
    {
        Die();
    } 
    private void Die() 
    {
        trackVRRig = false;
        deadFilter.SetActive(true);
        if (isLocalPlayer)
        {
            head.GetComponent<MeshRenderer>().enabled = true;
        }
        bow.GetComponent<PlayerBow>().ResetBow();
        head.GetComponent<BoxCollider>().enabled = true;
        body.GetComponent<Rigidbody>().isKinematic = false;
        body.GetComponent<Rigidbody>().useGravity = true;
        body.GetComponent<BoxCollider>().isTrigger = false;
        head.GetComponent<Rigidbody>().isKinematic = false;
        head.GetComponent<Rigidbody>().useGravity = true;
        head.GetComponent<BoxCollider>().isTrigger = false;
        bow.GetComponent<Rigidbody>().isKinematic = false;
        bow.GetComponent<Rigidbody>().useGravity = true;
        bow.GetComponent<BoxCollider>().enabled = true;
        CmdRemoveArrowInHand();        
    }
    [ClientRpc]
    public void RpcRespawn(Vector3 spawnLocation)
    {       
        spawn(spawnLocation);
        Respawn(spawnLocation);
    }
    private void Respawn(Vector3 spawnLocation)
    {
        deadFilter.SetActive(false);
        body.GetComponent<Rigidbody>().isKinematic = true;
        body.GetComponent<Rigidbody>().useGravity = false;
        body.GetComponent<BoxCollider>().isTrigger = true;
        head.GetComponent<Rigidbody>().isKinematic = true;
        head.GetComponent<Rigidbody>().useGravity = false;
        head.GetComponent<BoxCollider>().isTrigger = true;
        bow.GetComponent<Rigidbody>().isKinematic = true;
        bow.GetComponent<Rigidbody>().useGravity = false;
        bow.GetComponent<BoxCollider>().enabled = false;
        head.GetComponent<BoxCollider>().enabled = false;

        if (isLocalPlayer)
        {
            head.GetComponent<MeshRenderer>().enabled = false;
        }
        trackVRRig = true;
    }
}
