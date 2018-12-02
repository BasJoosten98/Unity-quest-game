using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerBow : NetworkBehaviour
{

    //fields

    private float arrowSpeed;
    //private ParticipantID owner;
    private LineRenderer lr;
    private Transform[] points;
    private List<GameObject> flyingArrows;
    private GameObject newArrow;
    private Vector3 midOriginalPos;

    [SyncVar]
    private bool bowIsBeingUsed;

    //properties
    public bool BowIsBeingUsed { get { return this.bowIsBeingUsed; } }
    public Transform midPoint { get { return this.points[1]; } }

    //methods
    void Start()
    {
        getPoints();

        lr = this.GetComponent<LineRenderer>();
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;       
        lr.positionCount = points.Length;

        midOriginalPos = points[1].localPosition;    
        flyingArrows = new List<GameObject>();

        drawNewPoints();
    }
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            if (bowIsBeingUsed)
            {
                if (newArrow != null) //arrow exists in bow (not dropped or removed)
                {
                    if (points[1].localPosition.z > midOriginalPos.z)
                    {
                        resetBowString();
                        newArrow = null;
                        bowIsBeingUsed = false;
                    }
                    drawNewPoints();
                }
                else
                {
                    resetBowString();
                    bowIsBeingUsed = false;
                }
            }
            if (flyingArrows.Count > 0)
            {
                translateArrows();
            }
        }
        else
        {
            if (BowIsBeingUsed)
            {
                drawNewPoints();
            }
            if (flyingArrows.Count > 0)
            {
                translateArrows();
            }
        }      
    }
    private void resetBowString()
    {
        bowIsBeingUsed = false;
        points[1].localPosition = midOriginalPos;
        drawNewPoints();
    }
    //public void SetOwner(ParticipantID id)
    //{
    //    if (isLocalPlayer)
    //    {
    //        if (owner == null)
    //        {
    //            owner = id;
    //        }
    //    }
    //    else { Debug.LogWarning("Non local player: PlayerBow.SetOwner " + Time.frameCount); }
    //}
    public void SetStringMaterial(Material m)
    {
         this.lr.material = m;
    } 
    public GameObject getFlyingArrowWithAbility()
    {
        if (isLocalPlayer)
        {
            for (int i = 0; i < flyingArrows.Count; i++)
            {
                if (flyingArrows[i].GetComponent<Arrow>().Ability != Ability.None)
                {
                    return flyingArrows[i];
                }
            }
        }
        else { Debug.LogWarning("Non local player: PlayerBow.getFlyingArrowWithAbility " + Time.frameCount); }
        return null;
    }
    private void translateArrows()
    {
        for (int i = 0; i < flyingArrows.Count; i++)
        {
            if (flyingArrows[i] != null)
            {
                Transform trans = flyingArrows[i].GetComponent<Transform>();
                Vector3 direction = Vector3.forward * flyingArrows[i].GetComponent<Arrow>().Speed * Time.deltaTime;
                trans.Translate(direction, trans);
            }
            else
            {
                flyingArrows.Remove(flyingArrows[i]);
            }
        }
    }
    private void getPoints()
    {
        points = new Transform[3];
        points[0] = this.transform.GetChild(0); //Top
        points[1] = this.transform.GetChild(1); //Mid
        points[2] = this.transform.GetChild(2); //Bottom
    }
    private void drawNewPoints()
    {
        lr.SetPosition(0, points[0].localPosition);
        lr.SetPosition(1, points[1].localPosition);
        lr.SetPosition(2, points[2].localPosition);
    } 
    public bool AttachArrowToString(GameObject Arrow, GameObject ArrowHand, float halfArrowLenght)
    {
        if(newArrow == null && !bowIsBeingUsed)
        {
            float distanceBetweenHandAndString = Vector3.Distance(ArrowHand.transform.position, midPoint.transform.position);
            if(distanceBetweenHandAndString <= 0.3f)
            {
                midPoint.position = ArrowHand.transform.position;
                if(midPoint.localPosition.z < -0.19f)
                {
                    newArrow = Arrow;
                    newArrow.transform.parent = midPoint.transform;
                    newArrow.transform.localEulerAngles = Vector3.zero;
                    newArrow.transform.position = midPoint.transform.position + newArrow.transform.forward * halfArrowLenght;
                    bowIsBeingUsed = true;
                    return true;
                }               
            }
        }
        return false;
    }
    public bool ReleaseString()
    {
        if (bowIsBeingUsed)
        {
            Vector3 currentPos = midPoint.localPosition; //string pull start point
            Vector3 destination = midOriginalPos; //string pull end point
            float distance = Mathf.Abs(Mathf.Abs(destination.z) - Mathf.Abs(currentPos.z));
            if (distance >= 0.35f) //string is streched far enough, SHOOT IT
            {
                CmdstartShootArrow(distance);
                StartCoroutine("ShootArrow");
                return true;
            }          
        }
        newArrow.transform.parent = null;
        this.transform.parent.GetComponent<Player>().AttachArrowToHand(newArrow);
        newArrow = null;
        return false;
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
    private IEnumerator ShootArrow(float distance)
    {
        Vector3 currentPos;
        Vector3 destination;
        float t = 0f;
        int timeIncrease = 20; // 1 sec divided by timeIncrease (how long releasing the string takes AND how fast the arrow goes)
        t = 0f;

        currentPos = midPoint.localPosition; //string pull start point
        destination = midOriginalPos; //string pull end point
        //arrowSpeed = Mathf.Abs(distance * this.transform.localScale.z * 2 * timeIncrease);
        arrowSpeed = Mathf.Abs(distance * this.transform.localScale.z * timeIncrease);  //calculation has been improved
        while (t < 1 && bowIsBeingUsed) //release string
        {
            t += Time.deltaTime * timeIncrease;
            points[1].localPosition = Vector3.Lerp(currentPos, destination, t);
            yield return null;
        }
        if (newArrow != null && bowIsBeingUsed) //prevent errors with resetbow method
        {
            newArrow.transform.parent = null;
            newArrow.GetComponent<Arrow>().SetArrow(arrowSpeed, 7/arrowSpeed);
            flyingArrows.Add(newArrow);
            newArrow = null;
        }
        else { Destroy(newArrow); newArrow = null; }
        resetBowString();
    }
    public void ResetBow()
    {
        points[1].GetComponent<Transform>().localPosition = midOriginalPos;
        bowIsBeingUsed = false;
        if (newArrow != null)
        {
            if (flyingArrows.Contains(newArrow))
            {
                flyingArrows.Remove(newArrow);
                Destroy(newArrow);
                newArrow = null;
            }
            //no need for destroying arrow if it is not in the flyingArrows list. this is done in Ienumerator releaseString.
        }
    }
}
