using UnityEngine;

public class ComponentFactory<T> : IFactory<T> where T : Component
{
    public string name => typeof(T).ToString();
    private Transform parentTransform;

    public ComponentFactory(Transform parentTransform = null) 
    {
        this.parentTransform = parentTransform;
    }
    
    public override T Create()
    {
        return Create(parentTransform);
    }

    public T Create(Transform parent)
    {
        if (parent == null)
        {
            parent = new GameObject().transform;
            parent.name = name;
        }

        return parent.gameObject.AddComponent<T>();
    }
}