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
        private Transform _transform;

        [Header("Memory Sizes")]
        [SerializeField] private int nutMemorySize = 5;
        [SerializeField] private int garbageCanMemorySize = 2;
        [SerializeField] private int treeMemorySize = 1;
        
        [Header("Targets")]
        //TODO Maybe change homeTreeTarget get so we do not return if (inFOV && !isUsable)
        public Target homeTreeTarget; // Needs to be created on start
        [SerializeField] private List<Target> targetsInFOV = new List<Target>();
        [SerializeField] private List<Target> targetsInMemory = new List<Target>();
        
        private Vector3 _curPos;
        private Vector3 _curForward;
        
        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond); //Set random seed
            _controller = gameObject.GetComponent<SController>();
            _transform = transform;
        }

        private void Start()
        {
            homeTreeTarget = new Target(_controller.homeTree, TargetType.HomeTree, TargetState.InMemory);
        }

        private void LateUpdate()
        {
            // Dictionary<GameObject, Target> visitedObjects = new Dictionary<GameObject, Target>();
            List<Target> newTargetsInFOV = new List<Target>();
            List<Target> newTargetsInMem = new List<Target>();

            int[] counts = {0, 0, 0};
            int[] maxMemSizes = new int[3];
            maxMemSizes[(int) TargetType.Nut] = nutMemorySize;
            maxMemSizes[(int) TargetType.GarbageCan] = garbageCanMemorySize;
            maxMemSizes[(int) TargetType.Tree] = treeMemorySize;
            
            _curPos = _transform.position;
            _curForward = _transform.forward;

            foreach (var target in targetsInFOV.Concat(targetsInMemory))
            {
                if (IsInFOV(target.location))
                {
                    target.state = TargetState.InFOV;
                    if (target.obj == null) continue; // skip collected nuts
                    newTargetsInFOV.Add(target);
                } 
                else if (counts[(int) target.type] < maxMemSizes[(int) target.type] 
                    // do not move target into memory from targetsInFOV if !usable
                         && !(target.state == TargetState.InFOV && !target.IsUsable())) 
                {
                    counts[(int) target.type]++;
                    target.state = TargetState.InMemory;
                    newTargetsInMem.Add(target); //Add to end not front
                } 
                else 
                {
                    target.state = TargetState.Forgotten;
                }
            }

            // var objQuery = 
            //     from obj in GameModel.Nuts.Concat(GameModel.GarbageCans).Concat(GameModel.Trees)
            //     where !visitedObjects.ContainsKey(obj) && IsInFOV(obj.transform.position)
            //     select obj;

            var objsVisited = targetsInFOV.Concat(targetsInMemory).Select(target => target.obj);

            newTargetsInFOV.AddRange(
                from obj in GameModel.Nuts
                    .Concat(GameModel.GarbageCans)
                    .Concat(GameModel.Trees)
                    .Except(objsVisited)
                where IsInFOV(obj.transform.position)
                select new Target(obj, TargetState.InFOV));

            targetsInFOV = newTargetsInFOV;
            targetsInMemory = newTargetsInMem;
            
            //Update homeTree target
            homeTreeTarget.state = IsInFOV(homeTreeTarget.location) ? TargetState.InFOV : TargetState.InMemory;
        }

        public Target GetRandomLocationTarget(float range, Vector2 fromLoc)
        {
            if (range < 0) throw new ArgumentOutOfRangeException(nameof(range)); // Safety check
            Vector3 source = new Vector3(fromLoc.x, GameModel.FloorHeight, fromLoc.y);
            Vector2 targetLoc = fromLoc;
            
            // minX <= x - rangeLow    <=> rangeLow <= x - minX
            // x + rangeHigh <= maxX   <=> rangeHigh <= maxX - x
            var xRangeLow = Mathf.Min(range, source.x - GameModel.MinX);
            var xRangeHigh = Mathf.Min(range, GameModel.MaxX - source.x);
            var zRangeLow = Mathf.Min(range, source.z - GameModel.MinZ);
            var zRangeHigh = Mathf.Min(range, GameModel.MaxZ - source.z);
            
            // x -> [x - rangeLow, x + rangeHigh]
            source.x += Random.Range(-xRangeLow, xRangeHigh);
            source.z += Random.Range(-zRangeLow, zRangeHigh);

            if (NavMesh.SamplePosition(source, out var closestHit, 5f, NavMesh.AllAreas))
                targetLoc = closestHit.position.GetHorizVector2();
            
            return new Target(targetLoc);
        }
        
        //TODO maybe add capability to return homeTreeTarget
        public List<Target> GetTargetsOfType(TargetType type)
        {
            if (!Target.IsDetectable(type)) // Safety check
                return null;
            
            return new List<Target>(
                targetsInFOV
                    .Where(target => target.IsUsable()) // Filter out targets in FOV that are not usable
                    .Concat(targetsInMemory)
                    .Where(target => target.type == type));
        }
        
        private bool IsInFOV(Vector3 posToCheck)
        {
            var angle = Vector3.Angle(_curForward, posToCheck - _curPos);
            var distance = Vector3.Distance(_curPos, posToCheck);
            return distance <= GameModel.SquirrelViewDistance
                   && angle <= GameModel.SquirrelViewAngle;
        }
        
    }
}