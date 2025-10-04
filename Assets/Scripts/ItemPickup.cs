using UnityEngine;

// Garante que o item tenha os componentes necess�rios
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Configura��es do Item")]
    [Tooltip("O quanto a velocidade do jogador � reduzida enquanto carrega este item (1.0 = sem redu��o, 0.5 = 50% mais lento).")]
    [SerializeField] private float modificadorVelocidade = 0.8f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool estaSendoCarregado = false;

    private void Awake()
    {
        // Pega as refer�ncias dos componentes no in�cio
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Retorna o multiplicador de velocidade que este item aplica ao jogador.
    /// </summary>
    public float GetModificadorVelocidade()
    {
        return modificadorVelocidade;
    }

    /// <summary>
    /// M�todo chamado pelo PlayerController quando o item � pego.
    /// </summary>
    /// <param name="pontoPegada">O Transform onde o item ficar� preso.</param>
    public void SerPego(Transform pontoPegada)
    {
        estaSendoCarregado = true;

        // Desativa a f�sica para que o item n�o se comporte de forma estranha
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        col.enabled = false;

        // "Gruda" o item no jogador
        transform.SetParent(pontoPegada);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// M�todo chamado pelo PlayerController quando o item � solto.
    /// ESTE � O M�TODO QUE ESTAVA FALTANDO!
    /// </summary>
    public void Soltar()
    {
        if (!estaSendoCarregado) return;

        estaSendoCarregado = false;

        // "Desgruda" o item do jogador
        transform.SetParent(null);

        // Reativa a f�sica do item para que ele possa cair e colidir novamente
        rb.isKinematic = false;
        col.enabled = true;

        // A for�a para arremessar o item � aplicada no PlayerController
    }
}