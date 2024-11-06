using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct CollectAppleSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerScore>();
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var appleCount = new NativeArray<byte>(1, Allocator.TempJob);
        //var appleCount = new NativeArray<int>(1, Allocator.TempJob);
        var poisonAppleCount = new NativeArray<byte>(1, Allocator.TempJob); // Added poison apple count

        state.Dependency = new CollisionJob
        {
            AppleLookup = SystemAPI.GetComponentLookup<AppleTag>(true),
            BasketLookup = SystemAPI.GetComponentLookup<BasketTag>(true),
            PoisonAppleLookup = SystemAPI.GetComponentLookup<PoisonAppleTag>(true),
            ECB = ecb,
            AppleCount = appleCount,
            PoisonAppleCount = poisonAppleCount // Pass poison apple count
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        state.Dependency.Complete();

        var playerScore = SystemAPI.GetSingleton<PlayerScore>();
        playerScore.Value += (appleCount[0] * 100); // Update score
        if(poisonAppleCount != null)
        {
            playerScore.Value -= (poisonAppleCount[0] * 100);
        }
        SystemAPI.SetSingleton(playerScore);
        // Dispose of NativeArrays after use
        appleCount.Dispose();
        poisonAppleCount.Dispose();
    }


    [BurstCompile]
    private struct CollisionJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentLookup<AppleTag> AppleLookup;
        [ReadOnly] public ComponentLookup<BasketTag> BasketLookup;
        [ReadOnly] public ComponentLookup<PoisonAppleTag> PoisonAppleLookup; // Added poison apple lookup

        public EntityCommandBuffer ECB;
        public NativeArray<byte> AppleCount;
        public NativeArray<byte> PoisonAppleCount; // Added poison apple count

        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA; // basket
            var entityB = collisionEvent.EntityB; // apple

            if (AppleLookup.HasComponent(entityA) && BasketLookup.HasComponent(entityB))
            {
                ECB.DestroyEntity(entityA);
                AppleCount[0] = 1; // Count normal apple
            }
            else if (AppleLookup.HasComponent(entityB) && BasketLookup.HasComponent(entityA))
            {
                ECB.DestroyEntity(entityB);
                AppleCount[0] = 1; // Count normal apple
            }

            // Check for poison apple collisions
            if (PoisonAppleLookup.HasComponent(entityA) && BasketLookup.HasComponent(entityB))
            {
                ECB.DestroyEntity(entityA);
                PoisonAppleCount[0] = 1; // Count poison apple
            }
            else if (PoisonAppleLookup.HasComponent(entityB) && BasketLookup.HasComponent(entityA))
            {
                ECB.DestroyEntity(entityB);
                PoisonAppleCount[0] = 1; // Count poison apple
            }
        }
    }
}
