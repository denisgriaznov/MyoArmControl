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

    public float getMean(List<float> input,int start)
    {
        float mean = 0;
        int cnt = input.Count;
        for (int i = start; i < cnt; i++)
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
        float mean = getMean(input,0);
        float sigma = getSigma(input);
        List<float> output = new List<float>();

        for (int i = 0; i < cnt; i++)
        {
            float cur = input[i];
            output.Add((cur - mean)/sigma);
        }

        return (output);
    }

    public float getMax(float[] f)
    {
        float max = 0;
        for(int i = 0; i < f.Length; i++)
        {
            if (f[i] > max) max = f[i];
        }

        return max;
    }

    public float correlation(float[] basepose, float[] newpose)
    {
        
        float corr = 0;
        for (int i = 0; i < basepose.Length; i++)
        {
            corr = corr + (basepose[i] - newpose[i]) * (basepose[i] - newpose[i]);
        }

        return corr;
    }

    public float[] shiftArray(float[] f,int shift)
    {
        float[] newarray = new float[f.Length];
        for (int i = 0; i < f.Length; i++)
        {
            newarray[(i+shift) % 8] = f[i];
        }
        return newarray;
    }
    public int getShift(float[] basepose, float[] newpose)
    {

        float corr = 100000;
        int idx = 0;
        for (int i = 0; i < basepose.Length; i++)
        {
           
            float current = correlation(basepose,shiftArray(newpose, i));
            if (current < corr)
            {
                idx = i;
                corr = current;
            }

        }

        return idx;
    }
}
