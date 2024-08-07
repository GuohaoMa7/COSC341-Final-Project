using System.Collections;
using UnityEngine;

public class DuckJumpController : MonoBehaviour
{
    private Animator animator;
    private bool isJumping = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartCoroutine(Jump());
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
}

