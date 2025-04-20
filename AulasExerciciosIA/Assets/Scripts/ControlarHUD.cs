using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlarHUD : MonoBehaviour
{
    public TextMeshProUGUI nomeArmaTexto; // Refer�ncia ao texto do nome da arma
    public Image spriteArma; // Refer�ncia � imagem do sprite da arma
    public TextMeshProUGUI municaoTexto; // Refer�ncia ao texto da muni��o
    public GameObject balasPainel; // Refer�ncia ao painel onde os sprites das balas ser�o instanciados
    public GameObject balaSpritePistola; // Prefab do sprite de bala da pistola
    public GameObject balaSpriteEspingarda; // Prefab do sprite de bala da espingarda
    public float espacamentoBalas = 40f; // Espa�amento entre os sprites
    public float rotacaoBala = 0f; // Rota��o dos sprites de bala

    private Image[] balaSprites; // Array de sprites das balas
    private int municaoAtual; // Muni��o atual
    private int tamanhoCarregador; // Tamanho m�ximo do carregador
    private int tipoArmaAtual; // Arma atual (0: pistola, 1: espingarda)

    void Start()
    {
        // Garante que o sprite da arma preserve a propor��o
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

        // Ajusta o tamanho do RectTransform pra evitar distor��o
        if (spriteArma.sprite != null)
        {
            Vector2 spriteSize = spriteArma.sprite.bounds.size;
            RectTransform rect = spriteArma.GetComponent<RectTransform>();
            float aspectRatio = spriteSize.x / spriteSize.y;
            float alturaDesejada = 50f;
            float larguraCalculada = alturaDesejada * aspectRatio;
            rect.sizeDelta = new Vector2(larguraCalculada, alturaDesejada);
        }

        // Atualiza o texto da muni��o
        municaoTexto.text = $"{novasBalasTotais}";

        // Se o tamanho do carregador ou o tipo de arma mudou, recria os sprites
        if (novoTamanhoCarregador != tamanhoCarregador || tipoArma != tipoArmaAtual)
        {
            tamanhoCarregador = novoTamanhoCarregador;
            tipoArmaAtual = tipoArma;
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

        // Verifica se o prefab t� configurado
        if (balaSpritePrefab == null)
        {
            Debug.LogError($"Prefab de sprite de bala {(tipoArmaAtual == 0 ? "Pistola" : "Espingarda")} n�o configurado!");
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

    // M�todo pra limpar o HUD de muni��o
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