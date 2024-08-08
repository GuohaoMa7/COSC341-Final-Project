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
    public GameObject duckPrefab;
    public Transform parentTransform;
    [SerializeField] private float jumpForce = 60f;
    private int maxDucks = 100; // Maximum number of ducks allowed

    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private bool isSleeping = false;
    private bool isDragging = false;
    private bool isJumping = false;
    private bool isManagingDucks = false;
    private List<GameObject> ducks = new List<GameObject>();

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing.");
        }

        rb.gravityScale = 1f; // Adjust this value to control the falling speed
        rb.mass = 1f; // Adjust the mass as needed

        StartCoroutine(RandomMovementRoutine());
        StartCoroutine(DuckCallRoutine());
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.S)) ToggleSleep();
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping) StartCoroutine(JumpRoutine());
        if (Input.GetKeyDown(KeyCode.Equals)) ToggleDuckManagement();
        if (isManagingDucks)
        {
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus)) AddDuck();
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) RemoveDuck();
        }
    }

    private void ToggleSleep()
    {
        isSleeping = !isSleeping;
        animator.SetBool("IsSleeping", isSleeping);
    }

    private void ToggleDuckManagement()
    {
        isManagingDucks = !isManagingDucks;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        if (quackQuack != null && audioSource != null)
        {
            audioSource.PlayOneShot(quackQuack);
        }
        rb.gravityScale = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            transform.position = ClampPosition(mousePosition);
            animator.SetFloat("MoveX", Mathf.Abs(mousePosition.x - transform.position.x));
            animator.SetBool("IsWalking", true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        animator.SetBool("IsWalking", false);
        rb.gravityScale = 1f;
    }

    private IEnumerator JumpRoutine()
    {
        animator.SetTrigger("Jump");
        isJumping = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1f); // Adjust this value based on your animation duration
        isJumping = false;
    }

    private void AddDuck()
    {
        if (ducks.Count >= maxDucks)
        {
            Debug.Log("Maximum number of ducks reached. Cannot add more.");
            return;
        }

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
        return new Vector2(Random.Range(minBoundary.x, maxBoundary.x), Random.Range(minBoundary.y, maxBoundary.y));
    }

    private IEnumerator RandomMovementRoutine()
    {
        while (true)
        {
            if (!isSleeping && !isDragging)
            {
                yield return StartCoroutine(MoveRandomly());
            }
            else
            {
                yield return new WaitForSeconds(5f);
            }
        }
    }

    private IEnumerator MoveRandomly()
    {
        float horizontal = Random.Range(-1f, 1f);
        Vector2 direction = new Vector2(horizontal, 0).normalized;
        float moveDuration = Random.Range(1f, 3f);
        float moveTime = 0f;

        while (moveTime < moveDuration)
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            transform.position = ClampPosition(transform.position);
            animator.SetFloat("MoveX", Mathf.Abs(direction.x));
            animator.SetBool("IsWalking", true);

            moveTime += Time.deltaTime;
            yield return null;
        }

        animator.SetFloat("MoveX", 0);
        animator.SetBool("IsWalking", false);

        yield return new WaitForSeconds(Random.Range(1f, 3f));
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
        float clampedX = Mathf.Clamp(position.x, minBoundary.x, maxBoundary.x);
        float clampedY = Mathf.Clamp(position.y, minBoundary.y, maxBoundary.y);
        return new Vector2(clampedX, clampedY);
    }
}
