using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct AppleMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var time = (float)state.WorldUnmanaged.Time.ElapsedTime;

        foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<AppleTag>())
        {
            var position = transform.ValueRW.Position;

            // Set the amplitude and frequency for the zigzag pattern
            float amplitude = 0.5f;  // Controls how far left and right the apple moves
            float frequency = 1f;    // Controls the speed of the zigzag motion

            // Apply zigzag movement on the X-axis while the apple moves down the Y-axis
            position.x += math.sin(time * frequency) * amplitude;
            position.y -= 0.5f; 

            // Update the position in the transform
            transform.ValueRW.Position = position;
        }
    }
}
