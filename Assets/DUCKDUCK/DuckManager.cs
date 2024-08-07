using System.Collections.Generic;
using UnityEngine;

public class DuckManager : MonoBehaviour
{
    public GameObject duckPrefab;
    public Transform parentTransform; 
    public List<GameObject> ducks = new List<GameObject>();
    public bool isManagingDucks = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            isManagingDucks = !isManagingDucks;
        }

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
        return new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
    }
}

