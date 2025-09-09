using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveSimples : MonoBehaviour
{
    public float speed = 3f;
    public float runSpeed = 6f;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : speed;

        transform.Translate(x * currentSpeed * Time.deltaTime, y * currentSpeed * Time.deltaTime, 0);

        // Só 3 estados: Parado, Andando ou Correndo
        if (x == 0 && y == 0)
        {
            anim.SetTrigger("Parado");
        }
        else if (isRunning)
        {
            anim.SetTrigger("Correndo");
        }
        else
        {
            anim.SetTrigger("Andando");
        }

        // Vira sprite quando anda para trás (costa para tela)
        if (y < 0)
        {
            GetComponent<SpriteRenderer>().flipY = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipY = false;
        }
    }
}