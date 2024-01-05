using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GOAP
{
    [RequireComponent(typeof(SController))]
    public class TargetingSystem : MonoBehaviour
    {
        private SController _controller;

        [Header("Memory Sizes")]
        [SerializeField] private int nutMemorySize = 5;
        [SerializeField] private int garbageCanMemorySize = 2;
        [SerializeField] private int treeMemorySize = 1;
        
        [Header("Targets")]
        public Target homeTreeTarget; // Needs to be created on start
        [SerializeField] private List<Target> targetsInFOV = new List<Target>();
        [SerializeField] private List<Target> targetsInMemory = new List<Target>();
        
        private Vector3 curPos;
        private Vector3 curForward;
        
        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond); //Set random seed
            _controller = gameObject.GetComponent<SController>();
        }

        private void Start()
        {
            homeTreeTarget = new Target(_controller.homeTree, TargetType.HomeTree, TargetState.InMemory);
        }

        private void LateUpdate()
        {
            Dictionary<GameObject, Target> newTargetsInFOV = new Dictionary<GameObject, Target>();
            List<Target> newTargetsInMem = new List<Target>();

            int[] counts = {0, 0, 0};
            int[] maxMemSizes = new int[3];
            maxMemSizes[(int) TargetType.Nut] = nutMemorySize;
            maxMemSizes[(int) TargetType.GarbageCan] = garbageCanMemorySize;
            maxMemSizes[(int) TargetType.Tree] = treeMemorySize;
            
            curPos = transform.position;
            curForward = transform.forward;

            foreach (var target in targetsInFOV.Concat(targetsInMemory))
            {
                if (IsInFOV(target.location)) 
                {
                    if (target.obj == null) continue; // Skip collected nuts
                    newTargetsInFOV[target.obj] = target;
                    target.state = TargetState.InFOV;
                } 
                else if (counts[(int) target.type]++ < maxMemSizes[(int) target.type])
                {
                    newTargetsInMem.Add(target); //Add to end not front
                    target.state = TargetState.InMemory;
                } 
                else 
                {
                    target.state = TargetState.Forgotten;
                }
            }

            var objQuery = 
                from obj in GameModel.Nuts.Concat(GameModel.GarbageCans).Concat(GameModel.Trees)
                where !newTargetsInFOV.ContainsKey(obj) && IsInFOV(obj.transform.position)
                select obj;

            foreach (var obj in objQuery)
            {
                // TODO Maybe force a replan to happen here (in case better cost plan)
                newTargetsInFOV[obj] = new Target(obj);
            }
            
            targetsInFOV = newTargetsInFOV.Values.ToList();
            targetsInMemory = newTargetsInMem;
            
            //Update homeTree target
            homeTreeTarget.state = newTargetsInFOV.ContainsKey(homeTreeTarget.obj) ? TargetState.InFOV : TargetState.InMemory;
        }

        public Target GetRandomLocationTarget(float range, Vector2 fromLoc)
        {
            Vector3 source = new Vector3(fromLoc.x, GameModel.FloorHeight, fromLoc.y);
            Vector2 targetLoc = fromLoc;
            
            // TODO Maybe update so cannot choose target outside gameBounds
            source.x += Random.Range(-range, range);
            source.z += Random.Range(-range, range);

            if (NavMesh.SamplePosition(source, out var closestHit, 5f, NavMesh.AllAreas))
                targetLoc = closestHit.position.GetHorizVector2();
            
            return new Target(targetLoc);
        }
        
        public List<Target> GetTargetsOfType(TargetType type)
        {
            if (!Target.IsDetectable(type)) 
                return null;
            
            return new List<Target>(
                targetsInFOV
                    .Concat(targetsInMemory)
                    .Where(target => target.type == type));
        }
        
        private bool IsInFOV(Vector3 posToCheck)
        {
            var angle = Vector3.Angle(curForward, posToCheck - curPos);
            var distance = Vector3.Distance(curPos, posToCheck);
            return distance <= GameModel.SquirrelViewDistance
                   && angle <= GameModel.SquirrelViewAngle;
        }
        
    }
}