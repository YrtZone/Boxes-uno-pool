using UnityEngine;

// Garante que o item tenha os componentes necessários
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("O quanto a velocidade do jogador é reduzida enquanto carrega este item (1.0 = sem redução, 0.5 = 50% mais lento).")]
    [SerializeField] private float modificadorVelocidade = 0.8f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool estaSendoCarregado = false;

    private void Awake()
    {
        // Pega as referências dos componentes no início
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
    /// Método chamado pelo PlayerController quando o item é pego.
    /// </summary>
    /// <param name="pontoPegada">O Transform onde o item ficará preso.</param>
    public void SerPego(Transform pontoPegada)
    {
        estaSendoCarregado = true;

        // Desativa a física para que o item não se comporte de forma estranha
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        col.enabled = false;

        // "Gruda" o item no jogador
        transform.SetParent(pontoPegada);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Método chamado pelo PlayerController quando o item é solto.
    /// ESTE É O MÉTODO QUE ESTAVA FALTANDO!
    /// </summary>
    public void Soltar()
    {
        if (!estaSendoCarregado) return;

        estaSendoCarregado = false;

        // "Desgruda" o item do jogador
        transform.SetParent(null);

        // Reativa a física do item para que ele possa cair e colidir novamente
        rb.isKinematic = false;
        col.enabled = true;

        // A força para arremessar o item é aplicada no PlayerController
    }
}