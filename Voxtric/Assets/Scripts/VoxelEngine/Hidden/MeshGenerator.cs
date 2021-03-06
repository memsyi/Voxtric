﻿using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VoxelEngine.MonoBehaviours;
using VoxelEngine.Hidden;

namespace VoxelEngine.Hidden
{
    public sealed class MeshGenerator
    {
        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uv = new List<Vector2>();
        private int _faceCount = 0;

        private void CubeEast(int x, int y, int z, byte blockID)
        {
            _vertices.Add(new Vector3(x + 1, y - 1, z));
            _vertices.Add(new Vector3(x + 1, y, z));
            _vertices.Add(new Vector3(x + 1, y, z + 1));
            _vertices.Add(new Vector3(x + 1, y - 1, z + 1));

            TextureDetails details = TextureFinder.TextureDetailsFor(blockID);
            Vector2 texturePosition = TextureFinder.AdjustForPosition(x, y, z, TextureFinder.TextureFace.East, details);
            Cube(texturePosition, true);
            _faceCount++;
        }

        private void CubeWest(int x, int y, int z, byte blockID)
        {
            _vertices.Add(new Vector3(x, y - 1, z + 1));
            _vertices.Add(new Vector3(x, y, z + 1));
            _vertices.Add(new Vector3(x, y, z));
            _vertices.Add(new Vector3(x, y - 1, z));

            TextureDetails details = TextureFinder.TextureDetailsFor(blockID);
            Vector2 texturePosition = TextureFinder.AdjustForPosition(x, y, z, TextureFinder.TextureFace.West, details);
            Cube(texturePosition, false);
            _faceCount++;
        }

        private void CubeTop(int x, int y, int z, byte blockID)
        {
            _vertices.Add(new Vector3(x, y, z + 1));
            _vertices.Add(new Vector3(x + 1, y, z + 1));
            _vertices.Add(new Vector3(x + 1, y, z));
            _vertices.Add(new Vector3(x, y, z));

            TextureDetails details = TextureFinder.TextureDetailsFor(blockID);
            Vector2 texturePosition = TextureFinder.AdjustForPosition(x, y, z, TextureFinder.TextureFace.Top, details);
            Cube(texturePosition, false);
            _faceCount++;
        }

        private void CubeBottom(int x, int y, int z, byte blockID)
        {
            _vertices.Add(new Vector3(x, y - 1, z));
            _vertices.Add(new Vector3(x + 1, y - 1, z));
            _vertices.Add(new Vector3(x + 1, y - 1, z + 1));
            _vertices.Add(new Vector3(x, y - 1, z + 1));

            TextureDetails details = TextureFinder.TextureDetailsFor(blockID);
            Vector2 texturePosition = TextureFinder.AdjustForPosition(x, y, z, TextureFinder.TextureFace.Bottom, details);
            Cube(texturePosition, true);
            _faceCount++;
        }

        private void CubeNorth(int x, int y, int z, byte blockID)
        {
            _vertices.Add(new Vector3(x + 1, y - 1, z + 1));
            _vertices.Add(new Vector3(x + 1, y, z + 1));
            _vertices.Add(new Vector3(x, y, z + 1));
            _vertices.Add(new Vector3(x, y - 1, z + 1));

            TextureDetails details = TextureFinder.TextureDetailsFor(blockID);
            Vector2 texturePosition = TextureFinder.AdjustForPosition(x, y, z, TextureFinder.TextureFace.North, details);
            Cube(texturePosition, false);
            _faceCount++;
        }

        private void CubeSouth(int x, int y, int z, byte blockID)
        {
            _vertices.Add(new Vector3(x, y - 1, z));
            _vertices.Add(new Vector3(x, y, z));
            _vertices.Add(new Vector3(x + 1, y, z));
            _vertices.Add(new Vector3(x + 1, y - 1, z));

            TextureDetails details = TextureFinder.TextureDetailsFor(blockID);
            Vector2 texturePosition = TextureFinder.AdjustForPosition(x, y, z, TextureFinder.TextureFace.South, details);
            Cube(texturePosition, true);
            _faceCount++;
        }

        private void Cube(Vector2 texturePos, bool flip)
        {
            _triangles.Add(_faceCount * 4);      //1
            _triangles.Add(_faceCount * 4 + 1);  //2
            _triangles.Add(_faceCount * 4 + 2);  //3

            _triangles.Add(_faceCount * 4);      //1
            _triangles.Add(_faceCount * 4 + 2);  //3
            _triangles.Add(_faceCount * 4 + 3);  //4

            float spacing = TextureFinder.TEXTURE_SPACING;
            if (flip)
            {
                _uv.Add(new Vector2(spacing * texturePos.x, spacing * texturePos.y));
                _uv.Add(new Vector2(spacing * texturePos.x, spacing * texturePos.y + spacing));
                _uv.Add(new Vector2(spacing * texturePos.x + spacing, spacing * texturePos.y + spacing));
                _uv.Add(new Vector2(spacing * texturePos.x + spacing, spacing * texturePos.y));
            }
            else
            {
                _uv.Add(new Vector2(spacing * texturePos.x + spacing, spacing * texturePos.y));
                _uv.Add(new Vector2(spacing * texturePos.x + spacing, spacing * texturePos.y + spacing));
                _uv.Add(new Vector2(spacing * texturePos.x, spacing * texturePos.y + spacing));
                _uv.Add(new Vector2(spacing * texturePos.x, spacing * texturePos.y));
            }
        }

        private void GenerateRegion(System.Object meshInfo)
        {
            lock (_vertices)
            {
                _vertices.Clear();
                _triangles.Clear();
                _uv.Clear();
                _faceCount = 0;
                MeshGeneratorInfo info = (MeshGeneratorInfo)meshInfo;
                for (int x = 0; x < VoxelData.SIZE; x++)
                {
                    for (int y = 0; y < VoxelData.SIZE; y++)
                    {
                        for (int z = 0; z < VoxelData.SIZE; z++)
                        {
                            Block block = info.region.GetBlock(x, y, z);
                            if (block.visible == 1)
                            {
                                if (x + 1 < VoxelData.SIZE)
                                {
                                    if (info.region.GetBlock(x + 1, y, z).visible == 0)
                                    {
                                        CubeEast(x, y, z, block.ID);
                                    }
                                }
                                else if (info.dataPosition.x < info.regionCollection.GetDimensions().x - 1)
                                {
                                    Region region = info.regionCollection.GetRegion(info.dataPosition.x + 1, info.dataPosition.y, info.dataPosition.z);
                                    if (!ReferenceEquals(region, null))
                                    {
                                        if (region.GetBlock(0, y, z).visible == 0)
                                        {
                                            CubeEast(x, y, z, block.ID);
                                        }
                                    }
                                }
                                if (x - 1 >= 0)
                                {
                                    if (info.region.GetBlock(x - 1, y, z).visible == 0)
                                    {
                                        CubeWest(x, y, z, block.ID);
                                    }
                                }
                                else if (info.dataPosition.x > 0)
                                {
                                    Region region = info.regionCollection.GetRegion(info.dataPosition.x - 1, info.dataPosition.y, info.dataPosition.z);
                                    if (!ReferenceEquals(region, null))
                                    {
                                        if (region.GetBlock(VoxelData.SIZE - 1, y, z).visible == 0)
                                        {
                                            CubeWest(x, y, z, block.ID);
                                        }
                                    }
                                }
                                if (y + 1 < VoxelData.SIZE)
                                {
                                    if (info.region.GetBlock(x, y + 1, z).visible == 0)
                                    {
                                        CubeTop(x, y, z, block.ID);
                                    }
                                }
                                else if (info.dataPosition.y < info.regionCollection.GetDimensions().y - 1)
                                {
                                    Region region = info.regionCollection.GetRegion(info.dataPosition.x, info.dataPosition.y + 1, info.dataPosition.z);
                                    if (!ReferenceEquals(region, null))
                                    {
                                        if (region.GetBlock(x, 0, z).visible == 0)
                                        {
                                            CubeTop(x, y, z, block.ID);
                                        }
                                    }
                                }
                                if (y - 1 >= 0)
                                {
                                    if (info.region.GetBlock(x, y - 1, z).visible == 0)
                                    {
                                        CubeBottom(x, y, z, block.ID);
                                    }
                                }
                                else if (info.dataPosition.y > 0)
                                {
                                    Region region = info.regionCollection.GetRegion(info.dataPosition.x, info.dataPosition.y - 1, info.dataPosition.z);
                                    if (!ReferenceEquals(region, null))
                                    {
                                        if (region.GetBlock(x, VoxelData.SIZE - 1, z).visible == 0)
                                        {
                                            CubeBottom(x, y, z, block.ID);
                                        }
                                    }
                                }
                                if (z + 1 < VoxelData.SIZE)
                                {
                                    if (info.region.GetBlock(x, y, z + 1).visible == 0)
                                    {
                                        CubeNorth(x, y, z, block.ID);
                                    }
                                }
                                else if (info.dataPosition.z < info.regionCollection.GetDimensions().z - 1)
                                {
                                    Region region = info.regionCollection.GetRegion(info.dataPosition.x, info.dataPosition.y, info.dataPosition.z + 1);
                                    if (!ReferenceEquals(region, null))
                                    {
                                        if (region.GetBlock(x, y, 0).visible == 0)
                                        {
                                            CubeNorth(x, y, z, block.ID);
                                        }
                                    }
                                }
                                if (z - 1 >= 0)
                                {
                                    if (info.region.GetBlock(x, y, z - 1).visible == 0)
                                    {
                                        CubeSouth(x, y, z, block.ID);
                                    }
                                }
                                else if (info.dataPosition.z > 0)
                                {
                                    Region region = info.regionCollection.GetRegion(info.dataPosition.x, info.dataPosition.y, info.dataPosition.z - 1);
                                    if (!ReferenceEquals(region, null))
                                    {
                                        if (region.GetBlock(x, y, VoxelData.SIZE - 1).visible == 0)
                                        {
                                            CubeSouth(x, y, z, block.ID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                info.region.SetMeshInformation(_vertices.ToArray(), _triangles.ToArray(), _uv.ToArray());
            }
        }

        public void GenerateMesh(MeshGeneratorInfo meshInfo)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateRegion), meshInfo);
        }
    }
}