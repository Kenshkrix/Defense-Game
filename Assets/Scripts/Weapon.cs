using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    GameObject owner; //The wielder's game object
    Unit wielder; //the wielder's unit script

    GameObject target;
    Unit victim;

    public Attack attackOne; //Basic attack prefab
    public Attack attackTwo; //Alternate attack prefab
    public Attack attackThree; //Bonus attack prefab
    //Maybe more attacks? Probably unnecessary
    
    BlackBoard BB;

    public float attackCooldown = 2.0f;
    public float chargeCooldown = 2.0f;
    public float specialCooldown = 2.0f;
    float attackTimer;
    float chargeTimer;
    float specialTimer;
    public float bonusSpeed;
    public float bonusRotation;
    public int attackCount = 1;
    public int attackMax = 1;
    public int chargeCount = 1;
    public int chargeMax = 1;
    public int specialCount = 1;
    public int specialMax = 1;
    bool attackReload;
    bool chargeReload;
    bool specialReload;

    //Only basic attacks are infinite, other attacks are either charged or based on ammunition
    public bool canChargeReload; //Reloading a charged or special attack prevents attacks for the duration
    public bool canSpecialReload; //Unless it gets interupted, which will waste the charging time
    bool attemptedAttack; //Set this when interupted the first time, end reload the second time, as a confirmation


    private void Awake() {
        attackTimer = Time.time;
        chargeTimer = Time.time;
        specialTimer = Time.time;
        if (owner == null) { owner = this.transform.parent.gameObject; }
        if (wielder == null) { wielder = GetComponentInParent<Unit>(); }
        BB = FindObjectOfType<BlackBoard>();


        //These should all be set in Prefab or deliberately if I make a weapon constructor, either way
        //avoid null references by setting everything to the basic attack
        if (attackTwo == null) { attackTwo = attackOne; }
        if (attackThree == null) { attackThree = attackOne; }
    }

    public void firstAttack(GameObject Target) {
        target = Target;
        victim = target.GetComponent<Unit>();
        firstAttack();
    }


    public void firstAttack() {
        if (chargeReload || specialReload) {
            if (attemptedAttack) {
                //Completely reset cooldowns as the player changed their mind about charging up attack

                chargeReload = false;
                specialReload = false;
                chargeTimer = Time.time;
                specialTimer = Time.time;

                attemptedAttack = false;
            } else { //Player gets one more change to verify
                attemptedAttack = true;
                //Add a recognizable 'reload click' noise and/or a graphical effect here to indicate a deliberate
                //blocking of the normal behavior

                return; //Cannot attack until they confirm with a second click
            }
        }
        if (attackCount < 1) { return; } //Give basic attacks a cooldown AND an ammo count, just in case
        if (Time.time < attackCooldown + attackTimer ||
            Time.time < chargeCooldown + chargeTimer ||
            Time.time < specialCooldown + specialTimer) { //has the attack cooled down?
            return; //No, exit
        } else {
            attackTimer = Time.time; //Yes, reset cooldown
            attackCount--; //and reduce attack count
            attackReload = true;
            StartCoroutine(Reload());
        }

        Attack newAttack = Instantiate(attackOne);
        newAttack.originUnit = wielder;
        newAttack.maxSpeed += bonusSpeed;
        newAttack.rotationSpeed += bonusRotation;

        newAttack.transform.position = this.transform.position;
        newAttack.transform.rotation = owner.transform.rotation;

        if (target != null && victim != null) {
            newAttack.targetUnit = victim;
        }

        if (newAttack.isProjectile) {
            newAttack.gameObject.transform.parent = null;
            Rigidbody rb = newAttack.GetComponent<Rigidbody>();
            rb.velocity = newAttack.maxSpeed * this.transform.forward;
            rb.AddForce(newAttack.maxSpeed * this.transform.forward);
        } else {
            newAttack.gameObject.transform.parent = owner.transform;
        }
    }

    public void secondAttack(GameObject Target) {
        target = Target;
        victim = target.GetComponent<Unit>();
        secondAttack();
    }
    public void secondReload()
    {
        if (canChargeReload)
        {
            if (Time.time < attackCooldown + attackTimer ||
               Time.time < chargeCooldown + chargeTimer ||
               Time.time < specialCooldown + specialTimer)
            {
                return;
            }
            chargeReload = true;
            chargeTimer = Time.time;
            StartCoroutine(Reload());
        }
    }
    public void secondAttack() {
        if (specialReload) {
            if (attemptedAttack) {
                //Completely reset cooldowns as the player changed their mind about charging up attack

                chargeReload = false;
                specialReload = false;
                chargeTimer = Time.time;
                specialTimer = Time.time;

                attemptedAttack = false;
            } else { //Player gets one more chance to verify
                attemptedAttack = true;
                //Add a recognizable 'reload click' noise and/or a graphical effect here to indicate a deliberate
                //blocking of the normal behavior

                return; //Cannot attack until they confirm with a second click
            }
        }
        if (chargeCount < 1) {
            if (canChargeReload) {
                chargeReload = true;
                StartCoroutine(Reload());
            }
            return;
        }
        if (Time.time < attackCooldown + attackTimer ||
            Time.time < chargeCooldown + chargeTimer ||
            Time.time < specialCooldown + specialTimer) {
            return;
        } else {
            chargeTimer = Time.time;
            chargeCount--;
            //chargeReload = true;
            //StartCoroutine(Reload());
        }
        Attack newAttack = Instantiate(attackTwo);
        newAttack.originUnit = wielder;
        newAttack.maxSpeed += bonusSpeed;
        newAttack.rotationSpeed += bonusRotation;

        newAttack.transform.position = this.transform.position;
        newAttack.transform.rotation = owner.transform.rotation;

        if (target != null && victim != null) {
            newAttack.targetUnit = victim;
        }
        if (newAttack.isProjectile) {
            newAttack.gameObject.transform.parent = null;
            Rigidbody rb = newAttack.GetComponent<Rigidbody>();
            rb.velocity = newAttack.maxSpeed * this.transform.forward;
            rb.AddForce(newAttack.maxSpeed * this.transform.forward);
        } else {
            newAttack.gameObject.transform.parent = owner.transform;
        }
    }

    public void thirdAttack(GameObject Target) {
        target = Target;
        victim = target.GetComponent<Unit>();
        thirdAttack();
    }

    public void thirdAttack() {
        if (chargeReload) {
            if (attemptedAttack) {
                //Completely reset cooldowns as the player changed their mind about charging up attack

                chargeReload = false;
                specialReload = false;
                chargeTimer = Time.time;
                specialTimer = Time.time;

                attemptedAttack = false;
            } else { //Player gets one more change to verify
                attemptedAttack = true;
                //Add a recognizable 'reload click' noise and/or a graphical effect here to indicate a deliberate
                //blocking of the normal behavior

                return; //Cannot attack until they confirm with a second click
            }
        }
        if (specialCount < 1) {
            if (canSpecialReload) {
                specialReload = true;
                StartCoroutine(Reload());
            }
            return;
        }
        if (Time.time < attackCooldown + attackTimer ||
            Time.time < chargeCooldown + chargeTimer ||
            Time.time < specialCooldown + specialTimer) {
            return;
        } else {
            specialTimer = Time.time;
            specialCount--;
            //specialReload = true;
            //StartCoroutine(Reload());
        }
        Attack newAttack = Instantiate(attackThree);
        newAttack.originUnit = wielder;
        newAttack.maxSpeed += bonusSpeed;
        newAttack.rotationSpeed += bonusRotation;

        newAttack.transform.position = this.transform.position;
        newAttack.transform.rotation = owner.transform.rotation;

        if (target != null && victim != null) {
            newAttack.targetUnit = victim;
        }
        if (newAttack.isProjectile) {
            newAttack.gameObject.transform.parent = null;
            Rigidbody rb = newAttack.GetComponent<Rigidbody>();
            rb.velocity = newAttack.maxSpeed * this.transform.forward;
            rb.AddForce(newAttack.maxSpeed * this.transform.forward);
        } else {
            newAttack.gameObject.transform.parent = owner.transform;
        }
    }

    //Every kind of attack will start the Reload coroutine
    IEnumerator Reload() { //It only ends when all of them have finished reloading, but that's probably not an issue
        while (attackReload || chargeReload || specialReload) {
            yield return new WaitForEndOfFrame();

            if (attackReload) {
                if (Time.time > attackTimer + attackCooldown) {
                    attackReload = false;
                    attackTimer = Time.time;
                    attackCount++;
                    if (attackCount < attackMax) {
                        attackReload = true;
                    }
                }
            }

            if (canChargeReload && chargeReload) {
                if (Time.time > chargeTimer + chargeCooldown) {
                    chargeReload = false;
                    chargeTimer = Time.time;
                    chargeCount++;
                    //if (chargeCount < chargeMax) {
                    //    chargeReload = true;
                    //}
                }
            }

            if (canSpecialReload && specialReload) {
                if (Time.time > specialTimer + specialCooldown) {
                    specialReload = false;
                    specialTimer = Time.time;
                    specialCount++;
                    //if (specialCount < specialMax) {
                    //    specialReload = true;
                    //}
                }
            }
        }
    }
}
