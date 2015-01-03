using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

    public Player Player;
    public Text Text_Level, Text_HP, Text_Mana;


    void Start() {
        Player = new Player();

        // Construct the playing board
        Item_Container = GameObject.Find("__Item_Container").transform;
        Build_Matrix();
        Populate_Matrix();

    }


#region ROUTINE & INPUT

    bool Dragging = false;
    Items Drag_Item;
    Transform Drag_Joint;
    float Drag_Depth,
        Drag_Force = 1000,
        Drag_Damping = 0;


    void FixedUpdate() {
        Text_Mana.text = String.Format("Bl {0} \nGr {1} \nRe {2} \nWh {3} \nYe {4}",
            Player.Mana(Mana_Colors.Blue),
            Player.Mana(Mana_Colors.Green),
            Player.Mana(Mana_Colors.Red),
            Player.Mana(Mana_Colors.White),
            Player.Mana(Mana_Colors.Yellow));

        Transformation_Buffer.ForEach(obj => { if (obj.Process()) Transformation_Buffer.Remove(obj); });
    }
    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer(Layer__Board_Item)) {

                    Drag_Item = Matrix.Find(obj => obj.Item.Transform == hit.transform).Item;
                    Release_Item(Drag_Item);

                    Drag_Depth = CameraPlane.CameraToPointDepth(Camera.main, hit.point);
                    Drag_Joint = Drag_AttachJoint(hit.rigidbody, hit.point);
                    Scale(hit.transform, hit.transform.localScale * 0.7f, 0.1f);
                    Dragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            if (Drag_Joint != null && Drag_Joint.gameObject != null)
                Destroy(Drag_Joint.gameObject);
            Destroy_Item(Drag_Item);

            Dragging = false;
            Drag_Item = null;

            Refresh_Matrix();
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

        ConfigurableJoint joint = obj.AddComponent<ConfigurableJoint>();
        joint.connectedBody = body;
        joint.configuredInWorldSpace = true;
        joint.xDrive = Drag_NewJointDrive(Drag_Force, Drag_Damping);
        joint.yDrive = Drag_NewJointDrive(Drag_Force, Drag_Damping);
        joint.zDrive = Drag_NewJointDrive(Drag_Force, Drag_Damping);
        joint.slerpDrive = Drag_NewJointDrive(Drag_Force, Drag_Damping);
        joint.rotationDriveMode = RotationDriveMode.Slerp;

        return obj.transform;
    }
    private JointDrive Drag_NewJointDrive(float force, float damping) {
        JointDrive drive = new JointDrive();
        drive.mode = JointDriveMode.Position;
        drive.positionSpring = force;
        drive.positionDamper = damping;
        drive.maximumForce = Mathf.Infinity;
        return drive;
    }

#endregion


#region 3D TRANSFORMATIONS

    class Transformation {
        public enum Operations {
            Move,
            Scale
        }

        Operations Operation;
        float Time;
        Transform Transform;
        Vector3 Target,
            Velocity = Vector3.zero;

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

    public static string Layer__Board_Item = "Board Item";

    public static int Mana_Color = 5;

    public enum Mana_Colors {
        Blue,
        Green,
        Red,
        White,
        Yellow
    }
    
    public static Color[] Lookup_Colors = new Color[] {
        Color.blue,
        Color.green,
        Color.red,
        Color.white,
        Color.yellow
    };

#endregion


#region PLAYING BOARD

    public GameObject proto_Stone_9;
    public GameObject[] proto_Particles = new GameObject[Mana_Color];

    Transform Item_Container,
        Item_Dragging;

    #region MATRIX: BOARD ITEMS

        public class Items {
            public enum Types {
                Stone,
                Consumable,
                Equipment
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

        List<Cell> Matrix = new List<Cell>();

        public Cell Cell_Above(Vector2 Coords) { return Matrix.Find(obj => obj.X == Coords.x && obj.Y == Coords.y + 1); }
        public Cell Cell_Below(Vector2 Coords) { return Matrix.Find(obj => obj.X == Coords.x && obj.Y == Coords.y - 1); }
        public Cell Cell_Top(Vector2 Coords) {
            List<Cell> listCol = Matrix.FindAll(obj => obj.X == Coords.x);
            return Matrix.Find(obj => obj.X == Coords.x && obj.Y == listCol.Max(o => o.Y));
        }
        public List<Cell> Cell_Tops() {
            List<Cell> listBuffer = new List<Cell>();
            Matrix.ForEach(obj => {
                if (listBuffer.FindAll(pred => pred.X == obj.X).Count == 0)
                    listBuffer.Add(obj);
                else
                    for (int i = 0; i < listBuffer.Count; i++)
                        if (listBuffer[i].X == obj.X && listBuffer[i].Y < obj.Y)
                            listBuffer[i] = obj;
            });
            return listBuffer;
        }

    #endregion


    void Build_Matrix() {
        // Run through all __Item_Positions...
        List<Transform> itemPositions = new List<Transform>(GameObject.Find("__Item_Positions").GetComponentsInChildren<Transform>());
        itemPositions = itemPositions.FindAll(obj => obj.name.Contains(";"));

        itemPositions.ForEach(obj => {
            // Make sure we're dealing with an actual item position: gameobject naming convention "[X];[Y]"
            Cell newCell = new Cell();
            newCell.Location = obj;
            newCell.X = int.Parse(obj.name.Split(new char[] { ';' })[0]);
            newCell.Y = int.Parse(obj.name.Split(new char[] { ';' })[1]);
            Matrix.Add(newCell);
        });

    }
    void Populate_Matrix() {
        Matrix.ForEach(obj => {
            obj.Item = Create_Stone(obj);
        });
    }
    void Refresh_Matrix() {
        Matrix.ForEach(obj => {
            if (obj.isEmpty)
                obj.Item = Create_Stone(obj);
        });
    }
    void Iterate_Matrix__Down() {
        bool isComplete = false;
        while (!isComplete) {
            isComplete = true;
            Matrix.ForEach(obj => {
                if (obj.isEmpty) {
                    Cell cellAbove = Cell_Above(obj.Coords);
                    if (cellAbove != null && !cellAbove.isEmpty) {
                        obj.Item = cellAbove.Item;                                                  // Swap the Item among cells
                        cellAbove.Item = null;
                        Move(obj.Item.Object.transform, obj.Location.position, 0.1f);                      // Move the actual gameObject
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
        if (incCollided != Drag_Item.Object && incCollided.layer == LayerMask.NameToLayer(Game.Layer__Board_Item))
            Destroy_Item(incCollided);
    }
    void Release_Item(Items incItem) {
        Matrix.Find(obj => obj.Item == incItem).Item = null;
    }
    void Destroy_Item(GameObject incObject) {
        Matrix.ForEach(obj => {
            if (obj.Item != null && obj.Item.Object == incObject)
                Destroy_Item(obj.Item);
        });
    }
    void Destroy_Item(Items incItem) {
        Player.Add_Mana(incItem.Color, 1);

        GameObject instParticles = (GameObject)GameObject.Instantiate(proto_Particles[incItem.Color.GetHashCode()]);
        instParticles.transform.Translate(new Vector3(incItem.Object.transform.position.x, 0, incItem.Object.transform.position.y));
        instParticles.SetActive(true);

        GameObject.Destroy(incItem.Object);
        Cell incCell = Matrix.Find(cell => cell.Item != null && cell.Item == incItem);
        if (incCell != null)
            incCell.Item = null;
    }

    Items Create_Stone(Cell incCell) {
        Items eachItem;
        eachItem = Randomize__Stone(Item_Container);

        eachItem.Object.GetComponent<__Items>().Item_Collision += new __Items.Item_Collision_Handler(Collide_Items);
        eachItem.Object.transform.Translate(incCell.Location.position);
        eachItem.Object.SetActive(true);

        return eachItem;
    }
    public Items Randomize__Stone(Transform incParent) {
        Items thisStone = new Items();
        thisStone.Type = Items.Types.Stone;
        thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9);
        thisStone.Color = (Game.Mana_Colors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Mana_Colors)).Length);

        if (thisStone.Object.renderer.material != null)
            thisStone.Object.renderer.material.color = Lookup_Colors[thisStone.Color.GetHashCode()];

        thisStone.Object.transform.parent = incParent;
        return thisStone;
    }


 #endregion
}
