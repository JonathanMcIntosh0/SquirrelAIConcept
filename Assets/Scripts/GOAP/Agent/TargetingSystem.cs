using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tree;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GOAP.Agent
{
    [RequireComponent(typeof(SController))]
    public class TargetingSystem : MonoBehaviour
    {
        private Transform _transform; // Cache transform for quick access
        public TreeController homeTreeController; // Gets set by GameGenerator
        private PlayerController _playerController;

        [Header("Memory Settings")]
        [SerializeField] private int nutMemorySize = 5;
        [SerializeField] private int garbageCanMemorySize = 2;
        [SerializeField] private int treeMemorySize = 1;
        [SerializeField] private float maxMemoryTimer = 15f; // Max time in seconds we can store a target in memory
        
        [Header("Targets")]
        //TODO Maybe change homeTreeTarget get so we do not return if (inFOV && !isUsable)
        [SerializeField] public Target homeTreeTarget; // Needs to be created on start
        [SerializeField] private List<Target> targetsInFOV = new List<Target>();
        [SerializeField] private List<Target> targetsInMemory = new List<Target>();
        
        private Vector3 _curPos;
        private Vector3 _curForward;
        
        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond); //Set random seed
            _transform = transform;
            _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        private void Start()
        {
            homeTreeTarget = new Target(homeTreeController, TargetType.HomeTree, TargetState.InMemory);
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
                    target.memoryTimer = 0f;
                    // skip collected nuts (don't skip trees or cans)
                    if (target.objController == null) continue; 
                    newTargetsInFOV.Add(target);
                } 
                else if (counts[(int) target.type] < maxMemSizes[(int) target.type] 
                         && target.memoryTimer < maxMemoryTimer
                    // do not move target into memory from targetsInFOV if !usable
                         && !(target.state == TargetState.InFOV && !target.IsUsable())) 
                {
                    counts[(int) target.type]++;
                    target.state = TargetState.InMemory;
                    target.memoryTimer += Time.deltaTime;
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

            var objControllersVisited = 
                targetsInFOV.Concat(targetsInMemory).Select(target => target.objController);

            newTargetsInFOV.AddRange(
                from objController in GameModel.Nuts
                    .Concat<MonoBehaviour>(GameModel.GarbageCans)
                    .Concat<MonoBehaviour>(GameModel.Trees)
                    .Except<MonoBehaviour>(objControllersVisited)
                where IsInFOV(objController.transform.position.GetHorizVector2())
                select new Target(objController, TargetState.InFOV));

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
        
        public Target GetRunAwayTarget(float distance, Vector2 fromLoc)
        {
            Vector3 source = new Vector3(fromLoc.x, GameModel.FloorHeight, fromLoc.y);
            Vector2 targetLoc = fromLoc;
            
            source += distance * (source - _playerController.transform.position).normalized;

            if (NavMesh.SamplePosition(source, out var closestHit, 5f, NavMesh.AllAreas))
                targetLoc = closestHit.position.GetHorizVector2();
            
            return new Target(targetLoc);
        }
        
        [NotNull] public List<Target> GetTargetsOfType(TargetType type)
        {
            if (!Target.IsDetectable(type)) // Safety check
                return new List<Target>();
            
            return new List<Target>(
                targetsInFOV
                    // .Where(target => target.IsUsable()) // Filter out targets in FOV that are not usable
                    .Concat(targetsInMemory)
                    .Where(target => target.type == type));
        }

        public int GetTargetCountOfType(TargetType type)
        {
            return !Target.IsDetectable(type) ? 0 
                : targetsInFOV.Concat(targetsInMemory).Count(target => target.type == type);
        }
        
        private bool IsInFOV(Vector2 posToCheck)
        {
            var pos = new Vector3(posToCheck.x, GameModel.FloorHeight, posToCheck.y);
            // var angle = Vector3.Angle(_curForward, pos - _curPos);
            var distance = Vector3.Distance(_curPos, pos);
            return distance <= GameModel.SquirrelViewDistance;
            
            // Allow close objects (within distance <= 0.5f) to be "seen"
            // return distance <= 0.5f || 
            //        distance <= GameModel.SquirrelViewDistance
            //        && angle <= GameModel.SquirrelViewAngle;
        }
        
    }
}