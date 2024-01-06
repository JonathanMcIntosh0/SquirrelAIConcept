using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GarbageCan;
using Tree;
using UnityEngine;

namespace Squirrel
{
    public struct WorldVector
    {
        private GameObject Player;
        private GameObject Squirrel;

        /* --------------------------------------- *
         *          WORLD STATE COMPONENTS
         * --------------------------------------- */
        
        // Squirrel Memory (When simulating/path planing we will not update these 
        public GameObject HomeTree;
        public GameObject NearestTree;
        public Queue<GameObject> NearestGarbageCans;
        public Queue<GameObject> NearestNuts;
    
        // Location related
        public Vector3 CurPosition;
        public GameObject CurGameObject; //Game object we are currently at. Null if not at any game object.
        
        // Flags
        public bool AtClimbableObject; //TODO should be helper function (just a wrapper for curGameObject)
        public bool IsPlayerNear;
        public bool IsStuck; //Can be set when Investigating a garbage bin.
                             //Should not replan when true since we cannot act on it until after.
                             //TODO should be helper function (just a wrapper for curGameObject)
        public bool IsHiding; //Set to true when action plan is to hide.
                              //This indicates to ignore IsPlayerNear when checking preconditions of certain actions.
                              //TODO may remove/replace with a component indicating current subgoal

        /* --------------------------------------- *
         *          WORLD STATE FUNCTIONS
         * --------------------------------------- */
        
        // Constructor
        public WorldVector(GameObject player, GameObject squirrel, GameObject homeTree)
        {
            Player = player;
            Squirrel = squirrel;
        
            HomeTree = homeTree;
            NearestTree = homeTree;
            NearestGarbageCans = new Queue<GameObject>(2);
            NearestNuts = new Queue<GameObject>(5);

            CurPosition = squirrel.transform.position;
            CurGameObject = homeTree;
            AtClimbableObject = true;

            IsPlayerNear = false;
            IsStuck = false;
            IsHiding = false;
        }

        // Called every frame to update world state
        public void UpdateState()
        {
            CurPosition = Squirrel.transform.position;
            UpdateMemory();
            CurGameObject = GetCurGameObject();
            
            AtClimbableObject = CurGameObject != null && 
                                (CurGameObject.CompareTag("Tree") || CurGameObject.CompareTag("Garbage Can"));

            IsPlayerNear = Player.transform.GetHorizDistance(CurPosition) <= GameModel.SquirrelViewDistance;

            IsStuck = CheckIfStuck();
        }

        //Note that OnFloor is simply a wrapper for CurPosition (i.e. not needed in world vector, but more of a helper method)
        // public bool OnFloor => Mathf.Approximately(CurPosition.y, GameModel.FloorHeight); 
        
        public bool AtHeight(float y) => Mathf.Approximately(CurPosition.y, y);
        
        /* ---------------------------------- *
         *      PRIVATE HELPER FUNCTIONS
         * ---------------------------------- */

        private bool CheckIfStuck()
        {
            return CurGameObject != null &&
                   CurGameObject.CompareTag("Garbage Can") &&
                   AtHeight(GameModel.GarbageCanHeight) &&
                   CurGameObject.GetComponent<GarbageCanController>().State == State.Trap;
        }
        
        
        private GameObject GetCurGameObject()
        {
            const float tolerance = 0.1f;
            
            GameObject nearestObj = null;
            var nearestDist = tolerance;

            //TODO could also check homeTree (should be nearest tree though if at homeTree so probably no need)
            var listToCheck = new List<GameObject>();
            listToCheck.Add(CurGameObject);
            listToCheck.Add(NearestTree);
            listToCheck.AddRange(NearestGarbageCans);
            listToCheck.AddRange(NearestNuts);

            foreach (var obj in listToCheck)
            {
                if (obj == null) continue;
                
                var d = GetHorizDistanceFromSquirrel(obj);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearestObj = obj;
                }
            }

            return nearestObj;
        }

        //TODO update descriptions
        private void UpdateMemory()
        {
            //Find objects in field of view of squirrel
            var treesInFOV = new List<GameObject>();
            var cansInFOV = new List<GameObject>();
            var nutsInFOV = new List<GameObject>();
            //First we will find all trees in FOV. Note that we will not consider anything blocking trees as it is simpler
            //and somewhat realistic as trees are quite tall.
            foreach (var tree in GameModel.Trees)
            {
                if (Squirrel.transform.GetHorizDistance(tree.transform.position) <= GameModel.SquirrelViewDistance 
                    && Squirrel.transform.GetAngleOfSight(tree.transform.position) <= GameModel.SquirrelViewAngle)
                {
                    treesInFOV.Add(tree);

                    UpdateNearestTree(tree);
                }
            }

            //Next we will scan for garbage cans. For this we will allow other garbage cans and trees to block sight.
            foreach (var can in GameModel.GarbageCans)
            {
                if (Squirrel.transform.GetHorizDistance(can.transform.position) <= GameModel.SquirrelViewDistance 
                    && Squirrel.transform.GetAngleOfSight(can.transform.position) <= GameModel.SquirrelViewAngle)
                {
                    cansInFOV.Add(can);
                }
            }
            UpdateGarbageCans(cansInFOV);
        
            //We will scan nuts based on trees in FOV
            foreach (var tree in treesInFOV)
            {
                // foreach (var nut in tree.GetComponent<TreeController>().nuts)
                // {
                //     if (Squirrel.transform.GetHorizDistance(nut.transform.position) <= GameModel.SquirrelViewDistance 
                //         && Squirrel.transform.GetAngleOfSight(nut.transform.position) <= GameModel.SquirrelViewAngle)
                //     {
                //         nutsInFOV.Add(nut);
                //
                //     }
                // }
            }
            UpdateNuts(nutsInFOV);
            
            //TODO Scan nuts added by player
        }

        private bool UpdateNearestTree(GameObject tree)
        {
            var d1 = NearestTree.transform.GetHorizDistance(CurPosition);
            var d2 = tree.transform.GetHorizDistance(CurPosition);

            if (d2 < d1) NearestTree = tree;
            return d2 < d1;
        }

        private void UpdateGarbageCans(List<GameObject> cans)
        {
            //Remove duplicate elements then add new cans
            NearestGarbageCans = new Queue<GameObject>(NearestGarbageCans.Except(cans).Concat(cans));
            while (NearestGarbageCans.Count > 2) NearestGarbageCans.Dequeue(); //Dequeue until 2 left
        }

        private void UpdateNuts(List<GameObject> nuts)
        {
            //Remove duplicate elements then add new cans
            NearestNuts = new Queue<GameObject>(NearestNuts.Except(nuts).Concat(nuts));
            while (NearestNuts.Count > 5) NearestNuts.Dequeue(); //Dequeue until 5 left
        }
        

        private float GetHorizDistanceFromSquirrel(GameObject obj)
        {
            if (obj == null) return Mathf.Infinity;
            var d = obj.transform.GetHorizDistance(CurPosition);

            if (obj.CompareTag("Nut")) return d;
            
            d -= GameModel.SquirrelRadius;
            
            if (obj.CompareTag("Tree")) return d - GameModel.TreeTrunkRadius;
            if (obj.CompareTag("Garbage Can")) return  d - GameModel.GarbageCanRadius;
            
            return Mathf.Infinity;
        }
    
    }
}