using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DuckController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float moveSpeed = 5f;
    public AudioClip duckCall;
    public AudioClip quackQuack;
    public Vector2 minBoundary = new Vector2(-1600, -900);
    public Vector2 maxBoundary = new Vector2(1600, 900);
    public GameObject duckPrefab; // The duck prefab to instantiate
    public Transform parentTransform; // Parent transform to organize ducks in the hierarchy

    private Animator animator;
    private AudioSource audioSource;
    private bool isSleeping = false;
    private bool isDragging = false;
    private bool isJumping = false;
    private bool isManagingDucks = false;
    private List<GameObject> ducks = new List<GameObject>();

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(RandomMovement());
        StartCoroutine(DuckCallRoutine());
    }

    private void Update()
    {
        // Check for sleep input
        if (Input.GetKeyDown(KeyCode.S))
        {
            isSleeping = !isSleeping;
            animator.SetBool("IsSleeping", isSleeping);
        }

        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartCoroutine(Jump());
        }

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

    public void OnPointerDown(PointerEventData eventData)
    {
        // Start dragging and play quack quack sound
        isDragging = true;
        if (quackQuack != null && audioSource != null)
        {
            audioSource.PlayOneShot(quackQuack);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow the mouse while dragging
        if (isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clampedPosition = ClampPosition(mousePosition);
            transform.position = clampedPosition;

            if (animator != null)
            {
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveY", 0);
                animator.SetBool("IsWalking", false);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Stop dragging
        isDragging = false;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }

    private IEnumerator Jump()
    {
        isJumping = true;
        animator.SetTrigger("Jump");

        // Assuming the jump animation takes 1 second
        yield return new WaitForSeconds(1f);

        isJumping = false;
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
        return new Vector2(Random.Range(minBoundary.x, maxBoundary.x), Random.Range(minBoundary.y, maxBoundary.y));
    }

    private IEnumerator RandomMovement()
    {
        while (true)
        {
            if (!isSleeping && !isDragging)
            {
                float horizontal = Random.Range(-1f, 1f);
                float vertical = Random.Range(-1f, 1f);

                Vector2 direction = new Vector2(horizontal, vertical).normalized;

                float moveDuration = Random.Range(1f, 3f);
                float moveTime = 0f;

                while (moveTime < moveDuration)
                {
                    Vector2 newPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
                    transform.position = ClampPosition(newPosition);

                    if (animator != null)
                    {
                        animator.SetFloat("MoveX", direction.x);
                        animator.SetFloat("MoveY", direction.y);
                        animator.SetBool("IsWalking", true);
                    }

                    moveTime += Time.deltaTime;
                    yield return null;
                }

                // Stop the duck
                if (animator != null)
                {
                    animator.SetFloat("MoveX", 0);
                    animator.SetFloat("MoveY", 0);
                    animator.SetBool("IsWalking", false);
                }

                // Stop for a random duration
                float stopDuration = Random.Range(1f, 3f);
                yield return new WaitForSeconds(stopDuration);
            }
            else
            {
                // If sleeping or dragging, wait for a short duration before checking again
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator DuckCallRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            if (!isSleeping && !isDragging && duckCall != null && audioSource != null)
            {
                audioSource.PlayOneShot(duckCall);
            }
        }
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        // Clamp the position to stay within the defined boundaries
        float clampedX = Mathf.Clamp(position.x, minBoundary.x, maxBoundary.x);
        float clampedY = Mathf.Clamp(position.y, minBoundary.y, maxBoundary.y);
        return new Vector2(clampedX, clampedY);
    }
}
