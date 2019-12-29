using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScoreTooltip : MonoBehaviour
{
    BlackBoard BB;

    private void Awake() {
        BB = FindObjectOfType<BlackBoard>();
    }
    public void WaEnter() {
        //For 1234567
        //string format "{0:n}" produces 1,234,567.00
        //string format "{0:n0}" produces 1,234,567
        BB.tAmountPoints.text = string.Format("{0:n0}",BB.playerPoints);

        //This tells BB to stop updating the Points text while the player is moused over it
        BB.mouseOverPoints = true;
    }
    public void WaExit() {
        BB.mouseOverPoints = false;
    }
}
