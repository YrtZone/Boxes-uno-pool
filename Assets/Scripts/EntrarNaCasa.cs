using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrarNaCasa : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeDaCenaInterior = "InteriorDaCasa";
    public KeyCode teclaPraEntrar = KeyCode.E;

    private bool jogadorPerto = false;

    void Update()
    {
        // Debug para saber se está detectando a tecla
        if (Input.GetKeyDown(teclaPraEntrar))
        {
            Debug.Log("Tecla E pressionada! Jogador perto: " + jogadorPerto);
        }

        // Se o jogador está perto e aperta E
        if (jogadorPerto && Input.GetKeyDown(teclaPraEntrar))
        {
            Debug.Log("TENTANDO CARREGAR CENA: " + nomeDaCenaInterior);
            SceneManager.LoadScene(nomeDaCenaInterior);
        }
    }

    // Quando o jogador entra na área da porta
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Algo entrou na porta: " + other.name);
        if (other.CompareTag("Player"))
        {
            jogadorPerto = true;
            Debug.Log("PLAYER DETECTADO! Aperte E para entrar na casa");
        }
        else
        {
            Debug.Log("Não é o player. Tag atual: " + other.tag);
        }
    }

    // Quando o jogador sai da área da porta
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            Debug.Log("Saiu da porta");
        }
    }
}