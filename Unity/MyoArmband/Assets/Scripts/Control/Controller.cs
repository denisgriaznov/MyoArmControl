using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour
{
    public GameObject prefab;
    public GameObject target;
    public ModeManager modeManager;
    public int countGreen = 0;
    public int countRed = 0;
    public Text greenText;
    public Text redText;
    void Awake()
    {
        StartCoroutine(spawner());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void greenScore()
    {
        countGreen += 1;
        greenText.text = countGreen.ToString();
    }
    public void redScore()
    {
        countRed += 1;
        redText.text = countRed.ToString();

    }
    IEnumerator spawner()
    {
        while (true)
        {  if (modeManager.current == 0)
            {
                Instantiate(prefab, target.transform.position, target.transform.rotation);
            }
                yield return new WaitForSecondsRealtime(3);
            
        }

    }
}
