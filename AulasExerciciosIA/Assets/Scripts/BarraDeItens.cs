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
        public int indiceArma; // �ndice da arma (0 pra pistola, 1 pra espingarda)

        public ItemBarra(TipoItem tipo, int indice = -1)
        {
            tipoItem = tipo;
            indiceArma = indice;
        }
    }

    [SerializeField] private int numeroDeSlots = 4; // N�mero de slots desbloqueados
    private List<ItemBarra> slots; // Lista de slots na barra de itens (apenas armas)
    private int[] municoesAcumuladas; // Quantidade de muni��o acumulada (�ndice 0: pistola, 1: espingarda)
    private bool[] armasColetadas; // Indica se a arma foi coletada (�ndice 0: pistola, 1: espingarda)
    private HUDBarraDeItens hudBarra; // Refer�ncia ao HUD da barra de itens
    private ControlarArma controlarArma; // Refer�ncia ao script ControlarArma

    void Awake()
    {
        // Inicializa os slots
        slots = new List<ItemBarra>();
        for (int i = 0; i < numeroDeSlots; i++)
        {
            slots.Add(new ItemBarra(TipoItem.Vazio));
        }

        // Inicializa as muni��es acumuladas e armas coletadas
        municoesAcumuladas = new int[2]; // �ndice 0: pistola, 1: espingarda
        armasColetadas = new bool[2]; // Inicialmente falso (nenhuma arma coletada)
    }

    void Start()
    {
        // Pega o script HUDBarraDeItens
        hudBarra = FindFirstObjectByType<HUDBarraDeItens>();
        if (hudBarra == null)
        {
            Debug.LogError("HUDBarraDeItens n�o encontrado na cena!");
        }

        // Pega o script ControlarArma
        controlarArma = FindFirstObjectByType<ControlarArma>();
        if (controlarArma == null)
        {
            Debug.LogError("ControlarArma n�o encontrado na cena durante o Start!");
        }

        // Atualiza a HUD
        if (hudBarra != null)
        {
            hudBarra.AtualizarSlots(slots, municoesAcumuladas, armasColetadas);
        }
    }

    void Update()
    {
        // Verifica teclas num�ricas pra selecionar slots
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelecionarSlot(i - 1);
            }
        }
    }

    // M�todo pra selecionar um slot
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
            // Se o slot n�o cont�m uma arma, limpa o HUD de muni��o
            ControlarHUD hud = FindFirstObjectByType<ControlarHUD>();
            if (hud != null)
            {
                hud.LimparHUDMunicao();
            }
        }
    }

    // M�todo pra adicionar uma arma � barra
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

                // Atualiza as balas totais da arma com a muni��o acumulada
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

        Debug.LogWarning("Nenhum slot vazio dispon�vel pra adicionar a arma!");
    }

    // M�todo pra adicionar muni��o
    public void AdicionarMunicao(Coletavel.TipoMunicao tipoMunicao, int quantidade)
    {
        int indiceArma = tipoMunicao == Coletavel.TipoMunicao.Pistola ? 0 : 1;
        municoesAcumuladas[indiceArma] += quantidade;
        Debug.Log($"Adicionado {quantidade} de muni��o tipo {tipoMunicao}. Total acumulado: {municoesAcumuladas[indiceArma]}");

        // Se a arma correspondente j� foi coletada, adiciona a muni��o �s balas totais
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

    // M�todo pra descartar uma arma da barra
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
            armasColetadas[indiceArma] = false; // Marca a arma como n�o coletada
            Debug.Log($"Descartada arma do slot {indiceSlot}: {indiceArma}");
            slots[indiceSlot] = new ItemBarra(TipoItem.Vazio);

            // Se a arma descartada estava equipada, limpa o HUD de muni��o
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

    // M�todo pra obter a lista de slots (usado pela HUD)
    public List<ItemBarra> GetSlots()
    {
        return slots;
    }

    // M�todo pra obter as muni��es acumuladas (usado pela HUD)
    public int[] GetMunicoesAcumuladas()
    {
        return municoesAcumuladas;
    }

    // M�todo pra obter as armas coletadas (usado pela HUD)
    public bool[] GetArmasColetadas()
    {
        return armasColetadas;
    }
}