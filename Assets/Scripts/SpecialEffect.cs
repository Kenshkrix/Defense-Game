using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEffect : MonoBehaviour
{
    public float scalePerSecond = .01f;
    public float rotatePerSecond = 1.0f;
    public float duration = 15.0f;
    public int frameSkips = 2;
    int frameCounter = 0;



    private void Awake() {
        StartCoroutine(OverTimeEffect());
        StartCoroutine(DestroyEffect(duration));
    }




    IEnumerator OverTimeEffect() {
        while (true) {
            yield return new WaitForEndOfFrame();
            frameCounter++;
            if (frameCounter % frameSkips == 0) {
                this.gameObject.transform.localScale *= 1.0f + scalePerSecond * Time.deltaTime;
                Vector3 rot = transform.rotation.eulerAngles;
                this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, 
                                                                                  rot.y, 
                                                                                  rot.z + rotatePerSecond * Time.deltaTime));
            }
        }
    }

    IEnumerator DestroyEffect(float seconds) {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}