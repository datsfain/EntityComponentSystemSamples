using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public float MoveSpeed = 1f;

        public class CubesBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new EnemyMovement
                {
                    InitialPos = authoring.transform.position,
                    MoveSpeed = authoring.MoveSpeed
                });
            }
        }
    }

    public readonly partial struct EnemyMovementAspect : IAspect
    {
        public readonly RefRO<EnemyMovement> EnemyMovement;
        public readonly RefRW<LocalTransform> Transform;

        public void Move(float gameTime)
        {
            var movement = new float3(EnemyMovement.ValueRO.MoveSpeed * math.sin(gameTime), 0f, 0f);
            Transform.ValueRW.Position = EnemyMovement.ValueRO.InitialPos + movement;
        }
    }

    public struct EnemyMovement : IComponentData
    {
        public float3 InitialPos;
        public float MoveSpeed;
    }

    public partial struct EnemyMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var totalTime = (float)state.WorldUnmanaged.Time.ElapsedTime;

            foreach (var rotating in SystemAPI.Query<EnemyMovementAspect>())
            {
                rotating.Move(totalTime);
            }
        }
    }


}