using UnityEngine;
using System.Collections.Generic;

// Script que gerencia a barra de itens do jogador (os "bolsos" do colete)
public class BarraDeItens : MonoBehaviour
{
    // Enum pra representar o tipo de item no slot
    public enum TipoItem
    {
        Vazio,
        Arma
    }

    // Classe pra representar um item na barra
    [System.Serializable]
    public class ItemBarra
    {
        public TipoItem tipoItem; // Tipo do item (arma ou vazio)
        public int indiceArma; // Índice da arma (0 pra pistola, 1 pra espingarda)

        public ItemBarra(TipoItem tipo, int indice = -1)
        {
            tipoItem = tipo;
            indiceArma = indice;
        }
    }

    [SerializeField] private int numeroDeSlots = 4; // Número de slots desbloqueados
    private List<ItemBarra> slots; // Lista de slots na barra de itens (apenas armas)
    private int[] municoesAcumuladas; // Quantidade de munição acumulada (índice 0: pistola, 1: espingarda)
    private bool[] armasColetadas; // Indica se a arma foi coletada (índice 0: pistola, 1: espingarda)
    private HUDBarraDeItens hudBarra; // Referência ao HUD da barra de itens
    private ControlarArma controlarArma; // Referência ao script ControlarArma

    void Awake()
    {
        // Inicializa os slots
        slots = new List<ItemBarra>();
        for (int i = 0; i < numeroDeSlots; i++)
        {
            slots.Add(new ItemBarra(TipoItem.Vazio));
        }

        // Inicializa as munições acumuladas e armas coletadas
        municoesAcumuladas = new int[2]; // Índice 0: pistola, 1: espingarda
        armasColetadas = new bool[2]; // Inicialmente falso (nenhuma arma coletada)
    }

    void Start()
    {
        // Pega o script HUDBarraDeItens
        hudBarra = FindFirstObjectByType<HUDBarraDeItens>();
        if (hudBarra == null)
        {
            Debug.LogError("HUDBarraDeItens não encontrado na cena!");
        }

        // Pega o script ControlarArma
        controlarArma = FindFirstObjectByType<ControlarArma>();
        if (controlarArma == null)
        {
            Debug.LogError("ControlarArma não encontrado na cena durante o Start!");
        }

        // Atualiza a HUD
        if (hudBarra != null)
        {
            hudBarra.AtualizarSlots(slots, municoesAcumuladas, armasColetadas);
        }
    }

    void Update()
    {
        // Verifica teclas numéricas pra selecionar slots
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelecionarSlot(i - 1);
            }
        }
    }

    // Método pra selecionar um slot
    private void SelecionarSlot(int indiceSlot)
    {
        if (indiceSlot < 0 || indiceSlot >= slots.Count)
        {
            Debug.LogWarning($"Slot {indiceSlot} fora dos limites!");
            return;
        }

        ItemBarra item = slots[indiceSlot];
        if (item.tipoItem == TipoItem.Arma && controlarArma != null)
        {
            controlarArma.TrocarArma(item.indiceArma);
            Debug.Log($"Arma no slot {indiceSlot} selecionada: {controlarArma.GetArma(item.indiceArma).nomeArma}");
        }
        else
        {
            // Se o slot não contém uma arma, limpa o HUD de munição
            ControlarHUD hud = FindFirstObjectByType<ControlarHUD>();
            if (hud != null)
            {
                hud.LimparHUDMunicao();
            }
        }
    }

    // Método pra adicionar uma arma à barra
    public void AdicionarArma(int indiceArma)
    {
        // Procura um slot vazio
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].tipoItem == TipoItem.Vazio)
            {
                slots[i] = new ItemBarra(TipoItem.Arma, indiceArma);
                armasColetadas[indiceArma] = true; // Marca a arma como coletada
                Debug.Log($"Arma {indiceArma} adicionada ao slot {i}.");

                // Atualiza as balas totais da arma com a munição acumulada
                if (controlarArma != null)
                {
                    ArmaBase arma = controlarArma.GetArma(indiceArma);
                    if (arma != null)
                    {
                        arma.balasTotais += municoesAcumuladas[indiceArma];
                        arma.AtualizarHUD();
                    }
                }

                if (hudBarra != null)
                {
                    hudBarra.AtualizarSlots(slots, municoesAcumuladas, armasColetadas);
                }
                return;
            }
        }

        Debug.LogWarning("Nenhum slot vazio disponível pra adicionar a arma!");
    }

    // Método pra adicionar munição
    public void AdicionarMunicao(Coletavel.TipoMunicao tipoMunicao, int quantidade)
    {
        int indiceArma = tipoMunicao == Coletavel.TipoMunicao.Pistola ? 0 : 1;
        municoesAcumuladas[indiceArma] += quantidade;
        Debug.Log($"Adicionado {quantidade} de munição tipo {tipoMunicao}. Total acumulado: {municoesAcumuladas[indiceArma]}");

        // Se a arma correspondente já foi coletada, adiciona a munição às balas totais
        if (armasColetadas[indiceArma] && controlarArma != null)
        {
            ArmaBase arma = controlarArma.GetArma(indiceArma);
            if (arma != null)
            {
                arma.balasTotais += quantidade;
                arma.AtualizarHUD();
            }
        }

        if (hudBarra != null)
        {
            hudBarra.AtualizarSlots(slots, municoesAcumuladas, armasColetadas);
        }
    }

    // Método pra descartar uma arma da barra
    public void DescartarItem(int indiceSlot)
    {
        if (indiceSlot < 0 || indiceSlot >= slots.Count)
        {
            Debug.LogError($"Slot {indiceSlot} fora dos limites!");
            return;
        }

        ItemBarra item = slots[indiceSlot];
        if (item.tipoItem == TipoItem.Arma)
        {
            int indiceArma = item.indiceArma;
            armasColetadas[indiceArma] = false; // Marca a arma como não coletada
            Debug.Log($"Descartada arma do slot {indiceSlot}: {indiceArma}");
            slots[indiceSlot] = new ItemBarra(TipoItem.Vazio);

            // Se a arma descartada estava equipada, limpa o HUD de munição
            ControlarHUD hud = FindFirstObjectByType<ControlarHUD>();
            if (hud != null)
            {
                hud.LimparHUDMunicao();
            }

            if (hudBarra != null)
            {
                hudBarra.AtualizarSlots(slots, municoesAcumuladas, armasColetadas);
            }
        }
    }

    // Método pra obter a lista de slots (usado pela HUD)
    public List<ItemBarra> GetSlots()
    {
        return slots;
    }

    // Método pra obter as munições acumuladas (usado pela HUD)
    public int[] GetMunicoesAcumuladas()
    {
        return municoesAcumuladas;
    }

    // Método pra obter as armas coletadas (usado pela HUD)
    public bool[] GetArmasColetadas()
    {
        return armasColetadas;
    }
}