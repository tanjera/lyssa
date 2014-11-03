using UnityEngine;
using System;
using System.Collections;

public class __Board_Item : MonoBehaviour {

	[HideInInspector]
	public __Board _Board;
	public __Board.__Item _This;

	void OnMouseDown() {
		_Board.Destroy_Item (_This);
	}
}
