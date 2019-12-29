using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    Rigidbody rb;

    public float lifeSpan = 5f; //How many seconds it should last
    public Unit originUnit; //Who launched this attack? Probably implement attacks on enemies as well
    public Unit targetUnit; //Is the attack aimed at a particular enemy? For homing/seeking type attacks
    public Vector3 targetPosition; //Is the attack aimed at a particular location? Most attacks should use this
    public Health health; //This attack's health, for projectiles and such which can be destroyed

    public int damage; //How much damage this does
    public dmgType damageType; //What kind of damage this does
    public int pierceCount; //How many enemies can it go through before it stops
    //public int bounceCount; //Not sure I want to bother with this TBH, maybe grenades with parabolic arcs or something
    public bool isProjectile;
    public float acceleration; //How quickly it accelerates, use for homing/seeking attacks probably
    public float maxSpeed; //Maximum speed, projectiles should generally just start at this;
                            //Maybe make a special perk that reduces initial speed in which case acceleration is more useful
    public float rotationSpeed; //If this projectile alters its trajectory or rotates around something this is degrees/second

    private void Awake() {
        if (rb == null) { rb = GetComponent<Rigidbody>(); }
        health = GetComponent<Health>();
        
        //Health deals with OnDeath cases, so go through there, do MaxInt/2 - 1 damage which should be plenty
        StartCoroutine(DestroyWhenConvenient(lifeSpan));
    }
    private void FixedUpdate() {
        if (isProjectile) { //Projectiles are not connected to their origin
            if (targetUnit != null) {
                Vector3 toT = (targetUnit.transform.position - this.transform.position).normalized;
                toT.y = 0.0f; //Attacks are basically 2d zones of action

                this.transform.rotation = //Consider switching from rotation-based to force-based
                    Quaternion.RotateTowards(this.transform.rotation,
                    Quaternion.LookRotation(toT, Vector3.up),
                    Time.deltaTime * rotationSpeed); //Change orientation before applying force

                rb.AddForce(this.transform.forward * acceleration); //Always apply force forward?
            }
            if ((rb.velocity.sqrMagnitude < maxSpeed * maxSpeed)) { //Not at max speed, go faster
                rb.AddForce(this.transform.forward * acceleration);
            }

            if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed) { //Went too far, only go max speed
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        } else {
            transform.RotateAround(Vector3.zero, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    //Use a trigger collider to determine whether this hits somebody
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == this.gameObject) { return; }
        if (other.gameObject == originUnit.gameObject) { return; }

        if (other.gameObject.CompareTag("Unit")) {
            Unit u = other.gameObject.GetComponent<Unit>();
            if (u.Faction != originUnit.Faction) { //Attacks should only hit enemies
                u.health.Damage(originUnit.gameObject, damage, damageType);
                if (pierceCount <= 0) {
                    StartCoroutine(DestroyWhenConvenient(0.01f));
                }
                pierceCount--;
            } else { //Hitting an ally, reduce pierce count but don't do damage
                if (pierceCount <= 0) {
                    StartCoroutine(DestroyWhenConvenient(0.01f));
                }
                pierceCount--;
            }
        } else if (other.gameObject.CompareTag("Destroyable")) { //Might give Destroyables a faction to avoid FF later
            Health h = other.gameObject.GetComponent<Health>();
            h.Damage(this.gameObject, damage, damageType);
            if (pierceCount <= 0) {
                StartCoroutine(DestroyWhenConvenient(0.01f));
            }
            pierceCount--;
        } else if (other.gameObject.CompareTag("Wall")) { //Walls are terrain features and thus invincible
            if (pierceCount <= 0) {
                StartCoroutine(DestroyWhenConvenient(0.01f));
            }
            pierceCount--;
        } else {
            //There are various incidental things that might hit the trigger, idgaf about irrelevant stuff though
        }
    }

    IEnumerator DestroyWhenConvenient(float convenientTime) {
        yield return new WaitForSeconds(convenientTime);
        //The most convenient way to find a convenient time to destroy this
        health.Damage(this.gameObject, 1073741823);
    }
}
