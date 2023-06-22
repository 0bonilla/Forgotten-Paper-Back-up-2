using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    private float speed; // Velocidad actual de Lito
    public float LitoSpeed; // Velocidad de Lito tranformado en Lito
    public float BarlitoSpeed; // Velocidad de Lito transformado en Barco estando en el piso
    public float BarlitoWaterMaxSpeed; // Velocidad de Lito transformado en Barco estando en el agua
    private float BarlitoWaterAcceleration = 5f; // Velocidad máxima que puede alcanzar el barco en el agua

    private int LookingAt; // Controlo hacia donde estoy mirando (Izquierda o Derecha)

    public bool water = false; // Controlador de si Lito tocó o no el agua

    //private bool inGround = true; // Variable para controlar si Lito está en el suelo o en una plataforma, y no en el aire
    public float AvionlitoSpeed; // Velocidad de Lito transformado en avion 

    private float JumpForce; // Variable que hace saltar a Lito
    public float LitoJump; // Valor con el que Lito salta
    public float BarlitoJump; // Fuerza de Lito al saltar con el barco

    private bool CanJump; // Controlo cuando puede saltar Lito

    public bool HasBarlito; // Controlo si puedo transformame en Barco
    public bool IsBarlito = false; // Controlo si estoy transformado en Barco

    public bool HasAvionlito; // Controlo si puedo transformame en Avion
    public bool IsAvionlito = false; // Controlo si estoy transformado en Avion

    public Rigidbody2D rb; // El Rigidbody de Lito
    private SpriteRenderer mySpriteRenderer; // Renderer de Lito
    private AnimationLito animLito; // Clase de Animación de Lito

    public Transform respawn; // Punto de Re-Aparición de Lito 
    private Vector2 MoveDirection; // Dirección en la que Lito se mueve

    public int TransformTo; // Controlo en qué me voy a transformar

    private List<string> validTags = new List<string>(); // Hago una lista de Tags válidos

    private float moveX;

    private bool jumpOutOfTheWater = false;
    public bool GrandpaIsTalking = false;


    // Start is called before the first frame update
    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animLito = FindObjectOfType<AnimationLito>();

        validTags.Add("floor");
        validTags.Add("OneWayPlatform");
        validTags.Add("Water");

        StatChange();
    }

    // Update is called once per frame
    void Update()
    {
        // Verifico los inputs que se hacen durante la partida
        InputManager();  
    }

    private void FixedUpdate()
    {
        flip(); // Giro al personaje cuando vaya hacia el otro lado
        moveX = Input.GetAxisRaw("Horizontal"); // Movimiento Horizontal
    }

    void InputManager()
    {
        if (moveX != 0 && !GrandpaIsTalking) // Si me estoy moviendo, hago la animación y llamo al método correspondiente
        {
            CharacterMovement();
        }
        else 
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        /*if((Input.GetKeyUp(KeyCode.A) && !Input.GetKeyDown(KeyCode.D)) || (Input.GetKeyUp(KeyCode.D) && !Input.GetKeyDown(KeyCode.A)))
        {
            AudioManager.Instance.Stop("walk");
        }*/

        MoveDirection = new Vector2(moveX, 0);

        LookingAt = (int) moveX;

        if (Input.GetButtonDown("Jump") && CanJump && !GrandpaIsTalking) // Verifico si Lito está en el piso y se precionó el botón de salto
        {
            Jump();
            AudioManager.Instance.Play("jump");
        }

        /*if(moveX == 0)
        {
            timeToPlay = Time.time;
        }

        if(timeToPlay + timeToCut < Time.time) 
        {
            stopSound = true;
        }*/

        if(!CanJump)
        {
            AudioManager.Instance.Stop("walk");
        }

        CharacterChange();
    }

    void CharacterMovement()
    {
        rb.velocity = new Vector2(MoveDirection.x * speed, rb.velocity.y);

        if (IsAvionlito) // Si el personaje es Avionlito, su velocidad cambia
        {
            rb.velocity = new Vector2(LookingAt * AvionlitoSpeed, -1f); //velocidad constante cuando te toca el avion
        }
    }

    // Método donde se realiza el salto
    void Jump()
    {
        if(CanJump) rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        CanJump = false;
    }

    void OutOfTheWater()
    {
        jumpOutOfTheWater = true;
        rb.AddForce(Vector2.up * JumpForce * 1.8f, ForceMode2D.Impulse);
    }

    // Método para dar vuelta al personaje
    void flip()
    {
       switch (LookingAt)
        {
            case 1:
                mySpriteRenderer.flipX = false;
                break;
            case -1:
                mySpriteRenderer.flipX = true;
                break;
        }
    }

    // Controlo si el personaje quiere transformarse en lito
    void CharacterChange()
    {
        if (Input.GetButtonDown("Transform Lito") && !water && !GrandpaIsTalking && (IsBarlito || IsAvionlito))
        {
            Jump();
            TransformTo = 0;
            IsBarlito = false;
            IsAvionlito = false;
            rb.velocity = new Vector2(0, rb.velocity.y); //reseteo velocidades en X y no en Y
            StatChange();
            animLito.TransformingLito();
            AudioManager.Instance.Play("transform");
        }

        if (Input.GetButtonDown("Transform Lito") && water && !GrandpaIsTalking)
        {
            OutOfTheWater();
            AudioManager.Instance.Play("transform");
        }

        if(Input.GetButtonDown("Transform Bar") && HasBarlito == true && !IsBarlito && !GrandpaIsTalking) //&& inGround
        {
            TransformTo = -1;
            IsBarlito = true;
            IsAvionlito = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
            Jump();
            StatChange();
            animLito.TransformingLito();
            AudioManager.Instance.Play("transform");
        }

        if(Input.GetButtonDown("Transform Avio") && HasAvionlito == true && !IsAvionlito && !GrandpaIsTalking)
        {
            TransformTo = 1;
            IsBarlito = false;
            IsAvionlito = true;
            Jump();
            StatChange();
            animLito.TransformingLito();
            AudioManager.Instance.Play("transform");
        }

    }

    void StatChange()
    {
        // Si Lito está en su estado de Lito, se le asignan las variables predeterminadas
        if (!IsBarlito && !IsAvionlito)
        {
            speed = LitoSpeed;
            JumpForce = LitoJump;
            rb.gravityScale = 2;
        }

        // Si Lito es un barco, verifico si está en el agua o no, y le asigno su velocidad y gravedad
        if (IsBarlito)
        {
            if(water && speed < BarlitoWaterMaxSpeed) 
            {
                speed += BarlitoWaterAcceleration * Time.deltaTime;
            }
            else{
                speed = BarlitoSpeed;
            }
            JumpForce = BarlitoJump;
            rb.gravityScale = 2.5f;
        }
    }

    /*private void OnCollisionStay2D(Collision2D collision)
    {
        // Si Lito toca el agua, siendo Lito, deberá volver al principio del juego
        if (collision.gameObject.tag == "Water" && !IsBarlito)
        {
            transform.position = respawn.transform.position;
            water = false;
        }
        // Si lito tocó el agua siendo barco, y comenzó a moverse, el barco comienza a acelerar
        if (collision.gameObject.tag == "Water" && IsBarlito && speed < BarlitoWaterMaxSpeed)   
        {
            water = true;
            if(moveX != 0) speed += BarlitoWaterAcceleration * Time.deltaTime;
            else speed = BarlitoSpeed;
        }
    }*/

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //solo cuando el jugador no tiene velocidad en Y puede saltar nuevamente
        if (validTags.Contains(collision.gameObject.tag) && rb.velocity.y == 0) 
        {
            CanJump = true;
        }
        // Si Lito choca contra el piso siendo avion, vuelve a ser Lito
        if((collision.gameObject.tag == "floor" || collision.gameObject.tag == "OneWayPlatform") && IsAvionlito)
        {
            //transform.position = respawn.transform.position;
            TransformTo = 0;
            IsBarlito = false;
            IsAvionlito = false;
            rb.velocity = new Vector2(0, rb.velocity.y); //reseteo velocidades en X y no en Y
            Jump();
            StatChange();
            animLito.TransformingLito();
            CanJump = true;
            AudioManager.Instance.Stop("wind");
        }
        if(collision.gameObject.tag == "floor" || collision.gameObject.tag == "OneWayPlatform")
        {
            //inGround = true;
            jumpOutOfTheWater = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collisionInfo)
    {
        // Si Lito salió del agua, pero sigue siendo barco, le asigno la velocidad correspondiente
        if (collisionInfo.gameObject.tag == "Water" && IsBarlito)   
        {
            water = false;
            speed = BarlitoSpeed;
        }
        if (validTags.Contains(collisionInfo.gameObject.tag))
        {
            CanJump = false;
        }
        /*if(collisionInfo.gameObject.tag == "floor")
        {
            inGround = false;
        }*/
    }

    // Verifico que Lito tocó el área final del nivel
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Final")
        {
            SceneLoader.Instance.mainMenu();
        }
        if(other.gameObject.tag == "Pencil")
        {
            GameManager.Instance.recolectados++;
            Destroy(other.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Water" && IsBarlito)   
        {
            water = true;
            rb.gravityScale = 2f;
            if(moveX != 0 && speed < BarlitoWaterMaxSpeed) speed += BarlitoWaterAcceleration * Time.deltaTime;
            else if (moveX == 0) speed = BarlitoSpeed;
            AudioManager.Instance.Play("water");
        }
        if (other.gameObject.tag == "Water" && !IsBarlito)
        {
            transform.position = respawn.transform.position;
            water = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Water" && IsBarlito && jumpOutOfTheWater)
        {
            TransformTo = 0;
            water = false;
            IsBarlito = false;
            IsAvionlito = false;
            rb.velocity = new Vector2(0, rb.velocity.y); //reseteo velocidades en X y no en Y
            StatChange();
            animLito.TransformingLito();
        }
    }

    public void PlayWind()
    {    
        AudioManager.Instance.Play("wind");
    }
    public void PlayWalk()
    {
        AudioManager.Instance.Play("walk");
    }
}
