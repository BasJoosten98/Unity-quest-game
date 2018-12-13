using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {

    //fields
    private float allowedSurfaceAngle = 45;
    private VRRig myVRRig;
    private bool busyWithAnimatedTeleport;

    private void Awake()
    {
        this.myVRRig = this.GetComponent<VRRig>();
    }
    //methods
    public bool TeleportTo(Vector3 location, bool atBottom, bool checkSpace, bool animated, int maxHeight)
    {
        Vector3 teleportLocation = location; 
        if (!atBottom)
        {
            int layerMask = 1; //evironment
            RaycastHit hit;
            if (Physics.Raycast(teleportLocation + Vector3.up*0.1f, -Vector3.up, out hit, maxHeight, layerMask))
            {
                if (measureSurface(hit))
                {
                    teleportLocation = hit.point;
                }
                else { teleportLocation = Vector3.zero; Debug.Log("Teleport failed: surface is too steep"); }

            }
            else { teleportLocation = Vector3.zero; Debug.Log("Teleport failed: no surface found"); }
        }
        if (checkSpace && teleportLocation != Vector3.zero)
        {
            int layerMask = 1; //evironment
            RaycastHit boxHit;
            float maxDistance = myVRRig.Head.transform.position.y - myVRRig.transform.position.y - 0.7f;
            if (maxDistance < 0) { maxDistance = 0; }
            if (Physics.BoxCast(teleportLocation + Vector3.up * 0.7f, new Vector3(0.2f, 0.2f, 0.2f), Vector3.up, out boxHit, Quaternion.Euler(0, 0, 0), maxDistance, layerMask)) 
            {
                //there is something in the way, DON'T TELEPORT
                teleportLocation = Vector3.zero;
                Debug.Log("Teleport failed: too little space for player (evironment)");
            }
            layerMask = 11; //other players or himself
            if (Physics.BoxCast(teleportLocation + Vector3.up * 0.7f, new Vector3(0.2f, 0.2f, 0.2f), Vector3.up, out boxHit, Quaternion.Euler(0, 0, 0), maxDistance, layerMask))
            {
                //there is something in the way, DON'T TELEPORT
                teleportLocation = Vector3.zero;
                Debug.Log("Teleport failed: too little space for player (other player)");
            }
        }
        if (teleportLocation != Vector3.zero) //doing the actual teleportation
        {
            
            if (animated)
            {
                if (busyWithAnimatedTeleport) { StopCoroutine("animatedTeleportEnumerator"); }
                animatedTeleport(teleportLocation);
            }
            else
            {
                if (busyWithAnimatedTeleport) { StopCoroutine("animatedTeleportEnumerator"); }
                flashTeleport(teleportLocation);
            }
            return true;
        }
        return false;
    }
    private void flashTeleport(Vector3 teleportLocation)
    {
        Vector3 headDifference = myVRRig.transform.position - myVRRig.Head.transform.position;
        headDifference = new Vector3(headDifference.x, 0, headDifference.z);
        myVRRig.transform.position = teleportLocation + headDifference;
    }
    private void animatedTeleport(Vector3 teleportLocation)
    {
        Vector3 headDifference = myVRRig.transform.position - myVRRig.Head.transform.position;
        headDifference = new Vector3(headDifference.x, 0, headDifference.z);
        //from = myVRRig position
        Vector3 goingTo = teleportLocation + headDifference;
        StartCoroutine("animatedTeleportEnumerator", goingTo);
    }
    private IEnumerator animatedTeleportEnumerator(Vector3 destination)
    {
        busyWithAnimatedTeleport = true;
        float t = 0f;
        while (t < 1) //pull string
        {
            t += Time.deltaTime;
            myVRRig.transform.position = Vector3.Lerp(myVRRig.transform.position, destination, t);
            yield return null;
        }
        busyWithAnimatedTeleport = false;
    }
    private bool measureSurface(RaycastHit hit)
    {
        float angleOfSurface = Vector3.Angle(Vector3.up, hit.normal);
        if (angleOfSurface <= allowedSurfaceAngle)
        {
            return true;
        }
        return false;
    }
}
