using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Collections;

// Script que gerencia os slots da barra de itens na HUD
public class HUDBarraDeItens : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab; // Prefab de um slot
    [SerializeField] private Transform slotsContainer; // Transform pai dos slots
    [SerializeField] private Sprite spriteArmaPistola; // Sprite pra arma pistola
    [SerializeField] private Sprite spriteArmaEspingarda; // Sprite pra arma espingarda

    private List<SlotData> slots = new List<SlotData>(); // Lista de slots exibidos
    private float tempoUltimoClique = 0f; // Tempo do �ltimo clique pra detectar duplo clique
    private float intervaloDuploClique = 0.3f; // Intervalo m�ximo pra considerar duplo clique (em segundos)
    private SlotData slotClicado = null; // Slot atualmente clicado
    private bool estaArrastando = false; // Indica se est� arrastando um slot
    private Vector2 posicaoInicialSlot; // Posi��o inicial do slot ao come�ar o arrastar
    private float distanciaMinimaDescartar = 100f; // Dist�ncia m�nima pra descartar (em pixels)

    // Classe pra armazenar dados de cada slot
    private class SlotData
    {
        public GameObject slotObj; // GameObject do slot
        public int indice; // �ndice do slot na barra
        public Button botao; // Componente Button do slot
        public RectTransform rectTransform; // RectTransform do slot
        public Image imagemFundo; // Componente Image do fundo do slot
    }

    void Start()
    {
        // Garante que o EventSystem est� presente na cena
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }

    // M�todo pra atualizar os slots na HUD
    public void AtualizarSlots(List<BarraDeItens.ItemBarra> slotsBarra, int[] municoesAcumuladas, bool[] armasColetadas)
    {
        // Remove os slots existentes
        foreach (SlotData slot in slots)
        {
            Destroy(slot.slotObj);
        }
        slots.Clear();

        // Cria novos slots pra cada arma na barra
        for (int i = 0; i < slotsBarra.Count; i++)
        {
            BarraDeItens.ItemBarra item = slotsBarra[i];
            if (item.tipoItem == BarraDeItens.TipoItem.Vazio)
            {
                continue; // N�o cria slot pra slots vazios
            }

            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            SlotData slotData = new SlotData
            {
                slotObj = slotObj,
                indice = i,
                botao = slotObj.GetComponent<Button>(),
                rectTransform = slotObj.GetComponent<RectTransform>(),
                imagemFundo = slotObj.GetComponent<Image>()
            };
            slots.Add(slotData);

            // Configura a sprite da arma
            Transform imagemTransform = slotObj.transform.Find("ImagemItem");
            if (imagemTransform == null)
            {
                Debug.LogError($"ImagemItem n�o encontrado no slot {i}!");
                continue;
            }
            Image imagemItem = imagemTransform.GetComponent<Image>();
            if (imagemItem == null)
            {
                Debug.LogError($"Componente Image n�o encontrado em ImagemItem no slot {i}!");
                continue;
            }
            imagemItem.sprite = item.indiceArma == 0 ? spriteArmaPistola : spriteArmaEspingarda;
            imagemItem.preserveAspect = true; // Preserva a propor��o do sprite

            // Ajusta o tamanho do RectTransform pra evitar distor��o
            if (imagemItem.sprite != null)
            {
                Vector2 spriteSize = imagemItem.sprite.bounds.size;
                RectTransform rect = imagemItem.GetComponent<RectTransform>();
                float aspectRatio = spriteSize.x / spriteSize.y;
                float alturaDesejada = 50f; // Altura fixa
                float larguraCalculada = alturaDesejada * aspectRatio;
                rect.sizeDelta = new Vector2(larguraCalculada, alturaDesejada);
            }

            // Configura o n�mero do slot (tecla correspondente)
            Transform numeroTransform = slotObj.transform.Find("TextoNumero");
            if (numeroTransform == null)
            {
                Debug.LogError($"TextoNumero n�o encontrado no slot {i}!");
                continue;
            }
            TextMeshProUGUI textoNumero = numeroTransform.GetComponent<TextMeshProUGUI>();
            if (textoNumero == null)
            {
                Debug.LogError($"Componente TextMeshProUGUI n�o encontrado em TextoNumero no slot {i}!");
                continue;
            }
            textoNumero.text = (i + 1).ToString(); // Ex.: slot 0 mostra "1"

            // Configura o texto da muni��o
            Transform municaoTransform = slotObj.transform.Find("TextoMunicao");
            if (municaoTransform == null)
            {
                Debug.LogError($"TextoMunicao n�o encontrado no slot {i}!");
                continue;
            }
            TextMeshProUGUI textoMunicao = municaoTransform.GetComponent<TextMeshProUGUI>();
            if (textoMunicao == null)
            {
                Debug.LogError($"Componente TextMeshProUGUI n�o encontrado em TextoMunicao no slot {i}!");
                continue;
            }
            textoMunicao.text = armasColetadas[item.indiceArma] ? municoesAcumuladas[item.indiceArma].ToString() : "";

            // Remove o texto de quantidade (usado anteriormente para muni��o nos slots)
            Transform textoQuantidadeTransform = slotObj.transform.Find("TextoQuantidade");
            if (textoQuantidadeTransform != null)
            {
                textoQuantidadeTransform.gameObject.SetActive(false);
            }

            // Adiciona eventos de clique ao slot
            slotData.botao.onClick.AddListener(() => OnSlotClick(slotData));
        }
    }

    // M�todo chamado quando o slot � clicado
    private void OnSlotClick(SlotData slotData)
    {
        float tempoAtual = Time.time;
        if (tempoAtual - tempoUltimoClique <= intervaloDuploClique)
        {
            // Duplo clique detectado: selecionar a arma
            StartCoroutine(PiscarSlot(slotData, true)); // Pisca antes de selecionar
            tempoUltimoClique = 0f; // Reseta o tempo pra evitar m�ltiplos duplos cliques
        }
        else
        {
            // Primeiro clique: registra o tempo e o slot clicado
            tempoUltimoClique = tempoAtual;
            slotClicado = slotData;
            posicaoInicialSlot = slotData.rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        // Verifica se est� arrastando
        if (slotClicado != null && Input.GetMouseButton(0))
        {
            // Se come�ou a arrastar (movimento m�nimo detectado)
            if (!estaArrastando)
            {
                Vector2 posicaoAtual = Input.mousePosition;
                Vector2 posicaoInicial = Camera.main.WorldToScreenPoint(slotClicado.rectTransform.position);
                if (Vector2.Distance(posicaoAtual, posicaoInicial) > 10f) // Movimento m�nimo pra iniciar arrastar
                {
                    estaArrastando = true;
                    slotClicado.imagemFundo.color = Color.red; // Muda a cor pra vermelho durante o arrastar
                }
            }

            if (estaArrastando)
            {
                // Move o slot com o mouse
                Vector2 posicaoMouse = Input.mousePosition;
                slotClicado.rectTransform.position = posicaoMouse;
            }
        }

        // Verifica se soltou o clique
        if (slotClicado != null && Input.GetMouseButtonUp(0))
        {
            if (estaArrastando)
            {
                // Calcula a dist�ncia arrastada
                Vector2 posicaoFinal = slotClicado.rectTransform.anchoredPosition;
                float distanciaArrastada = Vector2.Distance(posicaoFinal, posicaoInicialSlot);

                if (distanciaArrastada >= distanciaMinimaDescartar)
                {
                    // Descarta o item se arrastou o suficiente
                    StartCoroutine(PiscarSlot(slotClicado, false)); // Pisca antes de descartar
                }
                else
                {
                    // Volta o slot pra posi��o original se n�o arrastou o suficiente
                    slotClicado.rectTransform.anchoredPosition = posicaoInicialSlot;
                    slotClicado.imagemFundo.color = Color.white; // Volta a cor ao normal
                }
            }

            // Reseta as vari�veis de arrastar
            estaArrastando = false;
            slotClicado = null;
        }
    }

    // Corrotina pra piscar o slot antes de selecionar ou descartar
    private IEnumerator PiscarSlot(SlotData slotData, bool selecionarArma)
    {
        Image imagem = slotData.imagemFundo;
        Color corOriginal = imagem.color;
        for (int i = 0; i < 2; i++) // Pisca 2 vezes
        {
            imagem.color = Color.gray;
            yield return new WaitForSeconds(0.1f);
            imagem.color = corOriginal;
            yield return new WaitForSeconds(0.1f);
        }

        if (selecionarArma)
        {
            SelecionarArma(slotData.indice);
        }
        else
        {
            DescartarItem(slotData.indice);
        }
    }

    // M�todo pra selecionar a arma
    private void SelecionarArma(int indice)
    {
        BarraDeItens barra = FindFirstObjectByType<BarraDeItens>();
        if (barra != null)
        {
            List<BarraDeItens.ItemBarra> slotsBarra = barra.GetSlots();
            if (slotsBarra[indice].tipoItem == BarraDeItens.TipoItem.Arma)
            {
                ControlarArma controlarArma = FindFirstObjectByType<ControlarArma>();
                if (controlarArma != null)
                {
                    controlarArma.TrocarArma(slotsBarra[indice].indiceArma);
                }
            }
        }
        else
        {
            Debug.LogError("BarraDeItens n�o encontrada na cena!");
        }
    }

    // M�todo pra descartar o item
    private void DescartarItem(int indice)
    {
        BarraDeItens barra = FindFirstObjectByType<BarraDeItens>();
        if (barra != null)
        {
            barra.DescartarItem(indice);
        }
        else
        {
            Debug.LogError("BarraDeItens n�o encontrada na cena!");
        }
    }
}