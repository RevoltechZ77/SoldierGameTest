using UnityEngine;
using TMPro; // Necess�rio pra TextMeshPro
using UnityEngine.UI; // Necess�rio pra Image

public class ControlarHUD : MonoBehaviour
{
    public TextMeshProUGUI nomeArmaTexto; // Refer�ncia ao texto do nome da arma
    public Image spriteArma; // Refer�ncia � imagem do sprite da arma
    public TextMeshProUGUI municaoTexto; // Refer�ncia ao texto da muni��o
    public GameObject balasPainel; // Refer�ncia ao painel onde os sprites das balas ser�o instanciados
    public GameObject balaSpritePrefab; // Prefab do sprite de bala
    public float espacamentoBalas = 40f; // Espa�amento entre os sprites (ajust�vel no Inspector)
    public float rotacaoBala = 0f; // Rota��o dos sprites de bala (em graus, ajust�vel no Inspector)

    private Image[] balaSprites; // Array de sprites das balas
    private int municaoAtual; // Muni��o atual (pra comparar com o valor anterior)
    private int tamanhoCarregador; // Tamanho m�ximo do carregador

    void Start()
    {
        // Garante que o sprite da arma preserve a propor��o
        if (spriteArma != null)
        {
            spriteArma.preserveAspect = true; // For�a o "Preserve Aspect" em tempo de execu��o
        }
    }

    public void AtualizarMunicao(int novaMunicao, int novoTamanhoCarregador, int novasBalasTotais, string nomeArma, Sprite spriteArmaAtual)
    {
        // Atualiza o nome da arma
        nomeArmaTexto.text = nomeArma;

        // Atualiza o sprite da arma
        spriteArma.sprite = spriteArmaAtual;

        // Ajusta o tamanho do RectTransform como seguran�a extra pra evitar distor��o
        if (spriteArma.sprite != null)
        {
            // Pega as dimens�es do sprite
            Vector2 spriteSize = spriteArma.sprite.bounds.size;
            RectTransform rect = spriteArma.GetComponent<RectTransform>();

            // Calcula a propor��o do sprite (largura/altura)
            float aspectRatio = spriteSize.x / spriteSize.y;

            // Define um tamanho base (ex.: altura fixa de 50, ajusta a largura pra manter a propor��o)
            float alturaDesejada = 50f; // Ajust�vel
            float larguraCalculada = alturaDesejada * aspectRatio;

            // Aplica as novas dimens�es ao RectTransform
            rect.sizeDelta = new Vector2(larguraCalculada, alturaDesejada);
        }

        // Atualiza o texto pra mostrar apenas o total de balas
        municaoTexto.text = $"{novasBalasTotais}";

        // Se o tamanho do carregador mudou (ex.: troca de arma), recria os sprites
        if (novoTamanhoCarregador != tamanhoCarregador)
        {
            tamanhoCarregador = novoTamanhoCarregador;
            CriarSpritesBalas();
        }

        // Se a muni��o mudou, atualiza os sprites
        if (novaMunicao != municaoAtual)
        {
            municaoAtual = novaMunicao;
            AtualizarSpritesBalas();
        }
    }

    void CriarSpritesBalas()
    {
        // Remove os sprites existentes, se houver
        if (balaSprites != null)
        {
            foreach (Image sprite in balaSprites)
            {
                Destroy(sprite.gameObject);
            }
        }

        // Cria novos sprites com base no tamanho do carregador
        balaSprites = new Image[tamanhoCarregador];
        for (int i = 0; i < tamanhoCarregador; i++)
        {
            GameObject balaObj = Instantiate(balaSpritePrefab, balasPainel.transform);
            RectTransform rect = balaObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * espacamentoBalas, 0); // Posiciona os sprites em linha
            rect.rotation = Quaternion.Euler(0, 0, rotacaoBala); // Aplica a rota��o
            balaSprites[i] = balaObj.GetComponent<Image>();
        }
    }

    void AtualizarSpritesBalas()
    {
        // Atualiza a cor dos sprites com base na muni��o atual
        for (int i = 0; i < balaSprites.Length; i++)
        {
            if (i < municaoAtual)
            {
                balaSprites[i].color = Color.yellow; // Bala cheia (ajust�vel)
            }
            else
            {
                balaSprites[i].color = Color.gray; // Bala vazia (ajust�vel)
            }
        }
    }
}