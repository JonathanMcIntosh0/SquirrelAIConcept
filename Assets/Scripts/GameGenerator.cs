using System;
using System.Collections.Generic;
using System.Linq;
using GarbageCan;
using GOAP.Agent;
using Tree;
using UnityEngine;
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

    public void GenerateGame()
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
             // Make it so squirrels have different priorities to prevent getting stuck on each other
             squirrel.GetComponent<NavigationSystem>().avoidancePriority = 50 + i;
             GameModel.Squirrels[i] = squirrel.GetComponent<SController>();

         }
        
    }

    // To generate our random points we will generate two lists of sparse floats between GameModel.Min_X and
    // GameModel.Max_X (and for Z), such that the distance between any 2 points is at least MinDistance.
    // We will then randomise the order of the
    // lists and pair the entries of both lists to get our points.
    private Vector3[] GETRandomPoints(int count)
    {
        Vector3[] points = new Vector3[count];

        var minDistance = 2 * Mathf.Max(GameModel.TreeRadius, GameModel.GarbageCanRadius);
        var pts = GenRandomPoints(
            GameModel.MinX, GameModel.MaxX, 
            GameModel.MinZ, GameModel.MaxZ, 
            minDistance, count);
        
        for (int i = 0; i < count; i++)
        {
            points[i] = new Vector3(pts[i].x, 0f, pts[i].y);
        }

        return points;
    }
    
    // Generate a random list of 15 sparsely separated floats between min and max
    private static List<Vector2> GenRandomPoints(float minX, float maxX, float minY, float maxY, float minDistance, int count)
    {
        // Sorted lists of already chosen values
        List<Vector2> pts = new List<Vector2>(count);

        const int maxFails = 50;
        var fails = 0;
        while (pts.Count < count && fails < maxFails)
        {
            var x = Random.Range(minX + minDistance, maxX - minDistance);
            // Get sorted list of points (ordered by y val) within (x - 2 * _minDistance, x + 2 * _minDistance)
            // List<float> ys = pts.Where(v => x - 2 * _minDistance < v.x && v.x < x + 2 * _minDistance).Select(p => p.y).ToList();
            List<float> ys = new List<float>();
            ys.Add(minY);
            ys.AddRange(
                from p in pts
                where x - minDistance < p.x && p.x < x + minDistance
                orderby p.y select p.y);
            ys.Add(maxY);

            var prevY = minY;
            var dTotal = ys.Aggregate(0f, (acc, y) =>
            {
                var res = Mathf.Max(0f, y - prevY - 2 * minDistance);
                prevY = y;
                return acc + res;
            });
            // }, acc => acc + Mathf.Max(0f, maxY - prevY - 2 * _minDistance));

            if (dTotal == 0)
            {
                fails++;
                continue;
            }

            //Choose which 2 y in ys we want to generate our new point between 
            var r = Random.Range(0f, dTotal);
            var curIdx = 0;
            // Let s be the sum of "valid" distances between y in ys until cur.Next 
            var s = Mathf.Max(0f, ys[curIdx + 1] - ys[curIdx] - 2 * minDistance);
            // Iterate until r <= s. If r <= s then we choose our next point between cur and cur.Next
            while (r > s)
            {
                // Update cur and s
                curIdx++;
                s += Mathf.Max(0f, ys[curIdx + 1] - ys[curIdx] - 2 * minDistance);
            }
            
            //Now we will choose a new point between cur and cur.Next
            var y1 = ys[curIdx] + minDistance;
            var y2 = ys[curIdx + 1] - minDistance;
            var y = GETAdjustedRandom(y1, y2); // Adjust to be more centralised
            
            pts.Add(new Vector2(x, y));
        }

        if (fails >= maxFails) Debug.LogError($"Not enough space to generate {count} random points!");
        return pts;
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
}
