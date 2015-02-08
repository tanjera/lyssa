using System;
using UnityEngine;

public class __Definitions {

    public static string
        Program_Name = "Lyssa",
        Program_Version = "0.15.0207";

    public static string Layer__Interactive = "Interactive",
                            Layer__Mouseover = "Mouseover",
                            
                            Object__Game_Controller = "_Game Controller",
                            Object__Console = "_Console",
                            Object__Tooltip = "_Tooltip",

                            Object__Playing_Board__Container = "Playing_Board__Item_Container",
                            Object__Playing_Board__Positions = "Playing_Board__Item_Positions",

                            Label_Tile = "Tile__Playing_Board";

    public static int __EP_Colors = 6;

    public enum EP_Colors {
        Red,
        White,
        Yellow,
        Purple,
        Green,
        Black
    }

    public enum EP_Colors__Opposites {
        White,
        Red,
        Purple,
        Yellow,
        Black,
        Green
    }

    public static Color[] EP_Colors__Lookup = new Color[] {
        Color.red,
        Color.white,
        Color.yellow,
        Color.magenta,
        Color.green,
        Color.black
    };
}
