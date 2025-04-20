using UnityEngine;
using TMPro;

public class Coletavel : MonoBehaviour
{
    public enum TipoColetavel
    {
        MunicaoPistola,
        MunicaoEspingarda,
        ArmaPistola,
        ArmaEspingarda
    }

    public TipoColetavel tipoColetavel;
    public int quantidade; // Quantidade (usado apenas para munição)
    [SerializeField] private Sprite spritePistola;
    [SerializeField] private Sprite spriteEspingarda;
    [SerializeField] private Sprite spriteArmaPistola;
    [SerializeField] private Sprite spriteArmaEspingarda;
    [SerializeField] private SpriteRenderer spriteItem;
    [SerializeField] private TextMeshPro textoQuantidade;

    private SpriteRenderer spriteMoldura;

    void Awake()
    {
        spriteMoldura = GetComponent<SpriteRenderer>();
        if (spriteMoldura == null)
        {
            Debug.LogError("SpriteRenderer da moldura não encontrado no coletável!");
            Destroy(gameObject);
            return;
        }

        if (spriteItem == null)
        {
            Debug.LogError("SpriteRenderer do item não configurado no coletável!");
            Destroy(gameObject);
            return;
        }

        if (textoQuantidade == null)
        {
            Debug.LogError("TextMeshPro não configurado no coletável!");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        switch (tipoColetavel)
        {
            case TipoColetavel.MunicaoPistola:
                spriteItem.sprite = spritePistola;
                break;
            case TipoColetavel.MunicaoEspingarda:
                spriteItem.sprite = spriteEspingarda;
                break;
            case TipoColetavel.ArmaPistola:
                spriteItem.sprite = spriteArmaPistola;
                quantidade = 0; // Armas não têm quantidade
                break;
            case TipoColetavel.ArmaEspingarda:
                spriteItem.sprite = spriteArmaEspingarda;
                quantidade = 0; // Armas não têm quantidade
                break;
            default:
                Debug.LogError($"Tipo de coletável inválido: {tipoColetavel}");
                Destroy(gameObject);
                return;
        }

        AtualizarTextoQuantidade();
    }

    private void AtualizarTextoQuantidade()
    {
        textoQuantidade.text = (tipoColetavel == TipoColetavel.MunicaoPistola || tipoColetavel == TipoColetavel.MunicaoEspingarda) ? quantidade.ToString() : "";
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BarraDeItens barra = collision.gameObject.GetComponent<BarraDeItens>();
            if (barra == null)
            {
                Debug.LogError("BarraDeItens não encontrada no jogador!");
                return;
            }

            switch (tipoColetavel)
            {
                case TipoColetavel.MunicaoPistola:
                    barra.AdicionarMunicao(Coletavel.TipoMunicao.Pistola, quantidade);
                    break;
                case TipoColetavel.MunicaoEspingarda:
                    barra.AdicionarMunicao(Coletavel.TipoMunicao.Espingarda, quantidade);
                    break;
                case TipoColetavel.ArmaPistola:
                    barra.AdicionarArma(0);
                    break;
                case TipoColetavel.ArmaEspingarda:
                    barra.AdicionarArma(1);
                    break;
            }

            Destroy(gameObject);
        }
    }

    // Método auxiliar para compatibilidade com DropadorColetavel
    public TipoMunicao GetTipoMunicao()
    {
        return tipoColetavel == TipoColetavel.MunicaoPistola ? TipoMunicao.Pistola : TipoMunicao.Espingarda;
    }

    // Enum auxiliar para compatibilidade com outros scripts
    public enum TipoMunicao
    {
        Pistola,
        Espingarda
    }
}