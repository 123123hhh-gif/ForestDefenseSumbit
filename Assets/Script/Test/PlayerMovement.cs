using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        // 获取玩家身上的 NavMeshAgent 组件
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 从摄像机发射一条射线到鼠标位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 如果射线碰撞到了物体
            if (Physics.Raycast(ray, out hit))
            {
                // 将目标点设置为碰撞点的位置（即地面上的点）
                agent.SetDestination(hit.point);
            }
        }
    }
}