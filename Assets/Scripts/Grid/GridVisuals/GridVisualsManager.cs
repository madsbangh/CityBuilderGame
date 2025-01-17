﻿using Unity.Entities;
using UnityEngine;

public class GridVisualsManager : MonoBehaviour
{
    [SerializeField] private MeshFilter _pathMeshFilter;
    [SerializeField] private MeshFilter _pathDebugMeshFilter;
    [SerializeField] private MeshFilter _treeMeshFilter;
    [SerializeField] private MeshFilter _interactableDebugMeshFilter;
    [SerializeField] private MeshFilter _healthBarMeshFilter;
    [SerializeField] private MeshFilter _occupationDebugMeshFilter;

    private readonly PathGridVisual _pathGridVisual = new();
    private readonly PathGridDebugVisual _pathGridDebugVisual = new();
    private readonly TreeGridVisual _treeGridVisual = new();
    private readonly InteractableGridDebugVisual _interactableGridDebugVisual = new();
    private readonly HealthbarGridVisual _healthbarGridVisual = new();
    private readonly OccupationDebugGridVisual _occupationDebugGridVisual = new();

    private bool _hasUpdatedOnce;

    private SystemHandle _gridManagerSystemHandle;

    private void Awake()
    {
        _pathMeshFilter.mesh = _pathGridVisual.CreateMesh();
        _pathDebugMeshFilter.mesh = _pathGridDebugVisual.CreateMesh();
        _treeMeshFilter.mesh = _treeGridVisual.CreateMesh();
        _interactableDebugMeshFilter.mesh = _interactableGridDebugVisual.CreateMesh();
        _healthBarMeshFilter.mesh = _healthbarGridVisual.CreateMesh();
        _occupationDebugMeshFilter.mesh = _occupationDebugGridVisual.CreateMesh();
    }

    private void LateUpdate()
    {
        // HACK: This is done in late-update to make sure, the GridManagerSystem has been created. Not sure, if it's necessary though...
        if (_gridManagerSystemHandle == default)
        {
            _gridManagerSystemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<GridManagerSystem>();
            var gridManagerTemp = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<GridManager>(_gridManagerSystemHandle);

            var gridSize = gridManagerTemp.WalkableGrid.Length;
            _pathGridVisual.InitializeMesh(gridSize);
            _pathGridDebugVisual.InitializeMesh(gridSize);
            _treeGridVisual.InitializeMesh(gridSize);
            _interactableGridDebugVisual.InitializeMesh(gridSize);
            _healthbarGridVisual.InitializeMesh(gridSize);
            _occupationDebugGridVisual.InitializeMesh(gridSize);
        }

        var wasDirty = false;
        var gridManager = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<GridManager>(_gridManagerSystemHandle);

        TryUpdateWalkableGridVisuals(ref gridManager, ref wasDirty);
        TryUpdateDamageableGridVisuals(ref gridManager, ref wasDirty);
        TryUpdateOccupiableGridVisuals(ref gridManager, ref wasDirty);
        TryUpdateInteractableGridVisuals(ref gridManager, ref wasDirty);

        if (wasDirty)
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(_gridManagerSystemHandle, gridManager);
        }

        _hasUpdatedOnce = true;
    }


    private void TryUpdateWalkableGridVisuals(ref GridManager gridManager, ref bool wasDirty)
    {
        var showDebug = DebugGlobals.ShowWalkableGrid();
        _pathDebugMeshFilter.gameObject.SetActive(showDebug);
        _pathMeshFilter.gameObject.SetActive(!showDebug);

        if (gridManager.WalkableGridIsDirty)
        {
            gridManager.WalkableGridIsDirty = false;
            wasDirty = true;

            if (!_hasUpdatedOnce)
            {
                // This visual is a static background:
                _pathGridVisual.UpdateVisual(gridManager);
            }

            if (showDebug)
            {
                _pathGridDebugVisual.UpdateVisual(gridManager);
            }
        }
    }

    private void TryUpdateDamageableGridVisuals(ref GridManager gridManager, ref bool wasDirty)
    {
        if (gridManager.DamageableGridIsDirty)
        {
            gridManager.DamageableGridIsDirty = false;
            wasDirty = true;

            _treeGridVisual.UpdateVisual(gridManager);
            _healthbarGridVisual.UpdateVisual(gridManager);
        }
    }

    private void TryUpdateOccupiableGridVisuals(ref GridManager gridManager, ref bool wasDirty)
    {
        var showDebug = DebugGlobals.ShowOccupationGrid();
        _occupationDebugMeshFilter.gameObject.SetActive(showDebug);

        if (gridManager.OccupiableGridIsDirty)
        {
            gridManager.OccupiableGridIsDirty = false;
            wasDirty = true;

            if (showDebug)
            {
                _occupationDebugGridVisual.UpdateVisual(gridManager);
            }
        }
    }

    private void TryUpdateInteractableGridVisuals(ref GridManager gridManager, ref bool wasDirty)
    {
        var showDebug = DebugGlobals.ShowInteractableGrid();
        _interactableDebugMeshFilter.gameObject.SetActive(showDebug);
        if (showDebug)
        {
            _interactableGridDebugVisual.UpdateVisual(gridManager);
        }
    }
}