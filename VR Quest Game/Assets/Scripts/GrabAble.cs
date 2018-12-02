using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAble : MonoBehaviour {

    public bool uniqueHold; //object needs to be hold in a specific way (like a bow and arrow)
    public Vector3 uniqueHoldPosition; //this is relative to origin
    public Vector3 uniqueHoldRotation; //this is relative to origin
    private Rigidbody rb;
    private bool[] rbSettings;
    private bool hasBeenGrabbed;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rbSettings = new bool[] { rb.useGravity, rb.isKinematic };
    }

    public bool Grab(Transform Origin)
    {
        if (!hasBeenGrabbed)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            this.transform.parent = Origin;
            if (uniqueHold)
            {
                this.transform.localPosition = uniqueHoldPosition;
                this.transform.localRotation = Quaternion.Euler(uniqueHoldRotation);
            }
            hasBeenGrabbed = true;
            return true;
        }
        return false;
        
    }
    public bool Release(Vector3 Velocity)
    {
        if (hasBeenGrabbed)
        {
            rb.useGravity = rbSettings[0];
            rb.isKinematic = rbSettings[1];
            this.transform.parent = null;
            rb.velocity = Velocity;
            hasBeenGrabbed = false;
            Debug.Log("From " + Velocity + " to " + this.rb.velocity);
            return true;
        }
        return false;       
    }
}
