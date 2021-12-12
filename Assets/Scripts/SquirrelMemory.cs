using System.Collections.Generic;
using System.Linq;
using Tree;
using UnityEngine;

public class SquirrelMemory
{
    private readonly GameObject _squirrel;
    
    public readonly GameObject HomeTree;
    public GameObject NearestTree;
    public Queue<GameObject> GarbageCans;
    public Queue<GameObject> Nuts;

    public SquirrelMemory(GameObject squirrel, GameObject homeTree)
    {
        _squirrel = squirrel;
        
        HomeTree = homeTree;
        NearestTree = homeTree;
        GarbageCans = new Queue<GameObject>(2);
        Nuts = new Queue<GameObject>(5);
    }

    private bool UpdateNearestTree(GameObject tree)
    {
        var d1 = GetDistance(NearestTree.transform.position);
        var d2 = GetDistance(tree.transform.position);

        if (d2 < d1) NearestTree = tree;
        return d2 < d1;
    }

    private void UpdateGarbageCans(List<GameObject> cans)
    {
        //Remove duplicate elements then add new cans
        GarbageCans = new Queue<GameObject>(GarbageCans.Except(cans).Concat(cans));
        while (GarbageCans.Count > 2) GarbageCans.Dequeue(); //Dequeue until 2 left
    }
    
    private void UpdateNuts(List<GameObject> nuts)
    {
        //Remove duplicate elements then add new cans
        Nuts = new Queue<GameObject>(Nuts.Except(nuts).Concat(nuts));
        while (Nuts.Count > 5) Nuts.Dequeue(); //Dequeue until 5 left
    }

    public void UpdateMemory()
    {
        //Find objects in field of view of squirrel
        var treesInFOV = new List<GameObject>();
        var cansInFOV = new List<GameObject>();
        var nutsInFOV = new List<GameObject>();
        //First we will find all trees in FOV. Note that we will not consider anything blocking trees as it is simpler
        //and somewhat realistic as trees are quite tall.
        foreach (var tree in GameModel.Trees)
        {
            if (GetDistance(tree.transform.position) <= GameModel.SquirrelViewDistance 
                && GetAngleOfSight(tree.transform.position) <= GameModel.SquirrelViewAngle)
            {
                treesInFOV.Add(tree);

                UpdateNearestTree(tree);
            }
        }

        //Next we will scan for garbage cans. For this we will allow other garbage cans and trees to block sight.
        foreach (var can in GameModel.GarbageCans)
        {
            if (GetDistance(can.transform.position) <= GameModel.SquirrelViewDistance 
                && GetAngleOfSight(can.transform.position) <= GameModel.SquirrelViewAngle)
            {
                cansInFOV.Add(can);
                UpdateGarbageCans(cansInFOV);
            }
        }
        
        //We will scan nuts based on trees in FOV
        foreach (var tree in treesInFOV)
        {
            foreach (var nut in tree.GetComponent<TreeController>().nuts)
            {
                if (GetDistance(nut.transform.position) <= GameModel.SquirrelViewDistance 
                    && GetAngleOfSight(nut.transform.position) <= GameModel.SquirrelViewAngle)
                {
                    nutsInFOV.Add(nut);

                    UpdateNuts(nutsInFOV);
                }
            }
        }
    }

    private float GetAngleOfSight(Vector3 p)
    {
        var cur = _squirrel.transform.position;
        return Vector3.Angle(_squirrel.transform.forward, p - cur);
    }

    private float GetDistance(Vector3 p)
    {
        var cur = _squirrel.transform.position;
        return Vector3.Distance(cur, p);
    }
}