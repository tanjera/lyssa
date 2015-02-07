using UnityEngine;
using System;
using System.Collections;

public class __Destructable : MonoBehaviour {

    public class Damages {
        public int Hitpoints;

        public float Mult__Shield = 1f,
                Mult__Armor = 1f,
                Mult__Structure = 1f;

        public Damages(int iHitpoints, float xShield, float xArmor, float xStructure) {
            Hitpoints = iHitpoints;
            
            Mult__Shield = xShield;
            Mult__Armor = xArmor;
            Mult__Structure = xStructure;
        }
    }
    
    public enum Factions {
        NONE,
        COMPACT,
        FEDERATE
    }


    public Factions Faction;
    
    /* Game mechanic variables */
    public float HP__Structure = 100,
            HP__Armor = 100,
            HP__Shield = 100,
            HP__Energy = 100,

        HP_Max__Structure = 100,
            HP_Max__Armor = 100,
            HP_Max__Shield = 100,
            HP_Max__Energy = 100;


    public delegate void _On_Hitpoints(__Destructable Target);
    public event _On_Hitpoints On_Hitpoints;

    public void Damage(__Destructable.Damages _Damage) {

        if (HP__Shield > 0) {
            HP__Shield -= _Damage.Hitpoints * _Damage.Mult__Shield;
            if (HP__Shield < 0) {
                _Damage.Hitpoints -= (int)Mathf.Abs(HP__Shield);
                HP__Shield = 0;
            } else
                _Damage.Hitpoints = 0;
        }

        if (HP__Armor > 0) {
            HP__Armor -= _Damage.Hitpoints * _Damage.Mult__Armor;
            if (HP__Armor < 0) {
                _Damage.Hitpoints -= (int)Mathf.Abs(HP__Armor);
                HP__Armor = 0;
            } else
                _Damage.Hitpoints = 0;
        }

        if (HP__Structure > 0) {
            HP__Structure -= _Damage.Hitpoints * _Damage.Mult__Structure;
            if (HP__Structure < 0) {
                _Damage.Hitpoints -= (int)Mathf.Abs(HP__Structure);
                HP__Structure = 0;
            } else
                _Damage.Hitpoints = 0;
        }

        if (HP__Structure <= 0)
            Destroyed();

        if (On_Hitpoints != null)
            On_Hitpoints(this);
    }

    public void Destroyed() {

        HP__Structure = 0;
        HP__Armor = 0;
        HP__Shield = 0;
        HP__Energy = 0;

        HP_Max__Structure = 0;
        HP_Max__Armor = 0;
        HP_Max__Shield = 0;
        HP_Max__Energy = 0;
    }
}
