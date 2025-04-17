using UnityEngine;

public class MoverQuadrado : MonoBehaviour
{
    public float velocidade = 5f; // Velocidade de movimento
    public float forcaPulo = 5f; // Força do pulo
    private Rigidbody2D rb;
    private bool noChao = true; // Verifica se tá no chão
    private Animator animator; // Referência ao Animator
    public LayerMask camadaChao; // Layer do chão
    public Vector2 tamanhoCaixaChao = new Vector2(0.8f, 0.2f); // Tamanho da caixa de checagem
    public Transform pontoChecagemChao; // Ponto logo abaixo do Quadrado
    private SpriteRenderer spriteRenderer; // Referência ao SpriteRenderer
    private float direcaoFlip = 1f; // 1 (direita), -1 (esquerda)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D não encontrado no Quadrado!");
            enabled = false;
            return;
        }
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator não encontrado no Quadrado!");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer não encontrado no Quadrado!");
            enabled = false;
            return;
        }
        if (pontoChecagemChao == null)
        {
            Debug.LogWarning("PontoChecagemChao não definido! Criando automaticamente.");
            GameObject ponto = new GameObject("PontoChecagemChao");
            ponto.transform.SetParent(transform);
            ponto.transform.localPosition = new Vector3(0, -0.5f, 0); // Abaixo do Quadrado
            pontoChecagemChao = ponto.transform;
        }
    }

    void Update()
    {
        // Movimento horizontal
        float movimentoX = Input.GetAxisRaw("Horizontal"); // -1 (A), 0, 1 (D)
        rb.linearVelocity = new Vector2(movimentoX * velocidade, rb.linearVelocity.y);

        // Flip com base no cursor
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool cursorAtivo = Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0; // Cursor se moveu
        if (cursorAtivo && mousePos.x < transform.position.x)
        {
            spriteRenderer.flipX = true; // Olha pra esquerda
            direcaoFlip = -1f;
        }
        else if (cursorAtivo && mousePos.x > transform.position.x)
        {
            spriteRenderer.flipX = false; // Olha pra direita
            direcaoFlip = 1f;
        }
        // Flip com base no movimento, se cursor não tá ativo
        else if (movimentoX < 0)
        {
            spriteRenderer.flipX = true; // Olha pra esquerda
            direcaoFlip = -1f;
        }
        else if (movimentoX > 0)
        {
            spriteRenderer.flipX = false; // Olha pra direita
            direcaoFlip = 1f;
        }

        // Passa velocidade pro Animator
        if (animator != null)
        {
            animator.SetFloat("VelocidadeX", Mathf.Abs(movimentoX));
        }

        // Pulo
        if (Input.GetKeyDown(KeyCode.Space) && noChao)
        {
            rb.AddForce(Vector2.up * forcaPulo, ForceMode2D.Impulse);
            noChao = false;
            Debug.Log("Pulo executado!");
        }
    }

    void FixedUpdate()
    {
        // Verifica se tá no chão
        noChao = Physics2D.OverlapBox(pontoChecagemChao.position, tamanhoCaixaChao, 0f, camadaChao);
      
    }

    void OnDrawGizmos()
    {
        // Desenha caixa de checagem no Editor pra depuração
        if (pontoChecagemChao != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pontoChecagemChao.position, tamanhoCaixaChao);
        }
    }

    public void AplicarCoice()
    {
        Debug.Log("Coice recebido no MoverQuadrado!");
    }

    public float GetDirecaoFlip()
    {
        return direcaoFlip;
    }
}