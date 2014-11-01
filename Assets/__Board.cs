using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Board : MonoBehaviour {



	public int __Board_Width = 10;
	public int __Board_Height = 10;
	public Transform __Gem_Position__Transforms;
		

	List<Vector3> __Gem_Positions = new List<Vector3>();
	List<__Gem_Handler.__Gem> _Gems = new List<__Gem_Handler.__Gem> ();

	__Game_Handler _Game_Handler;
	__Gem_Handler _Gem_Handler;


	void Start() {
		_Game_Handler = (__Game_Handler)GameObject.FindObjectOfType(typeof(__Game_Handler));
		_Gem_Handler = (__Gem_Handler)FindObjectOfType(typeof(__Gem_Handler));

		foreach (Transform eachPosition in __Gem_Position__Transforms)
			__Gem_Positions.Add (eachPosition.position);

		Populate ();
	}

	public void Populate() {
		for (int i = 0; i < __Gem_Positions.Count; i++) {
				__Gem_Handler.__Gem eachGem = _Gem_Handler.Create_Gem__Random();
				eachGem._Object.SetActive(true);
				eachGem._Object.transform.Translate(__Gem_Positions[i]);
				_Gems.Add (eachGem);
			}
	}
}
