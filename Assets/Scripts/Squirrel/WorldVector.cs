using System.Collections.Generic;
using System.Linq;
using Tree;
using UnityEngine;

namespace Squirrel
{
    public struct WorldVector
    {
        private GameObject Player;
        private GameObject Squirrel; //Squirrel the world vector is attached to
        // public SquirrelMemory Memory;
    
        // Squirrel Memory
        public GameObject HomeTree;
        public GameObject NearestTree;
        public Queue<GameObject> NearestGarbageCans;
        public Queue<GameObject> NearestNuts;
    
        public Vector3 CurPosition;
        public GameObject CurGameObject; //Game object we are currently at. Null if not at any game object.
        public bool AtClimbableObject;
    
        public bool IsPlayerNear;

        public enum HeightState
        {
            TreeTop, GarbageCanTop, Floor, Neither
        }

        public HeightState HState;
    
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
        
            HState = HeightState.Floor;
        }

        public void UpdateState()
        {
            CurPosition = Squirrel.transform.position;
            UpdateMemory();
            UpdateCurGameObject();

            AtClimbableObject = CurGameObject != null &&
                                (CurGameObject.CompareTag("Tree") || CurGameObject.CompareTag("Garbage Can"));

            IsPlayerNear = (GetHorizDistance(Player.transform.position) <= GameModel.SquirrelViewDistance);

            if (Mathf.Approximately(CurPosition.y, GameModel.SquirrelHomeHeight)) HState = HeightState.TreeTop;
            else if (Mathf.Approximately(CurPosition.y, GameModel.GarbageCanHeight)) HState = HeightState.GarbageCanTop;
            else if (Mathf.Approximately(CurPosition.y, 0f)) HState = HeightState.Floor;
            else HState = HeightState.Neither;
            //Note when climbing on tree we may very briefly get HState of GarbageCanTop
        }
    
        private void UpdateCurGameObject()
        {
            var tolerance = 0.1f + GameModel.SquirrelRadius;

            //Check if still at curGameObject
            if (CurGameObject != null)
            {
                switch (CurGameObject.tag)
                {
                    case "Tree":
                        tolerance += GameModel.TreeTrunkRadius;
                        break;
                    case "Garbage Can":
                        tolerance += GameModel.GarbageCanRadius;
                        break;
                    case "Nut":
                        tolerance += GameModel.NutRadius;
                        break;
                }
                if (GetHorizDistance(CurGameObject.transform.position) <= tolerance) return;
            }
        
            //Check if at any gameObjects stored in memory
            if (GetHorizDistance(NearestTree.transform.position) <= tolerance + GameModel.TreeTrunkRadius)
            {
                CurGameObject = NearestTree;
                return;
            }

            foreach (var can in NearestGarbageCans)
            {
                if (GetHorizDistance(can.transform.position) <= tolerance + GameModel.GarbageCanRadius)
                {
                    CurGameObject = can;
                    return;
                }
            }
        
            foreach (var nut in NearestNuts)
            {
                //Not including squirrel radius here since squirrels can stand on nuts
                if (GetHorizDistance(nut.transform.position) <= 0.1f + GameModel.NutRadius) 
                {
                    CurGameObject = nut;
                    return;
                }
            }

            //If here then not near any gameObject
            CurGameObject = null; 
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
                if (GetHorizDistance(tree.transform.position) <= GameModel.SquirrelViewDistance 
                    && GetAngleOfSight(tree.transform.position) <= GameModel.SquirrelViewAngle)
                {
                    treesInFOV.Add(tree);

                    UpdateNearestTree(tree);
                }
            }

            //Next we will scan for garbage cans. For this we will allow other garbage cans and trees to block sight.
            foreach (var can in GameModel.GarbageCans)
            {
                if (GetHorizDistance(can.transform.position) <= GameModel.SquirrelViewDistance 
                    && GetAngleOfSight(can.transform.position) <= GameModel.SquirrelViewAngle)
                {
                    cansInFOV.Add(can);
                }
            }
            UpdateGarbageCans(cansInFOV);
        
            //We will scan nuts based on trees in FOV
            foreach (var tree in treesInFOV)
            {
                foreach (var nut in tree.GetComponent<TreeController>().nuts)
                {
                    if (GetHorizDistance(nut.transform.position) <= GameModel.SquirrelViewDistance 
                        && GetAngleOfSight(nut.transform.position) <= GameModel.SquirrelViewAngle)
                    {
                        nutsInFOV.Add(nut);

                    }
                }
            }
            UpdateNuts(nutsInFOV);
        }

        private bool UpdateNearestTree(GameObject tree)
        {
            var d1 = GetHorizDistance(NearestTree.transform.position);
            var d2 = GetHorizDistance(tree.transform.position);

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

        private float GetAngleOfSight(Vector3 p)
        {
            return Vector3.Angle(Squirrel.transform.forward, p - CurPosition);
        }

        private float GetHorizDistance(Vector3 p)
        {
            var a = new Vector2(CurPosition.x, CurPosition.z);
            var b = new Vector2(p.x, p.z);
            return Vector2.Distance(a, b);
        }
    
    }
}