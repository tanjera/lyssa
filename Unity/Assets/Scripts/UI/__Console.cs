using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class __Console : MonoBehaviour {

    __Game_Handler _Game;

    bool Active = false;
    public GameObject Panel;
    public Text Input_Text,
                Log,
                Version;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object__Game_Controller).GetComponent<__Game_Handler>();
    }

    void Start() {
        Version.text = String.Format("{0} v{1}", __Definitions.Program_Name.ToLower(), __Definitions.Program_Version.ToLower());
    }

    void Update() {
        if (Active) {
            foreach (char c in Input.inputString) {
                switch (c) {
                    case '`':
                        return;

                    case '\n':
                    case '\r':
                        Output(string.Concat("> ", Input_Text.text));
                        Parse(Input_Text.text);
                        Input_Text.text = "";
                        break;

                    case '\b':
                        if (Input_Text.text.Length > 0)
                            Input_Text.text = Input_Text.text.Remove(Input_Text.text.Length - 1);
                        break;

                    default:
                        Input_Text.text += c;
                        break;
                }
            }
        }
    }

    public void Toggle() {
        Active = !Active;
        Panel.SetActive(Active);
    }

    void Parse(string incString) {
        string[] incCommand = incString.ToLower().Split(' ');

        if (incCommand.Length == 0)
            return;

        switch (incCommand[0].Trim()) {
            default:
                Output("... input not recognized :(");
                return;


            case "set":
                __Player incPlayer;   // Which player are we setting values for?
                
                if (incCommand.Length == 1) { Set_Help(); return; }

                switch (incCommand[1].Trim()) {
                    default:
                    case "help": Set_Help(); return;
                    case "player": incPlayer = _Game.Player; break;
                    case "enemy1": incPlayer = _Game.Enemy_1; break;
                    case "enemy2": incPlayer = _Game.Enemy_2; break;
                    case "enemy3": incPlayer = _Game.Enemy_3; break;
                }

                if (incCommand.Length < 3) { Set_Help(); return; }
                switch (incCommand[2].Trim()) {
                    default: return;
                    case "ep":
                        int incAmount = int.Parse(incCommand[3]);
                        if (incAmount < 0 || incAmount > 100)
                            incAmount = incAmount < 0 ? 0 : 100;
                        incPlayer.Ship.EP_Percent = incAmount;
                        Output(String.Format("{0}'s energy points set to {1}%", incPlayer.Name, incAmount));
                    return;
                }

                return;


            case "clear":
                Log.text = "";
                return;
        }

        Output("... input error, unable to process :'(");
    }

    void Set_Help() {
        Output("set [player/enemy1/enemy2/enemy3] [field] [value] [values] \nfields: ep");
    }

    void Output(string incOutput) {
        Log.text = string.Concat(Log.text, "\n", incOutput);
        // Scroll the console down...
        if (Log.preferredHeight > Log.rectTransform.rect.height) {
            Log.text = Log.text.Trim('\n', ' ');
            Log.text = Log.text.Substring(Log.text.IndexOf('\n'));
        }
    }
}
