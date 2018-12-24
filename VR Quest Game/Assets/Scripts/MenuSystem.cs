using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour {

    //fields
    public GameObject participantLine;
    public Transform participantList; 
    private RectTransform rtpl;
    private int updateCounter;
    private bool menuIsOpened;

    //properties
    public bool MenuIsOpened { get { return menuIsOpened; } }
    //methods
    private void Start()
    {
        menuIsOpened = true;
        rtpl = participantLine.GetComponent<RectTransform>();
        createParticipantLines(ScoreboardSystem.TeamSize);
        TurnOffMenu();
    }
    private void FixedUpdate()
    {
        if (menuIsOpened)
        {
            if (updateCounter >= 100)
            {
                updateCounter = 0;
                updateStats();               
            }
            updateCounter++;
        }
    }
    private void updateStatsOLD()
    {
        //GameObject child;
        //for (int i = 0; i < ScoreboardSystem.TeamSize * 2; i++)
        //{
        //    if (ScoreboardSystem.IDs[i] != null)
        //    {
        //        int spawnNumber = ScoreboardSystem.IDs[i].SpawnNumber;
        //        if (ScoreboardSystem.IDs[i].Team == Team.Blue)
        //        {
        //            if (spawnNumber < this.transform.childCount)
        //            {
        //                child = this.transform.GetChild(spawnNumber).gameObject;
        //                child.GetComponent<Text>().text = ScoreboardSystem.IDs[i].Name;
        //                child.transform.GetChild(0).GetComponent<Text>().text = ScoreboardSystem.Kills[i].ToString();
        //                child.transform.GetChild(1).GetComponent<Text>().text = ScoreboardSystem.Deads[i].ToString();
        //            }
        //        }
        //        else if (ScoreboardSystem.IDs[i].Team == Team.Red)
        //        {
        //            if (spawnNumber + ScoreboardSystem.TeamSize < this.transform.childCount)
        //            {
        //                child = this.transform.GetChild(spawnNumber + ScoreboardSystem.TeamSize).gameObject;
        //                child.GetComponent<Text>().text = ScoreboardSystem.IDs[i].Name;
        //                child.transform.GetChild(0).GetComponent<Text>().text = ScoreboardSystem.Kills[i].ToString();
        //                child.transform.GetChild(1).GetComponent<Text>().text = ScoreboardSystem.Deads[i].ToString();
        //            }
        //        }
        //    }
        //}
    } //this was the old way of doing it
    private void updateStats()
    {
        bool newSlotsMade = false;
        GameObject child;
        for (int i = 0; i < ScoreboardSystem.TeamSize * 2; i++)
        {
            if (ScoreboardSystem.Names[i] != null)
            {
                if (i < this.participantList.childCount) 
                {
                    child = this.participantList.GetChild(i).gameObject;

                    child.GetComponent<Text>().text = ScoreboardSystem.Names[i];
                    child.transform.GetChild(0).GetComponent<Text>().text = ScoreboardSystem.Kills[i].ToString();
                    child.transform.GetChild(1).GetComponent<Text>().text = ScoreboardSystem.Deads[i].ToString();
                }
                else if(!newSlotsMade)
                {
                    int count = 0;
                    for(int j = participantList.childCount - 1; j >= 1; j--)
                    {
                        Destroy(participantList.GetChild(j));
                        count++;
                    }
                    Debug.Log("MenuSystem: " + count + " slots have been removed");
                    createParticipantLines(ScoreboardSystem.TeamSize);
                    i = 0;
                    newSlotsMade = true;
                }
                else
                {
                    Debug.Log("MenuSystem: Slots and teamsize don't match each other!");
                }
            }
        }
    }
    private void createParticipantLines(int teamSize)
    {
        int count = 0;
        GameObject newLine;
        Vector3 startRed = new Vector3(0.575f, 0, 0);
        Vector3 heightDifference = Vector3.up * rtpl.localScale.z * rtpl.sizeDelta.y;
        for (int b = 1; b < teamSize; b++) //blue team side, 1 example has already been placed (so b = 1)
        {
            newLine = Instantiate(participantLine);
            newLine.GetComponent<RectTransform>().SetParent(this.participantList);
            newLine.GetComponent<RectTransform>().localRotation = rtpl.localRotation;
            newLine.GetComponent<RectTransform>().localScale = rtpl.localScale;
            newLine.GetComponent<RectTransform>().localPosition = rtpl.localPosition - b * heightDifference;
            newLine.GetComponent<Text>().text = "";
            newLine.transform.GetChild(0).GetComponent<Text>().text = "";
            newLine.transform.GetChild(1).GetComponent<Text>().text = "";
            count++;
        }
        for(int r = 0; r < teamSize; r++) //red team side
        {
            newLine = Instantiate(participantLine);
            newLine.GetComponent<RectTransform>().SetParent(this.participantList);
            //newLine.transform.parent = this.participantList;
            newLine.GetComponent<RectTransform>().localRotation = rtpl.localRotation;
            newLine.GetComponent<RectTransform>().localScale = rtpl.localScale;
            newLine.GetComponent<RectTransform>().localPosition = rtpl.localPosition - r * heightDifference + startRed;
            newLine.GetComponent<Text>().text = "";
            newLine.transform.GetChild(0).GetComponent<Text>().text = "";
            newLine.transform.GetChild(1).GetComponent<Text>().text = "";
            count++;
        }
        Debug.Log("MenuSystem: printed " + count + " slots. total slots: " + participantList.childCount);
    }
    public void TurnOffMenu()
    {
        if (menuIsOpened)
        {
            menuIsOpened = false;
            for (int i = 0; i < this.transform.childCount; i++)
            {
                this.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
    public void TurnOnMenu()
    {
        if (!menuIsOpened)
        {       
            updateCounter = 0;
            for (int i = 0; i < this.transform.childCount; i++)
            {
                this.transform.GetChild(i).gameObject.SetActive(true);
            }
            updateStats();
            menuIsOpened = true;
        }
    }

}
