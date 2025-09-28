using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveSimples : MonoBehaviour
{
    public float speed = 3f;
    public float runSpeed = 3f;
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

        // ADICIONE ESTAS LINHAS AQUI:
        anim.SetFloat("Horizontal", x);
        anim.SetFloat("Vertical", y);
        anim.SetFloat("Speed", Mathf.Abs(x) + Mathf.Abs(y));

        // Mover o jogador
        transform.Translate(x * speed * Time.deltaTime, y * speed * Time.deltaTime, 0);

        // O resto do seu código continua igual...
        Debug.Log("X: " + x + " Y: " + y);

        // Decidir qual animação tocar
        string newState = "";

        if (x == 0 && y == 0)
        {
            newState = "Parado";
        }
    }
    }