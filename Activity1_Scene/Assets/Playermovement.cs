using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    InputAction moveAction;

    void Start()
    {
        // 从项目的 Input Actions 中找到 Move
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        // 获取键盘、手柄提供的二维移动输入
        Vector2 input = moveAction.ReadValue<Vector2>();

        // 把二维输入转换成三维地面移动
        Vector3 movement = new Vector3(input.x, 0f, input.y);

        // 在世界坐标中移动 Player
        transform.Translate(
            movement * moveSpeed * Time.deltaTime,
            Space.World
        );
    }
}