using Unity.Entities;
using UnityEngine;

// Empty components can be used to tag entities
public struct AppleTag : IComponentData
{
}

public struct PoisonAppleTag : IComponentData
{

}

public struct AppleBottomY : IComponentData
{
    // If you have only one field in a component, name it "Value"

    public float Value;
}

[DisallowMultipleComponent]
public class AppleAuthoring : MonoBehaviour
{
    [SerializeField] private float bottomY = -14f;
    [SerializeField] private bool isPoison;

    private class AppleAuthoringBaker : Baker<AppleAuthoring>
    {
        public override void Bake(AppleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

           
            AddComponent(entity, new AppleBottomY { Value = authoring.bottomY });

            if (authoring.isPoison)
            {
                AddComponent<PoisonAppleTag>(entity);
            }
            else
            {
                AddComponent<AppleTag>(entity);
            }
        }
    }
}
