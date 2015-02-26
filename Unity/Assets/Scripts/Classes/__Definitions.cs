using System;
using UnityEngine;

public class __Definitions {

    public static string
        programName = "Lyssa",
        programVersion = "0.1.250207";

    public static string layerInteractive = "Interactive",
                            layerKinematic = "Kinematic",
                            layerMouseover = "Mouseover",
                            
                            Object_gameController = "_Game Controller",
                            Object_Console = "UI__Console",
                            Object_Tooltip = "UI__Tooltip",
                            Object_gameHeader = "UI__Game_Header",

                            Object_playingBoard_Container = "Playing_Board__Item_Container",
                            Object_playingBoard_Positions = "Playing_Board__Item_Positions",

                            Object_shipShield = "Shield, Collider",

                            labelTile = "Tile__Playing_Board";

    public static int epColors_Count = 6;

    public enum epColors {
        Yellow,
        Green,
        Blue,
        Purple,
        Red,
        White
    }

    public static Color[] epColors_Lookup = new Color[] {
        Color.yellow,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.red,
        Color.white
    };

    public static string 
        prefabTile = "Prefabs/Prototype__Tile_Energy",
        prefabAmmo = "Prefabs/Ammo";
}
