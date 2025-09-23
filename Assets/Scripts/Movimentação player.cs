using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveSimples : MonoBehaviour
{
    public float speed = 3f;
    public float runSpeed = 6f;
    private Animator anim;
    private string currentState = "";
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        // Mover o jogador
        transform.Translate(x * speed * Time.deltaTime, y * speed * Time.deltaTime, 0);
        // Mostrar no console o que está acontecendo
        Debug.Log("X: " + x + " Y: " + y);
        // Decidir qual animação tocar
        string newState = "";

        if (x == 0 && y == 0)
        {
            newState = "Parado";
        }
        else if (y < 0)
        {
            newState = "CostaRun";
        }
        else if (y > 0)
        {
            newState = "FrenteRun";
        }
        else if (x > 0)
        {
            newState = "Idle rigth";
        }
        else if (x < 0)
        {
            newState = "Idle left";
        }

        // Só chama o trigger se mudou de estado
        if (newState != currentState)
        {
            // Limpa o trigger anterior para evitar acúmulo
            if (!string.IsNullOrEmpty(currentState))
            {
                anim.ResetTrigger(currentState);
            }

            Debug.Log("Chamando: " + newState);
            anim.SetTrigger(newState);
            currentState = newState;
        }
    }
}