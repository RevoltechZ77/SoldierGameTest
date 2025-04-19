using UnityEngine;

// Classe abstrata base para todas as armas. Define propriedades e m�todos comuns.
public abstract class ArmaBase : MonoBehaviour
{
    // --- Propriedades comuns a todas as armas (v�m do ScriptableObject ArmaConfig) ---
    public string nomeArma; // Nome da arma (ex.: "Carrion 9mm", "ESP Cano Curto")
    public int tamanhoCarregador; // Quantidade m�xima de balas no carregador
    public int municaoAtual; // Quantidade atual de balas no carregador
    public int balasTotais; // Balas dispon�veis fora do carregador
    public float tempoEntreTiros; // Intervalo entre disparos (cad�ncia)
    public float tempoRecarga; // Tempo necess�rio pra recarregar a arma
    public float tempoAtrasoRecargaZerada; // Tempo de atraso quando a recarga � iniciada com o carregador zerado
    public float forcaRecuo; // For�a do recuo visual da arma
    public float forcaRecuoBraco; // Multiplicador do recuo pro bra�o
    public float forcaCoice; // For�a do coice aplicado ao jogador
    public bool coice; // Se a arma aplica coice ao jogador
    public float angulo; // �ngulo de dispers�o dos proj�teis (ex.: pra espingarda)
    public int projeteisPorTiro; // Quantidade de proj�teis por disparo (ex.: 3 pra espingarda)
    public float velocidadeProjetil; // Velocidade do proj�til
    public float alcance; // Dist�ncia m�xima que o proj�til percorre
    public float rotacaoSpriteProjetil; // Rota��o adicional do sprite do proj�til
    public Vector3 offsetArma; // Posi��o relativa da arma em rela��o ao bra�o
    public Vector3 offsetPontaArma; // Posi��o da ponta da arma (onde o proj�til � instanciado)
    public GameObject projetilPrefab; // Prefab do proj�til
    public AudioClip somDisparo; // Som do disparo
    public AudioClip somRecarga; // Som da recarga
    public Sprite spriteArma; // Sprite visual da arma

    // --- Refer�ncias externas (componentes do jogo) ---
    protected ControlarHUD controlarHUD; // Script do HUD pra atualizar a interface
    protected AudioSource audioSource; // Componente pra tocar sons
    protected ControlarBraco controlarBraco; // Script do bra�o pra aplicar recuo
    protected Rigidbody2D soldadoRb; // Rigidbody do jogador pra aplicar coice
    protected MoverQuadrado moverSoldado; // Script de movimento do jogador

    // --- Estado da recarga ---
    public bool estaRecarregando; // Indica se a arma est� recarregando (p�blico pra ser acessado por ControlarArma)
    protected float tempoInicioRecarga; // Momento em que a recarga come�ou

    // M�todo pra inicializar a arma com as refer�ncias externas
    public virtual void Inicializar(ControlarHUD hud, AudioSource audio, ControlarBraco braco, Rigidbody2D rb, MoverQuadrado mover)
    {
        controlarHUD = hud; // Inicializa o HUD
        audioSource = audio; // Inicializa o componente de �udio
        controlarBraco = braco; // Inicializa o script do bra�o
        soldadoRb = rb; // Inicializa o Rigidbody do jogador
        moverSoldado = mover; // Inicializa o script de movimento
        municaoAtual = tamanhoCarregador; // Come�a com o carregador cheio
        balasTotais = 300; // Inicia com 300 balas totais
        estaRecarregando = false; // N�o est� recarregando no in�cio
    }

    // M�todo abstrato que cada arma deve implementar pra definir como recarrega
    public abstract void Recarregar();

    // M�todo pra atualizar o HUD com as informa��es da arma (p�blico pra ser chamado por ControlarArma)
    public void AtualizarHUD()
    {
        if (controlarHUD != null)
        {
            // Chama o m�todo do HUD pra atualizar a interface com a muni��o atual, total, nome e sprite da arma
            controlarHUD.AtualizarMunicao(municaoAtual, tamanhoCarregador, balasTotais, nomeArma, spriteArma, GetTipoArma());
        }
    }

    // M�todo abstrato pra retornar o tipo da arma (usado pelo HUD)
    public abstract int GetTipoArma();

    // Verifica se a arma pode atirar com base no tempo e na muni��o
    // Marcado como virtual pra permitir que classes derivadas o sobrescrevam
    public virtual bool PodeAtirar(float tempoAtual, float tempoUltimoTiro)
    {
        return !estaRecarregando && municaoAtual > 0 && tempoAtual >= tempoUltimoTiro + tempoEntreTiros;
    }

    // M�todo pra disparar a arma
    public void Atirar(Vector3 posicaoArma, Quaternion rotacaoBraco, float direcaoFlip, Vector3 mousePos)
    {
        // Toca o som de disparo
        if (audioSource != null && somDisparo != null)
        {
            audioSource.PlayOneShot(somDisparo);
            Debug.Log($"Tocando som de disparo: {somDisparo.name}");
        }

        // Calcula a dire��o do tiro (do bra�o at� o cursor)
        Vector3 direcao = (mousePos - posicaoArma).normalized;
        float distanciaRecuo = forcaRecuo * 0.05f; // Calcula o deslocamento do recuo visual
        Vector3 deslocamentoRecuo = -direcao * distanciaRecuo; // Deslocamento na dire��o oposta ao tiro

        // Aplica recuo ao bra�o
        if (controlarBraco != null)
        {
            float recuoBraco = (forcaRecuo * 0.05f) * forcaRecuoBraco;
            controlarBraco.AplicarRecuo(direcao, recuoBraco);
        }

        // Aplica coice ao jogador, se configurado
        if (coice)
        {
            Vector2 direcaoCoice = -direcao.normalized; // Dire��o oposta ao tiro
            soldadoRb.AddForce(direcaoCoice * forcaCoice, ForceMode2D.Impulse); // Aplica for�a
            if (moverSoldado != null)
            {
                moverSoldado.AplicarCoice(); // Notifica o script de movimento
            }
        }

        // Loop pra criar cada proj�til (ex.: 1 pra pistola, 3 pra espingarda)
        for (int i = 0; i < projeteisPorTiro; i++)
        {
            // Calcula o desvio angular pra m�ltiplos proj�teis (ex.: -5�, 0�, +5� pra espingarda)
            float t = projeteisPorTiro > 1 ? (float)i / (projeteisPorTiro - 1) : 0f;
            float desvio = Mathf.Lerp(-angulo, angulo, t);
            Vector3 direcaoRotacionada = Quaternion.Euler(0, 0, desvio) * direcao;
            Vector2 direcaoDesvio = new Vector2(direcaoRotacionada.x, direcaoRotacionada.y).normalized;
            float anguloTiro = Mathf.Atan2(direcaoDesvio.y, direcaoDesvio.x) * Mathf.Rad2Deg;
            float anguloFinal = anguloTiro + rotacaoSpriteProjetil; // Ajusta a rota��o do sprite do proj�til
            Quaternion rotacaoDesvio = Quaternion.Euler(0, 0, anguloFinal);

            // Ajusta a posi��o da ponta da arma com base no flip (esquerda/direita)
            Vector3 offsetPontaArmaAjustado = offsetPontaArma;
            offsetPontaArmaAjustado.x *= direcaoFlip;
            Vector3 posicaoTiro = posicaoArma + rotacaoBraco * offsetPontaArmaAjustado; // Posi��o de spawn do proj�til

            // Instancia o proj�til
            GameObject projetil = Instantiate(projetilPrefab, posicaoTiro, rotacaoDesvio);
            ControlarProjetil controlarProjetil = projetil.GetComponent<ControlarProjetil>();
            if (controlarProjetil != null)
            {
                controlarProjetil.SetAlcance(alcance); // Define o alcance do proj�til
            }

            // Aplica velocidade ao proj�til
            Rigidbody2D rbProjetil = projetil.GetComponent<Rigidbody2D>();
            Vector2 velocidadeFinal = direcaoDesvio * this.velocidadeProjetil; // Usa a propriedade velocidadeProjetil da classe
            rbProjetil.linearVelocity = velocidadeFinal; // Define a velocidade do proj�til
        }

        municaoAtual--; // Reduz a muni��o do carregador
        AtualizarHUD(); // Atualiza o HUD com a nova quantidade de muni��o
    }
}