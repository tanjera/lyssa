using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class __Header : MonoBehaviour {

    __Game_Handler _Game;

    public Text Player__Info,
                Player__HP_Shield,
                Player__HP_Armor,
                Player__HP_Hull,
                Player__EP,
                
                Enemy__Info,
                Enemy__HP_Shield,
                Enemy__HP_Armor,
                Enemy__HP_Hull;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object__Game_Controller).GetComponent<__Game_Handler>();
    }

    void Start() {
    }

    void Update() {
        if (_Game.Player.Ship != null) {
            Player__Info.text = _Game.Player.Name;
            Player__HP_Shield.text = String.Format("{0} / {1}", _Game.Player.Ship.HP__Shield, _Game.Player.Ship.HP_Max__Shield);
            Player__HP_Armor.text = String.Format("{0} / {1}", _Game.Player.Ship.HP__Armor, _Game.Player.Ship.HP_Max__Armor);
            Player__HP_Hull.text = String.Format("{0} / {1}", _Game.Player.Ship.HP__Hull, _Game.Player.Ship.HP_Max__Hull);
            Player__EP.text = String.Format("{0}", _Game.Player.Ship.EP_Fraction);
        }

        if (_Game.Enemy_1.Ship != null) {
            Enemy__Info.text = _Game.Enemy_1.Name;
            Enemy__HP_Shield.text = String.Format("{0} / {1}", _Game.Enemy_1.Ship.HP__Shield, _Game.Enemy_1.Ship.HP_Max__Shield);
            Enemy__HP_Armor.text = String.Format("{0} / {1}", _Game.Enemy_1.Ship.HP__Armor, _Game.Enemy_1.Ship.HP_Max__Armor);
            Enemy__HP_Hull.text = String.Format("{0} / {1}", _Game.Enemy_1.Ship.HP__Hull, _Game.Enemy_1.Ship.HP_Max__Hull);
        }
    }
}
