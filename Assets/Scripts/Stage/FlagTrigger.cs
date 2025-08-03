using UnityEngine;

public class FlagTrigger : MonoBehaviour
{
    public StageManager stageManager; // Inspector에서 연결

    void OnTriggerEnter(Collider other)
    {
        // 플레이어가 닿았을 때만 반응 (Player 태그 사용 또는 TopDownController 체크 등)
        if (other.CompareTag("Player"))
        {
            if (stageManager != null)
            {
                stageManager.OnStageClear();
            }
        }
    }
}
