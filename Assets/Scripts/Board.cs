using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

	Game_Handler Game_Handler;

	/* Definitions and functions for the array of items */

	int amountColumns = 0;
	List<int> amountRows = new List<int> (); // index = column #; some columns may have different amount of rows than others
	List<Transform> itemPositions = new List<Transform> ();

	List<Board_Item.Items> listItems = new List<Board_Item.Items>();

	void Start() {
		Game_Handler = (Game_Handler)GameObject.FindObjectOfType (typeof(Game_Handler));

		Count_Rows_and_Columns ();
		Populate_Board ();
	}

    void FixedUpdate() {
        if (Input.GetMouseButtonUp(0)) {
            Update_Board();
            Count_Rows_and_Columns();
        }
    }

	void Count_Rows_and_Columns() {
		// Run through all __Item_Positions and determine the total number of rows and columns to minimize future recursing
		itemPositions.AddRange(transform.FindChild ("__Item_Positions").GetComponentsInChildren<Transform> ());
		itemPositions = itemPositions.FindAll(obj => obj.name.Contains(";"));

		itemPositions.ForEach(delegate(Transform obj) {
			// Make sure we're dealing with an actual item position: gameobject naming convention "[row];[col]"
			int row, col;
			if ((col = int.Parse(obj.name.Split(new char[] {';'})[1])) > amountColumns)
				amountColumns = col; 
			row = int.Parse(obj.name.Split(new char[] {';'})[0]);
			if (amountRows.Count <= col && col > 0)
				amountRows.AddRange(new int[(col - amountRows.Count) + 1] );
			if (amountRows.Count > 0 && amountRows[col] < row )
			    amountRows[col] = row;
		});
	}


	void Populate_Board() {
		Transform itemParent = transform.FindChild ("__Items");
		itemPositions.ForEach (delegate(Transform obj) {
			Board_Item.Items eachItem;
			/* ****** TO-DO: RANDOMIZE whether to create stones, items, special power ups, w/e */
			eachItem = Create_Stone__Random (itemParent);

			// Place the item on the board, locate it on a row & column, and activate
			eachItem.Object.transform.Translate (obj.position);
			eachItem.Row = int.Parse (obj.name.Split (new char[] {';'}) [0]);
			eachItem.Column = int.Parse (obj.name.Split (new char[] {';'}) [1]);
            eachItem.State = Board_Item.Items.States.Resting;
			eachItem.Object.SetActive (true);	

			// Link the child script back to this handler for easy back-and-forth
			Board_Item eachBoardItem = ((Board_Item)eachItem.Object.GetComponent<Board_Item> ());
            eachBoardItem.Item = eachItem;
			eachBoardItem.Board = this;

			listItems.Add (eachItem);
		});
	}

	void Update_Board() {
		// Fill any empty spaces by dropping gems down in their _Columns
		for (int i = 0; i <= amountColumns; i++) {
			// Grab a list of all _Items in each column, parsing column by column in a buffer
			List<Board_Item.Items> bufColumns = listItems.FindAll (x => x.Column == i);
			bufColumns.Sort(delegate(Board_Item.Items x, Board_Item.Items y) { return x.Row.CompareTo(y.Row); });
			// Now parse row by row ascending from the *second from* bottom row to check for holes in that column
			for (int j = 0; j < bufColumns.Count; j++) {
				// If there is space between this item and the one two items down...
				// Or if this is the first item but is not in the bottom row...
                if ((j > 0 && bufColumns[j].Row - 1 >= 0 && bufColumns[j-1].Row < bufColumns[j].Row - 1) 
				    || (j == 0 && bufColumns[j].Row > 0)) {
                    Vector3 target = itemPositions.Find (obj 
	                       => bufColumns[j].Row - 1 == int.Parse (obj.name.Split (new char[] {';'}) [0]) 
	                       && bufColumns[j].Column == int.Parse (obj.name.Split (new char[] {';'}) [1]))
						.position;
                    // And move the actual _Item
                    Game_Handler.Move_Smooth(bufColumns[j].Object.transform, target, 0.1f);
                    bufColumns[j].Row--; // And set the _Item to be one row down
                    j--; // And recurse the same _Item again
				}
            }
        }
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

        listItems.Remove(incItem);
        GameObject.Destroy (incItem.Object);
	}



	/* Definitions and functions for individual stones, creation, etc */

    public GameObject proto_Stone_9;


	// Create and return a random stone
	public Board_Item.Items Create_Stone__Random(Transform incParent) {
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
