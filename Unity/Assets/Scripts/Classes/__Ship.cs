using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Ship : MonoBehaviour {

    __Game_Handler _Game;

    [HideInInspector]
    public __Ship Target;

    [HideInInspector]
    public double HP__Hull, HP__Armor, HP__Shield,
                    HP_Max__Hull, HP_Max__Armor, HP_Max__Shield,
                    EP_Percent = 0, EP_Increment = 1;
    
    [HideInInspector]
    public __Definitions.EP_Colors[] EP__Primaries = new __Definitions.EP_Colors[3];

    List<__Ship__Weapon> Modules__Weapons;
    List<__Ship__Shield> Modules__Shield;
    List<__Ship__Armor>  Modules__Armor;
    List<__Ship__Hull>   Modules__Hull;

    public delegate void Stats_Changed__Handler(__Ship sender);
    public event Stats_Changed__Handler Stats_Changed;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object__Game_Controller).GetComponent<__Game_Handler>();
     
        Modules__Weapons = new List<__Ship__Weapon>();
        Modules__Shield = new List<__Ship__Shield>();
        Modules__Armor = new List<__Ship__Armor>();
        Modules__Hull = new List<__Ship__Hull>();
    }

    public void Energy_Process(__Definitions.EP_Colors incColor, __Definitions.EP_Colors incDragging) {
        bool isPrimary = false;
        foreach (__Definitions.EP_Colors eachColor in EP__Primaries)
            if (incColor == eachColor)
                isPrimary = true;

        if (isPrimary && incColor == incDragging)
            EP_Percent = EP_Percent + 1 > 100 
                ? 100 : EP_Percent + 1;
        else 
            EP_Percent = EP_Percent - 1 < 0
                ? 0 : EP_Percent - 1;
    }

    public void Module_Add(__Ship__Weapon incModule) {
        Modules__Weapons.Add(incModule);
    }
    public void Module_Add(__Ship__Shield incModule) {
        Modules__Shield.Add(incModule);

        HP__Shield += incModule.HP;
        HP_Max__Shield += incModule.HP_Max;
    }
    public void Module_Add(__Ship__Armor incModule) {
        Modules__Armor.Add(incModule);

        HP__Armor += incModule.HP;
        HP_Max__Armor += incModule.HP_Max;
    }
    public void Module_Add(__Ship__Hull incModule) {
        Modules__Hull.Add(incModule);

        HP__Hull += incModule.HP;
        HP_Max__Hull += incModule.HP_Max;
    }

    public void Attack() {
        if (Target == null)
            return;

        Modules__Weapons.ForEach( weapon => {
            GameObject particleAmmo = Instantiate(Resources.Load<GameObject>(__Definitions.Prefab__Ammo)) as GameObject;
            _Game.Move(particleAmmo.transform, Target.transform.position, 0.5f, true);

            Target.Damage(weapon.Damage_Base,
                weapon.Damage_Modifier[__Ship__Weapon.Damage_Modifier__Index.Shield.GetHashCode()],
                weapon.Damage_Modifier[__Ship__Weapon.Damage_Modifier__Index.Armor.GetHashCode()],
                weapon.Damage_Modifier[__Ship__Weapon.Damage_Modifier__Index.Hull.GetHashCode()]);
        });
    }

    public void Damage(double rawDamage, double multShield, double multArmor, double multHull) {
        double buf;

        buf = rawDamage;
        rawDamage = rawDamage > HP__Shield / multShield ? rawDamage - (HP__Shield / multShield) : 0;
        HP__Shield = HP__Shield > (buf * multShield) ? HP__Shield - (buf * multShield) : 0;

        buf = rawDamage;
        rawDamage = rawDamage > HP__Armor / multArmor ? rawDamage - (HP__Armor / multArmor) : 0;
        HP__Armor = HP__Armor > (buf * multArmor) ? HP__Armor - (buf * multArmor) : 0;

        buf = rawDamage;
        rawDamage = rawDamage > HP__Hull / multHull ? rawDamage - (HP__Hull / multHull) : 0;
        HP__Hull = HP__Hull > (buf * multHull) ? HP__Hull - (buf * multHull) : 0;

        if (HP__Hull <= 0)
            Destroyed();

        if (Stats_Changed != null)
            Stats_Changed(this);
    }
    public void Destroyed() {
        HP__Hull = 0;
        HP__Armor = 0;
        HP__Shield = 0;

        HP_Max__Hull = 0;
        HP_Max__Armor = 0;
        HP_Max__Shield = 0;
    }
}