using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSeguePlayer : MonoBehaviour
{
    [Header("Seguir Player")]
    [Tooltip("Referência ao transform do jogador. Será preenchido automaticamente ao iniciar a cena.")]
    public Transform player;
    public float velocidadeCamera = 5f;

    void Start()
    {
        // A forma mais robusta de encontrar o player persistente é usando o Singleton.
        // Isso garante que a câmera sempre encontrará o player correto, mesmo após mudar de cena.
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;
            Debug.Log("✅ Câmera conectada ao Player persistente!");
        }
        else
        {
            // Se, por algum motivo, a câmera iniciar antes do player, este erro aparecerá.
            Debug.LogError("❌ Câmera não encontrou a instância do PlayerController! Verifique se o Player existe na sua cena inicial.");
        }
    }

    // O método FixedUpdate é geralmente melhor para câmeras que seguem objetos de física (Rigidbody)
    // para evitar "tremidas".
    void FixedUpdate()
    {
        // Se não tem player, não faz nada
        if (player == null)
        {
            return;
        }

        SeguirPlayer();
    }

    void SeguirPlayer()
    {
        // Onde a câmera quer ir (seguir o player)
        Vector3 posicaoDesejada = new Vector3(
            player.position.x,  // X do player
            player.position.y,  // Y do player
            transform.position.z // Mantém o Z original da câmera (geralmente -10)
        );

        // Mover suavemente para lá
        transform.position = Vector3.Lerp(
            transform.position,
            posicaoDesejada,
            velocidadeCamera * Time.fixedDeltaTime // Use fixedDeltaTime por estar no FixedUpdate
        );
    }
}