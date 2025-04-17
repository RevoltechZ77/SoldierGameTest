using UnityEngine;

public class ControlarBraco : MonoBehaviour
{
    private MoverQuadrado moverQuadrado; // Refer�ncia ao script MoverQuadrado pra direcaoFlip
    private bool estaRecuando; // Controla se o Braco t� recuando
    private float tempoRecuo; // Tempo atual do recuo
    private float duracaoRecuo = 0.1f; // Dura��o do recuo do Braco
    private Vector3 deslocamentoRecuo; // Deslocamento do recuo do Braco
    private Vector3 posicaoOriginal; // Posi��o original do Braco sem recuo
    private SpriteRenderer spriteRenderer; // Componente pra flipar o sprite do Braco
    public Vector3 offsetBraco; // Offset do Braco em rela��o ao Quadrado (ajust�vel no Inspector)

    void Start()
    {
        // Pega o script MoverQuadrado do Quadrado (pai)
        moverQuadrado = GetComponentInParent<MoverQuadrado>();
        if (moverQuadrado == null)
        {
            Debug.LogError("MoverQuadrado n�o encontrado no Quadrado!");
            enabled = false;
            return;
        }
        // Pega o SpriteRenderer do Braco
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer n�o encontrado no Braco!");
            enabled = false;
            return;
        }
        // Salva a posi��o original do Braco
        posicaoOriginal = offsetBraco;
        transform.localPosition = posicaoOriginal;
    }

    void Update()
    {
        // Pega a dire��o de flip do Quadrado (1: direita, -1: esquerda)
        float direcaoFlip = moverQuadrado.GetDirecaoFlip();
        // Flipa o sprite do Braco se olhando pra esquerda
        spriteRenderer.flipX = (direcaoFlip < 0);
        // Ajusta a posi��o do Braco com base no flip pra manter o ombro no lugar
        Vector3 posicaoAjustada = offsetBraco;
        posicaoAjustada.x *= direcaoFlip;
        transform.localPosition = posicaoAjustada;

        // Converte a posi��o do cursor de tela pra mundo
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        // Calcula a dire��o do Braco at� o cursor
        Vector2 direcao = (mousePos - transform.position).normalized;
        // Calcula o �ngulo base da dire��o
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

        // Separa os c�lculos com base no flip
        if (direcaoFlip > 0) // Apontando pra direita (positivo)
        {
            // Sem ajustes adicionais, rota��o normal
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }
        else // Apontando pra esquerda (negativo)
        {
            // Ajusta o �ngulo pra compensar o flipX
            // O flipX inverte o eixo X do sprite, o que faz o eixo Y (cima/baixo) parecer invertido
            // Precisamos refletir o �ngulo em torno do eixo Y pra corrigir isso
            angulo = 180f + angulo;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }

        // Controla o recuo visual do Braco
        if (estaRecuando)
        {
            tempoRecuo += Time.deltaTime; // Incrementa o tempo do recuo
            float t = tempoRecuo / duracaoRecuo; // Progresso do recuo (0 a 1)
            if (t >= 1f)
            {
                estaRecuando = false; // Termina o recuo
                transform.localPosition = posicaoAjustada; // Volta � posi��o ajustada
            }
            else
            {
                // Interpola entre a posi��o recuada e a ajustada
                transform.localPosition = Vector3.Lerp(posicaoAjustada + deslocamentoRecuo, posicaoAjustada, t);
            }
        }
    }

    public void AplicarRecuo(Vector3 direcao, float distanciaRecuo)
    {
        // Aplica recuo ao Braco (metade do recuo da arma)
        deslocamentoRecuo = -direcao * (distanciaRecuo * 0.5f);
        estaRecuando = true;
        tempoRecuo = 0f;
    }
}