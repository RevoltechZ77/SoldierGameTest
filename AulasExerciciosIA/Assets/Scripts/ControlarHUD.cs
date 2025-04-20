using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlarHUD : MonoBehaviour
{
    public TextMeshProUGUI nomeArmaTexto; // Referência ao texto do nome da arma
    public Image spriteArma; // Referência à imagem do sprite da arma
    public TextMeshProUGUI municaoTexto; // Referência ao texto da munição
    public GameObject balasPainel; // Referência ao painel onde os sprites das balas serão instanciados
    public GameObject balaSpritePistola; // Prefab do sprite de bala da pistola
    public GameObject balaSpriteEspingarda; // Prefab do sprite de bala da espingarda
    public float espacamentoBalas = 40f; // Espaçamento entre os sprites
    public float rotacaoBala = 0f; // Rotação dos sprites de bala

    private Image[] balaSprites; // Array de sprites das balas
    private int municaoAtual; // Munição atual
    private int tamanhoCarregador; // Tamanho máximo do carregador
    private int tipoArmaAtual; // Arma atual (0: pistola, 1: espingarda)

    void Start()
    {
        // Garante que o sprite da arma preserve a proporção
        if (spriteArma != null)
        {
            spriteArma.preserveAspect = true;
        }
    }

    public void AtualizarMunicao(int novaMunicao, int novoTamanhoCarregador, int novasBalasTotais, string nomeArma, Sprite spriteArmaAtual, int tipoArma)
    {
        // Atualiza o nome da arma
        nomeArmaTexto.text = nomeArma;

        // Atualiza o sprite da arma
        spriteArma.sprite = spriteArmaAtual;

        // Ajusta o tamanho do RectTransform pra evitar distorção
        if (spriteArma.sprite != null)
        {
            Vector2 spriteSize = spriteArma.sprite.bounds.size;
            RectTransform rect = spriteArma.GetComponent<RectTransform>();
            float aspectRatio = spriteSize.x / spriteSize.y;
            float alturaDesejada = 50f;
            float larguraCalculada = alturaDesejada * aspectRatio;
            rect.sizeDelta = new Vector2(larguraCalculada, alturaDesejada);
        }

        // Atualiza o texto da munição
        municaoTexto.text = $"{novasBalasTotais}";

        // Se o tamanho do carregador ou o tipo de arma mudou, recria os sprites
        if (novoTamanhoCarregador != tamanhoCarregador || tipoArma != tipoArmaAtual)
        {
            tamanhoCarregador = novoTamanhoCarregador;
            tipoArmaAtual = tipoArma;
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
        // Remove os sprites existentes
        if (balaSprites != null)
        {
            foreach (Image sprite in balaSprites)
            {
                Destroy(sprite.gameObject);
            }
        }

        // Escolhe o prefab com base no tipo de arma
        GameObject balaSpritePrefab = tipoArmaAtual == 0 ? balaSpritePistola : balaSpriteEspingarda;

        // Verifica se o prefab tá configurado
        if (balaSpritePrefab == null)
        {
            Debug.LogError($"Prefab de sprite de bala {(tipoArmaAtual == 0 ? "Pistola" : "Espingarda")} não configurado!");
            return;
        }

        // Cria novos sprites
        balaSprites = new Image[tamanhoCarregador];
        for (int i = 0; i < tamanhoCarregador; i++)
        {
            GameObject balaObj = Instantiate(balaSpritePrefab, balasPainel.transform);
            RectTransform rect = balaObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * espacamentoBalas, 0);
            rect.rotation = Quaternion.Euler(0, 0, rotacaoBala);
            balaSprites[i] = balaObj.GetComponent<Image>();
        }
    }

    void AtualizarSpritesBalas()
    {
        // Atualiza a cor dos sprites
        for (int i = 0; i < balaSprites.Length; i++)
        {
            if (i < municaoAtual)
            {
                balaSprites[i].color = Color.white;
            }
            else
            {
                balaSprites[i].color = Color.gray;
            }
        }
    }

    // Método pra limpar o HUD de munição
    public void LimparHUDMunicao()
    {
        nomeArmaTexto.text = "";
        spriteArma.sprite = null;
        municaoTexto.text = "";
        if (balaSprites != null)
        {
            foreach (Image sprite in balaSprites)
            {
                Destroy(sprite.gameObject);
            }
        }
        balaSprites = null;
        municaoAtual = 0;
        tamanhoCarregador = 0;
        tipoArmaAtual = -1;
    }
}