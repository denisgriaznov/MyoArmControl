using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataProcessing : MonoBehaviour
{
    private int n = 11;
    private int[] A;
    private int sumA = 0;
    private int d;
    // Start is called before the first frame update
    void Start()
    {
        A = new int[11] { -36, 9, 44, 69, 84, 89, 84, 69, 44, 9, -36 };
        foreach (int ctr in A)
        {
            sumA = sumA + ctr;
        }
        d = (n - 1) / 2;
    }

    // Savizkiy-Golay 9 2
    public List<int> sav_gol(List<int> input)
    {
        for (int i = 0; i < input.Count; i++)
        {
            input[i] = Math.Abs(input[i]);
        }
        List<int> output = new List<int>();
        for (int i = d - 1; i < input.Count - d - 1; i++)
        {
            double y = 0;

            for (int j = 0; j < n; j++)
            {

                int m = input[i - d + 1 + j];
                y = y + m * A[j];
            }

            y = y / sumA;
            output.Add(Convert.ToInt32(y));

        }

        return (output);
    }

    // Moving Average filter
    public List<float> mov_av(List<float> input,int n)
    {
        int cnt = input.Count;
        int m =  ((n - 1) / 2);
        for (int i = 0; i < cnt; i++)
        {
            input[i] = Math.Abs(input[i]);
        }
        List<float> output = new List<float>();
        for (int i = 0; i < cnt; i++)
        {
            float y = 0.0f;

            for (int j = i - m; j < i + m; j++)
            {

                if ((j>-1) && (j<cnt)) 
                y = y + input[j];
            }

            y = y / n;
            output.Add(y);

        }

        return (output);
    }

    public float getMean(List<float> input)
    {
        float mean = 0;
        int cnt = input.Count;
        for (int i = 0; i < cnt; i++)
        {
            mean = mean + input[i];
        }
        mean = mean / cnt;
        return (mean);
    }

    public float getSigma(List<float> input)
    {
        float sigma = 0;
        int cnt = input.Count;
        for (int i = 0; i < cnt; i++)
        {
            sigma = sigma + input[i]* input[i];
        }
        sigma = (float) Math.Sqrt(sigma / cnt);
        return (sigma);
    }

    public List<float> stand(List<float> input)
    {   
        int cnt = input.Count;
        float mean = getMean(input);
        float sigma = getSigma(input);
        List<float> output = new List<float>();

        for (int i = 0; i < cnt; i++)
        {
            float cur = input[i];
            output.Add((cur - mean)/sigma);
        }

        return (output);
    }
}
