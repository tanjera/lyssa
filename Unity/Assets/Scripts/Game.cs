﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

    public Player Player_1,
        Enemy_1, Enemy_2, Enemy_3;

    public bool has_Enemy_1, has_Enemy_2, has_Enemy_3;

    void Start() {
        
        State__Major = Game_States__Major.Initializing;

        Player_1 = new Player(Player.Types.Human);
        Enemy_1 = new Player(Player.Types.Computer);
        Enemy_2 = new Player(Player.Types.Computer);
        Enemy_3 = new Player(Player.Types.Computer);

        Player_1.Stats_Changed += new Player.Stats_Changed_Handler(Update_Stats);
        Enemy_1.Stats_Changed += new Player.Stats_Changed_Handler(Update_Stats);
        Enemy_2.Stats_Changed += new Player.Stats_Changed_Handler(Update_Stats);
        Enemy_3.Stats_Changed += new Player.Stats_Changed_Handler(Update_Stats);

        Player_1.Combat_Output += new Player.Combat_Output_Handler(Combat_Output);
        Enemy_1.Combat_Output += new Player.Combat_Output_Handler(Combat_Output);
        Enemy_2.Combat_Output += new Player.Combat_Output_Handler(Combat_Output);
        Enemy_3.Combat_Output += new Player.Combat_Output_Handler(Combat_Output);

        // Construct the playing board
        Playing_Board__Container = GameObject.Find("Playing_Board__Item_Container").transform;
        Playing_Board__Positions = GameObject.Find("Playing_Board__Item_Positions").transform;
        Build__Matrix(Playing_Board, Playing_Board__Positions);
        Populate__Playing_Board();

        // Construct the player's mana board
        Mana_Board_Player__Container = GameObject.Find("Mana_Board_Player__Item_Container").transform;
        Mana_Board_Player__Positions = GameObject.Find("Mana_Board_Player__Item_Positions").transform;
        Build__Matrix(Mana_Board_Player, Mana_Board_Player__Positions);
        Populate__Mana_Board(Mana_Board_Player);

        if (has_Enemy_1) { // Construct the 1st enemy's mana board
            Mana_Board_Enemy_1__Container = GameObject.Find("Mana_Board_Enemy_1__Item_Container").transform;
            Mana_Board_Enemy_1__Positions = GameObject.Find("Mana_Board_Enemy_1__Item_Positions").transform;
            Build__Matrix(Mana_Board_Enemy_1, Mana_Board_Enemy_1__Positions);
            Populate__Mana_Board(Mana_Board_Enemy_1);
        }

        if (has_Enemy_2) { // Construct the 2nd enemy's mana board
            Mana_Board_Enemy_2__Container = GameObject.Find("Mana_Board_Enemy_2__Item_Container").transform;
            Mana_Board_Enemy_2__Positions = GameObject.Find("Mana_Board_Enemy_2__Item_Positions").transform;
            Build__Matrix(Mana_Board_Enemy_2, Mana_Board_Enemy_2__Positions);
            Populate__Mana_Board(Mana_Board_Enemy_2);
        }

        if (has_Enemy_3) { // Construct the 3rd enemy's mana board
            Mana_Board_Enemy_3__Container = GameObject.Find("Mana_Board_Enemy_3__Item_Container").transform;
            Mana_Board_Enemy_3__Positions = GameObject.Find("Mana_Board_Enemy_3__Item_Positions").transform;
            Build__Matrix(Mana_Board_Enemy_3, Mana_Board_Enemy_3__Positions);
            Populate__Mana_Board(Mana_Board_Enemy_3);
        }

        State__Major = Game_States__Major.Running;
        State__Minor = Game_States__Minor.Turn_Player__Idle;

        Hack__Setup_Test_States();
    }

    void Hack__Setup_Test_States() {
        Player_1.Character = Epio;
        Player_1.Target = Enemy_1;

        Enemy_1.Model = GameObject.Find("Wooden Dummy");
        Enemy_1._Animator = Enemy_1.Model.GetComponent<Animator>();
        Enemy_1.Character = Wooden_Dummy;
        Enemy_1.Target = Player_1;
    }


#region ROUTINE & INPUT

    enum Game_States__Major {
        Initializing,
        Paused,
        Running
    }
    enum Game_States__Minor {
        Null,
        Turn_Player__Idle,
        Turn_Player__Dragging,
        Turn_Player__Attack,
        Turn_Enemy1,
        Turn_Enemy2,
        Turn_Enemy3
    }

    Game_States__Major State__Major;
    Game_States__Minor State__Minor;

    Items Drag_Item;
    Mana_Colors Drag_Color;
    Transform Drag_Joint;
    float Drag_Depth,
        Drag_Force = 1000,
        Drag_Damping = 1;

    bool Console_Active = false;
    public GameObject Console_Panel;
    public Text Console_Input, Console_Log;
    
    public GameObject UI_Paused;
    public Text UI_Player_Info, UI_Enemy_Info,
        UI_Player_Info__Short, UI_Enemy_Info__Short,
        Combat_Log;

    void FixedUpdate() {
        Transformation_Buffer.ForEach(obj => { if (obj.Process()) Transformation_Buffer.Remove(obj); });
    }
    void Update() {
        
        /* Developer console functions */
        if (Input.GetKeyDown(KeyCode.BackQuote))
            Console_Toggle();
        if (Console_Active)
            Console_Process();


        /* Main game routine */
        switch (State__Major) {
            default:
            case Game_States__Major.Initializing:
                return;

            case Game_States__Major.Paused:
                if (Input.GetKeyDown(KeyCode.Escape))
                    Change_State(Game_States__Major.Running);
                break;

            case Game_States__Major.Running: {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Change_State(Game_States__Major.Paused);
                
                /* Player input if it's the player's turn, to drag stones */
                if (State__Minor == Game_States__Minor.Turn_Player__Idle) {
                    if (Input.GetMouseButtonDown(0)) {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit)) {
                            if (hit.transform.gameObject.layer == LayerMask.NameToLayer(Layer__Interactive)) {

                                Drag_Item = Playing_Board.Cells.Find(obj => obj.Item.Transform == hit.transform).Item;
                                Drag_Color = Drag_Item.Color;   // Last color dragged, never cleared, only reassigned for reference.
                                Release_Item(Drag_Item);

                                Drag_Depth = Camera.main.transform.InverseTransformPoint(hit.point).z;
                                Drag_Joint = Drag_AttachJoint(hit.rigidbody, hit.point);
                                Scale(hit.transform, hit.transform.localScale * 0.7f, 0.1f);
                                Change_State(Game_States__Minor.Turn_Player__Dragging);
                            }
                        }
                    }
                }

                if (State__Minor == Game_States__Minor.Turn_Player__Dragging) {
                    if (Input.GetMouseButton(0)) {
                        if (Drag_Joint == null)
                            return;
                        Drag_Joint.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    }

                    if (Input.GetMouseButtonUp(0)) {
                        if (Drag_Joint != null && Drag_Joint.gameObject != null)
                            Destroy(Drag_Joint.gameObject);
                        if (Drag_Item != null)
                            Destroy_Item(Drag_Item);

                        Change_State(Game_States__Minor.Turn_Player__Attack);
                        Drag_Item = null;

                        Refresh__Playing_Board();
                    } 
                }

                break;
            }
        }   
    }

    void Change_State(Game_States__Minor incState) {
        Game_States__Minor oldState = State__Minor;
        State__Minor = incState;

        switch (incState) {
            default: break;

            case Game_States__Minor.Turn_Player__Attack:
                if (Player_1 != null) {
                    Player_1.Attack(Player_1.Target);
                    Change_State(Game_States__Minor.Turn_Enemy1, 2);
                }
                else Change_State(Game_States__Minor.Turn_Enemy1);
                break;

            case Game_States__Minor.Turn_Enemy1:
                if (has_Enemy_1) {
                    Enemy_1.Attack(Enemy_1.Target);
                    Change_State(Game_States__Minor.Turn_Enemy2, 2);
                } 
                else Change_State(Game_States__Minor.Turn_Enemy2);
                break;

            case Game_States__Minor.Turn_Enemy2:
                if (has_Enemy_2) {
                    Enemy_2.Attack(Enemy_2.Target);
                    Change_State(Game_States__Minor.Turn_Enemy3, 2);
                }
                else Change_State(Game_States__Minor.Turn_Enemy3);
                break;

            case Game_States__Minor.Turn_Enemy3:
                if (has_Enemy_3) {
                    Enemy_3.Attack(Enemy_3.Target);
                    Change_State(Game_States__Minor.Turn_Player__Idle, 2);
                } 
                else Change_State(Game_States__Minor.Turn_Player__Idle);
                break;
        }
    }
    void Change_State(Game_States__Major incState) {
        Game_States__Major oldState = State__Major;
        State__Major = incState;

        if (incState == Game_States__Major.Paused)
            Pause();
        else if (incState == Game_States__Major.Running && oldState == Game_States__Major.Paused)
            Unpause();
    }
    void Change_State(Game_States__Minor incState, float waitTime) {
        StartCoroutine(Change_State__Delay(incState, waitTime));
    }
    IEnumerator Change_State__Delay(Game_States__Minor incState, float waitTime) {
        yield return new WaitForSeconds(waitTime);
        Change_State(incState);
    }

    void Pause() {

        UI_Player_Info.text = String.Format("<size=15>{0}</size>\n Level {1}\n\n Exp {2} / {3}\n Health {4} / {5}\n\n\n"
            + "<size=11>Mana Pool\n {6} G : {7} B : {8} W\n {9} Y : {10} R\n\n\n"
            + "Damage\n {11} G : {12} B : {13} W\n {14} Y : {15} R</size>",
            Player_1.Character.Name, Player_1.Character.Level,
            Player_1.Character.Experience, Player_1.Character.Experience_Max,
            Player_1.Character.Health, Player_1.Character.Health_Max,
            Player_1.Mana_Count[0], Player_1.Mana_Count[1], Player_1.Mana_Count[2], Player_1.Mana_Count[3], Player_1.Mana_Count[4],
            Player_1.Character.Damage[0], Player_1.Character.Damage[1], Player_1.Character.Damage[2], Player_1.Character.Damage[3], Player_1.Character.Damage[4]
            );

        // FIX: CHECK FOR ENEMIES (1, 2, or 3?) and CHECK IF NULL before accessing... ... ...
        UI_Enemy_Info.text = String.Format("<size=15>{0}</size>\n Level {1}\n\n Health {2} / {3}",
            Enemy_1.Character.Name, Enemy_1.Character.Level,
            Enemy_1.Character.Health, Enemy_1.Character.Health_Max
            );

        UI_Paused.SetActive(true);
    }
    void Unpause() {
        UI_Paused.SetActive(false);
    }

    void Console_Toggle() {
        Console_Active = !Console_Active;
        Console_Panel.SetActive(Console_Active);
    }
    void Console_Process() {
        foreach (char c in Input.inputString) {
            switch (c) {
                case '`':
                    return;

                case '\n':
                case '\r':
                    Console_Output(string.Concat("> ", Console_Input.text));    
                    Console_Parse(Console_Input.text);
                    Console_Input.text = "";
                    break;

                case '\b':
                    if (Console_Input.text.Length > 0)
                        Console_Input.text = Console_Input.text.Remove(Console_Input.text.Length - 1);
                    break;

                default:
                    Console_Input.text += c;
                    break;
            }
        }
    }
    void Console_Parse(string incString) {
        string[] incCommand = incString.ToLower().Split(' ');
        
        if (incCommand.Length == 0)
            return;
        
        switch (incCommand[0].Trim()) {
            default:
                Console_Output("... input not recognized :(");
                return;

            case "set":
                Player incPlayer;   // Which player are we setting values for?
                if (incCommand[1] == null) return;    
                switch (incCommand[1].Trim()) {
                    default: return;
                    case "help":
                        Console_Output("set [player/enemy1/enemy2/enemy3] [field] [value] [values] \nfields: level, health, health_max, mana, damage");
                        return;
                    case "player": incPlayer = Player_1; break;
                    case "enemy1": incPlayer = Enemy_1; break;
                    case "enemy2": incPlayer = Enemy_2; break;
                    case "enemy3": incPlayer = Enemy_3; break;
                }

                if (incCommand[2] == null || incCommand[3] == null) return;
                switch (incCommand[2].Trim()) {
                    default: return;
                    case "level":
                        incPlayer.Character.Level = int.Parse(incCommand[3].Trim());
                        Update_Stats();
                        return;
                    case "health":
                        incPlayer.Character.Health = int.Parse(incCommand[3].Trim());
                        if (incPlayer.Character.Health > incPlayer.Character.Health_Max)
                            incPlayer.Character.Health_Max = incPlayer.Character.Health;
                        Update_Stats();
                        return;
                    case "health_max": 
                        incPlayer.Character.Health_Max = int.Parse(incCommand[3].Trim());
                        Update_Stats();
                        return;
                    case "mana":
                        if (incCommand[4] == null || incCommand[5] == null || incCommand[6] == null || incCommand[7] == null) return;
                        incPlayer.Mana_Count[0] = int.Parse(incCommand[3].Trim());
                        incPlayer.Mana_Count[1] = int.Parse(incCommand[4].Trim());
                        incPlayer.Mana_Count[2] = int.Parse(incCommand[5].Trim());
                        incPlayer.Mana_Count[3] = int.Parse(incCommand[6].Trim());
                        incPlayer.Mana_Count[4] = int.Parse(incCommand[7].Trim());
                        Update_Stats();
                        return;
                    case "damage":
                        if (incCommand[4] == null || incCommand[5] == null || incCommand[6] == null || incCommand[7] == null) return;
                        incPlayer.Character.Damage[0] = double.Parse(incCommand[3].Trim());
                        incPlayer.Character.Damage[1] = double.Parse(incCommand[4].Trim());
                        incPlayer.Character.Damage[2] = double.Parse(incCommand[5].Trim());
                        incPlayer.Character.Damage[3] = double.Parse(incCommand[6].Trim());
                        incPlayer.Character.Damage[4] = double.Parse(incCommand[7].Trim());
                        return;
                }

                return;
            case "clear":
                Console_Log.text = "";
                return;
        }

        Console_Output("... input error, unable to process :'(");
    }
    void Console_Output(string incOutput) {
        Console_Log.text = string.Concat(Console_Log.text, "\n", incOutput);
        // Scroll the console down...
        if (Console_Log.preferredHeight > Console_Log.rectTransform.rect.height) {
            Console_Log.text = Console_Log.text.Trim('\n', ' ');
            Console_Log.text = Console_Log.text.Substring(Console_Log.text.IndexOf('\n'));
        }
    }

    void Combat_Output(string incOutput) {
        Combat_Log.text = string.Concat(Combat_Log.text, "\n", incOutput).Trim();
        // Scroll the combat log down...
        if (Combat_Log.preferredHeight > Combat_Log.rectTransform.rect.height) {
            Combat_Log.text = Combat_Log.text.Trim('\n', ' ');
            Combat_Log.text = Combat_Log.text.Substring(Combat_Log.text.IndexOf('\n'));
        }
    }
    void Update_Stats() {
        Refresh__Mana_Boards();

        if (UI_Player_Info__Short != null && Player_1.Character != null)
            UI_Player_Info__Short.text = String.Format("{0}\n Level {1}\n\n Exp {2} / {3}\n Health {4} / {5}",
                Player_1.Character.Name, Player_1.Character.Level,
                Player_1.Character.Experience, Player_1.Character.Experience_Max,
                Player_1.Character.Health, Player_1.Character.Health_Max
                );

        // FIX: CHECK FOR ENEMIES (1, 2, or 3?) and CHECK IF NULL before accessing... ... ...
        if (UI_Enemy_Info__Short != null && Enemy_1.Character != null)
            UI_Enemy_Info__Short.text = String.Format("{0}\n Level {1}\n\n Health {2} / {3}",
                Enemy_1.Character.Name, Enemy_1.Character.Level,
                Enemy_1.Character.Health, Enemy_1.Character.Health_Max
                );
    }

    Transform Drag_AttachJoint(Rigidbody body, Vector3 attachPoint) {
        GameObject obj = new GameObject("Attachment Point");
        obj.hideFlags = HideFlags.HideInHierarchy;
        obj.transform.position = attachPoint;

        Rigidbody rigid = obj.AddComponent<Rigidbody>();
        rigid.isKinematic = true;
        
        FixedJoint joint = obj.AddComponent<FixedJoint>();
        joint.connectedBody = body;

        return obj.transform;
    }

#endregion


#region 3D TRANSFORMATIONS

    class Transformation {
        public enum Operations {
            Move,
            Scale,
            Clamp_Rotation
        }

        Operations Operation;
        float Time;
        Transform Transform;
        Vector3 Target,
            Velocity;

        public Transformation(Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime) {
            Operation = incOperation;
            Transform = incTransform;
            Target = incTarget;
            Time = incTime;
        }

        public void Modify(Vector3 incTarget, float incTime) {
            if (Target == incTarget && Time == incTime)
                return;

            Target = incTarget;
            Time = incTime;
        }

        public bool Process() {
            if (Transform == null)
                return true; // If the Transform has been destroyed, remove it from the stack...

            switch (Operation) {
                default:
                case Operations.Move: return Move();
                case Operations.Scale: return Scale();
                case Operations.Clamp_Rotation: return Clamp_Rotation();
            }
        }
        bool Move() {
            Transform.position = Vector3.SmoothDamp(Transform.position, Target, ref Velocity, Time);
            return Transform.position == Target;
        }
        bool Scale() {
            Transform.localScale = Vector3.SmoothDamp(Transform.localScale, Target, ref Velocity, Time);
            return Transform.localScale == Target;
        }
        bool Clamp_Rotation() {
            Vector3 tempAngles = Transform.localEulerAngles;
            tempAngles.x = tempAngles.x < 180 ? Mathf.Clamp(tempAngles.x, 0, Target.x) : Mathf.Clamp(tempAngles.x, 360 - Target.x, 360);
            tempAngles.y = tempAngles.y < 180 ? Mathf.Clamp(tempAngles.y, 0, Target.y) : Mathf.Clamp(tempAngles.y, 360 - Target.y, 360);
            tempAngles.z = tempAngles.z < 180 ? Mathf.Clamp(tempAngles.z, 0, Target.z) : Mathf.Clamp(tempAngles.z, 360 - Target.z, 360);
            Transform.localEulerAngles = tempAngles;
            return false;
        }

        public Transform _Transform { get { return Transform; } }
        public Operations _Operation { get { return Operation; } }
    }
    List<Transformation> Transformation_Buffer = new List<Transformation>();

    public void Move(Transform incTransform, Vector3 incTarget, float incTime) 
        { Operate(Transformation.Operations.Move, incTransform, incTarget, incTime); }
    public void Scale (Transform incTransform, Vector3 incTarget, float incTime) 
        { Operate(Transformation.Operations.Scale, incTransform, incTarget, incTime); }
    void Operate(Transformation.Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime) {
        if (Transformation_Buffer.Find(obj => obj._Operation == incOperation && obj._Transform == incTransform) == null)
            Transformation_Buffer.Add(new Transformation(incOperation, incTransform, incTarget, incTime));
        else
            Transformation_Buffer.Find(obj => obj._Operation == incOperation && obj._Transform == incTransform).Modify(incTarget, incTime);
    }

#endregion


#region DEFINITIONS

    public static string Layer__Interactive = "Interactive";

    public static int _Mana_Colors = 5;
    public static float Mana_Board__Multiplier = 2.5f;

    public enum Mana_Colors {
        Green,
        Blue,
        White,
        Yellow,
        Red
    }
    
    public static Color[] Lookup_Colors = new Color[] {
        Color.green,
        Color.blue,
        Color.white,
        Color.yellow,
        Color.red
    };

    public enum Mana_Board {
        Experience,
        Health,
        Green,
        Blue,
        White,
        Yellow,
        Red
    }

    

#endregion


#region PLAYER

    public class Player {

        public enum Types {
            Human,
            Computer
        }

        Types _Type;
        Characters _Character;
        public GameObject Model;
        public Animator _Animator;
        public Player Target;


        public int[] Mana_Count = new int[Enum.GetValues(typeof(Game.Mana_Colors)).Length],
                        Mana_Buffer = new int[Enum.GetValues(typeof(Game.Mana_Colors)).Length];

        public delegate void Combat_Output_Handler(string combatOutput);
        public event Combat_Output_Handler Combat_Output;
        public delegate void Stats_Changed_Handler();
        public event Stats_Changed_Handler Stats_Changed;
        void Pass__Stats_Changed(Characters sender, EventArgs e) { Stats_Changed(); }

        public Player(Types incType) {
            _Type = incType;
        }
        public Characters Character {
            get { return _Character; }
            set { 
                _Character = value; 
                _Character.Stats_Changed += new Characters.Stats_Changed_Handler(Pass__Stats_Changed);
                Stats_Changed();
            }
        }

        public int Mana(int[] manaArray, Game.Mana_Colors Color) { return manaArray[(int)Color]; }
        public void Add_Mana(int[] manaArray, Game.Mana_Colors Color, int Amount) { 
            manaArray[(int)Color] += Amount;
            Stats_Changed();
        }
        public bool Use_Mana(int [] manaArray, Mana_Colors incColor, int Amount) {
            if (manaArray[(int)incColor] < Amount)
                return false;
            manaArray[(int)incColor] -= Amount;

            Stats_Changed();
            return true;
        }

        public void Attack(Player incTarget) {
            bool willHit = true;
            double totalDamage = 0;
     
            // Calculate damages
            if (_Type == Types.Human) {
                for (int i = 0; i < _Mana_Colors; i++)
                    totalDamage += Mana_Buffer[i] * Character.Damage[i];
                totalDamage = (int)totalDamage;
            }
            else if (_Type == Types.Computer) {
                willHit = UnityEngine.Random.Range(0f, 1f) < (float)Character.Hit_Chance;
                for (int i = 0; i < _Mana_Colors; i++)
                    totalDamage += Character.Damage[i];
                totalDamage *= UnityEngine.Random.Range(0.25f, 2.0f);
                totalDamage = (int)totalDamage;
            }
            
            // Output information to console
            if (Combat_Output != null) {
                if (willHit)
                    Combat_Output(String.Format("{0} attacked {1} for {2} damage!!", Character.Name, incTarget.Character.Name, totalDamage));
                else
                    Combat_Output(String.Format("{0}'s attack missed {1}!", Character.Name, incTarget.Character.Name));
            }
            if (willHit)
                incTarget.Character.Health = (int)(incTarget.Character.Health - totalDamage < 0 ? 0 : incTarget.Character.Health - totalDamage);

            if (incTarget.Character.Health == 0)
                Combat_Output(String.Format("*** {0} defeated {1}!!! ***", Character.Name, incTarget.Character.Name));

            // Animate models
            if (_Animator != null)
                _Animator.Play("Attack_1");
            if (Target._Animator != null)
                Target._Animator.Play("Defend_1");

            // Routine stuff
            Mana_Buffer = new int[_Mana_Colors];
            Stats_Changed();
        }
    }

    public class Characters {

        public string Name;

        public double Damage_per_Level, Hit_Chance;
        public double[] Damage = new double[_Mana_Colors];

        int _Health, _Health_Max, Health_per_Level,
            _Level, _Experience, _Experience_Max;

        public delegate void Stats_Changed_Handler(Characters sender, EventArgs e);
        public event Stats_Changed_Handler Stats_Changed;

        public Characters(string incName, int incHP_Base, int incHP_per_Lvl, double[] incDamage, double incDmg_per_Lvl, double incHit_Chance = 0.9, int incLvl = 1) {
            Name = incName;
            Level = incLvl;

            Health_per_Level = incHP_per_Lvl;
            _Health_Max = incHP_Base + (Level * Health_per_Level);
            _Health = _Health_Max;

            Hit_Chance = incHit_Chance;
            Damage_per_Level = incDmg_per_Lvl;
            for (int i = 0; i < _Mana_Colors; i++)
                Damage[i] = incDamage[i] * (Level * Damage_per_Level);
        }

        public int Level {
            get { return _Level; }
            set {
                if (value < 1)
                    return;

                int lvlDiff = value - _Level;
                _Level = value;

                _Health_Max += lvlDiff * Health_per_Level;
                _Health = _Health_Max;

                for (int i = 0; i < _Mana_Colors; i++)
                    Damage[i] = Damage[i] * (lvlDiff * Damage_per_Level);

                if (Stats_Changed != null)
                    Stats_Changed(this, new EventArgs()); 
            }
        }

        public int Health {
            get { return _Health; }
            set { _Health = value; Stats_Changed(this, new EventArgs()); }
        }
        public int Health_Max {
            get { return _Health_Max; }
            set { _Health_Max = value; Stats_Changed(this, new EventArgs()); }
        }

        public int Experience {
            get { return _Experience; }
            set { _Experience = value; Stats_Changed(this, new EventArgs()); }
        }
        public int Experience_Max {
            get { return _Experience_Max; }
            set { _Experience_Max = value; Stats_Changed(this, new EventArgs()); }
        }
    }

    static Characters
        Epio = new Characters("Epio",
            75, 25, new double[] { 0.1, 0.1, 1, 1, 0.1 }, 1.5),
        Wooden_Dummy = new Characters("Wooden Dummy",
            75, 25, new double[] { 0.1, 0.1, 1, 1, 0.1 }, 1.5);



#endregion


#region PLAYING BOARD & MANA BOARDS

    #region MATRICES (PLAYING & MANA BOARDS)

    public class Items {
        public enum Types {
            Stone,
            Consumable,
            Equipment,
            Badge
        }

        public Types Type;
        public Game.Mana_Colors Color;
        public GameObject Object;

        public int Column;
        public int Row;

        public Transform Transform {
            get { return Object == null ? null : Object.transform; }
        }
    }
    public class Cell {
        public int X, Y;
        public Transform Location;
        public Items Item;

        public bool isEmpty {
            get { return Item == null; }
        }
        public bool Active {
            get { return Item == null || Item.Object == null ? false : Item.Object.activeSelf; }
            set { if (Item != null && Item.Object != null) Item.Object.SetActive(value); }
        }

        public Vector2 Coords {
            get { return new Vector2(X, Y); }
        }
    }
    public class Matrix {
        public List<Cell> Cells = new List<Cell>();

        public Cell Cell_Above(Vector2 Coords) { return Cells.Find(obj => obj.X == Coords.x && obj.Y == Coords.y + 1); }
        public Cell Cell_Below(Vector2 Coords) { return Cells.Find(obj => obj.X == Coords.x && obj.Y == Coords.y - 1); }
        public Cell Cell_Top(Vector2 Coords) {
            List<Cell> listCol = Cells.FindAll(obj => obj.X == Coords.x);
            return Cells.Find(obj => obj.X == Coords.x && obj.Y == listCol.Max(o => o.Y));
        }
        public List<Cell> Cell_Tops() {
            List<Cell> listBuffer = new List<Cell>();
            Cells.ForEach(obj => {
                if (listBuffer.FindAll(pred => pred.X == obj.X).Count == 0)
                    listBuffer.Add(obj);
                else
                    for (int i = 0; i < listBuffer.Count; i++)
                        if (listBuffer[i].X == obj.X && listBuffer[i].Y < obj.Y)
                            listBuffer[i] = obj;
            });
            return listBuffer;
        }

        
    }   
    
    #endregion

    Matrix Playing_Board = new Matrix(),
        Mana_Board_Player = new Matrix(),
        Mana_Board_Enemy_1 = new Matrix(),
        Mana_Board_Enemy_2 = new Matrix(),
        Mana_Board_Enemy_3 = new Matrix();

    Transform Playing_Board__Container,
        Mana_Board_Player__Container,
        Mana_Board_Enemy_1__Container,
        Mana_Board_Enemy_2__Container,
        Mana_Board_Enemy_3__Container;
    
    Transform Playing_Board__Positions,
        Mana_Board_Player__Positions,
        Mana_Board_Enemy_1__Positions,
        Mana_Board_Enemy_2__Positions,
        Mana_Board_Enemy_3__Positions;

    public GameObject Prototype__Stone9,
                        Prototype__Stone9_Mini;
    public GameObject[] Prototype__Particles = new GameObject[_Mana_Colors];

    Transform Item_Dragging;

    void Build__Matrix(Matrix incMatrix, Transform incPositions) {
        // Run through all __Item_Positions...
        List<Transform> itemPositions = new List<Transform>(incPositions.GetComponentsInChildren<Transform>());
        itemPositions = itemPositions.FindAll(obj => obj.name.Contains(";"));

        for (int i = 0; i < itemPositions.Count; i++) {
            // Make sure we're dealing with an actual item position: gameobject naming convention "[X];[Y]"
            Cell newCell = new Cell();
            newCell.Location = itemPositions[i];
            newCell.X = int.Parse(itemPositions[i].name.Split(new char[] { ';' })[0]);
            newCell.Y = int.Parse(itemPositions[i].name.Split(new char[] { ';' })[1]);
            incMatrix.Cells.Add(newCell);
        }
    }
    void Populate__Playing_Board() {
        Playing_Board.Cells.ForEach(obj => {
            obj.Item = Create_Item(obj, Items.Types.Stone, Prototype__Stone9,
                (Mana_Colors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Mana_Colors)).Length),
                Playing_Board__Container, true, true);
        });
    }
    void Refresh__Playing_Board() {
        Playing_Board.Cells.ForEach(obj => {
            if (obj.isEmpty)
                obj.Item = Create_Item(obj, Items.Types.Stone, Prototype__Stone9,
                (Mana_Colors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Mana_Colors)).Length),
                Playing_Board__Container, true, true);
        });
    }
    void Iterate_Down__Playing_Board() {
        bool isComplete = false;
        while (!isComplete) {
            isComplete = true;
            Playing_Board.Cells.ForEach(obj => {
                if (obj.isEmpty) {
                    Cell cellAbove = Playing_Board.Cell_Above(obj.Coords);
                    if (cellAbove != null && !cellAbove.isEmpty) {
                        obj.Item = cellAbove.Item;                                                  // Swap the Item among cells
                        cellAbove.Item = null;
                        Move(obj.Item.Object.transform, obj.Location.position, 0.1f);               // Move the actual gameObject
                        isComplete = false;                                                         // Set to reiterate loop
                    }
                }
            });
        }
    }
    void Populate__Mana_Board(Matrix incMatrix) {
        incMatrix.Cells.ForEach(cell => {
            if (cell.Item != null)
                return;
        
            if (incMatrix == Mana_Board_Player) {
                    if (cell.X == Mana_Board.Experience.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Yellow, Mana_Board_Player__Container, false, false); 
                    }
                    else if (cell.X == Mana_Board.Health.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Red, Mana_Board_Player__Container, false, false);
                    }
                    else if (cell.X == Mana_Board.Green.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Green, Mana_Board_Player__Container, false, false);
                    }
                    else if (cell.X == Mana_Board.Blue.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Blue, Mana_Board_Player__Container, false, false);
                    }
                    else if (cell.X == Mana_Board.White.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.White, Mana_Board_Player__Container, false, false);
                    }
                    else if (cell.X == Mana_Board.Yellow.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Yellow, Mana_Board_Player__Container, false, false);
                    }
                    else if (cell.X == Mana_Board.Red.GetHashCode()) {
                        cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Red, Mana_Board_Player__Container, false, false);
                    }
            
            } else if (incMatrix == Mana_Board_Enemy_1)
                cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Red, Mana_Board_Enemy_1__Container, false, false);
            else if (incMatrix == Mana_Board_Enemy_2)
                cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Red, Mana_Board_Enemy_2__Container, false, false);
            else if (incMatrix == Mana_Board_Enemy_3)
                cell.Item = Create_Item(cell, Items.Types.Badge, Prototype__Stone9_Mini, Mana_Colors.Red, Mana_Board_Enemy_3__Container, false, false);

        });
    }
    void Refresh__Mana_Boards() {
        int cellCount;
        List<Cell> cellList;

        // Update player's experience bar by %
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.Experience.GetHashCode());
        cellCount = cellList.Count;
        for (int i = 0; i < cellCount; i++)
            if (Player_1.Character.Experience_Max > 0)
                cellList[i].Active = Player_1.Character.Experience_Max > 0 && cellList[i].Y < ((float)Player_1.Character.Experience / (float)Player_1.Character.Experience_Max) * cellCount;

        // Update player's health bar by %
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.Health.GetHashCode());
        cellCount = cellList.Count;
        for (int i = 0; i < cellCount; i++)
            if (Player_1.Character.Health_Max > 0)
                cellList[i].Active = Player_1.Character.Health_Max > 0 && cellList[i].Y < ((float)Player_1.Character.Health / (float)Player_1.Character.Health_Max) * cellCount;

        // Update the player's mana counts
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.Green.GetHashCode());
        for (int i = 0; i < cellList.Count; i++)
            cellList[i].Active = cellList[i].Y <= (Player_1.Mana(Player_1.Mana_Count, Mana_Colors.Green) / Mana_Board__Multiplier) - 1;
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.Blue.GetHashCode());
        for (int i = 0; i < cellList.Count; i++)
            cellList[i].Active = cellList[i].Y <= (Player_1.Mana(Player_1.Mana_Count, Mana_Colors.Blue) / Mana_Board__Multiplier) - 1;
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.White.GetHashCode());
        for (int i = 0; i < cellList.Count; i++)
            cellList[i].Active = cellList[i].Y <= (Player_1.Mana(Player_1.Mana_Count, Mana_Colors.White) / Mana_Board__Multiplier) - 1;
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.Yellow.GetHashCode());
        for (int i = 0; i < cellList.Count; i++)
            cellList[i].Active = cellList[i].Y <= (Player_1.Mana(Player_1.Mana_Count, Mana_Colors.Yellow) / Mana_Board__Multiplier) - 1;
        cellList = Mana_Board_Player.Cells.FindAll(cell => cell.X == Mana_Board.Red.GetHashCode());
        for (int i = 0; i < cellList.Count; i++)
            cellList[i].Active = cellList[i].Y <= (Player_1.Mana(Player_1.Mana_Count, Mana_Colors.Red) / Mana_Board__Multiplier) - 1;


        // Update enemies' health bars
        if (has_Enemy_1) {
            cellCount = Mana_Board_Enemy_1.Cells.Count;
            for (int i = 0; i < cellCount; i++)
                if (Enemy_1.Character != null && Enemy_1.Character.Health_Max > 0)
                    Mana_Board_Enemy_1.Cells[i].Active = Enemy_1.Character.Health_Max > 0 
                        && Mana_Board_Enemy_1.Cells[i].Y < ((float)Enemy_1.Character.Health / (float)Enemy_1.Character.Health_Max) * cellCount;
        }
        if (has_Enemy_2) {
            cellCount = Mana_Board_Enemy_2.Cells.Count;
            for (int i = 0; i < cellCount; i++)
                if (Enemy_2.Character != null && Enemy_2.Character.Health_Max > 0)
                    Mana_Board_Enemy_2.Cells[i].Active = Enemy_2.Character.Health_Max > 0 
                        && Mana_Board_Enemy_2.Cells[i].Y < ((float)Enemy_2.Character.Health / (float)Enemy_2.Character.Health_Max) * cellCount;
        }
        if (has_Enemy_3) {
            cellCount = Mana_Board_Enemy_3.Cells.Count;
            for (int i = 0; i < cellCount; i++)
                if (Enemy_3.Character != null && Enemy_3.Character.Health_Max > 0)
                    Mana_Board_Enemy_3.Cells[i].Active = Enemy_3.Character.Health_Max > 0 
                        && Mana_Board_Enemy_3.Cells[i].Y < ((float)Enemy_3.Character.Health / (float)Enemy_3.Character.Health_Max) * cellCount;
        }
    }

    void Collide_Items(GameObject sender, GameObject incCollided) {
        // 1. check to see if the item collided with is the one being dragged...
        // 2. if not, then destroy the item collided with...
        // 3. and add to mana pool
        if (incCollided != Drag_Item.Object && incCollided.layer == LayerMask.NameToLayer(Game.Layer__Interactive))
            Destroy_Item(incCollided);
    }
    void Release_Item(Items incItem) {
        Playing_Board.Cells.Find(obj => obj.Item == incItem).Item = null;
    }
    void Destroy_Item(GameObject incObject) {
        Playing_Board.Cells.ForEach(obj => {
            if (obj.Item != null && obj.Item.Object == incObject)
                Destroy_Item(obj.Item);
        });
    }
    void Destroy_Item(Items incItem) {
        if (State__Minor == Game_States__Minor.Turn_Player__Dragging) {
            // If we're hitting the same color we're dragging, add it to the mana buffer
            if (Drag_Color == incItem.Color) {
                Player_1.Add_Mana(Player_1.Mana_Buffer, incItem.Color, 1);
                Player_1.Add_Mana(Player_1.Mana_Count, incItem.Color, 1);
            }
            else { // Otherwise... subtract it from *that* color's mana buffer... otherwise subtract from *this* color's mana buffer
                if (Player_1.Use_Mana(Player_1.Mana_Count, incItem.Color, 1)) {
                    Player_1.Use_Mana(Player_1.Mana_Buffer, incItem.Color, 1);
                }
                else {
                    Player_1.Use_Mana(Player_1.Mana_Count, Drag_Color, 1);
                    Player_1.Use_Mana(Player_1.Mana_Buffer, Drag_Color, 1);
                    
                }
            }
        }

        GameObject instParticles = (GameObject)GameObject.Instantiate(Prototype__Particles[incItem.Color.GetHashCode()]);
        instParticles.transform.Translate(new Vector3(incItem.Object.transform.position.x, 0, incItem.Object.transform.position.y));
        instParticles.SetActive(true);

        GameObject.Destroy(incItem.Object);
        Cell incCell = Playing_Board.Cells.Find(cell => cell.Item != null && cell.Item == incItem);
        if (incCell != null)
            incCell.Item = null;
    }
    Items Create_Item(Cell incCell, Items.Types incType, GameObject incPrototype, Mana_Colors incColor, Transform incParent, bool attachCollision, bool setActive) {
        Items eachItem;
        eachItem = new Items();
        eachItem.Type = incType;
        eachItem.Object = (GameObject)GameObject.Instantiate(incPrototype);
        eachItem.Object.transform.parent = incParent;
        
        eachItem.Color = incColor;
        if (eachItem.Object.renderer.material != null)
            eachItem.Object.renderer.material.color = Lookup_Colors[eachItem.Color.GetHashCode()];

        if (attachCollision)
            eachItem.Object.GetComponent<__Items>().Item_Collision += new __Items.Item_Collision_Handler(Collide_Items);

        eachItem.Object.transform.Translate(incCell.Location.position);
        
        if (setActive)
            eachItem.Object.SetActive(true);

        return eachItem;
    }


 #endregion

}
