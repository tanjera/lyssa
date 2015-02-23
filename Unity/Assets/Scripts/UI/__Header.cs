using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class __Header : MonoBehaviour {

    __Game_Handler _Game;

    public RectTransform 
                Player__hpShield,
                Player__hpArmor,
                Player__hpHull,
                Player__ep,
                
                Enemy__hpShield,
                Enemy__hpArmor,
                Enemy__hpHull;

    public Image Player__Color1,
                 Player__Color2,
                 Player__Color3;

    Vector2 
                Player__hpShield_Original,
                Player__hpArmor_Original,
                Player__hpHull_Original,
                Player__EP_Original,

                Enemy__hpShield_Original,
                Enemy__hpArmor_Original,
                Enemy__hpHull_Original;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object_gameController).GetComponent<__Game_Handler>();

        Player__hpShield_Original = Player__hpShield.sizeDelta;
        Player__hpArmor_Original = Player__hpArmor.sizeDelta;
        Player__hpHull_Original = Player__hpHull.sizeDelta;
        Player__EP_Original = Player__ep.sizeDelta;

        Enemy__hpShield_Original = Enemy__hpShield.sizeDelta;
        Enemy__hpArmor_Original = Enemy__hpArmor.sizeDelta;
        Enemy__hpHull_Original = Enemy__hpHull.sizeDelta;
    }

    void FixedUpdate() {
        if (_Game.Player.Ship) {
            Player__hpShield.sizeDelta = new Vector2(Player__hpShield_Original.x * (float)(_Game.Player.Ship.hpShield / _Game.Player.Ship.hpShield_Max), Player__hpShield_Original.y);
            Player__hpArmor.sizeDelta = new Vector2(Player__hpArmor_Original.x * (float)(_Game.Player.Ship.hpArmor / _Game.Player.Ship.hpArmor_Max), Player__hpArmor_Original.y);
            Player__hpHull.sizeDelta = new Vector2(Player__hpHull_Original.x * (float)(_Game.Player.Ship.hpHull / _Game.Player.Ship.hpHull_Max), Player__hpHull_Original.y);
            Player__ep.sizeDelta = new Vector2(Player__EP_Original.x * (float)(_Game.Player.Ship.epPercent / 100), Player__EP_Original.y);

            Player__Color1.color = __Definitions.epColors_Lookup[_Game.Player.Ship.epPrimaries[0].GetHashCode()];
            Player__Color2.color = __Definitions.epColors_Lookup[_Game.Player.Ship.epPrimaries[1].GetHashCode()];
            Player__Color3.color = __Definitions.epColors_Lookup[_Game.Player.Ship.epPrimaries[2].GetHashCode()];
        }

        if (_Game.Enemy1.Ship) {
            Enemy__hpShield.sizeDelta = new Vector2(Enemy__hpShield_Original.x * (float)(_Game.Enemy1.Ship.hpShield / _Game.Enemy1.Ship.hpShield_Max), Enemy__hpShield_Original.y);
            Enemy__hpArmor.sizeDelta = new Vector2(Enemy__hpArmor_Original.x * (float)(_Game.Enemy1.Ship.hpArmor / _Game.Enemy1.Ship.hpArmor_Max), Enemy__hpArmor_Original.y);
            Enemy__hpHull.sizeDelta = new Vector2(Enemy__hpHull_Original.x * (float)(_Game.Enemy1.Ship.hpHull / _Game.Enemy1.Ship.hpHull_Max), Enemy__hpHull_Original.y);
        }
    }
}
