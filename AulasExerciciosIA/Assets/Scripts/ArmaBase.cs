using UnityEngine;

// Classe abstrata base para todas as armas. Define propriedades e métodos comuns.
public abstract class ArmaBase : MonoBehaviour
{
    // --- Propriedades comuns a todas as armas (vêm do ScriptableObject ArmaConfig) ---
    public string nomeArma; // Nome da arma (ex.: "Carrion 9mm", "ESP Cano Curto")
    public int tamanhoCarregador; // Quantidade máxima de balas no carregador
    public int municaoAtual; // Quantidade atual de balas no carregador
    public int balasTotais; // Balas disponíveis fora do carregador
    public float tempoEntreTiros; // Intervalo entre disparos (cadência)
    public float tempoRecarga; // Tempo necessário pra recarregar a arma
    public float tempoAtrasoRecargaZerada; // Tempo de atraso quando a recarga é iniciada com o carregador zerado
    public float forcaRecuo; // Força do recuo visual da arma
    public float forcaRecuoBraco; // Multiplicador do recuo pro braço
    public float forcaCoice; // Força do coice aplicado ao jogador
    public bool coice; // Se a arma aplica coice ao jogador
    public float angulo; // Ângulo de dispersão dos projéteis (ex.: pra espingarda)
    public int projeteisPorTiro; // Quantidade de projéteis por disparo (ex.: 3 pra espingarda)
    public float velocidadeProjetil; // Velocidade do projétil
    public float alcance; // Distância máxima que o projétil percorre
    public float rotacaoSpriteProjetil; // Rotação adicional do sprite do projétil
    public Vector3 offsetArma; // Posição relativa da arma em relação ao braço
    public Vector3 offsetPontaArma; // Posição da ponta da arma (onde o projétil é instanciado)
    public GameObject projetilPrefab; // Prefab do projétil
    public AudioClip somDisparo; // Som do disparo
    public AudioClip somRecarga; // Som da recarga
    public Sprite spriteArma; // Sprite visual da arma

    // --- Referências externas (componentes do jogo) ---
    protected ControlarHUD controlarHUD; // Script do HUD pra atualizar a interface
    protected AudioSource audioSource; // Componente pra tocar sons
    protected ControlarBraco controlarBraco; // Script do braço pra aplicar recuo
    protected Rigidbody2D soldadoRb; // Rigidbody do jogador pra aplicar coice
    protected MoverQuadrado moverSoldado; // Script de movimento do jogador

    // --- Estado da recarga ---
    public bool estaRecarregando; // Indica se a arma está recarregando (público pra ser acessado por ControlarArma)
    protected float tempoInicioRecarga; // Momento em que a recarga começou

    // Método pra inicializar a arma com as referências externas
    public virtual void Inicializar(ControlarHUD hud, AudioSource audio, ControlarBraco braco, Rigidbody2D rb, MoverQuadrado mover)
    {
        controlarHUD = hud; // Inicializa o HUD
        audioSource = audio; // Inicializa o componente de áudio
        controlarBraco = braco; // Inicializa o script do braço
        soldadoRb = rb; // Inicializa o Rigidbody do jogador
        moverSoldado = mover; // Inicializa o script de movimento
        municaoAtual = tamanhoCarregador; // Começa com o carregador cheio
        balasTotais = 300; // Inicia com 300 balas totais
        estaRecarregando = false; // Não está recarregando no início
    }

    // Método abstrato que cada arma deve implementar pra definir como recarrega
    public abstract void Recarregar();

    // Método pra atualizar o HUD com as informações da arma (público pra ser chamado por ControlarArma)
    public void AtualizarHUD()
    {
        if (controlarHUD != null)
        {
            // Chama o método do HUD pra atualizar a interface com a munição atual, total, nome e sprite da arma
            controlarHUD.AtualizarMunicao(municaoAtual, tamanhoCarregador, balasTotais, nomeArma, spriteArma, GetTipoArma());
        }
    }

    // Método abstrato pra retornar o tipo da arma (usado pelo HUD)
    public abstract int GetTipoArma();

    // Verifica se a arma pode atirar com base no tempo e na munição
    // Marcado como virtual pra permitir que classes derivadas o sobrescrevam
    public virtual bool PodeAtirar(float tempoAtual, float tempoUltimoTiro)
    {
        return !estaRecarregando && municaoAtual > 0 && tempoAtual >= tempoUltimoTiro + tempoEntreTiros;
    }

    // Método pra disparar a arma
    public void Atirar(Vector3 posicaoArma, Quaternion rotacaoBraco, float direcaoFlip, Vector3 mousePos)
    {
        // Toca o som de disparo
        if (audioSource != null && somDisparo != null)
        {
            audioSource.PlayOneShot(somDisparo);
            Debug.Log($"Tocando som de disparo: {somDisparo.name}");
        }

        // Calcula a direção do tiro (do braço até o cursor)
        Vector3 direcao = (mousePos - posicaoArma).normalized;
        float distanciaRecuo = forcaRecuo * 0.05f; // Calcula o deslocamento do recuo visual
        Vector3 deslocamentoRecuo = -direcao * distanciaRecuo; // Deslocamento na direção oposta ao tiro

        // Aplica recuo ao braço
        if (controlarBraco != null)
        {
            float recuoBraco = (forcaRecuo * 0.05f) * forcaRecuoBraco;
            controlarBraco.AplicarRecuo(direcao, recuoBraco);
        }

        // Aplica coice ao jogador, se configurado
        if (coice)
        {
            Vector2 direcaoCoice = -direcao.normalized; // Direção oposta ao tiro
            soldadoRb.AddForce(direcaoCoice * forcaCoice, ForceMode2D.Impulse); // Aplica força
            if (moverSoldado != null)
            {
                moverSoldado.AplicarCoice(); // Notifica o script de movimento
            }
        }

        // Loop pra criar cada projétil (ex.: 1 pra pistola, 3 pra espingarda)
        for (int i = 0; i < projeteisPorTiro; i++)
        {
            // Calcula o desvio angular pra múltiplos projéteis (ex.: -5°, 0°, +5° pra espingarda)
            float t = projeteisPorTiro > 1 ? (float)i / (projeteisPorTiro - 1) : 0f;
            float desvio = Mathf.Lerp(-angulo, angulo, t);
            Vector3 direcaoRotacionada = Quaternion.Euler(0, 0, desvio) * direcao;
            Vector2 direcaoDesvio = new Vector2(direcaoRotacionada.x, direcaoRotacionada.y).normalized;
            float anguloTiro = Mathf.Atan2(direcaoDesvio.y, direcaoDesvio.x) * Mathf.Rad2Deg;
            float anguloFinal = anguloTiro + rotacaoSpriteProjetil; // Ajusta a rotação do sprite do projétil
            Quaternion rotacaoDesvio = Quaternion.Euler(0, 0, anguloFinal);

            // Ajusta a posição da ponta da arma com base no flip (esquerda/direita)
            Vector3 offsetPontaArmaAjustado = offsetPontaArma;
            offsetPontaArmaAjustado.x *= direcaoFlip;
            Vector3 posicaoTiro = posicaoArma + rotacaoBraco * offsetPontaArmaAjustado; // Posição de spawn do projétil

            // Instancia o projétil
            GameObject projetil = Instantiate(projetilPrefab, posicaoTiro, rotacaoDesvio);
            ControlarProjetil controlarProjetil = projetil.GetComponent<ControlarProjetil>();
            if (controlarProjetil != null)
            {
                controlarProjetil.SetAlcance(alcance); // Define o alcance do projétil
            }

            // Aplica velocidade ao projétil
            Rigidbody2D rbProjetil = projetil.GetComponent<Rigidbody2D>();
            Vector2 velocidadeFinal = direcaoDesvio * this.velocidadeProjetil; // Usa a propriedade velocidadeProjetil da classe
            rbProjetil.linearVelocity = velocidadeFinal; // Define a velocidade do projétil
        }

        municaoAtual--; // Reduz a munição do carregador
        AtualizarHUD(); // Atualiza o HUD com a nova quantidade de munição
    }
}