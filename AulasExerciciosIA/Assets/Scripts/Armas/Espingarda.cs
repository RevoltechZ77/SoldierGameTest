using UnityEngine;

// Classe que define o comportamento de espingardas (recarga manual, uma bala por vez)
public class Espingarda : ArmaBase
{
    private int balasRecarregadas; // Contador de quantas balas já foram recarregadas
    private float tempoUltimoSomRecarga; // Tempo do último som de recarga
    private int balasNecessariasInicial; // Número inicial de balas necessárias pra recarga (fixo durante o processo)
    private int municaoInicial; // Munição no início da recarga (pra detectar disparos)

    // Inicializa a espingarda com as referências externas
    public override void Inicializar(ControlarHUD hud, AudioSource audio, ControlarBraco braco, Rigidbody2D rb, MoverQuadrado mover)
    {
        base.Inicializar(hud, audio, braco, rb, mover); // Chama o método base pra inicializar
        balasRecarregadas = 0; // Reseta o contador de balas recarregadas
        tempoUltimoSomRecarga = 0f; // Reseta o tempo do último som
        balasNecessariasInicial = 0; // Reseta o número inicial de balas necessárias
        municaoInicial = 0; // Reseta a munição inicial
    }

    // Sobrescreve o método PodeAtirar pra permitir disparos durante a recarga, desde que haja munição
    public override bool PodeAtirar(float tempoAtual, float tempoUltimoTiro)
    {
        // Remove a restrição de !estaRecarregando, permitindo atirar com balas parciais
        return municaoAtual > 0 && tempoAtual >= tempoUltimoTiro + tempoEntreTiros;
    }

    // Método que controla a recarga da espingarda (manual, com som pra cada bala)
    public override void Recarregar()
    {
        // Se não estiver recarregando, verifica se pode iniciar a recarga
        if (!estaRecarregando)
        {
            if (balasTotais > 0 && municaoAtual < tamanhoCarregador) // Verifica se há balas disponíveis e o carregador não está cheio
            {
                estaRecarregando = true; // Marca que a recarga começou
                tempoInicioRecarga = Time.time; // Registra o momento do início da recarga
                balasRecarregadas = 0; // Reseta o contador de balas recarregadas
                tempoUltimoSomRecarga = Time.time; // Permite tocar o primeiro som imediatamente
                balasNecessariasInicial = Mathf.Min(tamanhoCarregador - municaoAtual, balasTotais); // Calcula quantas balas serão recarregadas (fixo)
                municaoInicial = municaoAtual; // Armazena a munição inicial pra detectar disparos
                Debug.Log($"Iniciando recarga da {nomeArma}... Munição atual: {municaoAtual}/{tamanhoCarregador}, Balas totais: {balasTotais}, Balas a recarregar: {balasNecessariasInicial}");
            }
            else
            {
                if (balasTotais <= 0)
                {
                    Debug.Log($"Sem balas totais pra recarregar a {nomeArma}!");
                }
                else
                {
                    Debug.Log($"Carregador da {nomeArma} já está cheio!");
                }
            }
            return;
        }

        // Calcula quanto tempo passou desde o início da recarga
        float tempoDecorrido = Time.time - tempoInicioRecarga;

        // Calcula o intervalo entre os sons (baseado no número inicial de balas necessárias)
        float intervaloSom = balasNecessariasInicial > 0 ? tempoRecarga / balasNecessariasInicial : tempoRecarga;

        // Verifica se o jogador disparou durante a recarga (munição atual diminuiu em relação ao esperado)
        int municaoEsperada = municaoInicial + balasRecarregadas;
        if (municaoAtual < municaoEsperada)
        {
            Debug.Log($"Recarga interrompida! O jogador disparou durante a recarga da {nomeArma}. Munição atual: {municaoAtual}/{tamanhoCarregador}");
            estaRecarregando = false; // Interrompe a recarga
            balasRecarregadas = 0; // Reseta o contador
            balasNecessariasInicial = 0; // Reseta o número inicial de balas
            municaoInicial = 0; // Reseta a munição inicial
            return;
        }

        // Se não há mais balas pra recarregar, finaliza a recarga
        if (balasTotais <= 0 || municaoAtual >= tamanhoCarregador || balasRecarregadas >= balasNecessariasInicial)
        {
            estaRecarregando = false; // Marca que a recarga terminou
            balasRecarregadas = 0; // Reseta o contador
            balasNecessariasInicial = 0; // Reseta o número inicial de balas
            municaoInicial = 0; // Reseta a munição inicial
            Debug.Log($"Recarga manual concluída! Munição atual: {municaoAtual}/{tamanhoCarregador}, Balas totais da {nomeArma}: {balasTotais}");
            return;
        }

        // Toca o som de recarga pra cada bala e atualiza a munição
        if (balasRecarregadas < balasNecessariasInicial && Time.time >= tempoUltimoSomRecarga + intervaloSom)
        {
            // Toca o som de recarga pra cada bala
            if (audioSource != null && somRecarga != null)
            {
                audioSource.PlayOneShot(somRecarga);
                Debug.Log($"Tocando som de recarga (bala {balasRecarregadas + 1}/{balasNecessariasInicial}): {somRecarga.name}");
            }
            balasRecarregadas++; // Incrementa o contador de balas recarregadas
            tempoUltimoSomRecarga = Time.time; // Atualiza o tempo do último som
            municaoAtual++; // Adiciona uma bala ao carregador
            balasTotais--; // Reduz uma bala do total
            AtualizarHUD(); // Atualiza o HUD com a nova munição
        }
    }

    // Retorna o tipo da arma pro HUD (1 = espingarda)
    public override int GetTipoArma()
    {
        return 1;
    }
}