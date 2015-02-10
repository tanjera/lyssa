using System;
using UnityEngine;

public class __Definitions {

    public static string
        Program_Name = "Lyssa",
        Program_Version = "0.1.150207";

    public static string Layer__Interactive = "Interactive",
                            Layer__Mouseover = "Mouseover",
                            
                            Object__Game_Controller = "_Game Controller",
                            Object__Console = "UI__Console",
                            Object__Tooltip = "UI__Tooltip",
                            Object__Game_Header = "UI__Game_Header",

                            Object__Playing_Board__Container = "Playing_Board__Item_Container",
                            Object__Playing_Board__Positions = "Playing_Board__Item_Positions",

                            Label_Tile = "Tile__Playing_Board";

    public static int __EP_Colors = 6;

    public enum EP_Colors {
        Yellow,
        Green,
        Blue,
        Purple,
        Red,
        White
    }

    public static Color[] EP_Colors__Lookup = new Color[] {
        Color.yellow,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.red,
        Color.white
    };

    public static string 
        Prefab__Tile = "Prefabs/Prototype__Tile_Energy",
        Prefab__Ammo = "Particles/Ammo";
}
