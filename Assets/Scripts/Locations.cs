using System.Collections.Generic;
using UnityEngine;

public class Locations : MonoBehaviour
{
    public List<Transform> locations = new List<Transform>();

    private void Awake()
    {
        locations.Clear();
        locations.AddRange(GetComponentsInChildren<Transform>());
        locations.Remove(transform);
    }

}
