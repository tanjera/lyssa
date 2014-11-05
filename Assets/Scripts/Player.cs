using System;
using System.Collections.Generic;

public class Player {
    Character _Character = new List_Characters().Chirba;
    int[] _Mana = new int[Enum.GetValues(typeof(Definitions.Mana_Colors)).Length];

    public Character Character {
        get { return _Character; }
    }

    public int Mana(Definitions.Mana_Colors Color)
        { return _Mana[(int)Color]; }
    public int[] Mana()
        { return _Mana; }
    public void Add_Mana(Definitions.Mana_Colors Color, int Amount)
        { _Mana[(int)Color] += Amount; }
    
    public bool Use_Mana(Definitions.Mana_Colors Color, int Amount) {
        if (_Mana[(int)Color] < Amount)
            return false;
        
        _Mana[(int)Color] -= Amount;
        return true;
    }
}