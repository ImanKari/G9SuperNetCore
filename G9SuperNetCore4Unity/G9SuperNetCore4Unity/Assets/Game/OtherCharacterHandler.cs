using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class OtherCharacterHandler : MonoBehaviour
{
    private NavMeshAgent m_Agent;

    public long PlayerIdentity;

    public TextMeshProUGUI PlayerName;

    public AudioSource RunAudioSource;

    public string CharacterName { private set; get; }

    // Start is called before the first frame update
    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    public void MoveCharacter(Vector3 position)
    {
        m_Agent.SetDestination(position);
        if (!RunAudioSource.isPlaying)
            RunAudioSource.Play();
    }
}