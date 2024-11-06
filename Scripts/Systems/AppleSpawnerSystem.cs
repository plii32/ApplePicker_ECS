using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;


[UpdateAfter(typeof(TimerSystem))]
public partial struct AppleSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));


        // new SpawnJob { ECB = ecb }.Schedule();
        new SpawnJob { ECB = ecb, Random = random }.Schedule();
    }

    [BurstCompile]
    private partial struct SpawnJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public Unity.Mathematics.Random Random;

        private void Execute(in LocalTransform transform, in AppleSpawner spawner, ref Timer timer)
        {
            if (timer.Value > 0)
                return;

            timer.Value = spawner.Interval;

            // bool spawnPoison = UnityEngine.Random.value < 0.3f;
            bool spawnPoison = Random.NextFloat() < 0.1f;

            var poisonEntity = spawnPoison ? spawner.PoisonPrefab : spawner.Prefab;
            //var appleEntity = ECB.Instantiate(spawner.Prefab);
            var appleEntity = ECB.Instantiate(poisonEntity);
            ECB.SetComponent(appleEntity, LocalTransform.FromPosition(transform.Position));
        }
    }
}
