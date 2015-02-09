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

public class __Ship__Weapon {
    public string Name;

    public double Damage_Base;
    public double[] Damage_Modifier = new double[3];

    public double HP, HP_Max;

    public enum Damage_Modifier__Index {
        Shield,
        Armor,
        Hull
    }

    public __Ship__Weapon(string name, double dmgBase, double[] dmgMods) {
        Name = name;
        Damage_Base = dmgBase;
        Damage_Modifier = dmgMods;
    }
}


public class __Ship__Shield {
    public string Name;
    public double HP, HP_Max;

    public __Ship__Shield(string name, double hp) {
        Name = name;
        HP = hp;
        HP_Max = hp;
    }
}


public class __Ship__Armor {
    public string Name;
    public double HP, HP_Max;

    public __Ship__Armor(string name, double hp) {
        Name = name;
        HP = hp;
        HP_Max = hp;
    }
}


public class __Ship__Hull {
    public string Name;
    public double HP, HP_Max;

    public __Ship__Hull(string name, double hp) {
        Name = name;
        HP = hp;
        HP_Max = hp;
    }
}


public class __Ship__Modules {
    
    public static __Ship__Weapon
        Turret_Starter = new __Ship__Weapon( "Starter Light Turret", 5, new double[] { 0.75, 1, 1 } );

    public static __Ship__Shield
        Shield_Starter = new __Ship__Shield( "Starter Shield Booster", 30 );

    public static __Ship__Armor
        Armor_Starter = new __Ship__Armor("Starter Armor Plating", 10);

    public static __Ship__Hull
        Hull_Starter = new __Ship__Hull("Starter Hull Reinforcement", 10);

}