using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject specialAmmoPrefab;
    Unit wielder;
    GameObject projectileObject;
    Projectile projectile;
    BlackBoard BB;

    public float reloadCooldown = 10.0f;
    public float shotCooldown = 1.0f;
    float shotTimer;
    float reloadTimer;

    public float speedBonus;

    public int ammoCount = 1;
    public int ammoMax = 1;

    public int specialAmmoCount;
    public string weaponType;
    bool Loading;

    public void Awake() {
        reloadTimer = Time.time;
        shotTimer = Time.time;
        wielder = GetComponentInParent<Unit>();
        BB = FindObjectOfType<BlackBoard>();
    }

    private void Start() {
        StartCoroutine(explosiveAmmoGenerator());
    }
    public void FireSpecial() {
        if (specialAmmoCount <= 0) { return; }
        if (Time.time < shotCooldown + shotTimer) {
            return;
        }
        else {
            shotTimer = Time.time;
        }
        projectileObject = Instantiate(specialAmmoPrefab);

        specialAmmoCount--;

        projectile = projectileObject.GetComponent<Projectile>();
        projectile.originUnit = wielder;
        projectile.maxSpeed += speedBonus;

        projectileObject.transform.position = this.transform.position +
                                              (this.transform.parent.transform.forward * 1.5f);
        projectileObject.transform.position += Vector3.up / 2f;

        projectileObject.transform.rotation = this.transform.parent.transform.rotation;
        Rigidbody rb = projectileObject.GetComponent<Rigidbody>();

        rb.velocity = (projectile.maxSpeed * -this.transform.forward);
        rb.AddForce(projectile.maxSpeed * this.transform.forward);

        ammoCount--;

        Loading = true;
        StartCoroutine(Reload());
    }
    public void Fire() {
        if (ammoCount <= 0) { return; }
        if (Time.time < shotCooldown + shotTimer) {
            return;
        }
        else {
            shotTimer = Time.time;
        }
        projectileObject = Instantiate(projectilePrefab);
        

        projectile = projectileObject.GetComponent<Projectile>();
        projectile.originUnit = wielder;

        projectileObject.transform.position = this.transform.position +
                                              (this.transform.parent.transform.forward * 1.5f);
        projectileObject.transform.position += Vector3.up / 3f;

        projectileObject.transform.rotation = this.transform.parent.transform.rotation;
        Rigidbody rb = projectileObject.GetComponent<Rigidbody>();

        rb.velocity = (projectile.maxSpeed * -this.transform.forward);
        rb.AddForce(projectile.maxSpeed * this.transform.forward);

        ammoCount--;

        Loading = true;
        StartCoroutine(Reload());
    }

    IEnumerator Reload() {
        while (Loading) {
            yield return new WaitForEndOfFrame();
            if (Time.time > reloadTimer + reloadCooldown) {
                Loading = false;
                reloadTimer = Time.time;
                ammoCount++;
                if (ammoCount < ammoMax) {
                    Loading = true;
                }
            }
        }
    }

    IEnumerator explosiveAmmoGenerator() {
        int frameCount = 0;
        float timer = 6.0f;
        while (true) {
            yield return new WaitForEndOfFrame();
            frameCount++;
            timer -= Time.deltaTime;
            if (frameCount % 4 != 0){ continue; } //Only update GUI every fourth frame

            if (timer < 0.0f) {
                specialAmmoCount++;
                timer = 6.0f;
                BB.tAmmoTimer.text = "" + timer;
                frameCount = 0;
            } else {
                BB.tAmmoTimer.text = "" + timer;
            }
        }
    }
}
