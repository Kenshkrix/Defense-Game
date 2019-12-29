using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    BlackBoard BB;

    private void Awake() {
        if (BB == null) { BB = FindObjectOfType<BlackBoard>(); }
    }
}
