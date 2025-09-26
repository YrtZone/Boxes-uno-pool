using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntrarNaCasa : MonoBehaviour
{
    [Header("Configura��es")]
    public string nomeDaCenaInterior = "InteriorDaCasa";
    public KeyCode teclaPraEntrar = KeyCode.E;

    private bool jogadorPerto = false;

    void Update()
    {
        // Debug para saber se est� detectando a tecla
        if (Input.GetKeyDown(teclaPraEntrar))
        {
            Debug.Log("Tecla E pressionada! Jogador perto: " + jogadorPerto);
        }

        // Se o jogador est� perto e aperta E
        if (jogadorPerto && Input.GetKeyDown(teclaPraEntrar))
        {
            Debug.Log("TENTANDO CARREGAR CENA: " + nomeDaCenaInterior);
            SceneManager.LoadScene(nomeDaCenaInterior);
        }
    }

    // Quando o jogador entra na �rea da porta
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
            Debug.Log("N�o � o player. Tag atual: " + other.tag);
        }
    }

    // Quando o jogador sai da �rea da porta
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            Debug.Log("Saiu da porta");
        }
    }
}