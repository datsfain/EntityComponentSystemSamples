using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Game
{
    public struct Bullet : IComponentData
    {
        public float Speed;
        public float RemainingTime;
    }

    public partial struct BulletSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (bullet, transform, entity) in SystemAPI
                .Query<RefRW<Bullet>, RefRW<LocalTransform>>()
                .WithEntityAccess())
            {
                transform.ValueRW = transform.ValueRO.Translate(transform.ValueRO.Forward() * bullet.ValueRO.Speed * dt);

                if (bullet.ValueRO.RemainingTime <= 0f)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                bullet.ValueRW.RemainingTime -= SystemAPI.Time.DeltaTime;
            }
        }
    }
}