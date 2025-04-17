using UnityEngine;
using TMPro; // Necessário pra TextMeshPro
using UnityEngine.UI; // Necessário pra Image

public class ControlarHUD : MonoBehaviour
{
    public TextMeshProUGUI nomeArmaTexto; // Referência ao texto do nome da arma
    public Image spriteArma; // Referência à imagem do sprite da arma
    public TextMeshProUGUI municaoTexto; // Referência ao texto da munição
    public GameObject balasPainel; // Referência ao painel onde os sprites das balas serão instanciados
    public GameObject balaSpritePrefab; // Prefab do sprite de bala
    public float espacamentoBalas = 40f; // Espaçamento entre os sprites (ajustável no Inspector)
    public float rotacaoBala = 0f; // Rotação dos sprites de bala (em graus, ajustável no Inspector)

    private Image[] balaSprites; // Array de sprites das balas
    private int municaoAtual; // Munição atual (pra comparar com o valor anterior)
    private int tamanhoCarregador; // Tamanho máximo do carregador

    void Start()
    {
        // Garante que o sprite da arma preserve a proporção
        if (spriteArma != null)
        {
            spriteArma.preserveAspect = true; // Força o "Preserve Aspect" em tempo de execução
        }
    }

    public void AtualizarMunicao(int novaMunicao, int novoTamanhoCarregador, int novasBalasTotais, string nomeArma, Sprite spriteArmaAtual)
    {
        // Atualiza o nome da arma
        nomeArmaTexto.text = nomeArma;

        // Atualiza o sprite da arma
        spriteArma.sprite = spriteArmaAtual;

        // Ajusta o tamanho do RectTransform como segurança extra pra evitar distorção
        if (spriteArma.sprite != null)
        {
            // Pega as dimensões do sprite
            Vector2 spriteSize = spriteArma.sprite.bounds.size;
            RectTransform rect = spriteArma.GetComponent<RectTransform>();

            // Calcula a proporção do sprite (largura/altura)
            float aspectRatio = spriteSize.x / spriteSize.y;

            // Define um tamanho base (ex.: altura fixa de 50, ajusta a largura pra manter a proporção)
            float alturaDesejada = 50f; // Ajustável
            float larguraCalculada = alturaDesejada * aspectRatio;

            // Aplica as novas dimensões ao RectTransform
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

        // Se a munição mudou, atualiza os sprites
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
            rect.rotation = Quaternion.Euler(0, 0, rotacaoBala); // Aplica a rotação
            balaSprites[i] = balaObj.GetComponent<Image>();
        }
    }

    void AtualizarSpritesBalas()
    {
        // Atualiza a cor dos sprites com base na munição atual
        for (int i = 0; i < balaSprites.Length; i++)
        {
            if (i < municaoAtual)
            {
                balaSprites[i].color = Color.yellow; // Bala cheia (ajustável)
            }
            else
            {
                balaSprites[i].color = Color.gray; // Bala vazia (ajustável)
            }
        }
    }
}