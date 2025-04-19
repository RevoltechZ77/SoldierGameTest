using UnityEngine;

// Script que gerencia as armas do jogador, delegando a l�gica pra classe espec�fica de cada arma
public class ControlarArma : MonoBehaviour
{
    public ArmaConfig[] armasConfig; // Array de ScriptableObjects (Carrion 9mm, ESP Cano Curto)
    private ArmaBase[] armas; // Array de inst�ncias das armas (Pistola, Espingarda)
    private int armaAtual = 0; // �ndice da arma atual (0: Carrion 9mm, 1: ESP Cano Curto)
    private float tempoUltimoTiro; // Tempo do �ltimo tiro pra controlar cad�ncia
    private bool estaRecuando; // Controla se a arma est� recuando visualmente
    private float tempoRecuo; // Tempo atual do recuo visual
    private float duracaoRecuo = 0.2f; // Dura��o do recuo visual (em segundos)
    private Vector3 deslocamentoRecuo; // Deslocamento do recuo visual
    private Rigidbody2D soldadoRb; // Refer�ncia ao Rigidbody2D do jogador
    private MoverQuadrado moverSoldado; // Refer�ncia ao script de movimento do jogador
    private SpriteRenderer spriteRenderer; // Componente pra trocar e flipar sprites da arma
    private ControlarBraco controlarBraco; // Refer�ncia ao script do bra�o
    private ControlarHUD controlarHUD; // Refer�ncia ao script do HUD
    private AudioSource audioSource; // Componente pra tocar sons
    private bool estaAtrasandoRecarga; // Indica se a arma atual est� em um estado de atraso
    private float tempoInicioAtraso; // Momento em que o atraso come�ou

    void Start()
    {
        // --- Inicializa as refer�ncias externas ---
        // Pega o Rigidbody2D do jogador (pai na hierarquia)
        soldadoRb = GetComponentInParent<Rigidbody2D>();
        if (soldadoRb == null)
        {
            Debug.LogError("Rigidbody2D n�o encontrado no Soldado Player!");
            enabled = false; // Desativa o script se n�o encontrar
            return;
        }

        // Pega o script MoverQuadrado do jogador
        moverSoldado = GetComponentInParent<MoverQuadrado>();
        if (moverSoldado == null)
        {
            Debug.LogError("MoverQuadrado n�o encontrado no Soldado Player!");
            enabled = false;
            return;
        }

        // Pega o SpriteRenderer da arma
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer n�o encontrado no Arma!");
            enabled = false;
            return;
        }

        // Pega o script ControlarBraco do bra�o (pai da arma)
        controlarBraco = GetComponentInParent<ControlarBraco>();
        if (controlarBraco == null)
        {
            Debug.LogError("ControlarBraco n�o encontrado no Braco!");
        }

        // Pega o script ControlarHUD (geralmente no HUDCanvas)
        controlarHUD = FindFirstObjectByType<ControlarHUD>();
        if (controlarHUD == null)
        {
            Debug.LogError("ControlarHUD n�o encontrado na cena!");
        }

        // Pega o componente AudioSource da arma
        audioSource = GetComponent<SpriteRenderer>().GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource n�o encontrado no Arma! Adicionando um...");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Garante que n�o toca automaticamente
        }

        // --- Inicializa as armas ---
        // Verifica se h� armas configuradas no array armasConfig
        if (armasConfig == null || armasConfig.Length == 0)
        {
            Debug.LogError("Nenhuma arma configurada!");
            enabled = false;
            return;
        }

        // Cria o array de inst�ncias das armas
        armas = new ArmaBase[armasConfig.Length];
        for (int i = 0; i < armasConfig.Length; i++)
        {
            // Cria um GameObject vazio pra cada arma (pra anexar os scripts Pistola ou Espingarda)
            GameObject armaObj = new GameObject($"Arma_{i}");
            armaObj.transform.SetParent(transform); // Faz o GameObject ser filho do objeto Arma
            ArmaBase arma;

            // Cria a inst�ncia da arma com base no �ndice (0: Pistola, 1: Espingarda)
            if (i == 0) // Carrion 9mm (Pistola)
            {
                arma = armaObj.AddComponent<Pistola>(); // Adiciona o script Pistola
                arma.nomeArma = "Carrion 9mm"; // Define o nome
            }
            else // ESP Cano Curto (Espingarda)
            {
                arma = armaObj.AddComponent<Espingarda>(); // Adiciona o script Espingarda
                arma.nomeArma = "ESP Cano Curto"; // Define o nome
            }

            // --- Copia os dados do ScriptableObject (ArmaConfig) pra inst�ncia da arma ---
            arma.tamanhoCarregador = armasConfig[i].tamanhoCarregador;
            arma.tempoEntreTiros = armasConfig[i].tempoEntreTiros;
            arma.tempoRecarga = armasConfig[i].tempoRecarga;
            arma.tempoAtrasoRecargaZerada = armasConfig[i].tempoAtrasoRecargaZerada;
            arma.forcaRecuo = armasConfig[i].forcaRecuo;
            arma.forcaRecuoBraco = armasConfig[i].forcaRecuoBraco;
            arma.forcaCoice = armasConfig[i].forcaCoice;
            arma.coice = armasConfig[i].coice;
            arma.angulo = armasConfig[i].angulo;
            arma.projeteisPorTiro = armasConfig[i].projeteisPorTiro;
            arma.velocidadeProjetil = armasConfig[i].velocidadeProjetil;
            arma.alcance = armasConfig[i].alcance;
            arma.rotacaoSpriteProjetil = armasConfig[i].rotacaoSpriteProjetil;
            arma.offsetArma = armasConfig[i].offsetArma;
            arma.offsetPontaArma = armasConfig[i].offsetPontaArma;
            arma.projetilPrefab = armasConfig[i].projetilPrefab;
            arma.somDisparo = armasConfig[i].somDisparo;
            arma.somRecarga = armasConfig[i].somRecarga;
            arma.spriteArma = armasConfig[i].spriteArma; // Usa o campo spriteArma do ScriptableObject

            // Inicializa a arma com as refer�ncias externas
            arma.Inicializar(controlarHUD, audioSource, controlarBraco, soldadoRb, moverSoldado);
            armas[i] = arma; // Adiciona a arma ao array
        }

        TrocarArma(0); // Come�a com a Carrion 9mm
    }

    void Update()
    {
        // Verifica se o script est� ativado e todas as refer�ncias est�o inicializadas
        if (!enabled || moverSoldado == null || spriteRenderer == null || armas == null || armas.Length == 0)
        {
            Debug.LogWarning("Script desativado ou refer�ncias n�o inicializadas!");
            return;
        }

        // --- Troca de arma ---
        if (Input.GetKeyDown(KeyCode.Alpha1) && armaAtual != 0) // Tecla 1: Carrion 9mm
        {
            TrocarArma(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && armaAtual != 1) // Tecla 2: ESP Cano Curto
        {
            TrocarArma(1);
        }

        // --- Atualiza o sprite e a posi��o da arma ---
        float direcaoFlip = moverSoldado.GetDirecaoFlip(); // Pega a dire��o do jogador (1: direita, -1: esquerda)
        spriteRenderer.sprite = armas[armaAtual].spriteArma; // Define o sprite da arma atual
        spriteRenderer.flipX = (direcaoFlip < 0); // Flipa o sprite se o jogador t� olhando pra esquerda

        Vector3 offsetArmaAjustado = armas[armaAtual].offsetArma; // Pega o offset da arma atual
        offsetArmaAjustado.x *= direcaoFlip; // Ajusta o offset com base na dire��o
        transform.localPosition = offsetArmaAjustado; // Define a posi��o da arma

        // --- Controla o recuo visual da arma ---
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

        // --- Verifica recarga manual com a tecla "R" ---
        // Se pressionar "R", a recarga come�a imediatamente, ignorando qualquer atraso
        if (Input.GetKeyDown(KeyCode.R) && armas[armaAtual].municaoAtual < armas[armaAtual].tamanhoCarregador && armas[armaAtual].balasTotais > 0)
        {
            estaAtrasandoRecarga = false; // Cancela qualquer atraso em andamento
            armas[armaAtual].Recarregar(); // Inicia a recarga manual
        }

        // --- Gerencia o atraso da recarga autom�tica ---
        if (estaAtrasandoRecarga)
        {
            float tempoDecorridoAtraso = Time.time - tempoInicioAtraso;
            float tempoAtraso = armas[armaAtual].GetTipoArma() == 1 ? armas[armaAtual].tempoAtrasoRecargaZerada : 0f; // Atraso s� pra espingarda
            if (tempoDecorridoAtraso >= tempoAtraso)
            {
                estaAtrasandoRecarga = false; // Termina o atraso
                Debug.Log($"Atraso de recarga terminado! Iniciando recarga da {armas[armaAtual].nomeArma}.");
                armas[armaAtual].Recarregar(); // Inicia a recarga
            }
            else
            {
                Debug.Log($"Aguardando atraso de recarga da {armas[armaAtual].nomeArma}... Tempo restante: {(tempoAtraso - tempoDecorridoAtraso):F2} segundos.");
                return; // N�o prossegue at� o atraso terminar
            }
        }

        // --- Executa a recarga da arma atual ---
        if (armas[armaAtual].estaRecarregando)
        {
            armas[armaAtual].Recarregar(); // Chama o m�todo de recarga da arma atual (Pistola ou Espingarda)
        }

        // --- Verifica se a arma precisa recarregar automaticamente ---
        if (armas[armaAtual].municaoAtual <= 0 && armas[armaAtual].balasTotais > 0 && !armas[armaAtual].estaRecarregando && !estaAtrasandoRecarga)
        {
            // Se a arma � uma espingarda e o carregador est� zerado, inicia o atraso
            if (armas[armaAtual].GetTipoArma() == 1 && armas[armaAtual].municaoAtual == 0 && armas[armaAtual].tempoAtrasoRecargaZerada > 0)
            {
                estaAtrasandoRecarga = true;
                tempoInicioAtraso = Time.time;
                Debug.Log($"Iniciando recarga com carregador zerado! Atrasando recarga da {armas[armaAtual].nomeArma} por {armas[armaAtual].tempoAtrasoRecargaZerada:F2} segundos.");
            }
            else
            {
                armas[armaAtual].Recarregar(); // Inicia a recarga autom�tica sem atraso
            }
        }

        // --- Verifica se pode atirar ---
        bool mousePressed = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0); // Clique ou clique segurado
        if (mousePressed && armas[armaAtual].PodeAtirar(Time.time, tempoUltimoTiro))
        {
            // Converte a posi��o do cursor pra coordenadas do mundo
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            // Chama o m�todo de disparo da arma atual
            armas[armaAtual].Atirar(transform.position, transform.parent.rotation, direcaoFlip, mousePos);

            tempoUltimoTiro = Time.time; // Atualiza o tempo do �ltimo tiro
            estaRecuando = true; // Inicia o recuo visual
            tempoRecuo = 0f; // Reseta o tempo do recuo
            Vector3 direcao = (mousePos - transform.position).normalized; // Calcula a dire��o do tiro
            deslocamentoRecuo = -direcao * (armas[armaAtual].forcaRecuo * 0.05f); // Define o deslocamento do recuo
        }
    }

    // M�todo pra trocar a arma atual
    void TrocarArma(int novaArma)
    {
        armaAtual = novaArma; // Define o �ndice da nova arma
        Debug.Log($"Arma trocada: {armas[armaAtual].nomeArma}");
        spriteRenderer.sprite = armas[armaAtual].spriteArma; // Atualiza o sprite da arma

        // Cancela a recarga e o atraso da arma anterior
        armas[armaAtual].estaRecarregando = false;
        estaAtrasandoRecarga = false;
        armas[armaAtual].AtualizarHUD(); // Atualiza o HUD com os dados da nova arma

        // Inicia recarga se necess�rio (se n�o houver muni��o no carregador)
        if (armas[armaAtual].municaoAtual <= 0 && armas[armaAtual].balasTotais > 0)
        {
            if (armas[armaAtual].GetTipoArma() == 1 && armas[armaAtual].municaoAtual == 0 && armas[armaAtual].tempoAtrasoRecargaZerada > 0)
            {
                estaAtrasandoRecarga = true;
                tempoInicioAtraso = Time.time;
                Debug.Log($"Iniciando recarga com carregador zerado! Atrasando recarga da {armas[armaAtual].nomeArma} por {armas[armaAtual].tempoAtrasoRecargaZerada:F2} segundos.");
            }
            else
            {
                armas[armaAtual].Recarregar();
            }
        }
    }
}