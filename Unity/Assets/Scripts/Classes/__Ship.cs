using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Ship : MonoBehaviour {

    __Game_Handler _Game;

    public BezierSpline splineFlightPath;

    [HideInInspector]
    public __Ship Target;
    public __Player Player;
    [HideInInspector]
    public double hpHull, hpArmor, hpShield,
                    hpHull_Max, hpArmor_Max, hpShield_Max,
                    epPercent = 0, epIncrement = 1, epBuffer = 0;
    
    [HideInInspector]
    public __Definitions.epColors[] epPrimaries = new __Definitions.epColors[3];

    List<_shipWeapon> modulesWeapons;
    List<_shipShield> modulesShield;
    List<_shipArmor>  modulesArmor;
    List<_shipHull>   modulesHull;

    public delegate void statsChanged_Handler(__Ship sender);
    public event statsChanged_Handler statsChanged;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object_gameController).GetComponent<__Game_Handler>();
     
        modulesWeapons = new List<_shipWeapon>();
        modulesShield = new List<_shipShield>();
        modulesArmor = new List<_shipArmor>();
        modulesHull = new List<_shipHull>();
    }

    public void energyProcess(__Definitions.epColors incColor, __Definitions.epColors incDragging) {
        bool incPrimary = false;
        foreach (__Definitions.epColors eachColor in epPrimaries)
            if (incColor == eachColor)
                incPrimary = true;

        // Collects only the primary color being dragged
        if (incPrimary && incColor == incDragging)
            epPercent = epPercent + 1 > 100
                ? 100 : epPercent + 1;
        // Subtracts only when dragging a non-primary color and hitting other colors
        else if (!incPrimary && incColor != incDragging)
            epPercent = epPercent - 1 < 0
                ? 0 : epPercent - 1;

        // EP Buffer used in calculating attack strength: if you collected the same color you're dragging, add
        if (incColor == incDragging)
            epBuffer += 1;
    }

    public void moduleAdd(_shipWeapon incModule) {
        modulesWeapons.Add(incModule);
    }
    public void moduleAdd(_shipShield incModule) {
        modulesShield.Add(incModule);

        hpShield += incModule.HP;
        hpShield_Max += incModule.HP_Max;
    }
    public void moduleAdd(_shipArmor incModule) {
        modulesArmor.Add(incModule);

        hpArmor += incModule.HP;
        hpArmor_Max += incModule.HP_Max;
    }
    public void moduleAdd(_shipHull incModule) {
        modulesHull.Add(incModule);

        hpHull += incModule.HP;
        hpHull_Max += incModule.HP_Max;
    }

    public bool shieldsActive() {
        return hpShield > 0;
    }

    public void Attack() {
        if (Target == null)
            return;

        if (!Player.Human)
            epBuffer = UnityEngine.Random.Range(1, 10);

        modulesWeapons.ForEach( weapon => {
            /* Create ammunition */
            GameObject ammoObject = Instantiate(Resources.Load<GameObject>(__Definitions.prefabAmmo)) as GameObject;
            _Ammo ammoScript = ammoObject.GetComponent<_Ammo>();
            ammoScript.Target(Target);
            
            
            /* Ammunition kinematic operations*/
            ammoObject.SetActive(false);
            ammoObject.transform.position = this.transform.position;
            ammoObject.SetActive(true);
            ammoScript.kinemaOp = new __Game_Handler.kinemaOp(ammoObject.transform, Target.transform, 1f);
            ammoScript.kinemaOp.onComplete = delegate() { Destroy(ammoScript.kinemaOp.transObject.gameObject); };
            _Game.kinemaAdd(ammoScript.kinemaOp);

            /* Damage the target! */
            Target.Damage(Math.Pow(weapon.damageBase, Math.Sqrt(epBuffer)),
                weapon.damageModifier[_shipWeapon.damageModifier_Index.Shield.GetHashCode()],
                weapon.damageModifier[_shipWeapon.damageModifier_Index.Armor.GetHashCode()],
                weapon.damageModifier[_shipWeapon.damageModifier_Index.Hull.GetHashCode()]);
        });

        epBuffer = 0;
    }
    public void Damage(double rawDamage, double multShield, double multArmor, double multHull) {
        double buf;

        buf = rawDamage;
        rawDamage = rawDamage > hpShield / multShield ? rawDamage - (hpShield / multShield) : 0;
        hpShield = hpShield > (buf * multShield) ? hpShield - (buf * multShield) : 0;

        buf = rawDamage;
        rawDamage = rawDamage > hpArmor / multArmor ? rawDamage - (hpArmor / multArmor) : 0;
        hpArmor = hpArmor > (buf * multArmor) ? hpArmor - (buf * multArmor) : 0;

        buf = rawDamage;
        rawDamage = rawDamage > hpHull / multHull ? rawDamage - (hpHull / multHull) : 0;
        hpHull = hpHull > (buf * multHull) ? hpHull - (buf * multHull) : 0;

        if (hpHull <= 0)
            Destroyed();

        if (statsChanged != null)
            statsChanged(this);
    }
    public void Destroyed() {
        hpHull = 0;
        hpArmor = 0;
        hpShield = 0;

        hpHull_Max = 0;
        hpArmor_Max = 0;
        hpShield_Max = 0;
    }
}