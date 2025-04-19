using UnityEngine;

// Classe que define o comportamento de espingardas (recarga manual, uma bala por vez)
public class Espingarda : ArmaBase
{
    private int balasRecarregadas; // Contador de quantas balas j� foram recarregadas
    private float tempoUltimoSomRecarga; // Tempo do �ltimo som de recarga
    private int balasNecessariasInicial; // N�mero inicial de balas necess�rias pra recarga (fixo durante o processo)
    private int municaoInicial; // Muni��o no in�cio da recarga (pra detectar disparos)

    // Inicializa a espingarda com as refer�ncias externas
    public override void Inicializar(ControlarHUD hud, AudioSource audio, ControlarBraco braco, Rigidbody2D rb, MoverQuadrado mover)
    {
        base.Inicializar(hud, audio, braco, rb, mover); // Chama o m�todo base pra inicializar
        balasRecarregadas = 0; // Reseta o contador de balas recarregadas
        tempoUltimoSomRecarga = 0f; // Reseta o tempo do �ltimo som
        balasNecessariasInicial = 0; // Reseta o n�mero inicial de balas necess�rias
        municaoInicial = 0; // Reseta a muni��o inicial
    }

    // Sobrescreve o m�todo PodeAtirar pra permitir disparos durante a recarga, desde que haja muni��o
    public override bool PodeAtirar(float tempoAtual, float tempoUltimoTiro)
    {
        // Remove a restri��o de !estaRecarregando, permitindo atirar com balas parciais
        return municaoAtual > 0 && tempoAtual >= tempoUltimoTiro + tempoEntreTiros;
    }

    // M�todo que controla a recarga da espingarda (manual, com som pra cada bala)
    public override void Recarregar()
    {
        // Se n�o estiver recarregando, verifica se pode iniciar a recarga
        if (!estaRecarregando)
        {
            if (balasTotais > 0 && municaoAtual < tamanhoCarregador) // Verifica se h� balas dispon�veis e o carregador n�o est� cheio
            {
                estaRecarregando = true; // Marca que a recarga come�ou
                tempoInicioRecarga = Time.time; // Registra o momento do in�cio da recarga
                balasRecarregadas = 0; // Reseta o contador de balas recarregadas
                tempoUltimoSomRecarga = Time.time; // Permite tocar o primeiro som imediatamente
                balasNecessariasInicial = Mathf.Min(tamanhoCarregador - municaoAtual, balasTotais); // Calcula quantas balas ser�o recarregadas (fixo)
                municaoInicial = municaoAtual; // Armazena a muni��o inicial pra detectar disparos
                Debug.Log($"Iniciando recarga da {nomeArma}... Muni��o atual: {municaoAtual}/{tamanhoCarregador}, Balas totais: {balasTotais}, Balas a recarregar: {balasNecessariasInicial}");
            }
            else
            {
                if (balasTotais <= 0)
                {
                    Debug.Log($"Sem balas totais pra recarregar a {nomeArma}!");
                }
                else
                {
                    Debug.Log($"Carregador da {nomeArma} j� est� cheio!");
                }
            }
            return;
        }

        // Calcula quanto tempo passou desde o in�cio da recarga
        float tempoDecorrido = Time.time - tempoInicioRecarga;

        // Calcula o intervalo entre os sons (baseado no n�mero inicial de balas necess�rias)
        float intervaloSom = balasNecessariasInicial > 0 ? tempoRecarga / balasNecessariasInicial : tempoRecarga;

        // Verifica se o jogador disparou durante a recarga (muni��o atual diminuiu em rela��o ao esperado)
        int municaoEsperada = municaoInicial + balasRecarregadas;
        if (municaoAtual < municaoEsperada)
        {
            Debug.Log($"Recarga interrompida! O jogador disparou durante a recarga da {nomeArma}. Muni��o atual: {municaoAtual}/{tamanhoCarregador}");
            estaRecarregando = false; // Interrompe a recarga
            balasRecarregadas = 0; // Reseta o contador
            balasNecessariasInicial = 0; // Reseta o n�mero inicial de balas
            municaoInicial = 0; // Reseta a muni��o inicial
            return;
        }

        // Se n�o h� mais balas pra recarregar, finaliza a recarga
        if (balasTotais <= 0 || municaoAtual >= tamanhoCarregador || balasRecarregadas >= balasNecessariasInicial)
        {
            estaRecarregando = false; // Marca que a recarga terminou
            balasRecarregadas = 0; // Reseta o contador
            balasNecessariasInicial = 0; // Reseta o n�mero inicial de balas
            municaoInicial = 0; // Reseta a muni��o inicial
            Debug.Log($"Recarga manual conclu�da! Muni��o atual: {municaoAtual}/{tamanhoCarregador}, Balas totais da {nomeArma}: {balasTotais}");
            return;
        }

        // Toca o som de recarga pra cada bala e atualiza a muni��o
        if (balasRecarregadas < balasNecessariasInicial && Time.time >= tempoUltimoSomRecarga + intervaloSom)
        {
            // Toca o som de recarga pra cada bala
            if (audioSource != null && somRecarga != null)
            {
                audioSource.PlayOneShot(somRecarga);
                Debug.Log($"Tocando som de recarga (bala {balasRecarregadas + 1}/{balasNecessariasInicial}): {somRecarga.name}");
            }
            balasRecarregadas++; // Incrementa o contador de balas recarregadas
            tempoUltimoSomRecarga = Time.time; // Atualiza o tempo do �ltimo som
            municaoAtual++; // Adiciona uma bala ao carregador
            balasTotais--; // Reduz uma bala do total
            AtualizarHUD(); // Atualiza o HUD com a nova muni��o
        }
    }

    // Retorna o tipo da arma pro HUD (1 = espingarda)
    public override int GetTipoArma()
    {
        return 1;
    }
}