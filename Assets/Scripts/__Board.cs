using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Board : MonoBehaviour {

	__Game_Handler _Game_Handler;

	/* Definitions and functions for the array of items */

	int _count_Columns = 0;
	List<int> _count_Rows = new List<int> (); // index = column #; some columns may have different amount of rows than others
	List<Transform> _Item_Positions = new List<Transform> ();

	List<__Item> _Items = new List<__Item>();

	void Start() {
		_Game_Handler = (__Game_Handler)GameObject.FindObjectOfType (typeof(__Game_Handler));

		Count_Rows_and_Columns ();
		Populate_Board ();
	}

	void Count_Rows_and_Columns() {
		// Run through all __Item_Positions and determine the total number of rows and columns to minimize future recursing
		_Item_Positions.AddRange(transform.FindChild ("__Item_Positions").GetComponentsInChildren<Transform> ());
		_Item_Positions = _Item_Positions.FindAll(obj => obj.name.Contains(";"));

		_Item_Positions.ForEach(delegate(Transform obj) {
			// Make sure we're dealing with an actual item position: gameobject naming convention "[row];[col]"
			int row, col;
			if ((col = int.Parse(obj.name.Split(new char[] {';'})[1])) > _count_Columns)
				_count_Columns = col; 
			row = int.Parse(obj.name.Split(new char[] {';'})[0]);
			if (_count_Rows.Count <= col && col > 0)
				_count_Rows.AddRange(new int[(col - _count_Rows.Count) + 1] );
			if (_count_Rows.Count > 0 && _count_Rows[col] < row )
			    _count_Rows[col] = row;
		});
	}


	void Populate_Board() {
		_Item_Positions.ForEach (delegate(Transform obj) {
			__Item eachItem;
			/* ****** TO-DO: RANDOMIZE whether to create stones, items, special power ups, w/e */
			eachItem = Create_Stone__Random ();

			// Place the item on the board, locate it on a row & column, and activate
			eachItem._Object.transform.Translate (obj.position);
			eachItem._Row = int.Parse (obj.name.Split (new char[] {';'}) [0]);
			eachItem._Column = int.Parse (obj.name.Split (new char[] {';'}) [1]);
			eachItem._Object.SetActive (true);	

			// Link the child script back to this handler for easy back-and-forth
			__Board_Item eachBoardItem = ((__Board_Item)eachItem._Object.GetComponent<__Board_Item> ());
			eachBoardItem._This = eachItem;
			eachBoardItem._Board = this;

			_Items.Add (eachItem);
		});
	}

	void Update_Board() {

		// Fill any empty spaces by dropping gems down in their columns
		for (int i = 0; i <= _count_Columns; i++) {
			// Grab a list of all _Items in each column, parsing column by column in a buffer
			List<__Item> colArray = _Items.FindAll (x => x._Column == i);
			colArray.Sort(delegate(__Item x, __Item y) { return x._Row.CompareTo(y._Row); });

			// Now parse row by row ascending from the *second from* bottom row to check for holes in that column
			for (int j = 0; j < colArray.Count; j++) { 
				// If there is space between this item and the one two items down...
				// Or if this is the first item but is not in the bottom row...
				if ((j > 0 && colArray[j]._Row - 1 >= 0 && colArray[j-1]._Row < colArray[j]._Row - 1) 
				    || (j == 0 && colArray[j]._Row > 0)) {
					colArray[j]._Row--;	// Adjust the temporary column buffer accordingly
					Vector3 target = _Item_Positions.Find (obj 
	                       => colArray[j]._Row == int.Parse (obj.name.Split (new char[] {';'}) [0]) 
	                       && colArray[j]._Column == int.Parse (obj.name.Split (new char[] {';'}) [1]))
						.position;
					colArray[j]._Object.transform.position = target;
						// And move the actual _Item
					j = (j - 3 < 1) ? j : j -= 3; // And scoot backwards in the buffer in case there are consecutive holes
				} 
			}
		}
	}

	public void Destroy_Item(__Item incItem) {
		_Items.Remove (incItem);
		GameObject.Destroy (incItem._Object);

		Update_Board ();
		Count_Rows_and_Columns ();
	}



	/* Definitions and functions for individual stones, creation, etc */

	public class __Item {
		public enum __Type {
			Stone,
			Consumable,
			Equipment
		}

		public enum __Color {
			Black,
			Blue,
			Green,
			Red,
			White
		}
		
		public __Type _Type;
		public __Color _Color;
		public GameObject _Object;

		public int _Column;
		public int _Row;
	}
	
	public GameObject __Stone_Prototype__Black, __Stone_Prototype__Blue, __Stone_Prototype__Green,
	__Stone_Prototype__Red, __Stone_Prototype__White;

	__Item Create_Stone__Random() {
		__Item thisStone = new __Item ();

		thisStone._Type = __Item.__Type.Stone;
		thisStone._Color = (__Item.__Color)UnityEngine.Random.Range (0, Enum.GetNames (typeof(__Item.__Color)).Length);
		
		switch (thisStone._Color) {
		default:
		case __Item.__Color.Black: thisStone._Object = (GameObject)GameObject.Instantiate(__Stone_Prototype__Black); break;
		case __Item.__Color.Blue: thisStone._Object = (GameObject)GameObject.Instantiate(__Stone_Prototype__Blue); break;
		case __Item.__Color.Green: thisStone._Object = (GameObject)GameObject.Instantiate(__Stone_Prototype__Green); break;
		case __Item.__Color.Red: thisStone._Object = (GameObject)GameObject.Instantiate(__Stone_Prototype__Red); break;
		case __Item.__Color.White: thisStone._Object = (GameObject)GameObject.Instantiate(__Stone_Prototype__White); break;
		}
		
		thisStone._Object.transform.parent = this.transform;
		return thisStone;
	}
}
