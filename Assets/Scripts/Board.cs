using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

	Game_Handler Game_Handler;
    Transform itemParent;

	/* Definitions and functions for the array of items */

    public class Cell {
        public int X, Y;
        public Transform Location;
        public Board_Item Item;

        public bool isEmpty {
            get { return Item == null; }
        }

        public Vector2 Coords {
            get { return new Vector2(X, Y); }
        }
    }

    List<Cell> Matrix = new List<Cell>();

    public Cell Cell_Above (Vector2 Coords) 
        { return Matrix.Find(obj => obj.X == Coords.x && obj.Y == Coords.y + 1); }
    public Cell Cell_Below(Vector2 Coords) 
        { return Matrix.Find(obj => obj.X == Coords.x && obj.Y == Coords.y - 1); }

	void Start() {
        itemParent = transform.FindChild("__Items");
		Game_Handler = (Game_Handler)GameObject.FindObjectOfType (typeof(Game_Handler));

		Build_Matrix ();
		Populate_Matrix();
	}

    void FixedUpdate() {
        if (Input.GetMouseButtonUp(0))
            Iterate_Matrix();
    }

	void Build_Matrix() {
		// Run through all __Item_Positions...
        List<Transform> itemPositions = new List<Transform>(transform.FindChild("__Item_Positions").GetComponentsInChildren<Transform>());
		itemPositions = itemPositions.FindAll(obj => obj.name.Contains(";"));

		itemPositions.ForEach(obj => {
			// Make sure we're dealing with an actual item position: gameobject naming convention "[X];[Y]"
            Cell newCell = new Cell();
            newCell.Location = obj;
            newCell.X = int.Parse(obj.name.Split(new char[] {';'})[0]);
            newCell.Y = int.Parse(obj.name.Split(new char[] { ';' })[1]);
            Matrix.Add(newCell);
		});

	}

    void Populate_Matrix() {
        Matrix.ForEach(obj => {
            obj.Item = Create_Stone(obj);
        });
    }

    public void Iterate_Matrix(bool fillEmpty = false) {
        bool isComplete = false;
        while (!isComplete) {
            isComplete = true;
            Matrix.ForEach(obj => {
                if (obj.isEmpty) {
                    Cell cellAbove = Cell_Above(obj.Coords);
                    if (cellAbove != null && !cellAbove.isEmpty) {
                        obj.Item = cellAbove.Item;                                                  // Swap the Item among cells
                        cellAbove.Item = null;
                        Game_Handler.Move_Smooth(obj.Item.transform, obj.Location.position, 0.1f);  // Move the actual gameObject
                        isComplete = false;                                                         // Set to reiterate loop
                    }
                }
            });
        }
    }

    Board_Item Create_Stone(Cell incCell) {
        Board_Item.Items eachItem;
        eachItem = Randomize__Stone(itemParent);

        // Place the item on the board, locate it on a row & column, and activate
        eachItem.Object.transform.Translate(incCell.Location.position);
        
        eachItem.State = Board_Item.Items.States.Resting;
        eachItem.Object.SetActive(true);

        // Link the child script back to this handler for easy back-and-forth
        Board_Item eachBoardItem = ((Board_Item)eachItem.Object.GetComponent<Board_Item>());
        eachBoardItem.Item = eachItem;
        eachBoardItem.Board = this;

        return eachBoardItem;
    }

	public void Destroy_Item(Board_Item.Items incItem) {
        Game_Handler.Player.Add_Mana(incItem.Color, 1);

        Debug.Log(String.Format(
            "L{0}: {1} / {2} HP;  Bk {3} / Bl {4} / G {5} / P {6} / R {7} / W {8} / Y {9}",

            Game_Handler.Player.Character.Level,

            Game_Handler.Player.Character.Health_Current,
            Game_Handler.Player.Character.Health_Max,

            Game_Handler.Player.Mana(Definitions.Mana_Colors.Black),
            Game_Handler.Player.Mana(Definitions.Mana_Colors.Blue),
            Game_Handler.Player.Mana(Definitions.Mana_Colors.Green),
            Game_Handler.Player.Mana(Definitions.Mana_Colors.Purple),
            Game_Handler.Player.Mana(Definitions.Mana_Colors.Red),
            Game_Handler.Player.Mana(Definitions.Mana_Colors.White),
            Game_Handler.Player.Mana(Definitions.Mana_Colors.Yellow)
            ));
        
        GameObject.Destroy (incItem.Object);
        Cell incCell = Matrix.Find(cell => cell.Item != null && cell.Item.Item == incItem);
        if (incCell != null)
            incCell.Item = null;
	}



	/* Definitions and functions for individual stones, creation, etc */

    public GameObject proto_Stone_9;


	// Create and return a random stone
	public Board_Item.Items Randomize__Stone(Transform incParent) {
		Board_Item.Items thisStone = new Board_Item.Items ();
		thisStone.Type = Board_Item.Items.Types.Stone;
        thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9);
		
        thisStone.Color = (Definitions.Mana_Colors)UnityEngine.Random.Range (0, Enum.GetNames (typeof(Definitions.Mana_Colors)).Length);

        if(thisStone.Object.renderer.material != null)
            switch (thisStone.Color) {
		        default:
                case Definitions.Mana_Colors.Black: thisStone.Object.renderer.material.color = Color.black; break;
                case Definitions.Mana_Colors.Blue: thisStone.Object.renderer.material.color = Color.blue; break;
                case Definitions.Mana_Colors.Green: thisStone.Object.renderer.material.color = Color.green; break;
                case Definitions.Mana_Colors.Purple: thisStone.Object.renderer.material.color = Color.magenta; break;
                case Definitions.Mana_Colors.Red: thisStone.Object.renderer.material.color = Color.red; break;
                case Definitions.Mana_Colors.White: thisStone.Object.renderer.material.color = Color.white; break;
                case Definitions.Mana_Colors.Yellow: thisStone.Object.renderer.material.color = Color.yellow; break;
		}
		
		thisStone.Object.transform.parent = incParent;
		return thisStone;
	}

}
