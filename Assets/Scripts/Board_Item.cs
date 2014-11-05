using UnityEngine;
using System;
using System.Collections;

public class Board_Item : MonoBehaviour {

	[HideInInspector]
	public Board Board;
	public Item This;

	public class Item {
		public enum Types {
			Stone,
			Consumable,
			Equipment
		}
		
		public enum Colors {
			Black,
			Blue,
			Green,
			Purple,
			Red,
			White,
			Yellow
		}
		
		public Types Type;
		public Colors Color;
		public GameObject Object;
		
		public int Column;
		public int Row;
	}


	void OnMouseDown() {
		Board.Destroy_Item (This);
	}
}
