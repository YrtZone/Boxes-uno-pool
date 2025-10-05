using UnityEngine;
using UnityEngine.SceneManagement;

// Garante que o item tenha os componentes necessários
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("O quanto a velocidade do jogador é reduzida enquanto carrega este item (1.0 = sem redução, 0.5 = 50% mais lento).")]
    [SerializeField] private float modificadorVelocidade = 0.8f;

    [Header("Gerenciamento de Estado")]
    [Tooltip("ID único para este item. Ex: OvoFogo_01, ChaveCaverna, etc.")]
    // A principal mudança foi tornar esta variável 'public' para que o PlayerController possa acessá-la.
    [SerializeField] public string uniqueId;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool estaSendoCarregado = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Avisa se o item não tiver um ID, para que ele não participe do sistema de salvamento.
        if (string.IsNullOrEmpty(uniqueId))
        {
            Debug.LogWarning($"O item '{gameObject.name}' não tem um ID único definido. Ele não será gerenciado entre as cenas.", this);
        }

        // Se o GameStateManager existir e o item tiver um ID, ele se registra.
        if (GameStateManager.Instance != null && !string.IsNullOrEmpty(uniqueId))
        {
            GameStateManager.Instance.RegisterItem(uniqueId, gameObject.scene.name);
        }
    }

    // Os métodos OnEnable e OnDisable garantem que o item sempre verifique seu estado ao carregar uma cena.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Roda a verificação uma vez no início também.
        CheckState();
    }

    /// <summary>
    /// Este método é chamado toda vez que uma nova cena é carregada.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckState();
    }

    /// <summary>
    /// Verifica com o GameStateManager se este item deveria estar ativo nesta cena.
    /// </summary>
    private void CheckState()
    {
        if (GameStateManager.Instance == null || string.IsNullOrEmpty(uniqueId))
        {
            // Se não houver manager ou ID, não faz nada.
            return;
        }

        string correctScene = GameStateManager.Instance.GetItemCurrentScene(uniqueId);

        // Se o banco de dados diz que o item deveria estar em uma cena, mas não é a cena atual...
        if (correctScene != null && correctScene != gameObject.scene.name)
        {
            // ...então este objeto (a cópia da cena original) é desativado.
            gameObject.SetActive(false);
        }
        else
        {
            // Garante que o item esteja ativo se ele pertencer a esta cena.
            gameObject.SetActive(true);
        }
    }


    /// <summary>
    /// Retorna o multiplicador de velocidade que este item aplica ao jogador.
    /// </summary>
    public float GetModificadorVelocidade()
    {
        return modificadorVelocidade;
    }

    /// <summary>
    /// Método chamado pelo PlayerController quando o item é pego.
    /// </summary>
    public void SerPego(Transform pontoPegada)
    {
        estaSendoCarregado = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        col.enabled = false;
        transform.SetParent(pontoPegada);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Método chamado pelo PlayerController quando o item é solto.
    /// </summary>
    public void Soltar()
    {
        if (!estaSendoCarregado) return;
        estaSendoCarregado = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;
    }
}