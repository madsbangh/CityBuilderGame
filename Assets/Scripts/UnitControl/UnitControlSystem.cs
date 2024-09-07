using CodeMonkey.Utils;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct UnitSelection : IComponentData
{
}

public partial class UnitControlSystem : SystemBase
{
    private float3 startPosition;

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Mouse pressed
            startPosition = UtilsClass.GetMouseWorldPosition();
            SelectionAreaManager.Instance.SelectionArea.gameObject.SetActive(true);
            SelectionAreaManager.Instance.SelectionArea.position = startPosition;
        }

        if (Input.GetMouseButton(0))
        {
            // Mouse held down
            var selectionAreaSize = (float3)UtilsClass.GetMouseWorldPosition() - startPosition;
            SelectionAreaManager.Instance.SelectionArea.localScale = selectionAreaSize;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Mouse released
            float3 endPosition = UtilsClass.GetMouseWorldPosition();
            SelectionAreaManager.Instance.SelectionArea.gameObject.SetActive(false);

            var lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x), math.min(startPosition.y, endPosition.y), 0);
            var upperRightPosition = new float3(math.max(startPosition.x, endPosition.x), math.max(startPosition.y, endPosition.y), 0);

            bool selectOnlyOneEntity = false;
            var selectionAreaSize = math.distance(lowerLeftPosition, upperRightPosition);
            var selectionAreaMinSize = 2f;
            if (selectionAreaSize < selectionAreaMinSize)
            {
                lowerLeftPosition += new float3(-1, -1, 0) * (selectionAreaMinSize - selectionAreaSize) * 0.15f;
                upperRightPosition += new float3(1, 1, 0) * (selectionAreaMinSize - selectionAreaSize) * 0.15f;
                selectOnlyOneEntity = true;
            }


            var entityCommandBuffer = new EntityCommandBuffer(WorldUpdateAllocator);

            foreach (var (_,  entity) in SystemAPI.Query<RefRO<UnitSelection>>().WithEntityAccess())
            {
                entityCommandBuffer.RemoveComponent(entity, typeof(UnitSelection));
            }

            int selectedEntityCount = 0;
            foreach (var (localTransform,  entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithEntityAccess())
            {
                if (selectOnlyOneEntity && selectedEntityCount > 0)
                {
                    continue;
                }

                var entityPosition = localTransform.ValueRO.Position;
                if (entityPosition.x >= lowerLeftPosition.x &&
                    entityPosition.y >= lowerLeftPosition.y &&
                    entityPosition.x <= upperRightPosition.x &&
                    entityPosition.y <= upperRightPosition.y)
                {
                    entityCommandBuffer.AddComponent(entity, new UnitSelection());
                    selectedEntityCount++;
                }
            }

            entityCommandBuffer.Playback(EntityManager);
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (var (unitSelection, moveTo, entity) in SystemAPI.Query<RefRO<UnitSelection>, RefRW<MoveTo>>().WithEntityAccess())
            {
                moveTo.ValueRW.Position = UtilsClass.GetMouseWorldPosition();
                moveTo.ValueRW.Move = true;
            }
        }
    }
}