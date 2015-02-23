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
        Enemy1, Enemy2, Enemy3;
    public bool hasEnemy1, hasEnemy2, hasEnemy3;

    enum gameStates_Major {
        Initializing,
        Paused,
        Running
    }
    enum gameStates_Minor {
        Null,
        Turn_Player__Idle,
        Turn_Player__Dragging,
        Turn_Player__Attack,
        Turn_Enemy1,
        Turn_Enemy2,
        Turn_Enemy3
    }

    gameStates_Major stateMajor;
    gameStates_Minor stateMinor;

    float gameTime, gameTime_Delta;

    public class waitOp {
        public float Time;
        public delegate void funcTemplate();
        public funcTemplate Function;          // Function to run when the wait time ends

        public event funcTemplate eventComplete;
        public funcTemplate onComplete;         // Can subscribe to onComplete for event signaling...

        public waitOp(funcTemplate incFunction, float incTime) {
            Time = incTime;
            Function = incFunction;
        }
    }

    List<waitOp> waitBuffer = new List<waitOp>();

    Cell dragCell;
    Transform dragJoint;
    float dragDepth,
        dragForce = 1000,
        dragDamping = 1;
    Transform dragTransform;

    /* Things to be turned into prefabs then loaded programmatically... */
    public GameObject uiPaused;

        
#region ROUTINES

    void Awake() {
        _Header = GameObject.Find(__Definitions.Object_gameHeader).GetComponent<__Header>();
        
        GameObject findBuffer;
        
        findBuffer = GameObject.Find(__Definitions.Object_Console);
        if (findBuffer)
            _Console = findBuffer.GetComponent<__Console>();

        findBuffer = GameObject.Find(__Definitions.Object_Tooltip);
        if (findBuffer)
            _Tooltip = findBuffer.GetComponent<__Tooltip>();
    }
    void Start() {

        stateMajor = gameStates_Major.Initializing;

        Player = new __Player("Player", true);
        Enemy1 = new __Player("Enemy 1", false);
        Enemy2 = new __Player("Enemy 2", false);
        Enemy3 = new __Player("Enemy 3", false);


        // Construct the playing board
        playingBoard_Container = GameObject.Find(__Definitions.Object_playingBoard_Container).transform;
        playingBoard_Positions = GameObject.Find(__Definitions.Object_playingBoard_Positions).transform;
        playingBoard_Build();
        playingBoard_Populate();

        stateMajor = gameStates_Major.Running;
        stateMinor = gameStates_Minor.Turn_Player__Idle;


        // HACK HACK HACK SETUP TEST STATES
        // HACK HACK HACK SETUP TEST STATES
        
        
        Player.Ship = GameObject.Find("Ship, Omega Fighter").GetComponent<__Ship>();
        Player.Ship.Player = Player;
        Player.Ship.epPrimaries = new __Definitions.epColors[] 
            { __Definitions.epColors.Blue, 
                __Definitions.epColors.Green, 
                __Definitions.epColors.Yellow };
        Player.Ship.moduleAdd(_shipModules.Turret_Starter);
        Player.Ship.moduleAdd(_shipModules.Shield_Starter);
        Player.Ship.moduleAdd(_shipModules.Armor_Starter);
        Player.Ship.moduleAdd(_shipModules.Hull_Starter);
        
        Enemy1.Ship = GameObject.Find("Ship, Vertex").GetComponent<__Ship>();
        Enemy1.Ship.Player = Enemy1;
        Enemy1.Ship.moduleAdd(_shipModules.Turret_Starter);
        Enemy1.Ship.moduleAdd(_shipModules.Shield_Starter);
        Enemy1.Ship.moduleAdd(_shipModules.Armor_Starter);
        Enemy1.Ship.moduleAdd(_shipModules.Hull_Starter);

        Enemy1.Ship.Target = Player.Ship;
        Player.Ship.Target = Enemy1.Ship;
    }
    void Update() {


        kinemaProcess();            /* Process the kinema buffer */
        waitProcess();              /* Process the wait buffer */

        if (Input.GetKeyDown(KeyCode.Space))
            playingBoard_FadeOut(0.5f);

        /* Developer console functions */
        if (Input.GetKeyDown(KeyCode.BackQuote) && _Console)
            _Console.Toggle();

        /* Main game routine */
        switch (stateMajor) {
            default:
            case gameStates_Major.Initializing:
                gameTime = 0;
                return;

            case gameStates_Major.Paused:
                gameTime_Delta = 0;

                if (Input.GetKeyDown(KeyCode.Escape))
                    stateChange(gameStates_Major.Running);
                break;

            case gameStates_Major.Running: {
                gameTime += Time.deltaTime;
                gameTime_Delta = Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.Escape))
                    stateChange(gameStates_Major.Paused);
                
                /* Throw a raycast for processing... drag a tile? activate a special power? */
                Ray updateRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit updateHit;
                Physics.Raycast(updateRay, out updateHit, 1000f, LayerMask.GetMask(__Definitions.layerInteractive));

                /* Mouseover tooltips */
                /* Disabled for development strictly on touchpads/phones
                if (updateHit.transform && _Tooltip
                    && (updateHit.transform.gameObject.layer == LayerMask.NameToLayer(__Definitions.Layer__Mouseover)
                        || updateHit.transform.gameObject.layer == LayerMask.NameToLayer(__Definitions.Layer__Interactive)))
                    _Tooltip.Process(updateHit.transform);
                else if (_Tooltip)
                    _Tooltip.Process(null);
                 */

                /* Player's turn, drag stones, activate skills */
                if (stateMinor == gameStates_Minor.Turn_Player__Idle) {
                    if (Input.GetMouseButtonDown(0) && updateHit.transform 
                        && updateHit.transform.gameObject.layer == LayerMask.NameToLayer(__Definitions.layerInteractive)) {
                        
                        if (updateHit.transform.name == __Definitions.labelTile) {
                            dragCell = playingBoard.Cells.Find(obj => obj.Transform == updateHit.transform);

                            dragDepth = Camera.main.transform.InverseTransformPoint(updateHit.point).z;
                            dragJoint = dragAttachJoint(updateHit.rigidbody, updateHit.point);

                            kinemaOp kinemaScale = new kinemaOp(
                                kinemaOp.Operations.Scale, updateHit.transform, updateHit.transform.localScale * 0.7f, 0.2f);
                            kinemaAdd(kinemaScale);

                            stateChange(gameStates_Minor.Turn_Player__Dragging);
                        }
                    }
                }

                if (stateMinor == gameStates_Minor.Turn_Player__Dragging) {
                    if (Input.GetMouseButton(0)) {
                        if (dragJoint == null)
                            return;
                        dragJoint.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    }

                    if (Input.GetMouseButtonUp(0)) {
                        if (dragJoint && dragJoint.gameObject)
                            Destroy(dragJoint.gameObject);
                        if (dragCell != null)
                            tileDestroy(dragCell);

                        stateChange(gameStates_Minor.Turn_Player__Attack);
                        dragCell = null;

                        playingBoard_Refresh();
                    } 
                }

                break;
            }
        }   
    }

    void stateChange(gameStates_Major incState) {
        gameStates_Major oldState = stateMajor;
        stateMajor = incState;

        if (incState == gameStates_Major.Paused)
            Pause();
        else if (incState == gameStates_Major.Running && oldState == gameStates_Major.Paused)
            Unpause();
    }
    void stateChange(gameStates_Minor incState) {
        stateMinor = incState;

        switch (incState) {
            default: return;

            case gameStates_Minor.Turn_Player__Idle:
                playingBoard_FadeIn(0.5f);
                return;

            case gameStates_Minor.Turn_Player__Attack:
                if (Player != null) {
                    playingBoard_FadeOut(0.5f);
                    kinemaAdd(new kinemaOp(Player.Ship.transform, Player.Ship.splineFlightPath, 3f));
                    waitAdd(delegate() { Player.Ship.Attack(); }, 1f); 
                    waitAdd(delegate() { stateChange(gameStates_Minor.Turn_Enemy1); }, 2f);
                } else
                    stateChange(gameStates_Minor.Turn_Enemy1);
                return;

            case gameStates_Minor.Turn_Enemy1:
                if (hasEnemy1) {
                    Enemy1.Ship.Attack();
                    waitAdd(delegate() {
                        stateChange(gameStates_Minor.Turn_Enemy2);
                    }, 1f);
                }
                else
                    stateChange(gameStates_Minor.Turn_Enemy2);
                return;

            case gameStates_Minor.Turn_Enemy2:
                if (hasEnemy2)
                    Enemy2.Ship.Attack();
                stateChange(gameStates_Minor.Turn_Enemy3);
                return;

            case gameStates_Minor.Turn_Enemy3:
                if (hasEnemy3)
                    Enemy3.Ship.Attack();
                stateChange(gameStates_Minor.Turn_Player__Idle);
                return;
        }
    }

    void Pause() {
        uiPaused.SetActive(true);
    }
    void Unpause() {
        uiPaused.SetActive(false);
    }

    public void waitAdd(waitOp.funcTemplate incFunction, float incTime) {
        waitOp newOp = new waitOp(incFunction, incTime);
        waitBuffer.Add(newOp);
    }
    void waitProcess() {
        waitBuffer.ForEach(eachWait => {
            eachWait.Time -= gameTime_Delta;
            if (eachWait.Time <= 0) {
                if (eachWait.onComplete != null)
                    eachWait.onComplete();
                eachWait.Function();
                waitBuffer.Remove(eachWait);
            }
        });
    }


    Transform dragAttachJoint(Rigidbody body, Vector3 attachPoint) {
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


#region KINEMATICS

    public class kinemaOp {
        public enum Operations {
            Fade,
            Move,
            Move_Follow,
            Move_Spline,
            Scale,
        }

        public Operations Operation;
        public float timeCurrent, timeTotal;
        public Transform transObject, transTarget;
        public BezierSpline splineTransform;
        public bool splineLookForward;
        public Vector3 vectTarget, vectOriginal;
        public Color colorTarget, colorOriginal;
        /* Can subscribe to the completion event and/or add a function to be run when completed... */
        public delegate void eventComplete_Handler();
        public event eventComplete_Handler eventComplete;
        public eventComplete_Handler onComplete;

        public kinemaOp (Transform incTransform, Color incTarget, float incTime) {
            if (!incTransform.renderer)
                return;
            
            Operation = Operations.Fade;
            transObject = incTransform;
            colorOriginal = incTransform.renderer.material.color;
            colorTarget = incTarget;
            timeTotal = incTime;
            timeCurrent = incTime;
        }
        public kinemaOp(Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime) {
            if (incOperation != Operations.Move && incOperation != Operations.Scale)
                return;

            Operation = incOperation;
            transObject = incTransform;

            if (incOperation == Operations.Move)
                vectOriginal = incTransform.position;
            else if (incOperation == Operations.Scale)
                vectOriginal = incTransform.localScale;
            
            vectTarget = incTarget;
            timeTotal = incTime;
            timeCurrent = incTime;
        }
        public kinemaOp(Transform incTransform, Transform incTarget, float incTime) {
            Operation = Operations.Move_Follow;
            transObject = incTransform;
            vectOriginal = incTransform.position;
            transTarget = incTarget;
            timeTotal = incTime;
            timeCurrent = incTime;
        }
        public kinemaOp(Transform incTransform, BezierSpline incSpline, float incTime, bool incLookForward = true) {
            Operation = Operations.Move_Spline;
            transObject = incTransform;

            splineTransform = incSpline;
            splineLookForward = incLookForward;
            timeTotal = incTime;
            timeCurrent = incTime;
        }
        public void Complete() {
            if (onComplete != null)
                onComplete();
            if (eventComplete != null)
                eventComplete();
        }
    }

    List<kinemaOp> kinemaBuffer = new List<kinemaOp>();

    public void kinemaAdd(kinemaOp incOperation) {
        if (kinemaBuffer.Find(obj => obj.Operation == incOperation.Operation && obj.transObject == incOperation.transObject) == null)
            kinemaBuffer.Add(incOperation);
    }
    void kinemaProcess() {
        kinemaBuffer.ForEach(eachKin => {
            if (eachKin == null || eachKin.transObject == null) {
                kinemaBuffer.Remove(eachKin);
                return;
            }
            
            switch (eachKin.Operation) {
                default:
                case kinemaOp.Operations.Move:
                    eachKin.transObject.position = Vector3.Lerp(eachKin.vectOriginal, eachKin.vectTarget, 1 - (eachKin.timeCurrent / eachKin.timeTotal));
                    break;

                case kinemaOp.Operations.Move_Follow:
                    eachKin.transObject.position = Vector3.Lerp(eachKin.vectOriginal, eachKin.transTarget.position, 1 - (eachKin.timeCurrent / eachKin.timeTotal));
                    break;

                case kinemaOp.Operations.Move_Spline:
                    eachKin.transObject.position = eachKin.splineTransform.GetPoint(1 - (eachKin.timeCurrent / eachKin.timeTotal));
		            if (eachKin.splineLookForward)
                        eachKin.transObject.LookAt(eachKin.transObject.position + eachKin.splineTransform.GetDirection(1 - (eachKin.timeCurrent / eachKin.timeTotal)));
                    break;

                case kinemaOp.Operations.Scale:
                    eachKin.transObject.localScale = Vector3.Lerp(eachKin.vectOriginal, eachKin.vectTarget, 1 - (eachKin.timeCurrent / eachKin.timeTotal));
                    break;

                case kinemaOp.Operations.Fade:
                    // Alpha is float between 0 (clear) and 1 (opaque)
                    if (eachKin.transObject.renderer) {
                        Color bufColor = eachKin.transObject.renderer.material.color;
                        bufColor.a = Mathf.Lerp(eachKin.colorOriginal.a, eachKin.colorTarget.a, 1 - (eachKin.timeCurrent / eachKin.timeTotal));
                        eachKin.transObject.renderer.material.color = bufColor;
                    }
                    break;
            }

            eachKin.timeCurrent -= gameTime_Delta;
            if (eachKin.timeCurrent <= 0) {
                eachKin.Complete();
                kinemaBuffer.Remove(eachKin);
                return;
            }
        });
    }


#endregion


#region PLAYING BOARD


    Matrix playingBoard = new Matrix();
    bool playingBoard_Active = true;
    Transform playingBoard_Container;
    Transform playingBoard_Positions;

    

    public class Cell {
        public int X, Y;
        public Transform Location;

        public __Definitions.epColors Color;
        public GameObject Object;

        public Transform Transform {
            get { return Object == null ? null : Object.transform; }
        }
        public bool Empty {
            get { return Object == null; }
        }
        public bool Active {
            get { return Object == null ? false : Object.activeSelf; }
            set { if (Object) Object.SetActive(value); }
        }
        public Vector2 Coords {
            get { return new Vector2(X, Y); }
        }
    }
    public class Matrix {
        public List<Cell> Cells = new List<Cell>();

        public Cell cellAbove(Vector2 Coords) { return Cells.Find(obj => obj.X == Coords.x && obj.Y == Coords.y + 1); }
        public Cell cellBelow(Vector2 Coords) { return Cells.Find(obj => obj.X == Coords.x && obj.Y == Coords.y - 1); }
        public Cell cellTop(Vector2 Coords) {
            List<Cell> listCol = Cells.FindAll(obj => obj.X == Coords.x);
            return Cells.Find(obj => obj.X == Coords.x && obj.Y == listCol.Max(o => o.Y));
        }
        public List<Cell> cellTops() {
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

    void playingBoard_Build() {
        // Run through all __Item_Positions...
        List<Transform> itemPositions = new List<Transform>(playingBoard_Positions.GetComponentsInChildren<Transform>());
        itemPositions = itemPositions.FindAll(obj => obj.name.Contains(";"));

        for (int i = 0; i < itemPositions.Count; i++) {
            // Make sure we're dealing with an actual item position: gameobject naming convention "[X];[Y]"
            Cell newCell = new Cell();
            newCell.Location = itemPositions[i];
            newCell.X = int.Parse(itemPositions[i].name.Split(new char[] { ';' })[0]);
            newCell.Y = int.Parse(itemPositions[i].name.Split(new char[] { ';' })[1]);
            playingBoard.Cells.Add(newCell);
        }
    }
    void playingBoard_Populate() {
        playingBoard.Cells.ForEach(obj => {
            tileCreate(ref obj, Resources.Load<GameObject>(__Definitions.prefabTile),
                (__Definitions.epColors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(__Definitions.epColors)).Length),
                playingBoard_Container, true, true);
            obj.Object.name = __Definitions.labelTile;
            obj.Object.SetActive(playingBoard_Active);
        });
    }
    void playingBoard_Refresh() {
        playingBoard.Cells.ForEach(obj => {
            if (obj.Empty) {
                tileCreate(ref obj, Resources.Load<GameObject>(__Definitions.prefabTile),
                    (__Definitions.epColors)UnityEngine.Random.Range(0, Enum.GetNames(typeof(__Definitions.epColors)).Length),
                    playingBoard_Container, true, true);
                obj.Object.name = __Definitions.labelTile;
                obj.Object.SetActive(playingBoard_Active);
            }
        });
    }
    void playingBoard_IterateDown() {
        /* bool isComplete = false;
        while (!isComplete) {
            isComplete = true;
            Playing_Board.Cells.ForEach(obj => {
                if (obj.Empty) {
                    Cell cellAbove = Playing_Board.Cell_Above(obj.Coords);
                    if (cellAbove && !cellAbove.Empty) {
                        obj.Tile = cellAbove.Tile;                                                  // Swap the Item among cells
                        cellAbove.Tile = null;
                        Move(obj.Tile.Object.transform, obj.Location.position, 0.1f);               // Move the actual gameObject
                        isComplete = false;                                                         // Set to reiterate loop
                    }
                }
            });
        }*/
    }

    void playingBoard_Show() {
        playingBoard_Active = true;
        playingBoard.Cells.ForEach(obj => {
            obj.Object.SetActive(true);
        });
    }
    void playingBoard_Hide() {
        playingBoard_Active = false;
        playingBoard.Cells.ForEach(obj => {
            obj.Object.SetActive(false);
        });
    }
    void playingBoard_FadeIn(float incSeconds) {
        playingBoard_Active = true;
        playingBoard.Cells.ForEach(obj => {
            if (obj.Object == null || obj.Object.renderer == null)
                return;

            obj.Object.SetActive(true);
            kinemaOp fadeOp = new kinemaOp(obj.Transform,
                new Color(obj.Transform.renderer.material.color.r, obj.Transform.renderer.material.color.g, obj.Transform.renderer.material.color.b, 1),
                incSeconds);
            kinemaAdd(fadeOp);
        });
    }
    void playingBoard_FadeOut(float incSeconds) {
        playingBoard_Active = false;
        playingBoard.Cells.ForEach(obj => {
            if (obj.Object == null || obj.Object.renderer == null)
                return;

            kinemaOp fadeOp = new kinemaOp(obj.Transform,
                new Color(obj.Transform.renderer.material.color.r, obj.Transform.renderer.material.color.g, obj.Transform.renderer.material.color.b, 0),
                incSeconds);
            fadeOp.onComplete = delegate() { obj.Object.SetActive(false); };
            kinemaAdd(fadeOp);
        });
    }
    

    void tileCollide(GameObject sender, GameObject incCollided) {
        if (incCollided != dragCell.Object && incCollided.layer == LayerMask.NameToLayer(__Definitions.layerInteractive))
            playingBoard.Cells.ForEach(obj => {
                if (obj.Object && obj.Object == incCollided)
                    tileDestroy(obj);
            });
    }
    void tileDestroy(Cell incCell) {
        if (stateMinor == gameStates_Minor.Turn_Player__Dragging)
            Player.Ship.Energy_Process(incCell.Color, dragCell.Color);

        // Play a particle system depending on whether it is being dragged or not
        _Tile findTile = incCell.Object.GetComponent<_Tile>();
        if (incCell == dragCell && findTile && findTile.Particle_onDrag) {
            findTile.Particle_onDrag.transform.SetParent(null);
            findTile.Particle_onDrag.Play();
        }
        else if (incCell != dragCell && findTile && findTile.Particle_onDestroy) {
            findTile.Particle_onDestroy.transform.SetParent(null);
            findTile.Particle_onDestroy.particleSystem.startColor = __Definitions.epColors_Lookup[incCell.Color.GetHashCode()];
            findTile.Particle_onDestroy.Play();
        }

        GameObject.Destroy(incCell.Object);
        incCell.Object = null;
    }
    void tileCreate(ref Cell incCell, GameObject incPrototype, __Definitions.epColors incColor, Transform incParent, bool attachCollision, bool setActive) {
        incCell.Object = (GameObject)GameObject.Instantiate(incPrototype);
        incCell.Object.transform.parent = incParent;

        incCell.Color = incColor;
        if (incCell.Object.renderer.material)
            incCell.Object.renderer.material.color = __Definitions.epColors_Lookup[incColor.GetHashCode()];

        if (attachCollision)
            incCell.Object.GetComponent<_Tile>().Tile_Collision += new _Tile.Tile_Collision_Handler(tileCollide);

        incCell.Object.transform.Translate(incCell.Location.position);

        if (setActive)
            incCell.Object.SetActive(true);
    }

#endregion

}
