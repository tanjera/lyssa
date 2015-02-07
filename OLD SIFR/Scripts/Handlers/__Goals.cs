using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Goals : MonoBehaviour {
	
	public class Goal {
		public string _Name = "",
						_Description = "";
		public float _Timer__Tick = 0.5f,
					_Timer__Current = 0f;
		
		public bool _Achieved = false;
		public delegate bool __Status();
		public delegate void __Response();
		public __Status _Status;
		public __Response _Response;
		
		// Multi-part goal series variables
		public bool _Serial = false;
		public int _Serial_Index = 0;
		public List<Goal> _Serial_List = new List<Goal>();
		
		public Goal(string Name, string Description, __Status Status, __Response Response, float Check_Every = 0.5f) {
			// For single goals or each part of multipart goals
			_Name = Name;
			_Description = Description;
			_Status = Status;
			_Response = Response;
			_Timer__Tick = Check_Every;
		}
		public Goal(string Name, string Description, __Status Status, __Response Response, 
				List<Goal> Serial_List, float Check_Every = 0.5f) {
			_Name = Name;
			_Description = Description;
			_Status = Status;
			_Response = Response;
			_Timer__Tick = Check_Every;
			
			_Serial = true;
			_Serial_List = Serial_List;
			_Serial_Index = 0;
		}
	}
	
	__Game _Game;
	__Menu _Goal_Display;
	
	public List<Goal> _Goals = new List<Goal>();
	
	void Awake() {
		_Game = GameObject.Find("__Game Controller").GetComponent<__Game>();
		_Goal_Display = (Instantiate(GameObject.Find("__Goals Display GUI")) as GameObject).GetComponent<__Menu>().Instantiate(this.gameObject);
	}
	void Update() {
		for (int i = _Goals.Count - 1; i >= 0; i--) {
			if (_Goals[i]._Achieved)
				continue;
			
			_Goals[i]._Timer__Current -= _Game._Delta_Time;				// Iterate the timer
			if (_Goals[i]._Timer__Current < 0) {						// If timer is up
				_Goals[i]._Timer__Current = _Goals[i]._Timer__Tick;		// Reset the timer
				
				if (_Goals[i]._Serial) {
					if (_Goals[i]._Serial_List[_Goals[i]._Serial_Index]._Status()) {	// Then check the status of the goal
						_Goals[i]._Serial_List[_Goals[i]._Serial_Index]._Achieved = true;
						_Goals[i]._Serial_List[_Goals[i]._Serial_Index]._Response();	// And if it's met, run the response
						
						if (_Goals[i]._Serial_Index == _Goals[i]._Serial_List.Count - 1)
							_Goals[i]._Achieved = true;
						else
							_Goals[i]._Serial_Index++;
						
						Display__Update();
					}
				} else if (!_Goals[i]._Serial) {
				
					if (_Goals[i]._Status()) {								// Then check the status of the goal
						_Goals[i]._Achieved = true;
						_Goals[i]._Response();								// And if it's met, run the response
						Display__Update();
					}
				}
			}
		}
	}
	
	public void Display__Update() {
		if (_Goals.Count == 0 && _Goal_Display.Visible) {
			_Goal_Display.Text__Clear("_Goals__Text");
			_Goal_Display.Hide();
			return;
		} else if (_Goals.Count > 0 && !_Goal_Display.Visible)
			_Goal_Display.Show();
		
		string goalText = "";
		foreach (Goal eachGoal in _Goals) {
			if (eachGoal._Serial) {
				goalText += String.Format("{0}{1}{2}{3}",
					eachGoal._Serial_List[eachGoal._Serial_Index]._Achieved ? "<color=silver>" : "<b>", 
					eachGoal._Serial_List[eachGoal._Serial_Index]._Description,
					eachGoal._Serial_List[eachGoal._Serial_Index]._Achieved ? "</color>" : "</b>", 
					Environment.NewLine);
			} else if (!eachGoal._Serial) {
				goalText += String.Format("{0}{1}{2}{3}",
					eachGoal._Achieved ? "<color=silver>" : "<b>", eachGoal._Description,
					eachGoal._Achieved ? "</color>" : "</b>", Environment.NewLine);
			}
		}
		_Goal_Display.Text__Set("_Goals__Text", goalText.Trim(Environment.NewLine.ToCharArray()), "black");
	}
	
	public void Add(Goal _Inc) {
		_Goals.Add(_Inc);
		Display__Update();
	}
	public void Remove(Goal _Inc) {
		_Goals.Remove(_Inc);	
		Display__Update();
	}
}
