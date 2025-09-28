using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSeguePlayer : MonoBehaviour
{
    [Header("Seguir Player")]
    public Transform player; // ⭐ ARRASTE SEU PLAYER AQUI! ⭐
    public float velocidadeCamera = 5f;

    void Start()
    {
        // Se não colocou o player, tenta encontrar automaticamente
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("✅ Player encontrado!");
            }
            else
            {
                Debug.LogError("❌ PLAYER NÃO ENCONTRADO! Arraste o Player na caixinha!");
            }
        }
    }

    void Update()
    {
        SeguirPlayer();
    }

    void SeguirPlayer()
    {
        // Se não tem player, não faz nada
        if (player == null)
        {
            Debug.Log("⚠️ Player não conectado!");
            return;
        }

        // Onde a câmera quer ir (seguir o player)
        Vector3 posicaoDesejada = new Vector3(
            player.position.x,  // X do player
            player.position.y,  // Y do player
            -10                 // Z sempre -10 (câmera longe)
        );

        // Mover suavemente para lá
        transform.position = Vector3.Lerp(
            transform.position,
            posicaoDesejada,
            velocidadeCamera * Time.deltaTime
        );
    }
}