using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class __Tooltip : MonoBehaviour {

    public GameObject Tooltip_Panel;
    public Text Tooltip_Text;

    public void Process(Transform incMouseover) {
        if (incMouseover == null) {
            Tooltip_Panel.SetActive(false);
            return;
        }
        /* 
         * Add tooltip information here...
         */
        else {
            Tooltip_Panel.SetActive(false);
            return;
        }

        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Tooltip_Panel.transform.position = new Vector3(
            newPosition.x,
            newPosition.y,
            Tooltip_Panel.transform.position.z);
        Tooltip_Panel.SetActive(true);
    }
}