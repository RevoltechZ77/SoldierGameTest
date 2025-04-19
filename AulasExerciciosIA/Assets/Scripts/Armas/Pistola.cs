using UnityEngine;

// Classe que define o comportamento de pistolas (recarga autom�tica com atraso no som)
public class Pistola : ArmaBase
{
    private bool somRecargaTocou; // Controla se o som da recarga j� foi tocado
    private float tempoUltimaAtualizacaoHUD; // Tempo da �ltima atualiza��o do HUD
    private float atrasoAtualizacaoHUD = 0.6f; // Atraso entre o som e a atualiza��o visual (em segundos)

    // Inicializa a pistola com as refer�ncias externas
    public override void Inicializar(ControlarHUD hud, AudioSource audio, ControlarBraco braco, Rigidbody2D rb, MoverQuadrado mover)
    {
        base.Inicializar(hud, audio, braco, rb, mover); // Chama o m�todo base pra inicializar
        somRecargaTocou = false; // Reseta o controle do som
        tempoUltimaAtualizacaoHUD = 0f; // Reseta o tempo da �ltima atualiza��o
    }

    // M�todo que controla a recarga da pistola (autom�tica, com atraso no som)
    public override void Recarregar()
    {
        // Se n�o estiver recarregando, verifica se pode iniciar a recarga
        if (!estaRecarregando)
        {
            if (balasTotais > 0) // Verifica se h� balas dispon�veis pra recarregar
            {
                estaRecarregando = true; // Marca que a recarga come�ou
                tempoInicioRecarga = Time.time; // Registra o momento do in�cio da recarga
                somRecargaTocou = false; // Reseta o controle do som
                Debug.Log($"Iniciando recarga da {nomeArma}...");
            }
            else
            {
                Debug.Log($"Sem balas totais pra recarregar a {nomeArma}!");
            }
            return;
        }

        // Calcula quanto tempo passou desde o in�cio da recarga
        float tempoDecorrido = Time.time - tempoInicioRecarga;

        // Toca o som de recarga pouco antes do fim da recarga
        if (!somRecargaTocou && tempoDecorrido >= tempoRecarga - atrasoAtualizacaoHUD)
        {
            if (audioSource != null && somRecarga != null)
            {
                audioSource.PlayOneShot(somRecarga); // Toca o som
                Debug.Log($"Tocando som de recarga: {somRecarga.name}");
            }
            else
            {
                Debug.LogWarning("AudioSource ou somRecarga n�o configurado!");
            }
            somRecargaTocou = true; // Marca que o som foi tocado
            tempoUltimaAtualizacaoHUD = Time.time; // Registra o momento do som
        }

        // Atualiza a muni��o e o HUD s� depois do tempo total e do atraso
        if (tempoDecorrido >= tempoRecarga && somRecargaTocou && Time.time >= tempoUltimaAtualizacaoHUD + atrasoAtualizacaoHUD)
        {
            int balasNecessarias = tamanhoCarregador - municaoAtual; // Calcula quantas balas faltam
            if (balasTotais >= balasNecessarias) // Se h� balas suficientes pra recarga completa
            {
                municaoAtual = tamanhoCarregador; // Enche o carregador
                balasTotais -= balasNecessarias; // Reduz as balas totais
                Debug.Log($"Recarga autom�tica conclu�da! Balas totais da {nomeArma}: {balasTotais}");
            }
            else if (balasTotais > 0) // Se h� balas, mas n�o o suficiente (recarga parcial)
            {
                municaoAtual += balasTotais; // Adiciona o que tem
                balasTotais = 0; // Zera as balas totais
                Debug.Log($"Recarga parcial! Muni��o atual: {municaoAtual}, Balas totais da {nomeArma}: {balasTotais}");
            }
            estaRecarregando = false; // Marca que a recarga terminou
            somRecargaTocou = false; // Reseta o controle do som
            AtualizarHUD(); // Atualiza o HUD com a nova muni��o
        }
    }

    // Retorna o tipo da arma pro HUD (0 = pistola)
    public override int GetTipoArma()
    {
        return 0;
    }
}