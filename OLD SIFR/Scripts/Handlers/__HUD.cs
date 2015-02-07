using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class __HUD : MonoBehaviour {

    public enum Affinities {
        NEUTRAL,
        FRIENDLY,
        ENEMY
    }

    __Game _Game;

    __Player _Player;
    public __Ship _Ship;
    __Destructable _Target;
    Transform _Target__Reticle;
    List<__Destructable> _Targets = new List<__Destructable>();

    public Material Material__Energy,
                Material__Hull,
                Material__Armor,
                Material__Shields;

    List<Transform> Bar_R1 = new List<Transform>(),
                        Bar_R2 = new List<Transform>(),
                        Bar_R3 = new List<Transform>(),
                        Bar_R4 = new List<Transform>(),

                        Bar_L1 = new List<Transform>(),
                        Bar_L2 = new List<Transform>(),
                        Bar_L3 = new List<Transform>(),
                        Bar_L4 = new List<Transform>();


    void Awake() {

        _Game = GameObject.Find("__Game Controller").GetComponent<__Game>();
        _Player = GameObject.Find("__Player").GetComponent<__Player>();
        _Ship = _Player._Ship__Current;

        _Target__Reticle = transform.Find("Target_Box");

        /* Load all of the bars into their respective arrays */
        Transform[] _Transforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform _Each in _Transforms) {
            if (_Each.name.StartsWith("R_1_"))
                Bar_R1.Add(_Each);
            else if (_Each.name.StartsWith("R_2_"))
                Bar_R2.Add(_Each);
            else if (_Each.name.StartsWith("R_3_"))
                Bar_R3.Add(_Each);
            else if (_Each.name.StartsWith("R_4_"))
                Bar_R4.Add(_Each);
            
            else if (_Each.name.StartsWith("L_1_"))
                Bar_L1.Add(_Each);
            else if (_Each.name.StartsWith("L_2_"))
                Bar_L2.Add(_Each);
            else if (_Each.name.StartsWith("L_3_"))
                Bar_L3.Add(_Each);
            else if (_Each.name.StartsWith("L_4_"))
                Bar_L4.Add(_Each);
        }

        /* Sort by name... */

        Bar_R1.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });
        Bar_R2.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });
        Bar_R3.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });
        Bar_R4.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });

        Bar_L1.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });
        Bar_L2.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });
        Bar_L3.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });
        Bar_L4.Sort(delegate(Transform t1, Transform t2) { return t1.name.CompareTo(t2.name); });

        /* Set the materials for all of the bars */
        Bar_R1.ForEach((obj) => { obj.renderer.material = Material__Energy; });
        Bar_R2.ForEach((obj) => { obj.renderer.material = Material__Hull; });
        Bar_R3.ForEach((obj) => { obj.renderer.material = Material__Armor; });
        Bar_R4.ForEach((obj) => { obj.renderer.material = Material__Shields; });

        Bar_L1.ForEach((obj) => { obj.renderer.material = Material__Energy; });
        Bar_L2.ForEach((obj) => { obj.renderer.material = Material__Hull; });
        Bar_L3.ForEach((obj) => { obj.renderer.material = Material__Armor; });
        Bar_L4.ForEach((obj) => { obj.renderer.material = Material__Shields; });


        Ship__Set(_Ship);
        Target__Clear();
    }
    void Update() {
        if (_Target != null)
            Target__Reticle();
    }

    public void Target__Reticle() {
        // Move the reticle on the screen to hover over the target...

        _Target__Reticle.transform.position = Vector3.Lerp(Camera.main.transform.position, _Target.transform.position,
            Vector3.Distance(Camera.main.transform.position, _Target__Reticle.transform.position)
            / Vector3.Distance(Camera.main.transform.position, _Target.transform.position));

        _Target__Reticle.renderer.enabled = true;

        /* Enables the model's targeting cage... sometimes is hard to see though, so disabled.
         * 
        Transform _Cage = _Target.transform.Find("__Targeting Cage");
        if (_Cage != null) {
            _Cage.renderer.enabled = Vector3.Distance(Camera.main.transform.position, _Target.transform.position) < 200;
            _Target__Reticle.renderer.enabled = !_Cage.renderer.enabled;
        } else
            _Target__Reticle.renderer.enabled = true;
        */
    }

    public void Target__Enemy() {
        _Targets = new List<__Destructable>(GameObject.FindObjectsOfType(typeof(__Destructable)) as __Destructable[])
            .FindAll((obj) => Target__Affinity(obj) == Affinities.ENEMY);

        _Targets.Sort(delegate(__Destructable d1, __Destructable d2) {
            return Vector3.Distance(_Player.transform.position, d1.transform.position)
                .CompareTo(Vector3.Distance(_Player.transform.position, d2.transform.position)); });

        if (_Targets.Count > 0) {
            Target__Clear();

            if (_Target == _Targets[0] && _Targets.Count > 1)
                Target__Set(_Targets[1]);
            else
                Target__Set(_Targets[0]);
        }
    }
    public void Target__Friendly() {
        _Targets = new List<__Destructable>(GameObject.FindObjectsOfType(typeof(__Destructable)) as __Destructable[])
            .FindAll((obj) => Target__Affinity(obj) == Affinities.FRIENDLY);

        _Targets.Sort(delegate(__Destructable d1, __Destructable d2) {
            return Vector3.Distance(_Player.transform.position, d1.transform.position)
                .CompareTo(Vector3.Distance(_Player.transform.position, d2.transform.position));
        });

        if (_Targets.Count > 0) {
            Target__Clear();

            if (_Target == _Targets[0] && _Targets.Count > 1)
                Target__Set(_Targets[1]);
            else
                Target__Set(_Targets[0]);
        }
    }
    public void Target__Nearest() {
        _Targets = new List<__Destructable>(GameObject.FindObjectsOfType(typeof(__Destructable)) as __Destructable[]);

        _Targets.Sort(delegate(__Destructable d1, __Destructable d2) {
            return Vector3.Distance(_Player.transform.position, d1.transform.position)
                .CompareTo(Vector3.Distance(_Player.transform.position, d2.transform.position));
        });

        if (_Targets.Count > 0) {
            Target__Clear();
            Target__Set(_Targets[0]);
        }
    }
    public Affinities Target__Affinity(__Destructable Incoming) {
        // Is the target a friendly or an enemy??
        // Based on factions, reputation... may eventually get complex...

        if (Incoming.Faction == _Ship.Faction)
            return Affinities.FRIENDLY;
        else if (Incoming.Faction == __Destructable.Factions.NONE)
            return Affinities.NEUTRAL;
        else
            return Affinities.ENEMY;
    }

    public void Target__Set (__Destructable Target) {
        _Target = Target;
        Target__Update(Target);
        _Target.On_Hitpoints += new __Destructable._On_Hitpoints(Target__Update);
    }
    public void Target__Clear() {
        if (_Target != null)
            _Target.On_Hitpoints -= new __Destructable._On_Hitpoints(Target__Update);

        _Target__Reticle.renderer.enabled = false;

        Bar_L1.ForEach((obj) => obj.renderer.enabled = false);
        Bar_L2.ForEach((obj) => obj.renderer.enabled = false);
        Bar_L3.ForEach((obj) => obj.renderer.enabled = false);
        Bar_L4.ForEach((obj) => obj.renderer.enabled = false);
    }
    public void Target__Update(__Destructable Target) {
        float n = 0;

        // Update shield hitpoints bar
        n = (Target.HP__Shield / Target.HP_Max__Shield) * Bar_L4.Count;
        for (int i = 0; i < Bar_L4.Count; i++)
            Bar_L4[i].renderer.enabled = i < n;

        // Update armor hitpoints bar
        n = (Target.HP__Armor / Target.HP_Max__Armor) * Bar_L3.Count;
        for (int i = 0; i < Bar_L3.Count; i++)
            Bar_L3[i].renderer.enabled = i < n;
        
        // Update structure hitpoints bar
        n = (Target.HP__Structure / Target.HP_Max__Structure) * Bar_L2.Count;
        for (int i = 0; i < Bar_L2.Count; i++)
            Bar_L2[i].renderer.enabled = i < n;

        // Update energy hitpoints bar
        n = (Target.HP__Energy / Target.HP_Max__Energy) * Bar_L1.Count;
        for (int i = 0; i < Bar_L1.Count; i++)
            Bar_L1[i].renderer.enabled = i < n;

        Transform _Cage = _Target.transform.Find("__Targeting Cage");
        
        switch (Target__Affinity(_Target)) {
            default:
            case Affinities.NEUTRAL:
                _Target__Reticle.renderer.material.color = Color.grey;
                if (_Cage != null)
                    _Cage.renderer.material.color = Color.grey;
                break;
            case Affinities.ENEMY:
                _Target__Reticle.renderer.material.color = Color.red;
                if (_Cage != null)
                    _Cage.renderer.material.color = Color.red;
                break;
            case Affinities.FRIENDLY:
                _Target__Reticle.renderer.material.color = Color.green;
                if (_Cage != null)
                    _Cage.renderer.material.color = Color.green;
                break;

        }
    }

    public void Ship__Set(__Ship Ship) {
        _Ship = Ship;
        Ship__Update(Ship);
        _Ship.On_Hitpoints += new __Destructable._On_Hitpoints(Ship__Update);
    }
    public void Ship_Clear() {
        if (_Ship != null)
            _Ship.On_Hitpoints -= new __Destructable._On_Hitpoints(Ship__Update);
    }
    public void Ship__Update(__Destructable Ship) {
        float n = 0;

        // Update shield hitpoints bar
        n = (Ship.HP__Shield / Ship.HP_Max__Shield) * Bar_R4.Count;
        for (int i = 0; i < Bar_R4.Count; i++)
            Bar_R4[i].renderer.enabled = i < n;

        // Update armor hitpoints bar
        n = (Ship.HP__Armor / Ship.HP_Max__Armor) * Bar_R3.Count;
        for (int i = 0; i < Bar_R3.Count; i++)
            Bar_R3[i].renderer.enabled = i < n;

        // Update structure hitpoints bar
        n = (Ship.HP__Structure / Ship.HP_Max__Structure) * Bar_R2.Count;
        for (int i = 0; i < Bar_R2.Count; i++)
            Bar_R2[i].renderer.enabled = i < n;
    }


}
