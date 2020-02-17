using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public bool isCatched = false;
    public int color = 0;
    // Start is called before the first frame update
    Controller controller;
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("Controller").GetComponent<Controller>();
       color =  Random.Range(1, 3);
       if (color == 1)
        {
            this.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            this.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCatched)
        {
            transform.Translate(Vector3.back * Time.deltaTime);
            if (color == 1)
            {
                if (transform.position.x < -2.0f)
                {
                    controller.redScore();
                    Destroy(gameObject);
                }
            }
            else
            {
                if (transform.position.x > 2.0f)
                {
                    controller.greenScore();
                    Destroy(gameObject);
                }
            }
            if (transform.position.z < -7.0f) Destroy(gameObject);
        }
    }
}
