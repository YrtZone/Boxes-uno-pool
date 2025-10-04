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

    [Header("Configurações de Território")]
    [SerializeField] private float distanciaMaximaDoNinho = 12f;
    [SerializeField] private float velocidadeVolta = 2f;

    [Header("Patrulha Aleatória ao Redor do Ninho")]
    [SerializeField] private float velocidadePatrulha = 2f;
    [SerializeField] private float raioPatrulhaAleatoria = 6f; // Raio ao redor do ninho
    [SerializeField] private float tempoEsperaNosPontos = 2f; // Tempo parado em cada ponto
    [SerializeField] private float tempoNoNinho = 3f; // Tempo parado no ninho após voltar
    [SerializeField] private float distanciaParaAlcancarPonto = 0.3f;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Referencias
    private Transform player;
    // << CORREÇÃO 1: Adicionar variável para guardar a referência ao PlayerController
    private PlayerController playerController;
    private Vector3 posicaoNinho;

    // Estado da galinha
    private float vidaAtual;
    private bool estaAtacando = false;
    private bool podeAtacar = true;
    private bool playerDetectado = false;
    private bool voltandoParaNinho = false;

    // Estado da patrulha
    private bool esperandoNoPonto = false;
    private bool paradaNoNinho = false;
    private float tempoInicioEspera;
    private float tempoChegadaNinho;

    // Patrulha aleatória
    private Vector3 pontoAleatorioDestino;
    private bool temPontoAleatorio = false;

    // Timers
    private float tempoUltimoAtaque;
    private float tempoInicioAtaque;

    void Start()
    {
        // Inicializar componentes
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // << CORREÇÃO 2: Obter a referência do PlayerController usando o Singleton
        // É mais eficiente e seguro que usar FindGameObjectWithTag
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;
            playerController = PlayerController.Instance;
        }

        // Configurar vida
        vidaAtual = vidaMaxima;

        // Marcar posição inicial como ninho
        posicaoNinho = transform.position;

        // Configurar física
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        // Começar parada no ninho
        paradaNoNinho = true;
        tempoChegadaNinho = Time.time;

        Debug.Log("Galinha nasceu! Vai patrulhar aleatoriamente ao redor do ninho.");
    }

    void Update()
    {
        if (player == null || vidaAtual <= 0)
            return;

        // Se o player morrer, a galinha para de persegui-lo
        if (playerController != null && !playerController.EstaVivo)
        {
            playerDetectado = false;
        }

        DetectarPlayer();

        // Verificar se está muito longe do ninho
        float distanciaDoNinho = Vector2.Distance(transform.position, posicaoNinho);

        if (playerDetectado)
        {
            // Player detectado - perseguir e atacar
            voltandoParaNinho = false;
            esperandoNoPonto = false;
            paradaNoNinho = false;
            temPontoAleatorio = false;

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
        else if (voltandoParaNinho || distanciaDoNinho > distanciaMaximaDoNinho)
        {
            // Player saiu do campo de visão OU saiu muito longe - voltar ao ninho
            voltandoParaNinho = true;
            esperandoNoPonto = false;
            paradaNoNinho = false;
            temPontoAleatorio = false;

            VoltarParaNinho();

            // Se chegou próximo ao ninho, ficar parada um tempo
            if (distanciaDoNinho <= 0.5f)
            {
                voltandoParaNinho = false;
                paradaNoNinho = true;
                tempoChegadaNinho = Time.time;
                transform.position = posicaoNinho; // Garantir posição exata
                Debug.Log("Galinha voltou ao ninho. Descansando por " + tempoNoNinho + " segundos...");
            }
        }
        else if (paradaNoNinho)
        {
            // Galinha parada no ninho esperando
            if (Time.time - tempoChegadaNinho >= tempoNoNinho)
            {
                paradaNoNinho = false;
                temPontoAleatorio = false; // Vai gerar novo ponto aleatório
                Debug.Log("Galinha terminou de descansar. Voltando a patrulhar!");
            }
            else
            {
                Parar();
            }
        }
        else
        {
            // Sem player detectado - patrulhar aleatoriamente ao redor do ninho
            PatrulharAleatorio();
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
        if (player == null || (playerController != null && !playerController.EstaVivo))
        {
            playerDetectado = false;
            return;
        }

        float distancia = Vector2.Distance(transform.position, player.position);

        // Se perdeu o player de vista, marcar para voltar ao ninho
        if (playerDetectado && distancia > distanciaDeteccao)
        {
            Debug.Log("Player saiu do campo de visão! Voltando ao ninho...");
        }

        playerDetectado = distancia <= distanciaDeteccao;
    }

    void PatrulharAleatorio()
    {
        // Se está esperando no ponto
        if (esperandoNoPonto)
        {
            if (Time.time - tempoInicioEspera >= tempoEsperaNosPontos)
            {
                esperandoNoPonto = false;
                temPontoAleatorio = false; // Vai gerar novo ponto
                Debug.Log("Galinha saindo do ponto. Gerando novo destino...");
            }
            else
            {
                Parar(); // Fica parada (animação idle)
                return;
            }
        }

        // Se não tem ponto, gerar novo ponto aleatório
        if (!temPontoAleatorio)
        {
            GerarPontoAleatorio();
        }

        // Calcular direção para o ponto aleatório
        Vector2 direcao = (pontoAleatorioDestino - transform.position).normalized;
        float distanciaAoPonto = Vector2.Distance(transform.position, pontoAleatorioDestino);

        // Verificar se chegou no ponto
        if (distanciaAoPonto <= distanciaParaAlcancarPonto)
        {
            esperandoNoPonto = true;
            tempoInicioEspera = Time.time;
            transform.position = pontoAleatorioDestino;
            Debug.Log("Galinha chegou no ponto aleatório. Parando...");
            Parar(); // Força parar imediatamente
            return;
        }

        // Mover em direção ao ponto aleatório
        MoverParaDirecao(direcao, velocidadePatrulha);
    }

    void GerarPontoAleatorio()
    {
        // Gerar ponto aleatório dentro do círculo ao redor do ninho
        Vector2 pontoAleatorio = Random.insideUnitCircle * raioPatrulhaAleatoria;
        pontoAleatorioDestino = posicaoNinho + new Vector3(pontoAleatorio.x, pontoAleatorio.y, 0);

        temPontoAleatorio = true;

        Debug.Log("Novo ponto aleatório gerado: " + pontoAleatorioDestino);
    }

    void MoverParaDirecao(Vector2 direcao, float velocidadeMovimento)
    {
        // Mover
        float moveX = direcao.x * velocidadeMovimento * Time.deltaTime;
        float moveY = direcao.y * velocidadeMovimento * Time.deltaTime;
        transform.Translate(moveX, moveY, 0);

        // Zerar velocity do Rigidbody
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Configurar animator para ANDAR
        if (animator != null)
        {
            animator.SetFloat("Speed", 1); // Maior que 0 = animação de andar
        }

        // Virar sprite na direção do movimento
        if (direcao.x > 0)
            spriteRenderer.flipX = false;
        else if (direcao.x < 0)
            spriteRenderer.flipX = true;
    }

    void PerseguirPlayer()
    {
        if (player == null || estaAtacando)
            return;

        // Calcular direção para o player
        Vector2 direcao = (player.position - transform.position).normalized;

        // Mover
        MoverParaDirecao(direcao, velocidade);

        Debug.Log("Perseguindo player! Distância: " + Vector2.Distance(transform.position, player.position).ToString("F2"));
    }

    void Parar()
    {
        // Parar movimento
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Configurar animator para PARADO (IDLE)
        if (animator != null)
        {
            animator.SetFloat("Speed", 0); // Speed = 0 = animação idle
        }
    }

    void VoltarParaNinho()
    {
        // Calcular direção para o ninho
        Vector2 direcaoNinho = (posicaoNinho - transform.position).normalized;
        float distancia = Vector2.Distance(transform.position, posicaoNinho);

        // Mover em direção ao ninho
        MoverParaDirecao(direcaoNinho, velocidadeVolta);

        Debug.Log("Voltando ao ninho... Distância: " + distancia.ToString("F2"));
    }

    void IniciarAtaque()
    {
        if (!podeAtacar)
            return;

        estaAtacando = true;
        podeAtacar = false;
        tempoInicioAtaque = Time.time;
        tempoUltimoAtaque = Time.time;

        // Parar movimento durante ataque
        if (rb != null)
            rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0); // Parar durante ataque
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

        // Iniciar cooldown do ataque
        Invoke(nameof(ResetarAtaque), tempoEntreAtaques - duracaoAtaque);
    }

    void ResetarAtaque()
    {
        podeAtacar = true;
    }

    void AtacarPlayer()
    {
        // << CORREÇÃO 3: Usar a referência já armazenada e acessar a propriedade sem parênteses
        if (playerController != null && playerController.EstaVivo)
        {
            playerController.ReceberDano(danoAtaque);
        }

        Debug.Log("Galinha causou " + danoAtaque + " de dano ao player!");
    }

    public void ReceberDano(float dano)
    {
        vidaAtual -= dano;

        // Efeito visual de dano
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

        // Destruir objeto após delay
        Destroy(gameObject, 0.5f);
    }

    void AtualizarAnimacao()
    {
        if (animator == null)
            return;

        // Sistema simples: apenas Speed controla tudo
        // Speed = 0 → Idle (parado)
        // Speed > 0 → Walk (andando)
    }

    // Desenhar gizmos para debug
    void OnDrawGizmosSelected()
    {
        Vector3 posNinho = Application.isPlaying ? posicaoNinho : transform.position;

        // Círculo de detecção do player (campo de visão)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);

        // Círculo de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        // Círculo da área máxima do ninho (limite de segurança)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(posNinho, distanciaMaximaDoNinho);

        // Área de patrulha aleatória ao redor do ninho
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(posNinho, raioPatrulhaAleatoria);

        // Desenhar ponto aleatório atual se existir
        if (Application.isPlaying && temPontoAleatorio)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pontoAleatorioDestino, 0.3f);
            Gizmos.DrawLine(transform.position, pontoAleatorioDestino);
        }

        // Desenhar linha para o ninho
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, posNinho);
    }

    // Detectar colisão com player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && estaAtacando)
        {
            AtacarPlayer();
        }
    }
}