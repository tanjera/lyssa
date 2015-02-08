using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Ship {

    public string Name;

    public double HP__Structure = 100,
                    HP__Armor = 100,
                    HP__Shield = 100,

                    HP_Max__Structure = 100,
                    HP_Max__Armor = 100,
                    HP_Max__Shield = 100;

    public double EP_Fraction = 0,
                    EP_Increment = 0.1;
    public __Definitions.EP_Colors[] EP__Primaries = new __Definitions.EP_Colors[3];

    public __Ship Target;

    public delegate void Stats_Changed__Handler(__Ship sender);
    public event Stats_Changed__Handler Stats_Changed;


    public __Ship(string name) {
        Name = name;
    }

    public void Energy_Add(__Definitions.EP_Colors incColor) {
        bool isPrimary = false;
        foreach (__Definitions.EP_Colors eachColor in EP__Primaries)
            if (incColor == eachColor)
                isPrimary = true;

        if (isPrimary)
            EP_Fraction = EP_Fraction + EP_Increment >= 1.0
                ? 1 : EP_Fraction + EP_Increment;
        else
            EP_Fraction = EP_Fraction - EP_Increment <= 0
                ? 0 : EP_Fraction - EP_Increment;
    }


    public void Attack__Basic() {
        if (Target == null)
            return;

        Target.Damage(5, 1, 1, 1);
    }
    public void Damage(double rawDamage, double multShield, double multArmor, double multStructure) {
        double buf;

        buf = rawDamage;
        rawDamage = rawDamage > HP__Shield / multShield ? rawDamage - (HP__Shield / multShield) : 0;
        HP__Shield = HP__Shield > (buf * multShield) ? HP__Shield - (buf * multShield) : 0;

        buf = rawDamage;
        rawDamage = rawDamage > HP__Armor / multArmor ? rawDamage - (HP__Armor / multArmor) : 0;
        HP__Armor = HP__Armor > (buf * multArmor) ? HP__Armor - (buf * multArmor) : 0;

        buf = rawDamage;
        rawDamage = rawDamage > HP__Structure / multStructure ? rawDamage - (HP__Structure / multStructure) : 0;
        HP__Structure = HP__Structure > (buf * multStructure) ? HP__Structure - (buf * multStructure) : 0;

        if (HP__Structure <= 0)
            Destroyed();

        if (Stats_Changed != null)
            Stats_Changed(this);
    }

    public void Destroyed() {
        HP__Structure = 0;
        HP__Armor = 0;
        HP__Shield = 0;

        HP_Max__Structure = 0;
        HP_Max__Armor = 0;
        HP_Max__Shield = 0;
    }
}