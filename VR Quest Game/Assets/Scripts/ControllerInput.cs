using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerInput : MonoBehaviour {

    //fields
    public SteamVR_Input_Sources Hand;
    private Teleport teleport;
    private Player player;
    private MenuSystem menu;
    private VRRig myVRRig;

    //methods
    void Start()
    {
        myVRRig = this.GetComponent<VRRig>();
        teleport = this.GetComponent<Teleport>();
        menu = this.transform.GetChild(3).GetComponent<MenuSystem>();
    }
    void Update()
    {
        if (teleport != null && player != null)
        {
            SteamVR_Input._default.inActions.Pose.GetLastLocalPosition(SteamVR_Input_Sources.LeftHand);
            //ability selecting
            if (SteamVR_Input._default.inActions.AbilitySelect.GetStateDown(Hand))
            {
                Vector2 fingerPos = SteamVR_Input._default.inActions.AbilityPad.GetAxis(Hand);
                if (fingerPos != Vector2.zero)
                {
                    if (Mathf.Abs(fingerPos.y) >= Mathf.Abs(fingerPos.x))
                    {
                        if (fingerPos.y >= 0) { player.SetOrTriggerAbility(Ability.None); } //up: none
                        else { player.SetOrTriggerAbility(Ability.Teleport); } //down: teleport
                    }
                    else
                    {
                        if (fingerPos.x >= 0) { player.SetOrTriggerAbility(Ability.Mega); } //right: mega
                        else { player.SetOrTriggerAbility(Ability.Explosive); } //left: explosive
                    }
                }
            }

            //small teleport
            if (SteamVR_Input._default.inActions.SmallTeleport.GetStateDown(Hand))
            {
                player.SmallTeleport();
            }

            //grab arrow & bow string
            if (SteamVR_Input._default.inActions.GrabArrow.GetStateDown(Hand)) //grab arrow
            {
                player.GrabOrReleaseArrow();
            }
            if (SteamVR_Input._default.inActions.GrabBowString.GetStateDown(Hand))
            {
                player.AttachArrowToBowString();
            }
            else if (SteamVR_Input._default.inActions.GrabBowString.GetStateUp(Hand))
            {
                player.ReleaseArrowFromBow();
            }
        }

        //open and close menu
        if (menu != null)
        {
            if (SteamVR_Input._default.inActions.Menu.GetStateDown(Hand))
            {
                menu.TurnOnMenu();
            }
            else if (SteamVR_Input._default.inActions.Menu.GetStateUp(Hand))
            {
                menu.TurnOffMenu();
            }
            if (menu.MenuIsOpened)
            {
                menu.transform.rotation = myVRRig.LeftHand.transform.rotation;
                menu.transform.Rotate(90, 0, 0, Space.Self);
                menu.transform.position = myVRRig.LeftHand.transform.position - menu.transform.forward * 0.19f;
                //menu.transform.rotation = player.bowTransform.rotation;
                //menu.transform.position = player.bowTransform.position - player.bowTransform.forward * 0.19f;
            }
        }  
    }
    public void SetPlayer(Player p)
    {
        if(p != null)
        {
            player = p;
        }
    }  
    public bool TeleportTo(Vector3 location, bool atBottom, bool checkSpace, bool animated, int maxHeight)
    {
        return teleport.TeleportTo(location, atBottom, checkSpace, animated, maxHeight);
    }
}
