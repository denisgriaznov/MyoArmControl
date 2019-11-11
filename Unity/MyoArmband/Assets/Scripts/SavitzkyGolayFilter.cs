using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SavitzkyGolayFilter : MonoBehaviour
{

    private int n = 11;
    private int order = 2;
    private int[] A;
    private int sumA = 0;
    private int d;
    // Start is called before the first frame update
    void Start()
    {
        A = new int[11] {-36, 9, 44, 69, 84, 89, 84, 69, 44, 9, -36 };
        foreach(int ctr in A)
        {
            sumA = sumA + ctr;
        }
        d = (n - 1) / 2;
    }

    public List<int> filtering(List<int> input)
    {
        for (int i = 0; i < input.Count; i++)
        {
            input[i] = Math.Abs(input[i]);
        }
        List<int> output = new List<int>();
        for(int i = d - 1;i < input.Count - d-1; i++)
        {
            double y = 0;
            
            for(int j = 0; j < n; j++)
            {

                int m = input[i - d + 1 + j];
                y = y + m * A[j];
            }

            y = y / sumA;
            output.Add(Convert.ToInt32(y));

        }

        return (output);
    }
}
