using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Player {

    public string Name;
    public bool Human;

    public __Ship Ship;
    public List<__Ship> Garage;
    
    public GameObject Model;

    public __Player(string name, bool human) {
        Name = name;
        Human = human;

        Ship = new __Ship("");
    }    
}