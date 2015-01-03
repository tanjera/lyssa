using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class Character {
    string Name;
    int Level_Current, Level_Max;
    int HP_Base, HP_Increments;

    int HP_Current, HP_Max;
    List<Skill> Skills = new List<Skill>();

    
    public Character(string incName, int incLevel_Max, int incHP_Base, int incHP_Increments, List<Skill> incSkills) {
        Name = incName;
        Level_Current = 1;
        Level_Max = incLevel_Max;

        HP_Base = incHP_Base;
        HP_Increments = incHP_Increments;
        
        HP_Max = Calculate__Health_Max();
        HP_Current = HP_Max;

        Skills = incSkills;
    }

    public int Level {
        get { return Level_Current; }
    }
    public void Add_Level(int levelUp) {
        levelUp = Level_Current + levelUp > Level_Max
            ? Level_Max - Level_Current : levelUp;

        Level_Current += levelUp;
        HP_Max += Calculate__Health_Increase(levelUp);
        HP_Current = HP_Max;
    }

    public int Health_Max {
        get { return HP_Max; }
        set { HP_Max = value; }
    }
    public int Health_Current {
        get { return HP_Current; }
        set { HP_Current = value; }
    }
    public void Adjust__Health_Current(int incValue) 
        { HP_Current += incValue; }
    public void Adjust__Health_Max(int incValue) 
        { HP_Max += incValue; }
    public int Calculate__Health_Max() 
        { return HP_Base + (int)((Level_Current - 1) * HP_Increments); }
    public int Calculate__Health_Increase(int levelAdjust) 
        { return (int)(levelAdjust * HP_Increments); }
}

public class List_Characters {
    public Character Chirba = new Character("Chirba", 100, 50, 55,
        new List<Skill>() {
            List_Skills.Fireball,
            List_Skills.Ice_Lance,
            List_Skills.Lightning_Bolt
    });
}
