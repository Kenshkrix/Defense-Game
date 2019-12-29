using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextColor : MonoBehaviour
{
    public Text text;
    public Color color;
    bool notFading = true;

    public void setColor(Color c) {
        color = c;
        text.color = color;
    }

    public void colorRed() {
        color = Color.red;
        text.color = color;
        if (notFading) {
            notFading = false;
            StartCoroutine(fadeText());
        }
    }
    public void colorGreen() {
        color = Color.green;
        text.color = color;
        if (notFading) {
            notFading = false;
            StartCoroutine(fadeText());
        }
    }

    IEnumerator fadeText() {
        while (color.a > 0.0f) {
            yield return new WaitForEndOfFrame();
            color.a -= 0.009f;
            text.color = color;
        }
        notFading = true;
    }
}
