using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class OtherCharacterHandler : MonoBehaviour
{
    private NavMeshAgent m_Agent;

    public long PlayerIdentity;

    public TextMeshProUGUI PlayerName;

    public int Kill;

    public int Dead;

    public AudioSource RunAudioSource;

    public string CharacterName { private set; get; }

    public GameObject Explosion;

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

    private void Update()
    {
        PlayerName.text = $"Player {PlayerIdentity}\nKill: {Kill} | Dead: {Dead}";
    }

    public void ReceiveAttack()
    {
        Explosion.SetActive(false);
        Explosion.SetActive(true);
        Invoke(nameof(DisableExplosion), 3);
    }

    private void DisableExplosion()
    {
        Explosion.SetActive(false);
    }
}