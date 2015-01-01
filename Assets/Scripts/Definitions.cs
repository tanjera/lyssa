using UnityEngine;
using System.Collections;

public static class Definitions {

    /* Definitions and functions for individual stones, creation, etc */

    public static int Mana_Amount = 7;

    public enum Mana_Colors {
        Black,
        Blue,
        Green,
        Purple,
        Red,
        White,
        Yellow
    }

    public static Color[] Lookup_Colors = new Color[] {
        Color.black,
        Color.blue,
        Color.green,
        Color.magenta,
        Color.red,
        Color.white,
        Color.yellow
    };
}
