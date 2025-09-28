using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortaFacil : MonoBehaviour
{
    [Header("Configurações da Porta")]
    public string nomeDaCenaDestino = "InteriorDaCasa";
    public Vector3 posicaoSpawn = new Vector3(0, 0, 0); // Onde o player vai aparecer
    public float tempoAnimacao = 1.5f;

    private Animator anim;
    private bool jogadorPerto = false;
    private bool processando = false;

    // Variável estática para guardar onde spawnar
    public static Vector3 proximaPosicao;
    public static bool temSpawnDefinido = false;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Se tem spawn definido, posicionar o player
        if (temSpawnDefinido)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = proximaPosicao;
                Debug.Log("Player posicionado em: " + proximaPosicao);
            }
            temSpawnDefinido = false; // Reset
        }
    }

    void Update()
    {
        if (jogadorPerto && Input.GetKeyDown(KeyCode.E) && !processando)
        {
            StartCoroutine(UsarPorta());
        }
    }

    IEnumerator UsarPorta()
    {
        processando = true;

        // Definir onde o player vai aparecer na próxima cena
        proximaPosicao = posicaoSpawn;
        temSpawnDefinido = true;

        Debug.Log("Usando porta...");

        // Animação (se existir)
        if (anim != null)
        {
            anim.Play("PortaAbrindo");
        }

        yield return new WaitForSeconds(tempoAnimacao);

        // Carregar cena
        SceneManager.LoadScene(nomeDaCenaDestino);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = true;
            Debug.Log("Aperte E para usar a porta");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            processando = false;
        }
    }
}