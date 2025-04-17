using UnityEngine;

public class ControlarArma : MonoBehaviour
{
    public ArmaConfig[] armas; // Lista com as duas armas (pistola e espingarda)
    private string[] nomesArmas = { "Pistola", "Espingarda" }; // Nomes das armas
    private int armaAtual = 0; // �ndice da arma atual (0: pistola, 1: espingarda)
    private float tempoUltimoTiro; // Tempo do �ltimo tiro pra controlar cad�ncia
    private int[] municaoPorArma; // Array pra armazenar a muni��o de cada arma
    private int balasTotais = 300; // Total de balas dispon�veis (inicia com 300)
    private bool estaRecarregando; // Indica se a arma t� recarregando
    private float tempoInicioRecarga; // Tempo em que a recarga come�ou
    private int balasRecarregadas; // Quantidade de balas j� recarregadas (pra recarga manual)
    private float tempoUltimoSomRecarga; // Tempo do �ltimo som de recarga (pra recarga manual)
    private int balasNecessarias; // Quantidade total de balas a recarregar
    private Rigidbody2D soldadoRb; // Refer�ncia ao Rigidbody2D do Soldado Player pra coice
    private bool estaRecuando; // Controla se a arma t� recuando visualmente
    private float tempoRecuo; // Tempo atual do recuo visual
    private float duracaoRecuo = 0.2f; // Dura��o do recuo visual
    private Vector3 deslocamentoRecuo; // Deslocamento do recuo visual
    private MoverQuadrado moverSoldado; // Refer�ncia ao script MoverQuadrado pra direcaoFlip
    public Sprite spritePistola; // Sprite da pistola
    public Sprite spriteEspingarda; // Sprite da espingarda
    private SpriteRenderer spriteRenderer; // Componente pra trocar e flipar sprites
    private ControlarBraco controlarBraco; // Refer�ncia ao script ControlarBraco
    private ControlarHUD controlarHUD; // Refer�ncia ao script ControlarHUD
    private AudioSource audioSource; // Componente pra tocar os sons de disparo

    void Start()
    {
        // Pega o Rigidbody2D do Soldado Player (pai na hierarquia)
        soldadoRb = GetComponentInParent<Rigidbody2D>();
        if (soldadoRb == null)
        {
            Debug.LogError("Rigidbody2D n�o encontrado no Soldado Player!");
            enabled = false; // Desativa o script se n�o encontrar
            return;
        }
        // Pega o script MoverQuadrado do Soldado Player
        moverSoldado = GetComponentInParent<MoverQuadrado>();
        if (moverSoldado == null)
        {
            Debug.LogError("MoverQuadrado n�o encontrado no Soldado Player!");
            enabled = false;
            return;
        }
        // Pega o SpriteRenderer da Arma
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer n�o encontrado no Arma!");
            enabled = false;
            return;
        }
        // Pega o script ControlarBraco do Braco (pai da Arma)
        controlarBraco = GetComponentInParent<ControlarBraco>();
        if (controlarBraco == null)
        {
            Debug.LogError("ControlarBraco n�o encontrado no Braco!");
        }
        // Pega o script ControlarHUD (pode estar em qualquer objeto, ex.: HUDCanvas)
        controlarHUD = FindFirstObjectByType<ControlarHUD>();
        if (controlarHUD == null)
        {
            Debug.LogError("ControlarHUD n�o encontrado na cena!");
        }
        // Pega o componente AudioSource da Arma
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource n�o encontrado no Arma! Adicionando um...");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Garante que n�o toca automaticamente
        }
        // Verifica se h� armas configuradas
        if (armas == null || armas.Length == 0)
        {
            Debug.LogError("Nenhuma arma configurada!");
            enabled = false;
            return;
        }
        // Inicializa o array de muni��o pra cada arma
        municaoPorArma = new int[armas.Length];
        for (int i = 0; i < armas.Length; i++)
        {
            municaoPorArma[i] = armas[i].tamanhoCarregador; // Inicia com o carregador cheio
        }
        // Inicia com a pistola
        armaAtual = 0;
        spriteRenderer.sprite = spritePistola;
        // Atualiza a HUD inicialmente
        if (controlarHUD != null)
        {
            controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spritePistola);
        }
    }

    void Update()
    {
        // Verifica se o script t� ativado e todas as refer�ncias necess�rias existem
        if (!enabled || moverSoldado == null || spriteRenderer == null || armas == null || armas.Length == 0)
        {
            Debug.LogWarning("Script desativado ou refer�ncias n�o inicializadas!");
            return;
        }

        // Troca pra pistola com tecla 1
        if (Input.GetKeyDown(KeyCode.Alpha1) && armas.Length > 0)
        {
            armaAtual = 0;
            estaRecarregando = false; // Cancela recarga ao trocar de arma
            spriteRenderer.sprite = spritePistola;
            Debug.Log("Arma trocada: Pistola");
            // Atualiza a HUD ao trocar de arma
            if (controlarHUD != null)
            {
                controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spritePistola);
            }
        }
        // Troca pra espingarda com tecla 2
        if (Input.GetKeyDown(KeyCode.Alpha2) && armas.Length > 1)
        {
            armaAtual = 1;
            estaRecarregando = false; // Cancela recarga ao trocar de arma
            spriteRenderer.sprite = spriteEspingarda;
            Debug.Log("Arma trocada: Espingarda");
            // Atualiza a HUD ao trocar de arma
            if (controlarHUD != null)
            {
                controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spriteEspingarda);
            }
        }

        // Pega a dire��o de flip do Soldado Player (1: direita, -1: esquerda)
        float direcaoFlip = moverSoldado.GetDirecaoFlip();
        // Flipa o sprite da arma se olhando pra esquerda
        spriteRenderer.flipX = (direcaoFlip < 0);

        // Ajusta a posi��o da arma com base no offset, invertendo X se flipado
        Vector3 offsetArmaAjustado = armas[armaAtual].offsetArma;
        offsetArmaAjustado.x *= direcaoFlip;
        transform.localPosition = offsetArmaAjustado;

        // A rota��o da Arma � herdada do Braco, ent�o n�o precisamos rotacionar aqui

        // Controla o recuo visual da arma
        if (estaRecuando)
        {
            tempoRecuo += Time.deltaTime; // Incrementa o tempo do recuo
            float t = tempoRecuo / duracaoRecuo; // Progresso do recuo (0 a 1)
            if (t >= 1f)
            {
                estaRecuando = false; // Termina o recuo
                transform.localPosition = offsetArmaAjustado; // Volta � posi��o original
            }
            else
            {
                // Interpola entre a posi��o recuada e a original
                transform.localPosition = Vector3.Lerp(offsetArmaAjustado + deslocamentoRecuo, offsetArmaAjustado, t);
            }
        }

        // Controla o processo de recarga
        if (estaRecarregando)
        {
            float tempoDecorrido = Time.time - tempoInicioRecarga;

            // Recarga manual: Toca um som por bala recarregada, com intervalo baseado em tempoRecarga
            if (armas[armaAtual].recargaManual)
            {
                // Calcula o intervalo entre os sons: tempoRecarga dividido pelo n�mero de balas
                float intervaloSom = balasNecessarias > 0 ? armas[armaAtual].tempoRecarga / balasNecessarias : armas[armaAtual].tempoRecarga;

                // Termina a recarga quando o tempo total for atingido
                if (tempoDecorrido >= armas[armaAtual].tempoRecarga)
                {
                    // Calcula quantas balas ainda faltam recarregar
                    int balasFaltando = balasNecessarias - balasRecarregadas;
                    if (balasFaltando > 0)
                    {
                        // Toca o som de recarga pra �ltima bala (se ainda n�o tocou)
                        if (audioSource != null && armas[armaAtual].somRecarga != null)
                        {
                            audioSource.PlayOneShot(armas[armaAtual].somRecarga);
                            Debug.Log($"Tocando som de recarga (bala {balasRecarregadas + 1}/{balasNecessarias}): {armas[armaAtual].somRecarga.name}");
                        }
                        else
                        {
                            Debug.LogWarning("AudioSource ou somRecarga n�o configurado!");
                        }
                        // Recarrega todas as balas restantes de uma vez
                        municaoPorArma[armaAtual] += balasFaltando;
                        balasTotais -= balasFaltando;
                        balasRecarregadas = balasNecessarias; // Marca todas as balas como recarregadas
                        // Atualiza a HUD
                        if (controlarHUD != null)
                        {
                            Sprite spriteAtual = (armaAtual == 0) ? spritePistola : spriteEspingarda;
                            controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spriteAtual);
                        }
                    }
                    // Termina a recarga
                    estaRecarregando = false;
                    balasRecarregadas = 0; // Reseta o contador
                    Debug.Log($"Recarga manual conclu�da! Balas totais restantes: {balasTotais}");
                }
                // Toca o som de recarga pra cada bala, com intervalo, mas s� se ainda n�o terminou
                else if (balasRecarregadas < balasNecessarias && Time.time >= tempoUltimoSomRecarga + intervaloSom)
                {
                    if (audioSource != null && armas[armaAtual].somRecarga != null)
                    {
                        audioSource.PlayOneShot(armas[armaAtual].somRecarga);
                        Debug.Log($"Tocando som de recarga (bala {balasRecarregadas + 1}/{balasNecessarias}): {armas[armaAtual].somRecarga.name}");
                    }
                    else
                    {
                        Debug.LogWarning("AudioSource ou somRecarga n�o configurado!");
                    }
                    balasRecarregadas++;
                    tempoUltimoSomRecarga = Time.time;
                    // Incrementa a muni��o uma bala por vez
                    municaoPorArma[armaAtual]++;
                    balasTotais--;
                    // Atualiza a HUD a cada bala recarregada
                    if (controlarHUD != null)
                    {
                        Sprite spriteAtual = (armaAtual == 0) ? spritePistola : spriteEspingarda;
                        controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spriteAtual);
                    }
                }
            }
            // Recarga autom�tica: Toca o som uma vez e recarrega tudo no final
            else
            {
                if (tempoDecorrido >= armas[armaAtual].tempoRecarga)
                {
                    // Calcula quantas balas s�o necess�rias pra encher o carregador
                    balasNecessarias = armas[armaAtual].tamanhoCarregador - municaoPorArma[armaAtual];
                    if (balasTotais >= balasNecessarias)
                    {
                        // Termina a recarga
                        municaoPorArma[armaAtual] = armas[armaAtual].tamanhoCarregador;
                        balasTotais -= balasNecessarias;
                        Debug.Log($"Recarga autom�tica conclu�da! Balas totais restantes: {balasTotais}");
                    }
                    else if (balasTotais > 0)
                    {
                        // Recarga parcial
                        municaoPorArma[armaAtual] += balasTotais;
                        balasTotais = 0;
                        Debug.Log($"Recarga parcial! Muni��o atual: {municaoPorArma[armaAtual]}, Balas totais: {balasTotais}");
                    }
                    else
                    {
                        Debug.Log("Sem balas totais para recarregar!");
                    }
                    // Toca o som de recarga (apenas pra recarga autom�tica)
                    if (audioSource != null && armas[armaAtual].somRecarga != null)
                    {
                        audioSource.PlayOneShot(armas[armaAtual].somRecarga);
                        Debug.Log($"Tocando som de recarga: {armas[armaAtual].somRecarga.name}");
                    }
                    else
                    {
                        Debug.LogWarning("AudioSource ou somRecarga n�o configurado!");
                    }
                    estaRecarregando = false;
                    // Atualiza a HUD ao terminar a recarga
                    if (controlarHUD != null)
                    {
                        Sprite spriteAtual = (armaAtual == 0) ? spritePistola : spriteEspingarda;
                        controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spriteAtual);
                    }
                }
            }
            return; // N�o permite atirar enquanto recarrega
        }

        // Verifica cada condi��o separadamente
        bool mousePressedDown = Input.GetMouseButtonDown(0); // Clique inicial
        bool mousePressedHold = Input.GetMouseButton(0); // Clique segurado
        bool mousePressed = mousePressedDown || mousePressedHold; // Combina as duas condi��es
        bool tempoOk = Time.time >= tempoUltimoTiro + armas[armaAtual].tempoEntreTiros;
        bool temMunicao = municaoPorArma[armaAtual] > 0;
        bool temBalasTotais = balasTotais > 0;

        // Log detalhado pra cada condi��o
        Debug.Log($"Condi��es de disparo: MousePressedDown={mousePressedDown}, MousePressedHold={mousePressedHold}, MousePressed={mousePressed}, TempoOk={tempoOk} (TempoAtual={Time.time}, UltimoTiro={tempoUltimoTiro}, Cadencia={armas[armaAtual].tempoEntreTiros}), TemMunicao={temMunicao} (MunicaoAtual={municaoPorArma[armaAtual]}), TemBalasTotais={temBalasTotais} (BalasTotais={balasTotais})");

        // Atira se todas as condi��es forem atendidas
        if (mousePressed && tempoOk && temMunicao && temBalasTotais)
        {
            Debug.Log("Condi��es pra atirar atendidas! Chamando Atirar...");
            Atirar();
            tempoUltimoTiro = Time.time; // Atualiza o tempo do �ltimo tiro
            municaoPorArma[armaAtual]--; // Reduz a muni��o do carregador da arma atual
            balasTotais--; // Reduz o total de balas
            Debug.Log($"Muni��o restante: {municaoPorArma[armaAtual]}, Balas totais: {balasTotais}");
            // Atualiza a HUD ao atirar
            if (controlarHUD != null)
            {
                Sprite spriteAtual = (armaAtual == 0) ? spritePistola : spriteEspingarda;
                controlarHUD.AtualizarMunicao(municaoPorArma[armaAtual], armas[armaAtual].tamanhoCarregador, balasTotais, nomesArmas[armaAtual], spriteAtual);
            }
            if (municaoPorArma[armaAtual] <= 0)
            {
                // Inicia a recarga se ainda houver balas totais
                if (balasTotais > 0)
                {
                    estaRecarregando = true;
                    tempoInicioRecarga = Time.time;
                    balasRecarregadas = 0; // Reseta o contador de balas recarregadas
                    tempoUltimoSomRecarga = Time.time; // Permite o primeiro som imediatamente
                    // Calcula quantas balas s�o necess�rias pra encher o carregador
                    balasNecessarias = armas[armaAtual].tamanhoCarregador - municaoPorArma[armaAtual];
                    Debug.Log($"Iniciando recarga... Balas necess�rias: {balasNecessarias}");
                }
                else
                {
                    Debug.Log("Sem balas totais para recarregar!");
                }
            }
        }
        else
        {
            Debug.Log("Condi��es pra atirar n�o atendidas!");
        }

        // Log adicional pra verificar se a barra de espa�o t� interferindo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Barra de espa�o pressionada! (Deve ser usada apenas pro pulo)");
        }
    }

    void Atirar()
    {
        // Log pra confirmar que o m�todo Atirar foi chamado
        Debug.Log("M�todo Atirar chamado!");

        // Toca o som de disparo da arma atual
        if (audioSource != null && armas[armaAtual].somDisparo != null)
        {
            audioSource.PlayOneShot(armas[armaAtual].somDisparo);
            Debug.Log($"Tocando som: {armas[armaAtual].somDisparo.name}");
        }
        else
        {
            Debug.LogWarning("AudioSource ou somDisparo n�o configurado!");
        }

        // Pega a dire��o de flip do Soldado Player
        float direcaoFlip = moverSoldado.GetDirecaoFlip();
        // Converte a posi��o do cursor de tela pra mundo
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        // Calcula a dire��o do Braco at� o cursor
        Vector3 direcao = (mousePos - transform.parent.position).normalized;
        // Calcula a dist�ncia real (n�o normalizada) entre o Braco e o cursor, s� pra depura��o
        float distanciaAoCursor = Vector3.Distance(mousePos, transform.parent.position);

        // Calcula o recuo visual da arma
        float distanciaRecuo = armas[armaAtual].forcaRecuo * 0.05f;
        deslocamentoRecuo = -direcao * distanciaRecuo; // Deslocamento na dire��o oposta
        estaRecuando = true; // Inicia o recuo
        tempoRecuo = 0f; // Reseta o tempo do recuo
        Debug.Log($"Recuo arma: distancia={distanciaRecuo}, direcao={direcao}");

        // Aplica recuo ao Braco usando o multiplicador configurado pra arma atual
        if (controlarBraco != null)
        {
            float recuoBraco = (armas[armaAtual].forcaRecuo * 0.05f) * armas[armaAtual].forcaRecuoBraco;
            controlarBraco.AplicarRecuo(direcao, recuoBraco);
            Debug.Log($"Aplicando recuo ao Braco: distancia={recuoBraco}, multiplicador={armas[armaAtual].forcaRecuoBraco}");
        }

        // Aplica coice no Soldado Player se configurado
        if (armas[armaAtual].coice)
        {
            Vector2 direcaoCoice = -direcao.normalized; // Dire��o oposta ao tiro
            soldadoRb.AddForce(direcaoCoice * armas[armaAtual].forcaCoice, ForceMode2D.Impulse); // Aplica for�a
            Debug.Log($"Coice aplicado: For�a={armas[armaAtual].forcaCoice}, Dire��o={direcaoCoice}");
            if (moverSoldado != null)
            {
                moverSoldado.AplicarCoice(); // Notifica o MoverQuadrado
            }
        }

        // Loop pra criar cada proj�til (1 pra pistola, 3 pra espingarda)
        for (int i = 0; i < armas[armaAtual].projeteisPorTiro; i++)
        {
            // Calcula o desvio angular (ex.: -5�, 0�, +5� pra espingarda)
            float t = armas[armaAtual].projeteisPorTiro > 1 ? (float)i / (armas[armaAtual].projeteisPorTiro - 1) : 0f;
            float desvio = Mathf.Lerp(-armas[armaAtual].angulo, armas[armaAtual].angulo, t);
            // Aplica o desvio na dire��o e normaliza corretamente como Vector2
            Vector3 direcaoRotacionada = Quaternion.Euler(0, 0, desvio) * direcao;
            Vector2 direcaoDesvio = new Vector2(direcaoRotacionada.x, direcaoRotacionada.y).normalized;
            // Calcula o �ngulo do tiro
            float anguloTiro = Mathf.Atan2(direcaoDesvio.y, direcaoDesvio.x) * Mathf.Rad2Deg;
            // Aplica o ajuste de rota��o do sprite do proj�til
            float anguloFinal = anguloTiro + armas[armaAtual].rotacaoSpriteProjetil;
            Quaternion rotacaoDesvio = Quaternion.Euler(0, 0, anguloFinal); // Rota��o ajustada do proj�til

            // Ajusta o offset da ponta da arma com base no flip
            Vector3 offsetPontaArmaAjustado = armas[armaAtual].offsetPontaArma;
            offsetPontaArmaAjustado.x *= direcaoFlip;
            // Calcula a posi��o de spawn do proj�til usando a rota��o do Braco
            Vector3 posicaoTiro = transform.position + transform.parent.rotation * offsetPontaArmaAjustado;
            Debug.Log($"Posi��o de tiro: {posicaoTiro} | Arma: {transform.position} | Offset Ajustado: {offsetPontaArmaAjustado} | Desvio: {desvio}");

            // Instancia o proj�til na posi��o e rota��o calculadas
            GameObject projetil = Instantiate(armas[armaAtual].projetilPrefab, posicaoTiro, rotacaoDesvio);

            // Configura o alcance do proj�til
            ControlarProjetil controlarProjetil = projetil.GetComponent<ControlarProjetil>();
            if (controlarProjetil != null)
            {
                controlarProjetil.SetAlcance(armas[armaAtual].alcance);
            }

            // Pega o Rigidbody2D do proj�til
            Rigidbody2D rbProjetil = projetil.GetComponent<Rigidbody2D>();
            // Aplica a velocidade fixa, usando a dire��o normalizada
            Vector2 velocidadeProjetil = direcaoDesvio * armas[armaAtual].velocidadeProjetil;
            rbProjetil.linearVelocity = velocidadeProjetil;
            // Log pra depura��o: dist�ncia, dire��o, velocidade aplicada e posi��o de spawn
            Debug.Log($"Dist�ncia ao cursor: {distanciaAoCursor}, Dire��o desvio: {direcaoDesvio}, Magnitude dire��o: {direcaoDesvio.magnitude}, Velocidade aplicada: {velocidadeProjetil.magnitude}, Posi��o de spawn: {posicaoTiro}");
        }
    }
}