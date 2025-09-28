using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalinhaInimiga : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaDeteccao = 8f;
    [SerializeField] private float distanciaAtaque = 1.5f;

    [Header("Configurações de Ataque")]
    [SerializeField] private float danoAtaque = 10f;
    [SerializeField] private float tempoEntreAtaques = 1.5f;
    [SerializeField] private float duracaoAtaque = 0.5f;

    [Header("Configurações de Vida")]
    [SerializeField] private float vidaMaxima = 30f;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Referencias
    private Transform player;
    private GameObject playerObj;

    // Estado da galinha
    private float vidaAtual;
    private bool estaAtacando = false;
    private bool podeAtacar = true;
    private bool playerDetectado = false;

    // Timers
    private float tempoUltimoAtaque;
    private float tempoInicioAtaque;

    void Start()
    {
        // Inicializar componentes
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Encontrar o player
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Configurar vida
        vidaAtual = vidaMaxima;

        // Configurar física
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (player == null || vidaAtual <= 0)
            return;

        DetectarPlayer();

        if (playerDetectado)
        {
            float distanciaPlayer = Vector2.Distance(transform.position, player.position);

            if (distanciaPlayer <= distanciaAtaque && podeAtacar && !estaAtacando)
            {
                IniciarAtaque();
            }
            else if (distanciaPlayer > distanciaAtaque && !estaAtacando)
            {
                PerseguirPlayer();
            }
        }
        else
        {
            Parar();
        }

        // Verificar se terminou o ataque
        if (estaAtacando && Time.time - tempoInicioAtaque >= duracaoAtaque)
        {
            TerminarAtaque();
        }

        AtualizarAnimacao();
    }

    void DetectarPlayer()
    {
        if (player == null)
            return;

        float distancia = Vector2.Distance(transform.position, player.position);
        playerDetectado = distancia <= distanciaDeteccao;
    }

    void PerseguirPlayer()
    {
        if (player == null || estaAtacando)
            return;

        // Calcular direção para o player
        Vector2 direcao = (player.position - transform.position).normalized;

        // Mover usando Transform.Translate (igual seu sistema)
        float moveX = direcao.x * velocidade * Time.deltaTime;
        float moveY = direcao.y * velocidade * Time.deltaTime;
        transform.Translate(moveX, moveY, 0);

        // Zerar velocity do Rigidbody se estiver usando
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Configurar parâmetros do Animator (igual seu sistema)
        if (animator != null)
        {
            animator.SetFloat("Horizontal", direcao.x);
            animator.SetFloat("Vertical", direcao.y);
            animator.SetFloat("Speed", Mathf.Abs(direcao.x) + Mathf.Abs(direcao.y));
        }

        // Virar sprite na direção do movimento
        if (direcao.x > 0)
            spriteRenderer.flipX = false;
        else if (direcao.x < 0)
            spriteRenderer.flipX = true;
    }

    void Parar()
    {
        // Parar movimento
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Configurar animator para estado parado (igual seu sistema)
        if (animator != null)
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Speed", 0);
        }
    }

    void IniciarAtaque()
    {
        if (!podeAtacar)
            return;

        estaAtacando = true;
        podeAtacar = false;
        tempoInicioAtaque = Time.time;
        tempoUltimoAtaque = Time.time;

        // Parar movimento durante ataque e configurar animator
        if (rb != null)
            rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Speed", 0);
            animator.SetBool("EstaAtacando", true);
        }

        // Atacar o player se ainda estiver no alcance
        float distanciaPlayer = Vector2.Distance(transform.position, player.position);
        if (distanciaPlayer <= distanciaAtaque)
        {
            AtacarPlayer();
        }

        Debug.Log("Galinha atacou! Cóóóóó!");
    }

    void TerminarAtaque()
    {
        estaAtacando = false;

        // Resetar animator
        if (animator != null)
        {
            animator.SetBool("EstaAtacando", false);
        }

        // Iniciar cooldown do ataque
        Invoke(nameof(ResetarAtaque), tempoEntreAtaques - duracaoAtaque);
    }

    void ResetarAtaque()
    {
        podeAtacar = true;
    }

    void AtacarPlayer()
    {
        // Atacar usando o PlayerController
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        if (playerController != null && playerController.EstaVivo())
        {
            playerController.ReceberDano(danoAtaque);
        }

        // Debug do ataque
        Debug.Log("Galinha causou " + danoAtaque + " de dano ao player!");
    }

    public void ReceberDano(float dano)
    {
        vidaAtual -= dano;

        // Efeito visual de dano (opcional)
        StartCoroutine(EfeitoDano());

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    System.Collections.IEnumerator EfeitoDano()
    {
        // Piscar vermelho quando recebe dano
        Color corOriginal = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = corOriginal;
    }

    void Morrer()
    {
        Debug.Log("Galinha morreu!");

        // Parar todas as ações
        rb.velocity = Vector2.zero;
        estaAtacando = false;

        // Opcional: animação de morte, som, etc.

        // Destruir objeto após delay
        Destroy(gameObject, 0.5f);
    }

    void AtualizarAnimacao()
    {
        if (animator == null)
            return;

        // O sistema já está sendo atualizado nos métodos de movimento
        // Apenas verificamos se precisa definir estado parado quando não há movimento nem ataque
        if (!estaAtacando && rb != null && rb.velocity.magnitude <= 0.1f)
        {
            animator.SetFloat("Speed", 0);
        }

        // Debug para verificar estados (igual seu sistema)
        bool estaMovendo = animator.GetFloat("Speed") > 0;
        if (estaMovendo)
        {
            Debug.Log("Galinha movendo - X: " + animator.GetFloat("Horizontal") + " Y: " + animator.GetFloat("Vertical"));
        }
    }

    // Desenhar gizmos para debug
    void OnDrawGizmosSelected()
    {
        // Círculo de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);

        // Círculo de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }

    // Detectar colisão com player (método alternativo)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && estaAtacando)
        {
            AtacarPlayer();
        }
    }
}