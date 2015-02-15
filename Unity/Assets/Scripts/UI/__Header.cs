using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class __Header : MonoBehaviour {

    __Game_Handler _Game;

    public RectTransform 
                Player__HP_Shield,
                Player__HP_Armor,
                Player__HP_Hull,
                Player__EP,
                
                Enemy__HP_Shield,
                Enemy__HP_Armor,
                Enemy__HP_Hull;

    public Image Player__Color1,
                 Player__Color2,
                 Player__Color3;

    Vector2 
                Player__HP_Shield__Original,
                Player__HP_Armor__Original,
                Player__HP_Hull__Original,
                Player__EP__Original,

                Enemy__HP_Shield__Original,
                Enemy__HP_Armor__Original,
                Enemy__HP_Hull__Original;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object__Game_Controller).GetComponent<__Game_Handler>();

        Player__HP_Shield__Original = Player__HP_Shield.sizeDelta;
        Player__HP_Armor__Original = Player__HP_Armor.sizeDelta;
        Player__HP_Hull__Original = Player__HP_Hull.sizeDelta;
        Player__EP__Original = Player__EP.sizeDelta;

        Enemy__HP_Shield__Original = Enemy__HP_Shield.sizeDelta;
        Enemy__HP_Armor__Original = Enemy__HP_Armor.sizeDelta;
        Enemy__HP_Hull__Original = Enemy__HP_Hull.sizeDelta;
    }

    void FixedUpdate() {
        if (_Game.Player.Ship != null) {
            Player__HP_Shield.sizeDelta = new Vector2(Player__HP_Shield__Original.x * (float)(_Game.Player.Ship.HP__Shield / _Game.Player.Ship.HP_Max__Shield), Player__HP_Shield__Original.y);
            Player__HP_Armor.sizeDelta = new Vector2(Player__HP_Armor__Original.x * (float)(_Game.Player.Ship.HP__Armor / _Game.Player.Ship.HP_Max__Armor), Player__HP_Armor__Original.y);
            Player__HP_Hull.sizeDelta = new Vector2(Player__HP_Hull__Original.x * (float)(_Game.Player.Ship.HP__Hull / _Game.Player.Ship.HP_Max__Hull), Player__HP_Hull__Original.y);
            Player__EP.sizeDelta = new Vector2(Player__EP__Original.x * (float)(_Game.Player.Ship.EP_Percent / 100), Player__EP__Original.y);

            Player__Color1.color = __Definitions.EP_Colors__Lookup[_Game.Player.Ship.EP__Primaries[0].GetHashCode()];
            Player__Color2.color = __Definitions.EP_Colors__Lookup[_Game.Player.Ship.EP__Primaries[1].GetHashCode()];
            Player__Color3.color = __Definitions.EP_Colors__Lookup[_Game.Player.Ship.EP__Primaries[2].GetHashCode()];
        }

        if (_Game.Enemy_1.Ship != null) {
            Enemy__HP_Shield.sizeDelta = new Vector2(Enemy__HP_Shield__Original.x * (float)(_Game.Enemy_1.Ship.HP__Shield / _Game.Enemy_1.Ship.HP_Max__Shield), Enemy__HP_Shield__Original.y);
            Enemy__HP_Armor.sizeDelta = new Vector2(Enemy__HP_Armor__Original.x * (float)(_Game.Enemy_1.Ship.HP__Armor / _Game.Enemy_1.Ship.HP_Max__Armor), Enemy__HP_Armor__Original.y);
            Enemy__HP_Hull.sizeDelta = new Vector2(Enemy__HP_Hull__Original.x * (float)(_Game.Enemy_1.Ship.HP__Hull / _Game.Enemy_1.Ship.HP_Max__Hull), Enemy__HP_Hull__Original.y);
        }
    }
}
