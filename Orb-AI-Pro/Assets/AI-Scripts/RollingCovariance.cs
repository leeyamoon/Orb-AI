using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RollingCovariance
{
    public int windowSize;
    public Queue<Vector2Int> positions;
    public Vector2 mean;
    public Vector2[] covarianceMatrix;
    public int n;


    public RollingCovariance(int winSize = 20)
    {
        windowSize = winSize;
        positions = new Queue<Vector2Int>();
        mean = Vector2Int.zero;
        covarianceMatrix = new[] { Vector2.zero, Vector2.zero };
        n = 0;
    }

    public void add_position(Vector2Int new_position)
    {
        if (n < windowSize)
        {
            positions.Enqueue(new_position);
            n++;
            Vector2 delta = new_position - mean;
            mean += delta / n;
            Vector2[] deltaProduct = OuterProduct(delta, delta);
            covarianceMatrix[0] += deltaProduct[0] * (n - 1) / n;
            covarianceMatrix[1] += deltaProduct[1] * (n - 1) / n;
        }
        else
        {
            var oldestPos = positions.Dequeue();
            positions.Enqueue(new_position);
            var newDelta = new_position - mean;
            var oldDelta = oldestPos - mean;
            mean += newDelta / windowSize;
            var newOuter = OuterProduct(newDelta, newDelta);
            var oldOuter = OuterProduct(oldDelta, oldDelta);
            covarianceMatrix[0] += (newOuter[0] - oldOuter[0])*((n - 1f) / n);
            covarianceMatrix[1] += (newOuter[1] - oldOuter[1])*((n - 1f) / n);
        }
    }

    private Vector2[] OuterProduct(Vector2 a, Vector2 b)
    {
        Vector2[] output = new Vector2[2];
        output[0] = new Vector2(a[0]*b[0], a[0]*b[1]);
        output[1] = new Vector2(a[1]*b[0], a[1]*b[1]);
        return output;
    }

    public Vector2[] get_covariance()
    {
        if (n < 2)
            return new[] { Vector2.zero, Vector2.zero };
        Vector2[] result = new Vector2[covarianceMatrix.Length];
        for (int i = 0; i < covarianceMatrix.Length; i++)
        {
            result[i] = covarianceMatrix[i] / (n - 1);
        }
        return result;
    }

    public float get_frobenius_norm()
    {
        var covariance = get_covariance();
        return Mathf.Sqrt(Mathf.Pow(covariance[0][0],2) + Mathf.Pow(covariance[0][1],2) +
                          Mathf.Pow(covariance[1][0],2) + Mathf.Pow(covariance[1][1],2));
    }

    public float calculate_frobenius_norm_for_circle(float radius)
    {
        float var = Mathf.Pow(radius ,2) / 4;
        return Mathf.Sqrt(2) * var;
    }
}
