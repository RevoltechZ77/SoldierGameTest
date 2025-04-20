using UnityEngine;
using UnityEngine.EventSystems;

// Script que gerencia as armas do jogador, delegando a lógica para a classe específica de cada arma
public class ControlarArma : MonoBehaviour
{
    public ArmaConfig[] armasConfig; // Array de ScriptableObjects (Carrion 9mm, ESP Cano Curto)
    private ArmaBase[] armas; // Array de instâncias das armas (Pistola, Espingarda)
    private int armaAtual = 0; // Índice da arma atual (0: Carrion 9mm, 1: ESP Cano Curto)
    private float tempoUltimoTiro; // Tempo do último tiro para controlar cadência
    private bool estaRecuando; // Controla se a arma está recuando visualmente
    private float tempoRecuo; // Tempo atual do recuo visual
    private float duracaoRecuo = 0.2f; // Duração do recuo visual (em segundos)
    private Vector3 deslocamentoRecuo; // Deslocamento do recuo visual
    private Rigidbody2D soldadoRb; // Referência ao Rigidbody2D do jogador
    private MoverQuadrado moverSoldado; // Referência ao script de movimento do jogador
    private SpriteRenderer spriteRenderer; // Componente para trocar e flipar sprites da arma
    private ControlarBraco controlarBraco; // Referência ao script do braço
    private ControlarHUD controlarHUD; // Referência ao script do HUD
    private AudioSource audioSource; // Componente para tocar sons

    void Start()
    {
        // --- Inicializa as referências externas ---
        // Pega o Rigidbody2D do jogador (pai na hierarquia)
        soldadoRb = GetComponentInParent<Rigidbody2D>();
        if (soldadoRb == null)
        {
            Debug.LogError("Rigidbody2D não encontrado no Soldado Player!");
            enabled = false; // Desativa o script se não encontrar
            return;
        }

        // Pega o script MoverQuadrado do jogador
        moverSoldado = GetComponentInParent<MoverQuadrado>();
        if (moverSoldado == null)
        {
            Debug.LogError("MoverQuadrado não encontrado no Soldado Player!");
            enabled = false;
            return;
        }

        // Pega o SpriteRenderer da arma
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer não encontrado no Arma!");
            enabled = false;
            return;
        }

        // Pega o script ControlarBraco do braço (pai da arma)
        controlarBraco = GetComponentInParent<ControlarBraco>();
        if (controlarBraco == null)
        {
            Debug.LogError("ControlarBraco não encontrado no Braco!");
        }

        // Pega o script ControlarHUD (geralmente no HUDCanvas)
        controlarHUD = FindFirstObjectByType<ControlarHUD>();
        if (controlarHUD == null)
        {
            Debug.LogError("ControlarHUD não encontrado na cena!");
        }

        // Pega o componente AudioSource da arma
        audioSource = GetComponent<SpriteRenderer>().GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource não encontrado no Arma! Adicionando um...");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Garante que não toca automaticamente
        }

        // --- Inicializa as armas ---
        // Verifica se há armas configuradas no array armasConfig
        if (armasConfig == null || armasConfig.Length == 0)
        {
            Debug.LogError("Nenhuma arma configurada no array armasConfig!");
            enabled = false;
            return;
        }

        // Cria o array de instâncias das armas
        armas = new ArmaBase[armasConfig.Length];
        Debug.Log($"Inicializando {armasConfig.Length} armas...");
        for (int i = 0; i < armasConfig.Length; i++)
        {
            // Verifica se o elemento no array armasConfig é nulo
            if (armasConfig[i] == null)
            {
                Debug.LogError($"ArmaConfig no índice {i} é nulo!");
                enabled = false;
                return;
            }

            // Cria um GameObject vazio para cada arma (para anexar os scripts Pistola ou Espingarda)
            GameObject armaObj = new GameObject($"Arma_{i}");
            armaObj.transform.SetParent(transform); // Faz o GameObject ser filho do objeto Arma
            ArmaBase arma;

            // Cria a instância da arma com base no índice (0: Pistola, 1: Espingarda)
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

            // --- Copia os dados do ScriptableObject (ArmaConfig) para a instância da arma ---
            arma.tamanhoCarregador = armasConfig[i].tamanhoCarregador;
            arma.tempoEntreTiros = armasConfig[i].tempoEntreTiros;
            arma.tempoRecarga = armasConfig[i].tempoRecarga;
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

            // Inicializa a arma com as referências externas
            arma.Inicializar(controlarHUD, audioSource, controlarBraco, soldadoRb, moverSoldado);
            armas[i] = arma; // Adiciona a arma ao array
            Debug.Log($"Arma {i} ({arma.nomeArma}) inicializada com sucesso.");
        }

        TrocarArma(0); // Começa com a Carrion 9mm
    }

    void Update()
    {
        // Verifica se o script está ativado e todas as referências estão inicializadas
        if (!enabled || moverSoldado == null || spriteRenderer == null || armas == null || armas.Length == 0)
        {
            Debug.LogWarning("Script desativado ou referências não inicializadas!");
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

        // --- Atualiza o sprite e a posição da arma ---
        float direcaoFlip = moverSoldado.GetDirecaoFlip(); // Pega a direção do jogador (1: direita, -1: esquerda)
        spriteRenderer.sprite = armas[armaAtual].spriteArma; // Define o sprite da arma atual
        spriteRenderer.flipX = (direcaoFlip < 0); // Flipa o sprite se o jogador tá olhando para a esquerda

        Vector3 offsetArmaAjustado = armas[armaAtual].offsetArma; // Pega o offset da arma atual
        offsetArmaAjustado.x *= direcaoFlip; // Ajusta o offset com base na direção
        transform.localPosition = offsetArmaAjustado; // Define a posição da arma

        // --- Controla o recuo visual da arma ---
        if (estaRecuando)
        {
            tempoRecuo += Time.deltaTime; // Incrementa o tempo do recuo
            float t = tempoRecuo / duracaoRecuo; // Progresso do recuo (0 a 1)
            if (t >= 1f)
            {
                estaRecuando = false; // Termina o recuo
                transform.localPosition = offsetArmaAjustado; // Volta à posição original
            }
            else
            {
                // Interpola entre a posição recuada e a original
                transform.localPosition = Vector3.Lerp(offsetArmaAjustado + deslocamentoRecuo, offsetArmaAjustado, t);
            }
        }

        // --- Verifica recarga manual com a tecla "R" ---
        if (Input.GetKeyDown(KeyCode.R) && armas[armaAtual].municaoAtual < armas[armaAtual].tamanhoCarregador && armas[armaAtual].balasTotais > 0)
        {
            armas[armaAtual].Recarregar(); // Inicia a recarga manual
        }

        // --- Executa a recarga da arma atual ---
        if (armas[armaAtual].estaRecarregando)
        {
            armas[armaAtual].Recarregar(); // Chama o método de recarga da arma atual (Pistola ou Espingarda)
        }

        // --- Verifica se a arma precisa recarregar automaticamente ---
        if (armas[armaAtual].municaoAtual <= 0 && armas[armaAtual].balasTotais > 0 && !armas[armaAtual].estaRecarregando)
        {
            armas[armaAtual].Recarregar(); // Inicia a recarga automática
        }

        // --- Verifica se pode atirar ---
        bool mousePressed = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0); // Clique ou clique segurado
        if (mousePressed && armas[armaAtual].PodeAtirar(Time.time, tempoUltimoTiro) && !EventSystem.current.IsPointerOverGameObject())
        {
            // Converte a posição do cursor para coordenadas do mundo
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            // Chama o método de disparo da arma atual
            armas[armaAtual].Atirar(transform.position, transform.parent.rotation, direcaoFlip, mousePos);

            tempoUltimoTiro = Time.time; // Atualiza o tempo do último tiro
            estaRecuando = true; // Inicia o recuo visual
            tempoRecuo = 0f; // Reseta o tempo do recuo
            Vector3 direcao = (mousePos - transform.position).normalized; // Calcula a direção do tiro
            deslocamentoRecuo = -direcao * (armas[armaAtual].forcaRecuo * 0.05f); // Define o deslocamento do recuo
        }
    }

    // Método para trocar a arma atual
    public void TrocarArma(int novaArma)
    {
        armaAtual = novaArma; // Define o índice da nova arma
        Debug.Log($"Arma trocada: {armas[armaAtual].nomeArma}");
        spriteRenderer.sprite = armas[armaAtual].spriteArma; // Atualiza o sprite da arma

        // Cancela a recarga da arma anterior
        armas[armaAtual].estaRecarregando = false;
        armas[armaAtual].AtualizarHUD(); // Atualiza o HUD com os dados da nova arma

        // Inicia recarga se necessário (se não houver munição no carregador)
        if (armas[armaAtual].municaoAtual <= 0 && armas[armaAtual].balasTotais > 0)
        {
            armas[armaAtual].Recarregar();
        }
    }

    // Método público para acessar uma arma específica pelo índice
    public ArmaBase GetArma(int indice)
    {
        if (indice < 0 || indice >= armas.Length)
        {
            Debug.LogError($"Índice de arma inválido: {indice}");
            return null;
        }
        Debug.Log($"Acessando arma no índice {indice}: {armas[indice].nomeArma}");
        return armas[indice];
    }
}