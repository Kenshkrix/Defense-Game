using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    float timeCreated;
    public Unit originUnit;
    public Unit targetUnit;
    public Vector3 targetPosition;
    public Health health;

    public int damage;
    public dmgType damageType;
    public int bounces;
    public float acceleration;
    public float maxSpeed;
    public float rotationSpeed;
    public float lifeSpan = 15f;


    private void Awake() {
        if (rb == null) { rb = GetComponent<Rigidbody>(); }
        health = GetComponent<Health>();
        timeCreated = Time.time;
    }

    private void FixedUpdate() {
        if (Time.time > timeCreated + lifeSpan) {
            health.Damage(this.gameObject, 1, damageType);
        }

        if (targetUnit != null) {
            Vector3 toT = (targetUnit.transform.position - this.transform.position).normalized;
            toT.y = 0.0f;
            this.transform.rotation =
                Quaternion.RotateTowards(this.transform.rotation,
                Quaternion.LookRotation(toT, Vector3.up),
                Time.deltaTime * rotationSpeed);
            rb.AddForce(this.transform.forward * acceleration);
        }
        if ((rb.velocity.sqrMagnitude < maxSpeed * maxSpeed)) {
            rb.AddForce(this.transform.forward * acceleration);
        }

        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == this.gameObject) { return; }
        if (other.gameObject == originUnit.gameObject) { return; }

        if (other.gameObject.CompareTag("Unit")) {
            Unit u = other.gameObject.GetComponent<Unit>();
            if (u.Faction != originUnit.Faction) {
                u.health.Damage(originUnit.gameObject, damage, damageType);
                if (bounces <= 0) {
                    StartCoroutine(DestroyWhenConvenient(0.01f));
                }
                bounces--;
            }
            else {
                if (bounces <= 0) {
                    StartCoroutine(DestroyWhenConvenient(0.01f));
                }
                bounces--;
            }
        } else if (other.gameObject.CompareTag("Destroyable")) {
            Health h = other.gameObject.GetComponent<Health>();
            h.Damage(this.gameObject, damage, damageType);
            if (bounces <= 0) {
                StartCoroutine(DestroyWhenConvenient(0.01f));
            }
            bounces--;
        }
        else if (other.gameObject.CompareTag("Wall")){
            if (bounces <= 0) {
                StartCoroutine(DestroyWhenConvenient(0.01f));
            }
            bounces--;
        }
        else {
            //Probably nothing
        }
    }

    IEnumerator DestroyWhenConvenient(float convenientTime) {
        yield return new WaitForSeconds(convenientTime);
        //The most convenient way to find a convenient time to destroy this
        health.Damage(this.gameObject, 2000000000);
    }
}
