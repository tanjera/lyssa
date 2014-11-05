using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Skill {
    public enum Types {
        Magic,
        Physical
    }

    public class Tier {
        int Level;
        int Damage;
        int[] Mana;     // Amount of mana required in each color (ordered by Definitions.Mana_Colors enum)

        public Tier(int incLevel, int incDamage, int[] incMana ) {
            Level = incLevel;
            Damage = incDamage;
            Mana = incMana;
        }
    }
        
    string Name;
    Types Type;
    List<Tier> Tiers = new List<Tier>();


    public Skill(string incName, Types incType, List<Tier> incTiers) {
        Name = incName;
        Type = incType;
        Tiers = incTiers;
    }
}

public static class List_Skills {
    public static Skill Fireball = new Skill("Fireball", Skill.Types.Magic, new List<Skill.Tier>() {
        new Skill.Tier(1, 5,   new int[] {0, 0, 0, 0, 5, 0, 0}),
        new Skill.Tier(2, 10,  new int[] {0, 0, 0, 0, 6, 0, 0}),
        new Skill.Tier(3, 15,  new int[] {2, 0, 0, 0, 6, 0, 0})
    });

    public static Skill Ice_Lance = new Skill("Ice Lance", Skill.Types.Magic, new List<Skill.Tier>() {
        new Skill.Tier(1, 8,   new int[] {0, 5, 0, 0, 0, 0, 0}),
        new Skill.Tier(2, 12,  new int[] {0, 8, 0, 0, 0, 0, 0}),
        new Skill.Tier(3, 18,  new int[] {0, 8, 0, 0, 0, 2, 0}),
        new Skill.Tier(4, 24,  new int[] {0, 10, 0, 0, 0, 4, 0})
    });

    public static Skill Lightning_Bolt = new Skill("Lightning Bolt", Skill.Types.Magic, new List<Skill.Tier>() {
        new Skill.Tier(1, 10,  new int[] {0, 6, 0, 0, 0, 0, 0}),
        new Skill.Tier(2, 18,  new int[] {0, 9, 0, 0, 0, 0, 0}),
        new Skill.Tier(3, 24,  new int[] {2, 10, 0, 0, 0, 0, 0}),
        new Skill.Tier(4, 32,  new int[] {3, 12, 0, 0, 0, 0, 0})
    });

    /* Mana colors
        Black,
        Blue,
        Green,
        Purple,
        Red,
        White,
        Yellow */

}
