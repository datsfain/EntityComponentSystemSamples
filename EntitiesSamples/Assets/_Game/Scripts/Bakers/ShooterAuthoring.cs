using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game
{
    public static class MathUtility
    {
        public static float3 GetMouseWorldPosition()
        {
            var cam = Camera.main;
            var mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane + 5f;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
    }

    public class ShooterAuthoring : MonoBehaviour
    {
        [Header("Shooting")]
        public GameObject BulletPrefab;
        public float BulletCooldown;
        public float BulletSpeed;
        public float BulletLifetime;


        public class Baker : Baker<ShooterAuthoring>
        {
            public override void Bake(ShooterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var prefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Shooter
                {
                    BulletPrefab = prefab,
                    BulletCooldown = authoring.BulletCooldown,
                    BulletSpeed = authoring.BulletSpeed,
                    BulletLifetime = authoring.BulletLifetime
                });
            }
        }
    }

    public struct Shooter : IComponentData
    {
        public Entity BulletPrefab;
        public float BulletCooldown;
        public float RemainingCooldown;
        public float BulletSpeed;
        public float BulletLifetime;
    }

    public partial class ShooterSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Shooter>();
        }

        protected override void OnUpdate()
        {
            var shooter = SystemAPI.GetSingletonRW<Shooter>();

            shooter.ValueRW.RemainingCooldown -= SystemAPI.Time.DeltaTime;
            if (shooter.ValueRW.RemainingCooldown > 0f) return;
            shooter.ValueRW.RemainingCooldown += shooter.ValueRO.BulletCooldown;

            if (Input.GetMouseButton(0))
            {
                var pos = MathUtility.GetMouseWorldPosition();
                SpawnBullet(shooter.ValueRO.BulletPrefab, pos, Camera.main.transform.forward, ref shooter);
            }
        }

        private void SpawnBullet(Entity prefab, Vector3 pos, Vector3 forward, ref RefRW<Shooter> shooter)
        {
            var instance = EntityManager.Instantiate(prefab);
            var rot = quaternion.LookRotation(forward, Vector3.up);

            var localTransform = EntityManager.GetComponentData<LocalTransform>(instance);
            localTransform.Position = pos;
            localTransform.Rotation = rot;
            EntityManager.SetComponentData(instance, localTransform);

            var bullet = new Bullet();
            bullet.Speed = shooter.ValueRO.BulletSpeed;
            bullet.RemainingTime = shooter.ValueRO.BulletLifetime;
            EntityManager.AddComponentData(instance, bullet);
        }
    }
}