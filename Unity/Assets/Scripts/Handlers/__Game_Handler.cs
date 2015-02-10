using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class __Game_Handler : MonoBehaviour {

    __Tooltip _Tooltip;
    __Console _Console;
    __Header _Header;

    public __Player Player,
        Enemy_1, Enemy_2, Enemy_3;
    public bool has_Enemy_1, has_Enemy_2, has_Enemy_3;

    Game_States__Major State__Major;
    Game_States__Minor State__Minor;
    
    Cell Drag_Cell;
    Transform Drag_Joint;
    float Drag_Depth,
        Drag_Force = 1000,
        Drag_Damping = 1;
    Transform Drag_Transform;

    Matrix Playing_Board = new Matrix();
    Transform Playing_Board__Container;
    Transform Playing_Board__Positions;

    /* Things to be turned into prefabs then loaded programmatically... */
    public GameObject UI_Paused;


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


#region ROUTINES

    void Awake() {
        _Header = GameObject.Find(__Definitions.Object__Game_Header).GetComponent<__Header>();
        
        GameObject findBuffer;
        
        findBuffer = GameObject.Find(__Definitions.Object__Console);
        if (findBuffer != null)
            _Console = findBuffer.GetComponent<__Console>();

        findBuffer = GameObject.Find(__Definitions.Object__Tooltip);
        if (findBuffer != null)
            _Tooltip = findBuffer.GetComponent<__Tooltip>();
    }
    void Start() {

        State__Major = Game_States__Major.Initializing;

        Player = new __Player("Player", true);
        Enemy_1 = new __Player("Enemy 1", false);
        Enemy_2 = new __Player("Enemy 2", false);
        Enemy_3 = new __Player("Enemy 3", false);


        // Construct the playing board
        Playing_Board__Container = GameObject.Find(__Definitions.Object__Playing_Board__Container).transform;
        Playing_Board__Positions = GameObject.Find(__Definitions.Object__Playing_Board__Positions).transform;
        Build__Playing_Board();
        Populate__Playing_Board();

        State__Major = Game_States__Major.Running;
        State__Minor = Game_States__Minor.Turn_Player__Idle;


        // HACK HACK HACK SETUP TEST STATES
        // HACK HACK HACK SETUP TEST STATES
        
        Player.Ship = GameObject.Find("Ship, Omega Fighter").GetComponent<__Ship>();
        Player.Ship.Module_Add(__Ship__Modules.Turret_Starter);
        Player.Ship.Module_Add(__Ship__Modules.Turret_Starter);
        Player.Ship.Module_Add(__Ship__Modules.Shield_Starter);
        Player.Ship.Module_Add(__Ship__Modules.Armor_Starter);
        Player.Ship.Module_Add(__Ship__Modules.Hull_Starter);
        
        Enemy_1.Ship = GameObject.Find("Ship, Vertex").GetComponent<__Ship>();
        Enemy_1.Ship.Module_Add(__Ship__Modules.Turret_Starter);
        Enemy_1.Ship.Module_Add(__Ship__Modules.Shield_Starter);
        Enemy_1.Ship.Module_Add(__Ship__Modules.Armor_Starter);
        Enemy_1.Ship.Module_Add(__Ship__Modules.Hull_Starter);

        Enemy_1.Ship.Target = Player.Ship;
        Player.Ship.Target = Enemy_1.Ship;
    }
    void FixedUpdate() {
        Transformation_Buffer.ForEach(obj => { if (obj.Process()) Transformation_Buffer.Remove(obj); });
    }
    void Update() {

        /* Developer console functions */
        if (Input.GetKeyDown(KeyCode.BackQuote) && _Console != null)
            _Console.Toggle();
        
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
                
                /* Throw a raycast for processing... mouseover tooltip? drag a stone? activate a special power? */
                Ray updateRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit updateHit;
                Physics.Raycast(updateRay, out updateHit);

                /* Mouseover tooltips */
                if (updateHit.transform != null && _Tooltip != null
                    && (updateHit.transform.gameObject.layer == LayerMask.NameToLayer(__Definitions.Layer__Mouseover)
                        || updateHit.transform.gameObject.layer == LayerMask.NameToLayer(__Definitions.Layer__Interactive)))
                    _Tooltip.Process(updateHit.transform);
                else if (_Tooltip != null)
                    _Tooltip.Process(null);

                /* Player's turn, drag stones, activate skills */
                if (State__Minor == Game_States__Minor.Turn_Player__Idle) {
                    if (Input.GetMouseButtonDown(0) && updateHit.transform != null 
                        && updateHit.transform.gameObject.layer == LayerMask.NameToLayer(__Definitions.Layer__Interactive)) {
                        
                        if (updateHit.transform.name == __Definitions.Label_Tile) {
                            Drag_Cell = Playing_Board.Cells.Find(obj => obj.Transform == updateHit.transform);

                            Drag_Depth = Camera.main.transform.InverseTransformPoint(updateHit.point).z;
                            Drag_Joint = Drag_AttachJoint(updateHit.rigidbody, updateHit.point);
                            Scale(updateHit.transform, updateHit.transform.localScale * 0.7f, 0.1f);
                            Change_State(Game_States__Minor.Turn_Player__Dragging);
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
                        if (Drag_Cell != null)
                            Destroy_Tile(Drag_Cell);

                        Change_State(Game_States__Minor.Turn_Player__Attack);
                        Drag_Cell = null;

                        Refresh__Playing_Board();
                    } 
                }

                break;
            }
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
    void Change_State(Game_States__Minor incState) {
        State__Minor = incState;

        switch (incState) {
            default: return;

            case Game_States__Minor.Turn_Player__Attack:
                if (Player != null)
                    Player.Ship.Attack();
                Change_State(Game_States__Minor.Turn_Enemy1);
                return;

            case Game_States__Minor.Turn_Enemy1:
                if (has_Enemy_1)
                    Enemy_1.Ship.Attack();
                Change_State(Game_States__Minor.Turn_Enemy2);
                return;

            case Game_States__Minor.Turn_Enemy2:
                if (has_Enemy_2)
                    Enemy_2.Ship.Attack();
                Change_State(Game_States__Minor.Turn_Enemy3);
                return;

            case Game_States__Minor.Turn_Enemy3:
                if (has_Enemy_3)
                    Enemy_3.Ship.Attack();
                Change_State(Game_States__Minor.Turn_Player__Idle);
                return;
        }
    }

    void Pause() {
        UI_Paused.SetActive(true);
    }
    void Unpause() {
        UI_Paused.SetActive(false);
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
        bool thenDestroy = false;

        public Transformation(Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime, bool incDestroy = false) {
            Operation = incOperation;
            Transform = incTransform;
            Target = incTarget;
            Time = incTime;
            thenDestroy = incDestroy;
        }

        public void Modify(Vector3 incTarget, float incTime, bool incDestroy = false) {
            thenDestroy = incDestroy;

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
            
            if (thenDestroy && Transform.position == Target)
                Destroy(Transform.gameObject);

            return Transform.position == Target;
        }
        bool Scale() {
            Transform.localScale = Vector3.SmoothDamp(Transform.localScale, Target, ref Velocity, Time);

            if (thenDestroy && Transform.position == Target)
                Destroy(Transform.gameObject);
            
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

    public void Move(Transform incTransform, Vector3 incTarget, float incTime, bool incDestroy = false) { 
        Operate(Transformation.Operations.Move, incTransform, incTarget, incTime, incDestroy); 
    }
    public void Scale(Transform incTransform, Vector3 incTarget, float incTime, bool incDestroy = false) {
        Operate(Transformation.Operations.Scale, incTransform, incTarget, incTime, incDestroy); 
    }
    void Operate(Transformation.Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime, bool incDestroy = false) {
        if (Transformation_Buffer.Find(obj => obj._Operation == incOperation && obj._Transform == incTransform) == null)
            Transformation_Buffer.Add(new Transformation(incOperation, incTransform, incTarget, incTime, incDestroy));
        else
            Transformation_Buffer.Find(obj => obj._Operation == incOperation && obj._Transform == incTransform).Modify(incTarget, incTime, incDestroy);
    }

#endregion


#region PLAYING BOARD

    public class Cell {
        public int X, Y;
        public Transform Location;

        public __Definitions.EP_Colors Color;
        public GameObject Object;

        public Transform Transform {
            get { return Object == null ? null : Object.transform; }
        }
        public bool Empty {
            get { return Object == null; }
        }
        public bool Active {
            get { return Object == null ? false : Object.activeSelf; }
            set { if (Object != null) Object.SetActive(value); }
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

    void Build__Playing_Board() {
        // Run through all __Item_Positions...
        List<Transform> itemPositions = new List<Transform>(Playing_Board__Positions.GetComponentsInChildren<Transform>());
        itemPositions = itemPositions.FindAll(obj => obj.name.Contains(";"));

        for (int i = 0; i < itemPositions.Count; i++) {
            // Make sure we're dealing with an actual item position: gameobject naming convention "[X];[Y]"
            Cell newCell = new Cell();
            newCell.Location = itemPositions[i];
            newCell.X = int.Parse(itemPositions[i].name.Split(new char[] { ';' })[0]);
            newCell.Y = int.Parse(itemPositions[i].name.Split(new char[] { ';' })[1]);
            Playing_Board.Cells.Add(newCell);
        }
    }
    void Populate__Playing_Board() {
        Playing_Board.Cells.ForEach(obj => {
            Create_Tile(ref obj, Resources.Load<GameObject>(__Definitions.Prefab__Tile),
                (__Definitions.EP_Colors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(__Definitions.EP_Colors)).Length),
                Playing_Board__Container, true, true);
            obj.Object.name = __Definitions.Label_Tile;
        });
    }
    void Refresh__Playing_Board() {
        Playing_Board.Cells.ForEach(obj => {
            if (obj.Empty) {
                Create_Tile(ref obj, Resources.Load<GameObject>(__Definitions.Prefab__Tile),
                    (__Definitions.EP_Colors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(__Definitions.EP_Colors)).Length),
                    Playing_Board__Container, true, true);
                obj.Object.name = __Definitions.Label_Tile;
            }
        });
    }
    void Iterate_Down__Playing_Board() {
        /* bool isComplete = false;
        while (!isComplete) {
            isComplete = true;
            Playing_Board.Cells.ForEach(obj => {
                if (obj.Empty) {
                    Cell cellAbove = Playing_Board.Cell_Above(obj.Coords);
                    if (cellAbove != null && !cellAbove.Empty) {
                        obj.Tile = cellAbove.Tile;                                                  // Swap the Item among cells
                        cellAbove.Tile = null;
                        Move(obj.Tile.Object.transform, obj.Location.position, 0.1f);               // Move the actual gameObject
                        isComplete = false;                                                         // Set to reiterate loop
                    }
                }
            });
        }*/
    }
    
    void Collide_Tile(GameObject sender, GameObject incCollided) {
        if (incCollided != Drag_Cell.Object && incCollided.layer == LayerMask.NameToLayer(__Definitions.Layer__Interactive))
            Playing_Board.Cells.ForEach(obj => {
                if (obj.Object != null && obj.Object == incCollided)
                    Destroy_Tile(obj);
            });
    }
    void Destroy_Tile(Cell incCell) {
        if (State__Minor == Game_States__Minor.Turn_Player__Dragging)
            Player.Ship.Energy_Add(incCell.Color);

        // Play a particle system depending on whether it is being dragged or not
        _Tile findTile = incCell.Object.GetComponent<_Tile>();
        if (incCell == Drag_Cell && findTile != null && findTile.Particle__On_Drag != null) {
            findTile.Particle__On_Drag.transform.SetParent(null);
            findTile.Particle__On_Drag.Play();
        }
        else if (incCell != Drag_Cell && findTile != null && findTile.Particle__On_Destroy != null) {
            findTile.Particle__On_Destroy.transform.SetParent(null);
            findTile.Particle__On_Destroy.particleSystem.startColor = __Definitions.EP_Colors__Lookup[incCell.Color.GetHashCode()];
            findTile.Particle__On_Destroy.Play();
        }

        GameObject.Destroy(incCell.Object);
        incCell.Object = null;
    }
    void Create_Tile(ref Cell incCell, GameObject incPrototype, __Definitions.EP_Colors incColor, Transform incParent, bool attachCollision, bool setActive) {
        incCell.Object = (GameObject)GameObject.Instantiate(incPrototype);
        incCell.Object.transform.parent = incParent;

        incCell.Color = incColor;
        if (incCell.Object.renderer.material != null)
            incCell.Object.renderer.material.color = __Definitions.EP_Colors__Lookup[incColor.GetHashCode()];

        if (attachCollision)
            incCell.Object.GetComponent<_Tile>().Tile_Collision += new _Tile.Tile_Collision_Handler(Collide_Tile);

        incCell.Object.transform.Translate(incCell.Location.position);

        if (setActive)
            incCell.Object.SetActive(true);
    }

#endregion

}
