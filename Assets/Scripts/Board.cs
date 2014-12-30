using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

	void Start() {
        itemParent = transform.FindChild("__Items");
		Game_Handler = (Game_Handler)GameObject.FindObjectOfType (typeof(Game_Handler));

		Build_Matrix ();
		Populate_Matrix();
	}

    void Update() {
        if (Input.GetMouseButtonUp(0)) {
            Refresh_Matrix();
        }
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

    public void Refresh_Matrix() {
        Matrix.ForEach(obj => {
            if (obj.isEmpty)
                obj.Item = Create_Stone(obj);
        });
    }

    public void Iterate_Matrix__Down() {
        bool isComplete = false;
        while (!isComplete) {
            isComplete = true;
            Matrix.ForEach(obj => {
                if (obj.isEmpty) {
                    Cell cellAbove = Cell_Above(obj.Coords);
                    if (cellAbove != null && !cellAbove.isEmpty) {
                        obj.Item = cellAbove.Item;                                                  // Swap the Item among cells
                        cellAbove.Item = null;
                        Game_Handler.Move(obj.Item.transform, obj.Location.position, 0.1f);         // Move the actual gameObject
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

    public void Release_Item(Board_Item incItem) {
        Matrix.Find(obj => obj.Item == incItem).Item = null;
    }

	public void Destroy_Item(Board_Item.Items incItem) {
        Game_Handler.Player.Add_Mana(incItem.Color, 1);

        GameObject instParticles = (GameObject)GameObject.Instantiate(proto_Particles);
        instParticles.transform.Translate(new Vector3(incItem.Object.transform.position.x, 0, incItem.Object.transform.position.y));
        instParticles.renderer.particleSystem.startColor = Definitions.Lookup_Colors[incItem.Color.GetHashCode()];
        instParticles.SetActive(true);

        GameObject.Destroy (incItem.Object);
        Cell incCell = Matrix.Find(cell => cell.Item != null && cell.Item.Item == incItem);
        if (incCell != null)
            incCell.Item = null;
	}



	/* Definitions and functions for individual stones, creation, etc */

    public GameObject proto_Stone_9;
    public GameObject proto_Particles;


	// Create and return a random stone
	public Board_Item.Items Randomize__Stone(Transform incParent) {
		Board_Item.Items thisStone = new Board_Item.Items ();
		thisStone.Type = Board_Item.Items.Types.Stone;
        thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9);
		
        thisStone.Color = (Definitions.Mana_Colors)UnityEngine.Random.Range (0, Enum.GetNames (typeof(Definitions.Mana_Colors)).Length);

        if(thisStone.Object.renderer.material != null)
            thisStone.Object.renderer.material.color = Definitions.Lookup_Colors[thisStone.Color.GetHashCode()];
		
		thisStone.Object.transform.parent = incParent;
		return thisStone;
	}

}
