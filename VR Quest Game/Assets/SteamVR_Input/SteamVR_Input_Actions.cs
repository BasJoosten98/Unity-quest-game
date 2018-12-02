// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Input
    {
        
        public static SteamVR_Action_Pose __actions_default_in_Pose;
        
        public static SteamVR_Action_Boolean __actions_default_in_GrabArrow;
        
        public static SteamVR_Action_Boolean __actions_default_in_GrabBowString;
        
        public static SteamVR_Action_Boolean __actions_default_in_Menu;
        
        public static SteamVR_Action_Vector2 __actions_default_in_AbilityPad;
        
        public static SteamVR_Action_Boolean __actions_default_in_AbilitySelect;
        
        public static SteamVR_Action_Boolean __actions_default_in_SmallTeleport;
        
        public static SteamVR_Action_Vibration __actions_default_out_Haptic;
        
        public static void Dynamic_InitializeActions()
        {
            SteamVR_Input.__actions_default_in_Pose.Initialize();
            SteamVR_Input.__actions_default_in_GrabArrow.Initialize();
            SteamVR_Input.__actions_default_in_GrabBowString.Initialize();
            SteamVR_Input.__actions_default_in_Menu.Initialize();
            SteamVR_Input.__actions_default_in_AbilityPad.Initialize();
            SteamVR_Input.__actions_default_in_AbilitySelect.Initialize();
            SteamVR_Input.__actions_default_in_SmallTeleport.Initialize();
            SteamVR_Input.__actions_default_out_Haptic.Initialize();
        }
        
        public static void Dynamic_InitializeInstanceActions()
        {
            Valve.VR.SteamVR_Input.__actions_default_in_Pose = ((SteamVR_Action_Pose)(SteamVR_Input_References.GetAction("__actions_default_in_Pose")));
            Valve.VR.SteamVR_Input.__actions_default_in_GrabArrow = ((SteamVR_Action_Boolean)(SteamVR_Input_References.GetAction("__actions_default_in_GrabArrow")));
            Valve.VR.SteamVR_Input.__actions_default_in_GrabBowString = ((SteamVR_Action_Boolean)(SteamVR_Input_References.GetAction("__actions_default_in_GrabBowString")));
            Valve.VR.SteamVR_Input.__actions_default_in_Menu = ((SteamVR_Action_Boolean)(SteamVR_Input_References.GetAction("__actions_default_in_Menu")));
            Valve.VR.SteamVR_Input.__actions_default_in_AbilityPad = ((SteamVR_Action_Vector2)(SteamVR_Input_References.GetAction("__actions_default_in_AbilityPad")));
            Valve.VR.SteamVR_Input.__actions_default_in_AbilitySelect = ((SteamVR_Action_Boolean)(SteamVR_Input_References.GetAction("__actions_default_in_AbilitySelect")));
            Valve.VR.SteamVR_Input.__actions_default_in_SmallTeleport = ((SteamVR_Action_Boolean)(SteamVR_Input_References.GetAction("__actions_default_in_SmallTeleport")));
            Valve.VR.SteamVR_Input.__actions_default_out_Haptic = ((SteamVR_Action_Vibration)(SteamVR_Input_References.GetAction("__actions_default_out_Haptic")));
            Valve.VR.SteamVR_Input.actions = new Valve.VR.SteamVR_Action[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_in_Pose,
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabArrow,
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabBowString,
                    Valve.VR.SteamVR_Input.__actions_default_in_Menu,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilityPad,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilitySelect,
                    Valve.VR.SteamVR_Input.__actions_default_in_SmallTeleport,
                    Valve.VR.SteamVR_Input.__actions_default_out_Haptic};
            Valve.VR.SteamVR_Input.actionsIn = new Valve.VR.SteamVR_Action_In[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_in_Pose,
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabArrow,
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabBowString,
                    Valve.VR.SteamVR_Input.__actions_default_in_Menu,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilityPad,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilitySelect,
                    Valve.VR.SteamVR_Input.__actions_default_in_SmallTeleport};
            Valve.VR.SteamVR_Input.actionsOut = new Valve.VR.SteamVR_Action_Out[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_out_Haptic};
            Valve.VR.SteamVR_Input.actionsVibration = new Valve.VR.SteamVR_Action_Vibration[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_out_Haptic};
            Valve.VR.SteamVR_Input.actionsPose = new Valve.VR.SteamVR_Action_Pose[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_in_Pose};
            Valve.VR.SteamVR_Input.actionsBoolean = new Valve.VR.SteamVR_Action_Boolean[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabArrow,
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabBowString,
                    Valve.VR.SteamVR_Input.__actions_default_in_Menu,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilitySelect,
                    Valve.VR.SteamVR_Input.__actions_default_in_SmallTeleport};
            Valve.VR.SteamVR_Input.actionsSingle = new Valve.VR.SteamVR_Action_Single[0];
            Valve.VR.SteamVR_Input.actionsVector2 = new Valve.VR.SteamVR_Action_Vector2[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilityPad};
            Valve.VR.SteamVR_Input.actionsVector3 = new Valve.VR.SteamVR_Action_Vector3[0];
            Valve.VR.SteamVR_Input.actionsSkeleton = new Valve.VR.SteamVR_Action_Skeleton[0];
            Valve.VR.SteamVR_Input.actionsNonPoseNonSkeletonIn = new Valve.VR.SteamVR_Action_In[]
            {
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabArrow,
                    Valve.VR.SteamVR_Input.__actions_default_in_GrabBowString,
                    Valve.VR.SteamVR_Input.__actions_default_in_Menu,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilityPad,
                    Valve.VR.SteamVR_Input.__actions_default_in_AbilitySelect,
                    Valve.VR.SteamVR_Input.__actions_default_in_SmallTeleport};
        }
    }
}
