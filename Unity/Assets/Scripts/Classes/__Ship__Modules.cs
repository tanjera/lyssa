using UnityEngine;
using System.Collections;

    /*
     * Implement:
     * 
     * - Base stats for each type of object
     *      - Shield
     *          - Energy required to recharge
     *          - Recharge time
     *          
     *      - Weapon
     *          - Special ammo?
     *          
     *      - Armor
     *          - Chance for deflection
     *          - Stealth (eg. radar deterrent)
     * 
     */

public class _shipWeapon {
    public string Name;
    public bool Active;
    public float damageBase;
    public float[] damageModifier = new float[3];

    public double hp, hpMax;

    public enum damageModifier_Index {
        Shield,
        Armor,
        Hull
    }

    public _shipWeapon(string name, float dmgBase, float[] dmgMods) {
        Name = name;
        Active = true;
        damageBase = dmgBase;
        damageModifier = dmgMods;
    }
}


public class _shipShield {
    public string Name;
    public bool Active;
    public double HP, HP_Max;

    public _shipShield(string name, double hp) {
        Name = name;
        Active = true;
        HP = hp;
        HP_Max = hp;
    }
}


public class _shipArmor {
    public string Name;
    public bool Active;
    public double HP, HP_Max;

    public _shipArmor(string name, double hp) {
        Name = name; 
        Active = true;
        HP = hp;
        HP_Max = hp;
    }
}


public class _shipHull {
    public string Name;
    public bool Active;
    public double HP, HP_Max;

    public _shipHull(string name, double hp) {
        Name = name;
        Active = true;
        HP = hp;
        HP_Max = hp;
    }
}


public class _shipModules {
    
    public static _shipWeapon
        Turret_Starter = new _shipWeapon("Starter Light Turret", 2f, new float[] { 0.75f, 1, 1 });

    public static _shipShield
        Shield_Starter = new _shipShield( "Starter Shield Booster", 30 );

    public static _shipArmor
        Armor_Starter = new _shipArmor("Starter Armor Plating", 10);

    public static _shipHull
        Hull_Starter = new _shipHull("Starter Hull Reinforcement", 20);

}