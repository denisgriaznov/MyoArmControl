using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeManager : MonoBehaviour
{
    //const int DETECTING = 0;
    //const int RECORDING = 1;
    //const int CALIBRATION = 2;

    public int current = -1;

    public Image[] buttons;
    public GameObject[] canvases;

    // Start is called before the first frame update
    void Start()
    {
        changeState(1);
    }

    public void changeState(int state)
    {
        if (current != state) {
            current = state;
            foreach (GameObject obj in canvases) {
                obj.SetActive(false);
            }
            canvases[current].SetActive(true);
            foreach (Image btn in buttons)
            {
                btn.color = Color.gray;
            }
            buttons[current].color = Color.white;
        }
        
    }
}
