using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    BlackBoard BB;
    Unit unit;
    public int hp = 1;
    public int hpMax = 1;

    public int defense = 0;
    public int flamingResist = 0;
    public int freezingResist = 0;
    public int electrocutionResist = 0;

    int shield = 0;
    public int shieldMax = 0;

    public int healOnDeath = 0;
    public int extraLives = 0;

    public dmgType explodeDamageType = dmgType.normal;
    public int explodeOnDeathDamage = 0;
    public float explodeOnDeathRadius = 0;
    public bool explodeFriendlyFire = true;
    public SpecialEffect onDeathEffect;

    bool bbModified = false;
    public bool dying = false;


    private void Awake() {
        if (BB == null) { BB = FindObjectOfType<BlackBoard>(); }
        unit = GetComponent<Unit>();
        hp = hpMax;
        shield = shieldMax;
    }
    public int Damage(GameObject damageSource, int dmg) {
        return Damage(damageSource, dmg, dmgType.normal);
    }
    public int Damage(GameObject damageSource, int dmg, dmgType dT) {
        int def = defense;
        if (dT == dmgType.fire) {
            def = flamingResist;
        } else if (dT == dmgType.ice) {
            def = freezingResist;
        } else if (dT == dmgType.lightning) {
            def = electrocutionResist;
        } else if (dT == dmgType.flat) {
            def = 0;
        }
        return calculateDamage(dmg, def);
    }
    public int calculateDamage(int damage, int defense) {
        if (damage <= defense) {
            damage = ((damage * defense) / defense * 8);
        } else {
            damage -= defense;
        }
        if (damage < 1) { damage = 1; }
        if (shield > 0) {
            if (shield > damage) {
                shield -= damage;
            } else {
                hp -= damage - shield;
                shield = 0;
            }
        } else {
            hp -= damage;
            if (hp <= 0) {
                if (healOnDeath > 0) {
                    hp += healOnDeath;
                }
            }
            if (hp <= 0) {
                if (extraLives > 0) {
                    hp = hpMax;
                    //Display loss of extra life
                    extraLives--;
                } else {
                    StartCoroutine(Die(0.001f));
                }
            }
        }
        if (this.gameObject == BB.player) {
            if (hp != 0) {
                BB.player.GetComponent<PlayerController>().updateHP();
                BB.undamaged = false;
            }
        }
        return hp;
    }
    IEnumerator Die(float seconds) {
        dying = true;
        while (BB.deathsThisFrame > 15) {
            yield return new WaitForEndOfFrame();
        }
        BB.deathsThisFrame += 1;
        if (this.gameObject.CompareTag("Unit") && !bbModified) {
            if (this.gameObject != BB.player.gameObject)
            {
                BB.changePoints(Random.Range(1, 4));
                BB.changeKills(1);
                bbModified = true;
            }
        }
        Collider[] cols;
        yield return new WaitForSeconds(seconds);
        if (explodeOnDeathDamage > 0 && explodeOnDeathRadius > 0) {
            cols = Physics.OverlapSphere(this.transform.position, explodeOnDeathRadius);
            foreach (Collider c in cols) {
                if (c.CompareTag("Unit")) {
                    Unit u = c.GetComponent<Unit>();
                    if (explodeFriendlyFire) {
                        u.health.Damage(this.gameObject, explodeOnDeathDamage, explodeDamageType);
                    } else if (u.Faction != unit.Faction) {
                        u.health.Damage(this.gameObject, explodeOnDeathDamage, explodeDamageType);
                    }
                } else if (c.CompareTag("Destroyable")) {
                    Health h = c.gameObject.GetComponent<Health>();
                    h.Damage(this.gameObject, explodeOnDeathDamage, explodeDamageType);
                }
            }
        }
        if (onDeathEffect) {
            Instantiate(onDeathEffect).transform.position = this.transform.position;
        }
        if (this.gameObject == BB.player.gameObject) {
            dying = true;
            StartCoroutine(stopDying());
        }
        else {
            Destroy(this.gameObject);
        }
    }
    IEnumerator stopDying()
    {
        yield return new WaitForSeconds(1f);
        BB.changePoints(-2000000000);
        hp = hpMax;
        BB.player.GetComponent<PlayerController>().updateHP();
        dying = false;
    }
}
public enum dmgType {
    normal, fire, ice, lightning, flat
}