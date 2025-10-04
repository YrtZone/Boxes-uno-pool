using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// =====================================================
// PLAYER CONTROLLER UNIFICADO (MOVIMENTO + VIDA + ITENS + OVOS)
// =====================================================
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    #region Singleton
    public static PlayerController Instance { get; private set; }
    #endregion

    #region Configurações de Movimento
    [Header("⚙️ Movimento")]
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float runMultiplier = 1.67f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    #endregion

    #region Vida
    [Header("❤️ Vida")]
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaAtual;
    private bool invulneravel = false;
    [SerializeField] private float tempoInvulneravel = 1f;
    #endregion

    #region UI
    [Header("📊 Interface")]
    [SerializeField] private Slider barraVida;
    [SerializeField] private Text textoVida;
    [SerializeField] private GameObject painelGameOver;
    #endregion

    #region Sistema de Itens / Ovos
    [Header("🥚 Sistema de Itens e Ovos")]
    [SerializeField] private float distanciaMaximaPegar = 2f;
    [SerializeField] private Transform pontoPegada;
    [SerializeField] private float forcaSoltar = 5f;
    [SerializeField] private KeyCode teclaPegarSoltar = KeyCode.X;
    [SerializeField] private LayerMask layerItens;
    [SerializeField] private bool mostrarRaioAlcance = true;
    [SerializeField] private Color corRaioAlcance = Color.green;

    private ItemPickup itemAtual = null;
    private bool estaCarregandoItem = false;
    private float velocidadeModificador = 1f;
    #endregion

    #region Componentes Internos
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector2 inputMovimento;
    private Vector2 velocidadeAtual;
    private bool estaCorrendo;
    // << CORREÇÃO 1: Transformando em propriedade pública com setter privado
    public bool EstaVivo { get; private set; } = true;
    #endregion

    #region Unity Events
    public UnityEvent<float> OnVidaMudou;
    public UnityEvent OnPlayerMorreu;
    public UnityEvent OnPlayerReviveu;
    #endregion

    #region Inicialização
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        vidaAtual = vidaMaxima;
        AtualizarUIVida();

        if (pontoPegada == null)
        {
            GameObject p = new GameObject("PontoPegada");
            p.transform.SetParent(transform);
            p.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);
            pontoPegada = p.transform;
        }
    }
    #endregion

    #region Loop Principal
    private void Update()
    {
        // << CORREÇÃO 2: Usando a nova propriedade
        if (!EstaVivo) return;

        LerInput();
        ProcessarInputMovimento();
        ProcessarInputItem();

        AtualizarAnimacoes();
        AtualizarUIVida();
    }

    private void FixedUpdate()
    {
        // << CORREÇÃO 3: Usando a nova propriedade
        if (!EstaVivo) return;
        MoverPlayer();
    }
    #endregion

    #region Movimento
    private void LerInput()
    {
        inputMovimento.x = Input.GetAxisRaw("Horizontal");
        inputMovimento.y = Input.GetAxisRaw("Vertical");
        inputMovimento.Normalize();
        estaCorrendo = Input.GetKey(KeyCode.LeftShift);
    }

    private void ProcessarInputMovimento()
    {
        float velocidadeAlvo = baseSpeed * (estaCorrendo ? runMultiplier : 1f) * velocidadeModificador;
        Vector2 velocidadeDesejada = inputMovimento * velocidadeAlvo;
        float suavizar = inputMovimento.magnitude > 0.01f ? acceleration : deceleration;
        velocidadeAtual = Vector2.Lerp(velocidadeAtual, velocidadeDesejada, suavizar * Time.deltaTime);
    }

    private void MoverPlayer()
    {
        rb.MovePosition(rb.position + velocidadeAtual * Time.fixedDeltaTime);
    }

    private void AtualizarAnimacoes()
    {
        if (!anim) return;
        anim.SetFloat("Horizontal", inputMovimento.x);
        anim.SetFloat("Vertical", inputMovimento.y);
        anim.SetFloat("Speed", velocidadeAtual.magnitude);
        anim.SetBool("CarregandoItem", estaCarregandoItem);
        anim.SetBool("EstaCorrendo", estaCorrendo);
    }
    #endregion

    #region Sistema de Itens e Ovos
    private void ProcessarInputItem()
    {
        if (Input.GetKeyDown(teclaPegarSoltar))
        {
            if (estaCarregandoItem)
                SoltarItem();
            else
                TentarPegarItem();
        }

        if (estaCarregandoItem && itemAtual != null)
            itemAtual.transform.position = pontoPegada.position;
    }

    private void TentarPegarItem()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, distanciaMaximaPegar, layerItens);
        float menorDistancia = float.MaxValue;
        ItemPickup itemMaisProximo = null;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Item"))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < menorDistancia)
                {
                    menorDistancia = dist;
                    itemMaisProximo = col.GetComponent<ItemPickup>();
                }
            }
        }

        if (itemMaisProximo != null)
            PegarItem(itemMaisProximo);
        else
            Debug.Log("Nenhum item próximo para pegar!");
    }

    private void PegarItem(ItemPickup item)
    {
        if (item == null) return;

        itemAtual = item;
        estaCarregandoItem = true;
        velocidadeModificador = item.GetModificadorVelocidade();

        item.SerPego(pontoPegada);
        Debug.Log($"Item '{item.name}' foi pego!");
    }

    private void SoltarItem()
    {
        Vector2 direcao = (inputMovimento.magnitude > 0.01f ? inputMovimento : Vector2.right).normalized;

        if (itemAtual != null)
        {
            // << CORREÇÃO FINAL: Nome do método com 'S' maiúsculo
            itemAtual.Soltar();
        }


        // ✅ SOLUÇÃO: Cache do Rigidbody2D em variável
        Rigidbody2D rb = itemAtual.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direcao * forcaSoltar, ForceMode2D.Impulse);
        }

        Debug.Log($"Item '{itemAtual.name}' foi solto!");

        itemAtual = null;
        estaCarregandoItem = false;
        velocidadeModificador = 1f;
    }
    #endregion

    #region Vida
    public void ReceberDano(float dano)
    {
        // << CORREÇÃO 4: Usando a nova propriedade
        if (!EstaVivo || invulneravel) return;

        vidaAtual = Mathf.Max(0, vidaAtual - dano);
        AtualizarUIVida();

        if (vidaAtual <= 0)
            Morrer();
        else
            StartCoroutine(PeriodoInvulneravel());
    }

    public void Curar(float quantia)
    {
        // << CORREÇÃO 5: Usando a nova propriedade
        if (!EstaVivo) return;
        vidaAtual = Mathf.Min(vidaMaxima, vidaAtual + quantia);
        AtualizarUIVida();
    }

    private IEnumerator PeriodoInvulneravel()
    {
        invulneravel = true;
        yield return new WaitForSeconds(tempoInvulneravel);
        invulneravel = false;
    }

    private void Morrer()
    {
        // << CORREÇÃO 6: Usando a nova propriedade
        EstaVivo = false;
        velocidadeAtual = Vector2.zero;
        anim.SetTrigger("Morrer");
        painelGameOver?.SetActive(true);
        OnPlayerMorreu?.Invoke();
    }

    public void Reviver()
    {
        // << CORREÇÃO 7: Usando a nova propriedade
        EstaVivo = true;
        vidaAtual = vidaMaxima;
        painelGameOver?.SetActive(false);
        anim.SetTrigger("Reviver");
        OnPlayerReviveu?.Invoke();
    }

    private void AtualizarUIVida()
    {
        if (barraVida != null)
            barraVida.value = vidaAtual / vidaMaxima;
        if (textoVida != null)
            textoVida.text = $"{vidaAtual:F0}/{vidaMaxima:F0}";
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (mostrarRaioAlcance)
        {
            Gizmos.color = corRaioAlcance;
            Gizmos.DrawWireSphere(transform.position, distanciaMaximaPegar);
        }
        if (pontoPegada != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pontoPegada.position, 0.2f);
        }
    }
    #endregion
}