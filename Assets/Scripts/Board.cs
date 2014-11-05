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

	List<Board_Item.Item> listItems = new List<Board_Item.Item>();

	void Start() {
		Game_Handler = (Game_Handler)GameObject.FindObjectOfType (typeof(Game_Handler));

		Count_Rows_and_Columns ();
		Populate_Board ();
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
			Board_Item.Item eachItem;
			/* ****** TO-DO: RANDOMIZE whether to create stones, items, special power ups, w/e */
			eachItem = Create_Stone__Random (itemParent);

			// Place the item on the board, locate it on a row & column, and activate
			eachItem.Object.transform.Translate (obj.position);
			eachItem.Row = int.Parse (obj.name.Split (new char[] {';'}) [0]);
			eachItem.Column = int.Parse (obj.name.Split (new char[] {';'}) [1]);
			eachItem.Object.SetActive (true);	

			// Link the child script back to this handler for easy back-and-forth
			Board_Item eachBoardItem = ((Board_Item)eachItem.Object.GetComponent<Board_Item> ());
			eachBoardItem.This = eachItem;
			eachBoardItem.Board = this;

			listItems.Add (eachItem);
		});
	}

	void Update_Board() {
		// Fill any empty spaces by dropping gems down in their _Columns
		for (int i = 0; i <= amountColumns; i++) {
			// Grab a list of all _Items in each column, parsing column by column in a buffer
			List<Board_Item.Item> bufColumns = listItems.FindAll (x => x.Column == i);
			bufColumns.Sort(delegate(Board_Item.Item x, Board_Item.Item y) { return x.Row.CompareTo(y.Row); });
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

	public void Destroy_Item(Board_Item.Item incItem) {
        Game_Handler.Player.Add_Mana(incItem.Color, 1);

        if (incItem.Color == Definitions.Mana_Colors.Black)
            Game_Handler.Player.Character.Add_Level(1);
        else if (incItem.Color == Definitions.Mana_Colors.White)
            Game_Handler.Player.Character.Add_Level(10);

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

		Update_Board ();
		Count_Rows_and_Columns ();
	}



	/* Definitions and functions for individual stones, creation, etc */

	// Prototypes for 9-sided stones
	public GameObject 
		proto_Stone_9_Black, 
		proto_Stone_9_Blue, 
		proto_Stone_9_Green,
		proto_Stone_9_Purple,
		proto_Stone_9_Red, 
		proto_Stone_9_White,
		proto_Stone_9_Yellow;


	// Create and return a random stone
	public Board_Item.Item Create_Stone__Random(Transform incParent) {
		Board_Item.Item thisStone = new Board_Item.Item ();
		thisStone.Type = Board_Item.Item.Types.Stone;
		thisStone.Color = (Definitions.Mana_Colors)UnityEngine.Random.Range (0, Enum.GetNames (typeof(Definitions.Mana_Colors)).Length);
		
		switch (thisStone.Color) {
		default:
		case Definitions.Mana_Colors.Black: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_Black); break;
        case Definitions.Mana_Colors.Blue: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_Blue); break;
        case Definitions.Mana_Colors.Green: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_Green); break;
        case Definitions.Mana_Colors.Purple: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_Purple); break;
        case Definitions.Mana_Colors.Red: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_Red); break;
		case Definitions.Mana_Colors.White: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_White); break;
        case Definitions.Mana_Colors.Yellow: thisStone.Object = (GameObject)GameObject.Instantiate(proto_Stone_9_Yellow); break;
		}
		
		thisStone.Object.transform.parent = incParent;
		return thisStone;
	}

}
