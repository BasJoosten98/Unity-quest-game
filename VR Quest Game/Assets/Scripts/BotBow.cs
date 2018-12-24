using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BotBow : NetworkBehaviour {

    //fields
    public GameObject arrowPreFab;

    private static float arrowSpeed;

    private ParticipantID owner;
    private LineRenderer lr;
    private Transform[] points;
    private List<GameObject> flyingArrows;
    private GameObject newArrow;
    private Vector3 midOriginalPos;
    private Material m_Arrow;
    private bool bowIsBeingUsed;

    //properties

    public static float ArrowSpeed { get { return arrowSpeed; } }

    //methods
    void Awake()
    {
        lr = this.GetComponent<LineRenderer>();
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        getPoints();
        lr.positionCount = points.Length;
        midOriginalPos = points[1].GetComponent<Transform>().localPosition;
        bowIsBeingUsed = false;
        drawNewPoints();
        flyingArrows = new List<GameObject>();
    }
    void FixedUpdate()
    {
        if (bowIsBeingUsed)
        {
            drawNewPoints();
        }
        if(flyingArrows.Count > 0)
        {
            translateArrows();
        }       
    }
    [Server]
    public void SetOwner(ParticipantID id)
    {
        owner = id;
    } 
    public void SetArrowMaterial(Material m)
    {
        this.lr.material = m;
        this.m_Arrow = m;
    }
    private void translateArrows()
    {
        for (int i = 0; i < flyingArrows.Count; i++)
        {
            if (flyingArrows[i] != null)
            {
                Transform trans = flyingArrows[i].transform;
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
        for (int i = 0; i < points.Length; i++)
        {
            if (i < this.GetComponent<Transform>().childCount)
            {
                points[i] = this.GetComponent<Transform>().GetChild(i).GetComponent<Transform>();
            }
            else
            {
                Debug.Log("getPoints() from bowScript error");
            }
        }
    }
    private void drawNewPoints()
    {
        lr.SetPosition(0, points[0].localPosition);
        lr.SetPosition(1, points[1].localPosition);
        lr.SetPosition(2, points[2].localPosition);
    }
    [Server]
    public GameObject createArrow()
    {
        if (!bowIsBeingUsed)
        {
            bowIsBeingUsed = true;
            newArrow = GameObject.Instantiate(arrowPreFab);            
            newArrow.GetComponent<MeshRenderer>().material = m_Arrow;
            newArrow.GetComponent<Transform>().parent = points[1].GetComponent<Transform>();
            newArrow.GetComponent<Transform>().rotation = points[1].GetComponent<Transform>().rotation;
            newArrow.GetComponent<Transform>().localPosition = Vector3.zero + (Vector3.forward * newArrow.GetComponent<BoxCollider>().size.z / 2);

            return newArrow;
            
            //de bound size is verkeerd doordat Bow een scale van 100 heeft. hierdoor word de bound size van arrow ook 100x groter (world space). dit is niet het geval met collider.size (local space).
            //Debug.Log("Mesh renderer: " + newArrow.GetComponent<MeshRenderer>().bounds.size.z);
            //Debug.Log("Renderer: " + newArrow.GetComponent<Renderer>().bounds.size.z);
            //Debug.Log("Box Collider: " + newArrow.GetComponent<BoxCollider>().bounds.size.z);
            //Debug.Log("Box Collider: " + newArrow.GetComponent<BoxCollider>().size.z);
        }
        return null;
    }
    public void BotShotForClient(GameObject arrow)
    {
        newArrow = arrow;
        bowIsBeingUsed = true;
        StartCoroutine("botShootArrow");
    }
    private IEnumerator botShootArrow()
    {
        Vector3 currentPos = points[1].localPosition;
        Vector3 destination = currentPos - Vector3.forward/ 2f;
        float t = 0f;

        while (t < 1 && bowIsBeingUsed) //pull string
        {
            t += Time.deltaTime;
            points[1].localPosition = Vector3.Lerp(currentPos, destination, t);
            yield return null;
        }

        int timeIncrease = 20; // 1 sec divided by timeIncrease (how long releasing the string takes AND how fast the arrow goes)
        t = 0f;
        currentPos = points[1].localPosition; //string pull start point
        destination = currentPos + Vector3.forward / 2f; //string pull end point
        if(arrowSpeed <= 0) { arrowSpeed = Mathf.Abs(Mathf.Abs(destination.z) - Mathf.Abs(currentPos.z)) *this.transform.parent.localScale.z * timeIncrease; } //calculation has been improved

        while (t < 1 && bowIsBeingUsed) //release string
        {
            t += Time.deltaTime * timeIncrease;
            points[1].localPosition = Vector3.Lerp(currentPos, destination, t);
            yield return null;
        }

        if (newArrow != null && bowIsBeingUsed) //prevent errors with resetbow method
        {
            newArrow.GetComponent<Transform>().parent = null;
            newArrow.GetComponent<Arrow>().SetArrow(arrowSpeed, 7/arrowSpeed);
            flyingArrows.Add(newArrow);
            newArrow = null;
        }
        else { Destroy(newArrow); newArrow = null; }

        drawNewPoints();
        points[1].GetComponent<Transform>().localPosition = midOriginalPos;
        bowIsBeingUsed = false;

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
