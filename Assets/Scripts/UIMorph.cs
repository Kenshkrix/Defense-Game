using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMorph : MonoBehaviour
{
    public RectTransform rect;
    public float posX, posY, width, height, anchor;
    private void Awake() {
        rect = this.GetComponent<RectTransform>();
    }
    private void Start() {
        StartCoroutine(morphTowards(5.0f));
    }

    IEnumerator morphTowards(float seconds) {
        Vector3 newPosition;
        float newWidth, newHeight;
        while (seconds > 0.0f) {
            yield return new WaitForEndOfFrame();
            newPosition = new Vector3(Mathf.Lerp(this.transform.position.x, posX, Time.deltaTime),
                                      Mathf.Lerp(this.transform.position.y, posY, Time.deltaTime));
            this.transform.position = newPosition;
            newWidth = Mathf.Lerp(rect.sizeDelta.x, width, Time.deltaTime);
            newHeight = Mathf.Lerp(rect.sizeDelta.y, height, Time.deltaTime);
            rect.sizeDelta.Set(newWidth, newHeight);



            seconds -= Time.deltaTime;
        }
        this.transform.position = new Vector3(posX, posY);
        rect.sizeDelta.Set(width, height);
    }
}
