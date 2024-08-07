using System.Collections.Generic;
using UnityEngine;

public class DuckManager : MonoBehaviour
{
    public GameObject duckPrefab; // The duck prefab to instantiate
    public Transform parentTransform; // Parent transform to organize ducks in the hierarchy
    private List<GameObject> ducks = new List<GameObject>();
    private bool isManagingDucks = false;

    private void Update()
    {
        // Check for duck management mode toggle
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            isManagingDucks = !isManagingDucks;
        }

        // Check for adding/removing ducks
        if (isManagingDucks)
        {
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                AddDuck();
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                RemoveDuck();
            }
        }
    }

    private void AddDuck()
    {
        GameObject newDuck = Instantiate(duckPrefab, GetRandomPosition(), Quaternion.identity, parentTransform);
        ducks.Add(newDuck);
    }

    private void RemoveDuck()
    {
        if (ducks.Count > 0)
        {
            GameObject duckToRemove = ducks[ducks.Count - 1];
            ducks.RemoveAt(ducks.Count - 1);
            Destroy(duckToRemove);
        }
    }

    private Vector2 GetRandomPosition()
    {
        // Adjust this to your scene setup
        return new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
    }
}
