using UnityEngine;
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
        Player_1 = new Player();

        // Construct the playing board
        Playing_Board__Container = GameObject.Find("Playing_Board__Item_Container").transform;
        Playing_Board__Positions = GameObject.Find("Playing_Board__Item_Positions").transform;
        
        // Populate the playing board
        Build__Matrix(Playing_Board, Playing_Board__Positions);
        Populate__Playing_Board();

        // Construct the player's mana board
        Mana_Board_Player__Container = GameObject.Find("Mana_Board_Player__Item_Container").transform;
        Mana_Board_Player__Positions = GameObject.Find("Mana_Board_Player__Item_Positions").transform;
        Build__Matrix(Mana_Board_Player, Mana_Board_Player__Positions);

        if (has_Enemy_1) { // Construct the 1st enemy's mana board
            Mana_Board_Enemy_1__Container = GameObject.Find("Mana_Board_Enemy_1__Item_Container").transform;
            Mana_Board_Enemy_1__Positions = GameObject.Find("Mana_Board_Enemy_1__Item_Positions").transform;
            Build__Matrix(Mana_Board_Enemy_1, Mana_Board_Enemy_1__Positions);    
        }

        if (has_Enemy_2) { // Construct the 2nd enemy's mana board
            Mana_Board_Enemy_2__Container = GameObject.Find("Mana_Board_Enemy_2__Item_Container").transform;
            Mana_Board_Enemy_2__Positions = GameObject.Find("Mana_Board_Enemy_2__Item_Positions").transform;
            Build__Matrix(Mana_Board_Enemy_2, Mana_Board_Enemy_2__Positions);
        }

        if (has_Enemy_3) { // Construct the 3rd enemy's mana board
            Mana_Board_Enemy_3__Container = GameObject.Find("Mana_Board_Enemy_3__Item_Container").transform;
            Mana_Board_Enemy_3__Positions = GameObject.Find("Mana_Board_Enemy_3__Item_Positions").transform;
            Build__Matrix(Mana_Board_Enemy_3, Mana_Board_Enemy_3__Positions);
        }
    }


#region ROUTINE & INPUT

    bool Dragging = false;
    Items Drag_Item;
    Transform Drag_Joint;
    float Drag_Depth,
        Drag_Force = 1000,
        Drag_Damping = 1;


    void FixedUpdate() {
        Transformation_Buffer.ForEach(obj => { if (obj.Process()) Transformation_Buffer.Remove(obj); });
    }
    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer(Layer__Interactive)) {

                    Drag_Item = Playing_Board.Cells.Find(obj => obj.Item.Transform == hit.transform).Item;
                    Release_Item(Drag_Item);

                    Drag_Depth = Camera.main.transform.InverseTransformPoint(hit.point).z;
                    Drag_Joint = Drag_AttachJoint(hit.rigidbody, hit.point);
                    Scale(hit.transform, hit.transform.localScale * 0.7f, 0.1f);
                    Dragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            if (Drag_Joint != null && Drag_Joint.gameObject != null)
                Destroy(Drag_Joint.gameObject);
            if (Drag_Item != null)
                Destroy_Item(Drag_Item);

            Dragging = false;
            Drag_Item = null;

            Refresh__Playing_Board();
        }

        if (Input.GetMouseButton(0)) {
            if (Drag_Joint == null)
                return;
            Drag_Joint.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
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

    public static int Mana_Color = 5;

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

#endregion


#region PLAYER

    public class Player {
        Character _Character = new List_Characters().Chirba;
        int[] _Mana = new int[Enum.GetValues(typeof(Game.Mana_Colors)).Length];

        public Character Character {
            get { return _Character; }
        }

        public int Mana(Game.Mana_Colors Color) { return _Mana[(int)Color]; }
        public int[] Mana() { return _Mana; }
        public void Add_Mana(Game.Mana_Colors Color, int Amount) { _Mana[(int)Color] += Amount; }

        public bool Use_Mana(Game.Mana_Colors Color, int Amount) {
            if (_Mana[(int)Color] < Amount)
                return false;

            _Mana[(int)Color] -= Amount;
            return true;
        }
    }


#endregion


#region CHARACTER

    public class Character {
        string Name;
        int Level_Current, Level_Max;
        int HP_Base, HP_Increments;

        int HP_Current, HP_Max;
        List<Skill> Skills = new List<Skill>();


        public Character(string incName, int incLevel_Max, int incHP_Base, int incHP_Increments, List<Skill> incSkills) {
            Name = incName;
            Level_Current = 1;
            Level_Max = incLevel_Max;

            HP_Base = incHP_Base;
            HP_Increments = incHP_Increments;

            HP_Max = Calculate__Health_Max();
            HP_Current = HP_Max;

            Skills = incSkills;
        }

        public int Level {
            get { return Level_Current; }
        }
        public void Add_Level(int levelUp) {
            levelUp = Level_Current + levelUp > Level_Max
                ? Level_Max - Level_Current : levelUp;

            Level_Current += levelUp;
            HP_Max += Calculate__Health_Increase(levelUp);
            HP_Current = HP_Max;
        }

        public int Health_Max {
            get { return HP_Max; }
            set { HP_Max = value; }
        }
        public int Health_Current {
            get { return HP_Current; }
            set { HP_Current = value; }
        }
        public void Adjust__Health_Current(int incValue) { HP_Current += incValue; }
        public void Adjust__Health_Max(int incValue) { HP_Max += incValue; }
        public int Calculate__Health_Max() { return HP_Base + (int)((Level_Current - 1) * HP_Increments); }
        public int Calculate__Health_Increase(int levelAdjust) { return (int)(levelAdjust * HP_Increments); }
    }

    public class List_Characters {
        public Character Chirba = new Character("Chirba", 100, 50, 55,
            new List<Skill>() {
            List_Skills.Fireball,
            List_Skills.Ice_Lance,
            List_Skills.Lightning_Bolt
        });
    }


#endregion


#region SKILLS

    public class Skill {
        public enum Types {
            Magic,
            Physical
        }

        public class Tier {
            int Level;
            int Damage;
            int[] Mana;     // Amount of mana required in each color (ordered by Definitions.Mana_Colors enum)

            public Tier(int incLevel, int incDamage, int[] incMana) {
                Level = incLevel;
                Damage = incDamage;
                Mana = incMana;
            }
        }

        string Name;
        Types Type;
        List<Tier> Tiers = new List<Tier>();


        public Skill(string incName, Types incType, List<Tier> incTiers) {
            Name = incName;
            Type = incType;
            Tiers = incTiers;
        }
    }

    public static class List_Skills {
        public static Skill Fireball = new Skill("Fireball", Skill.Types.Magic, new List<Skill.Tier>() {
            new Skill.Tier(1, 5,   new int[] {0, 0, 0, 0, 5, 0, 0}),
            new Skill.Tier(2, 10,  new int[] {0, 0, 0, 0, 6, 0, 0}),
            new Skill.Tier(3, 15,  new int[] {2, 0, 0, 0, 6, 0, 0})
        });

        public static Skill Ice_Lance = new Skill("Ice Lance", Skill.Types.Magic, new List<Skill.Tier>() {
            new Skill.Tier(1, 8,   new int[] {0, 5, 0, 0, 0, 0, 0}),
            new Skill.Tier(2, 12,  new int[] {0, 8, 0, 0, 0, 0, 0}),
            new Skill.Tier(3, 18,  new int[] {0, 8, 0, 0, 0, 2, 0}),
            new Skill.Tier(4, 24,  new int[] {0, 10, 0, 0, 0, 4, 0})
        });

        public static Skill Lightning_Bolt = new Skill("Lightning Bolt", Skill.Types.Magic, new List<Skill.Tier>() {
            new Skill.Tier(1, 10,  new int[] {0, 6, 0, 0, 0, 0, 0}),
            new Skill.Tier(2, 18,  new int[] {0, 9, 0, 0, 0, 0, 0}),
            new Skill.Tier(3, 24,  new int[] {2, 10, 0, 0, 0, 0, 0}),
            new Skill.Tier(4, 32,  new int[] {3, 12, 0, 0, 0, 0, 0})
        });
    }


#endregion


#region PLAYING BOARD

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
    public GameObject[] Prototype__Particles = new GameObject[Mana_Color];

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
            obj.Item = Create_Stone(obj);
        });
    }
    void Refresh__Playing_Board() {
        Playing_Board.Cells.ForEach(obj => {
            if (obj.isEmpty)
                obj.Item = Create_Stone(obj);
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
        Player_1.Add_Mana(incItem.Color, 1);

        GameObject instParticles = (GameObject)GameObject.Instantiate(Prototype__Particles[incItem.Color.GetHashCode()]);
        instParticles.transform.Translate(new Vector3(incItem.Object.transform.position.x, 0, incItem.Object.transform.position.y));
        instParticles.SetActive(true);

        GameObject.Destroy(incItem.Object);
        Cell incCell = Playing_Board.Cells.Find(cell => cell.Item != null && cell.Item == incItem);
        if (incCell != null)
            incCell.Item = null;
    }

    Items Create_Stone(Cell incCell) {
        Items eachItem;
        eachItem = Randomize__Stone(Playing_Board__Container);

        eachItem.Object.GetComponent<__Items>().Item_Collision += new __Items.Item_Collision_Handler(Collide_Items);
        eachItem.Object.transform.Translate(incCell.Location.position);
        eachItem.Object.SetActive(true);

        return eachItem;
    }
    public Items Randomize__Stone(Transform incParent) {
        Items thisStone = new Items();
        thisStone.Type = Items.Types.Stone;
        thisStone.Object = (GameObject)GameObject.Instantiate(Prototype__Stone9);
        thisStone.Color = (Game.Mana_Colors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Mana_Colors)).Length);

        if (thisStone.Object.renderer.material != null)
            thisStone.Object.renderer.material.color = Lookup_Colors[thisStone.Color.GetHashCode()];

        thisStone.Object.transform.parent = incParent;
        return thisStone;
    }


 #endregion

}
