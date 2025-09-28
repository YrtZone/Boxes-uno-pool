using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float speed = 3f;
    public float runSpeed = 5f;

    [Header("Configurações de Vida")]
    public float vidaMaxima = 100f;
    public float vidaAtual;

    [Header("UI da Vida")]
    public Slider barraVida;
    public Text textoVida;

    [Header("Efeitos Visuais")]
    public float tempoEfeitoDano = 0.2f;

    // Componentes
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Estado do player
    private bool estaCorrendo = false;
    private bool estaVivo = true;
    private string currentState = "";

    void Start()
    {
        // Inicializar componentes
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Configurar vida
        vidaAtual = vidaMaxima;
        AtualizarUIVida();

        // Configurar física se tiver Rigidbody2D
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        // Verificar se tem a tag correta
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
            Debug.Log("Tag 'Player' foi adicionada automaticamente!");
        }
    }

    void Update()
    {
        if (!estaVivo)
            return;

        MovimentarPlayer();
    }

    void MovimentarPlayer()
    {
        // Pegar input do jogador
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Verificar se está correndo
        estaCorrendo = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Calcular velocidade atual
        float velocidadeAtual = estaCorrendo ? runSpeed : speed;

        // Configurar parâmetros do Animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", x);
            anim.SetFloat("Vertical", y);
            anim.SetFloat("Speed", Mathf.Abs(x) + Mathf.Abs(y));
            anim.SetBool("EstaCorrendo", estaCorrendo);
        }

        // Mover o player
        transform.Translate(x * velocidadeAtual * Time.deltaTime, y * velocidadeAtual * Time.deltaTime, 0);

        // Zerar velocity do Rigidbody se estiver usando
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Debug do movimento
        if (x != 0 || y != 0)
        {
            Debug.Log("Player movendo - X: " + x + " Y: " + y + " Correndo: " + estaCorrendo);
        }

        // Decidir qual animação tocar
        string newState = "";
        if (x == 0 && y == 0)
        {
            newState = "Parado";
        }
        else if (estaCorrendo)
        {
            newState = "Correndo";
        }
        else
        {
            newState = "Andando";
        }

        // Trocar estado se necessário
        if (newState != currentState)
        {
            currentState = newState;
            Debug.Log("Estado do player: " + currentState);
        }
    }

    public void ReceberDano(float dano)
    {
        if (!estaVivo)
            return;

        vidaAtual -= dano;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);

        Debug.Log("Player recebeu " + dano + " de dano! Vida atual: " + vidaAtual);

        // Efeito visual de dano
        StartCoroutine(EfeitoDano());

        // Atualizar UI
        AtualizarUIVida();

        // Verificar se morreu
        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    public void Curar(float quantidadeCura)
    {
        if (!estaVivo)
            return;

        vidaAtual += quantidadeCura;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);

        Debug.Log("Player curou " + quantidadeCura + " pontos! Vida atual: " + vidaAtual);

        // Efeito visual de cura (opcional)
        StartCoroutine(EfeitoCura());

        // Atualizar UI
        AtualizarUIVida();
    }

    void Morrer()
    {
        estaVivo = false;

        Debug.Log("Player morreu!");

        // Parar movimento
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Configurar animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", 0);
            anim.SetFloat("Vertical", 0);
            anim.SetFloat("Speed", 0);
            anim.SetBool("EstaMorto", true);
        }

        // Aqui você pode adicionar:
        // - Tela de game over
        // - Reiniciar fase
        // - Voltar ao menu

        // Exemplo: reiniciar a cena após 3 segundos
        Invoke(nameof(ReiniciarJogo), 3f);
    }

    void ReiniciarJogo()
    {
        // Reiniciar a cena atual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void Reviver()
    {
        estaVivo = true;
        vidaAtual = vidaMaxima;

        if (anim != null)
        {
            anim.SetBool("EstaMorto", false);
        }

        AtualizarUIVida();

        Debug.Log("Player reviveu!");
    }

    void AtualizarUIVida()
    {
        // Atualizar barra de vida
        if (barraVida != null)
        {
            barraVida.value = vidaAtual / vidaMaxima;
        }

        // Atualizar texto da vida
        if (textoVida != null)
        {
            textoVida.text = vidaAtual.ToString("F0") + "/" + vidaMaxima.ToString("F0");
        }
    }

    System.Collections.IEnumerator EfeitoDano()
    {
        // Piscar vermelho quando recebe dano
        if (spriteRenderer != null)
        {
            Color corOriginal = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(tempoEfeitoDano);
            spriteRenderer.color = corOriginal;
        }
    }

    System.Collections.IEnumerator EfeitoCura()
    {
        // Piscar verde quando cura
        if (spriteRenderer != null)
        {
            Color corOriginal = spriteRenderer.color;
            spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(tempoEfeitoDano);
            spriteRenderer.color = corOriginal;
        }
    }

    // Métodos públicos para outros scripts verificarem
    public bool EstaVivo()
    {
        return estaVivo;
    }

    public float GetVidaAtual()
    {
        return vidaAtual;
    }

    public float GetVidaMaxima()
    {
        return vidaMaxima;
    }

    public bool EstaCorrendo()
    {
        return estaCorrendo;
    }

    // Para debug - mostrar informações na tela
    void OnGUI()
    {
        if (!estaVivo)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "PLAYER MORREU!");
        }

        GUI.Label(new Rect(10, 30, 200, 20), "Vida: " + vidaAtual.ToString("F0") + "/" + vidaMaxima.ToString("F0"));
        GUI.Label(new Rect(10, 50, 200, 20), "Estado: " + currentState);

        // Botões de teste (remova em produção)
        if (GUI.Button(new Rect(10, 70, 100, 30), "Receber Dano"))
        {
            ReceberDano(10);
        }

        if (GUI.Button(new Rect(120, 70, 80, 30), "Curar"))
        {
            Curar(20);
        }
    }
}