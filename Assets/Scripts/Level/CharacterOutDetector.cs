using UnityEngine;

public class CharacterOutDetector : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _gameFailedEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("레벨 벗어남");
            Destroy(other.gameObject);
            _gameFailedEvent.RaiseEvent();
        }
    }
}
