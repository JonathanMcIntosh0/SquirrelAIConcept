using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GarbageCan;
using GOAP;
using GOAP.Agent;
using Tree;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameGenerator : MonoBehaviour
{
    /*
     * This script generates all the trees, garbage bins and squirrels.
     * To randomly generate our trees and garbage bins we will generate 15 points within our game area and
     * randomly choose 5 of them to be bins and the rest trees.
     *
     * We will generate our points one by one ensuring that the distance between any 2 points (or wall) has x and z
     * components of at least 2.5. This means that each point will have a 5x5 box around it in which no other points nor
     * wall shall reside. I chose to do this component wise instead of having circular boundaries as it is easier to compute.
     *
     * The downside to coordinate wise randomness is that we will never have 2 points in a straight line (non diagonal)
     * within our game area.
     */

    //Min value for coordinate wise distance between 2 randomly generated points (or wall)
    private readonly float _minDistance = Mathf.Max(GameModel.TreeRadius, GameModel.GarbageCanRadius); 
    
    
    // Start is called before the first frame update
    void Awake()
    {
        Random.InitState(DateTime.Now.Millisecond); //Set random seed
        
        // Get 15 random points within our game area and randomly choose 5 of them to be garbage bins and the rest trees.
        // To choose our garbage cans we will simply take the first 5 entries since our getRandomPoints method randomises
        // the returned list as well.
        var points = GETRandomPoints(GameModel.NumGarbageCans + GameModel.NumTrees);

        for (int i = 0; i < GameModel.NumGarbageCans + GameModel.NumTrees; i++)
        {
            if (i < GameModel.NumGarbageCans)
            {
                //Spawn Garbage Can
                GameModel.GarbageCans[i] = Instantiate(
                    Resources.Load<GameObject>("Garbage"), 
                    points[i], Quaternion.identity).GetComponent<GarbageCanController>();
                // GameModel.GarbageCans[i].transform.position = points[i];
                // GameModel.GarbageCans[i].SetActive(false);
            }
            else
            {
                //Spawn Tree
                GameModel.Trees[i - GameModel.NumGarbageCans] = Instantiate(
                    Resources.Load<GameObject>("Tree"), 
                    points[i], Quaternion.identity).GetComponent<TreeController>();
                // GameModel.Trees[i - 5].transform.position = points[i];
                // GameModel.Trees[i - 5].SetActive(false);
            }
        }
        
        // Choose 5 random "home" trees and spawn squirrels.
        // Note that again since our list was randomized we may simply choose the first 5 trees.
         for (int i = 0; i < GameModel.NumSquirrels; i++)
         {
             // var r = Random.insideUnitCircle.normalized * (GameModel.TreeTrunkRadius + GameModel.SquirrelRadius);
             // var offsetR = new Vector3(r.x, 0f, r.y);
             // // var offestY = new Vector3(0f, 0f, 0f);
             // var offestY = new Vector3(0f, GameModel.MaxSquirrelHeight, 0f);
             // var offset = offsetR + offestY;

             var squirrel = Instantiate(
                 Resources.Load<GameObject>("Squirrel"),
                 GameModel.Trees[i].transform.position, Quaternion.identity);
             squirrel.name = $"Squirrel({i})";
             squirrel.GetComponent<TargetingSystem>().homeTreeController =
                 GameModel.Trees[i].GetComponent<TreeController>();
             // squirrel.SetActive(true);
             // sController.squirrelID = i;
             // sController.Memory = new SquirrelMemory(squirrel, GameModel.Trees[i]);
             GameModel.Squirrels[i] = squirrel.GetComponent<SController>();
             
             // GameModel.Squirrels[i].transform.rotation = Quaternion.LookRotation(offsetR);
         }
        
    }

    // To generate our random points we will generate two lists of sparse floats between GameModel.Min_X and
    // GameModel.Max_X (and for Z), such that the distance between any 2 points is at least MinDistance.
    // We will then randomise the order of the
    // lists and pair the entries of both lists to get our points.
    private Vector3[] GETRandomPoints(int count)
    {
        Vector3[] points = new Vector3[count];

        var xList = GenRandomList(GameModel.MinX, GameModel.MaxX, count);
        var zList = GenRandomList(GameModel.MinZ, GameModel.MaxZ, count);
        
        // Shuffle both lists as they are currently sorted.
        // Note that shuffling one list suffices as we would still get a list of random points in the end,
        // however we would still have some ordering to the list of points we return. If we shuffle both lists then
        // not only do we get a list of random points within our game area, but the list itself is also randomized.
        ShuffleList(xList);
        ShuffleList(zList);

        // for (int i = 0; i < 14; i++)
        // {
        //     var r = Random.Range(i, 15);
        //     (xList[i], xList[r]) = (xList[r], xList[i]); //swap the values at index i and r
        // }

        // Pair up the two lists to get our random points in the game area
        for (int i = 0; i < count; i++)
        {
            points[i] = new Vector3(xList[i], 0f, zList[i]);
        }

        return points;
    }

    // Generate a random list of 15 sparsely separated floats between min and max
    private float[] GenRandomList(float min, float max, int count)
    {
        // Sorted lists of already chosen values
        LinkedList<float> xs = new LinkedList<float>();

        // Add boundary coordinate components
        xs.AddFirst(min);
        xs.AddLast(max);

        //DTotal is the total "valid" distance for which we can choose a new point from. We will update this as we add new points.
        var dTotal = xs.Last.Value - xs.First.Value - 2 * _minDistance;

        // Generate points one by one
        for (int i = 0; i < count; i++)
        {
            //First we will choose which 2 points in xs we want to generate our new point between 
            var r = Random.Range(0f, dTotal);
            var cur = xs.First;
            // Here we let s be the sum of "valid" distances between points in xs until cur.Next 
            var s = Mathf.Max(0f, cur.Next.Value - cur.Value - 2 * _minDistance);
            // Iterate until r <= s. If r <= s then we choose our next point between cur and cur.Next
            while (r > s)
            {
                // Update cur and s
                cur = cur.Next;
                s += Mathf.Max(0f, cur.Next.Value - cur.Value - 2 * _minDistance);
            }
            
            //Now we will choose a new point between cur and cur.Next, then update dTotal accordingly
            var x1 = cur.Value + _minDistance;
            var x2 = cur.Next.Value - _minDistance;
            var x = GETAdjustedRandom(x1, x2);

            dTotal -= Mathf.Min(_minDistance, x - x1) + Mathf.Min(_minDistance, x2 - x);
            xs.AddAfter(cur, x);
        }
        
        //We now remove the boundary points and return our list of random floats
        xs.RemoveFirst();
        xs.RemoveLast();
        return xs.ToArray();
    }

    // Returns a random float between min and max (inclusive) according to a pseudo normalised distribution,
    // i.e. points closer to the middle are more favourable.
    // To do this we will generate a random between 0 and 1 then apply the inverse "S" curve 4(x - 0.5)^3 + 0.5,
    // this will give us a pseudo normalised random between 0 and 1, we will then scale this obtained value appropriately
    // such that we get a random value between min and max.
    // Note that our inverse "S" curve is simply x^3 transformed such that the inverse "S" part of the curve is
    // between 0 and 1, and we intersect the points (0, 0) and (1, 1).
    private static float GETAdjustedRandom(float min, float max)
    {
        var r = Random.value;
        var x = 4 * Mathf.Pow(r - 0.5f, 3) + 0.5f;
        return min + x * (max - min);
    }

    private static T[] ShuffleList<T>(T[] l)
    {
        for (int i = 0; i < l.Length - 1; i++)
        {
            var r = Random.Range(i, l.Length);
            (l[i], l[r]) = (l[r], l[i]); //swap the values at index i and r
        }

        return l;
    } 
}
