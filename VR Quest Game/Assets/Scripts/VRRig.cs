using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRig : MonoBehaviour {

    //fields
    private GameObject head;
    private GameObject leftHand;
    private GameObject rightHand;
    private ControllerInput conInput;

    //properties
    public GameObject Head { get { return this.head; } }
    public GameObject LeftHand { get { return this.leftHand; } }
    public GameObject RightHand { get { return this.rightHand; } }
    public ControllerInput ConInput { get { return this.conInput; } }

    //methods
    void Awake () {
        setRig();

	}
	public void SwapHand()
    {
        GameObject temp;
        temp = leftHand;
        leftHand = rightHand;
        rightHand = temp;
    }
    private void setRig()
    {
        this.head = this.transform.GetChild(2).gameObject;
        this.rightHand = this.transform.GetChild(1).gameObject;
        this.leftHand = this.transform.GetChild(0).gameObject;
        this.conInput = this.GetComponent<ControllerInput>();
    } 
}
