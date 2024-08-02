using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckController : MonoBehaviour
{
    public float moveSpeed = 5f; // 控制鸭子速度
    private Animator animator;   // 用于控制动画的Animator组件

    private void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 获取水平和垂直输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 计算移动的方向
        Vector2 direction = new Vector2(horizontal, vertical).normalized;

        // 移动鸭子
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        // 设置动画参数
        if (animator != null)
        {
            // 设置动画参数，假设你有一个“Walk”参数来播放走动动画
            animator.SetFloat("MoveX", horizontal);
            animator.SetFloat("MoveY", vertical);
            animator.SetBool("IsWalking", direction.magnitude > 0);
        }
    }
}
