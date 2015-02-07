using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class __Game : MonoBehaviour {
	
	__Audio _Audio;
	__Actions _Action;
	__Menu _Menu__Main;
	__Menu _Menu__Settings;
	
	public enum Game_States {
		RUNNING,
		PAUSED
	}
	Game_States _Game_State,
				_Last_State;
	bool[] _Last_State__Controls;
	
	float __Time;
	
	public bool _Can_Move = false,
				_Can_Look = false,
				_Can_Interface = false;
	
	public bool _Option__Captions = false,
				_Option__Error_Speech = false;
	public float _Option__Volume = 1f;
	
	void Awake() {
		_Audio = GameObject.Find("__Game Controller").GetComponent<__Audio>();
		_Action = GameObject.Find("__Game Controller").GetComponent<__Actions>();
        _Menu__Main = (Instantiate(GameObject.Find("__Main Menu GUI")) as GameObject).GetComponent<__Menu>().Instantiate(this.gameObject);
        _Menu__Settings = (Instantiate(GameObject.Find("__Main Settings GUI")) as GameObject).GetComponent<__Menu>().Instantiate(this.gameObject);
		
		_Game_State = Game_States.RUNNING;
		__Time = Time.time;
		
		GUI__Set();
	}
	void Update() {
		switch (_Game_State) {
		default:
			break;
			
		case Game_States.PAUSED:
			break;
			
		case Game_States.RUNNING:
			__Time += Time.deltaTime;
			break;
		}
	}
	
	void __Input (__Actions.Input_Collision _Input) {
		
		if (_Input.Type__GUI) {
			if (_Input.Action__Click && _Input.Key__Pressed) {
				if (_Input._GUI.name.StartsWith("_Button__Exit_Game"))
					Application.Quit();
				else if (_Input._GUI.name.StartsWith("_Button__New_Game"))
					Application.LoadLevel(Application.loadedLevel);
				else if (_Input._GUI.name.StartsWith("_Button__Resume"))
					Pause__Toggle();
				else if (_Input._GUI.name.StartsWith("_Button__Settings"))
					_Menu__Settings.Toggle();
				else if (_Input._GUI.name.StartsWith("_Button__Captions"))
					_Option__Captions = !_Option__Captions;
				else if (_Input._GUI.name.StartsWith("_Button__Error_Speech"))
					 _Option__Error_Speech = !_Option__Error_Speech;
			}
			
			GUI__Set();
		}
	}
	
	public void GUI__Set() {
		_Menu__Settings.Texture_Set("_Button__Captions__Box", 
			_Option__Captions ? _Menu__Settings._Box__Checked : _Menu__Settings._Box__Unchecked);
		_Menu__Settings.Texture_Set("_Button__Error_Speech__Box", 
			_Option__Error_Speech ? _Menu__Settings._Box__Checked : _Menu__Settings._Box__Unchecked);
	}
	public void Pause__Toggle() {
		if (_Game_State == Game_States.PAUSED) {
			_Game_State = _Last_State;
			
			_Menu__Main.Hide();
			_Menu__Settings.Hide();
			Controls__Set(_Last_State__Controls);
			
		} else {
			_Last_State = _Game_State;
			_Last_State__Controls = Controls__Get();
			_Game_State = Game_States.PAUSED;
			
			_Menu__Main.Show();
			Controls__Set(false, false, false);
		}
	}
	
	public void Controls__Set(bool Set_All)
		{ Controls__Set(Set_All, Set_All, Set_All); }
	public void Controls__Set(bool Move, bool Look, bool Interface) {
		_Can_Move = Move;
		_Can_Look = Look;
		_Can_Interface = Interface;
	}
	public void Controls__Set(bool[] _Array) 
		{ Controls__Set(_Array[0], _Array[1], _Array[2]); }
	public bool[] Controls__Get() 
		{ return new bool[] { _Can_Move, _Can_Look, _Can_Interface }; }
	public float _Time
		{ get { return __Time; } }
	public float _Delta_Time
		{ get { return _Game_State == Game_States.RUNNING ? Time.deltaTime : 0f; } }
	public Game_States _State
		{ get { return _Game_State; } }
	public bool _Paused
		{ get { return _Game_State == Game_States.PAUSED; } }
}
