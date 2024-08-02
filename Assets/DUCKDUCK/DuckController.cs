using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckController : MonoBehaviour
{
    public float moveSpeed = 5f; // ����Ѽ���ٶ�
    private Animator animator;   // ���ڿ��ƶ�����Animator���

    private void Start()
    {
        // ��ȡAnimator���
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // ��ȡˮƽ�ʹ�ֱ����
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // �����ƶ��ķ���
        Vector2 direction = new Vector2(horizontal, vertical).normalized;

        // �ƶ�Ѽ��
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        // ���ö�������
        if (animator != null)
        {
            // ���ö�����������������һ����Walk�������������߶�����
            animator.SetFloat("MoveX", horizontal);
            animator.SetFloat("MoveY", vertical);
            animator.SetBool("IsWalking", direction.magnitude > 0);
        }
    }
}
