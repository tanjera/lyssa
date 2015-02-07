using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class __Player : MonoBehaviour {

    /*
     * Implement:
     * 
     * - Money...
     * - Faction
     * 
     * - Active ship
     * - Hangar (with ships in one tab, items & modules in the other)
     * - Achievements
     * - Unlocked items
     * 
     * - Skills?
     *      Skills training will be like Civ, uses experience points earned from missions/kills, but player also
     *      has a baseline exp points/hour like Eve/BSG.
     *      
     * 
     * - Saved configurations (options menu, key assignments, etc)
     * - Socket?
     * 
     */

    int _Money = 0;

    public __Ship _Ship__Current;
    List<__Ship> _Ship__Hangar;

}
