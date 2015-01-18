﻿using UnityEngine;
using VoxelEngine.Hidden;
using System.Collections.Generic;
using System.IO;

namespace VoxelEngine.MonoBehaviours
{
    public sealed class RegionCollection : MonoBehaviour
    {
        public static List<RegionCollection> allCollections = new List<RegionCollection>();
        public static int totalLoadedRegions
        {
            get
            {
                int total = 0;
                foreach (RegionCollection regionCollection in allCollections)
                {
                    total += regionCollection._regionsLoaded;
                }
                return total;
            }
        }

        [SerializeField]
        private GameObject regionPrefab = null;
        private int _regionsLoaded = 0;

        Region[,,] _regions;
        private IntVec3 _dimensions;
        Transform _positionPointer;

        private Transform _concaveShapes;
        private Transform _convexShapes;

        public string collectionDirectory
        {
            get { return string.Format(@"{0}\Collections\{1}", ApplicationInitialiser.gameDirectory, name); }
        }

        private void Start()
        {
            Initialise(new IntVec3(3, 3, 3), "Test Region 1");
        }

        public void UnloadRegion(IntVec3 dataPosition)
        {
            Region region = GetRegion(dataPosition.x, dataPosition.y, dataPosition.z);
            _regions[dataPosition.x, dataPosition.y, dataPosition.z] = null;
            MonoBehaviour.Destroy(region.gameObject);
            _regionsLoaded--;
        }

        public void SaveAllVoxelData()
        {
            for (int x = 0; x < _dimensions.x; x++)
            {
                for (int y = 0; y < _dimensions.y; y++)
                {
                    for (int z = 0; z < _dimensions.z; z++)
                    {
                        Region region = GetRegion(x, y, z);
                        if (region != null)
                        {
                            region.SaveVoxelData(collectionDirectory);
                        }
                    }
                }
            }
        }

        public Region GetRegion(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= _dimensions.x || y >= _dimensions.y || z >= _dimensions.z)
            {
                Debug.LogError(string.Format("Region could not be retrieved: {0} is not a valid data position.", new IntVec3(x, y, z)));
                return null;
            }
            return _regions[x, y, z];
        }

        public IntVec3 GetDimensions()
        {
            return _dimensions;
        }

        public int GetRegionsLoaded()
        {
            return _regionsLoaded;
        }

        public Transform GetPositionPointer()
        {
            return _positionPointer;
        }

        public void Initialise(IntVec3 dimensions, string name)
        {
            if (GameObject.Find(name) != null)
            {
                Debug.LogError(string.Format("Region collection could not be created: {0} already exists.", name));
                MonoBehaviour.Destroy(gameObject);
                return;
            }
            transform.name = name;
            _dimensions = dimensions;
            _regions = new Region[dimensions.x, dimensions.y, dimensions.z];
            _concaveShapes = transform.GetChild(0);
            _convexShapes = transform.GetChild(1);
            _positionPointer = new GameObject("Position Pointer").GetComponent<Transform>();
            _positionPointer.parent = _concaveShapes;
            Directory.CreateDirectory(string.Format(@"{0}\Collections\{1}", ApplicationInitialiser.gameDirectory, name));
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int z = 0; z < dimensions.z; z++)
                    {
                        Region region = CreateRegion(x, y, z);
                        region.QueueMeshGeneration();
                    }
                }
            }
            allCollections.Add(this);
        }

        private void FixedUpdate()
        {
            _concaveShapes.rotation = _convexShapes.rotation;
            _concaveShapes.position = _convexShapes.position;
        }

        public Region CreateRegion(int x, int y, int z)
        {
            Region region = ((GameObject)Instantiate(regionPrefab)).GetComponent<Region>();
            region.Initialise(new IntVec3(x, y, z), this);
            _regionsLoaded++;
            _regions[x, y, z] = region;
            return region;
        }
    }
}