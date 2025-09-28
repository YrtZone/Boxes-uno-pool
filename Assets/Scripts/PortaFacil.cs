using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortaCompleta : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeDaCenaDestino = "InteriorDaCasa";
    public float tempoAnimacao = 1.5f; // Tempo para ver a animação completa

    private Animator anim;
    private bool jogadorPerto = false;
    private bool processandoPorta = false; // Evita spam de E

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (jogadorPerto && Input.GetKeyDown(KeyCode.E) && !processandoPorta)
        {
            StartCoroutine(AnimacaoCompletaEMudarCena());
        }
    }

    IEnumerator AnimacaoCompletaEMudarCena()
    {
        processandoPorta = true;

        Debug.Log("Porta abrindo... aguarde!");

        // Inicia a animação
        anim.SetTrigger("AtivarPorta");

        // Tempo suficiente para ver a animação completa
        yield return new WaitForSeconds(tempoAnimacao);

        Debug.Log("Entrando na casa!");
        SceneManager.LoadScene(nomeDaCenaDestino);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = true;
            Debug.Log("Aperte E para entrar na casa");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            processandoPorta = false; // Reset se sair da área
        }
    }
}