using UnityEngine;

public class ControlarProjetil : MonoBehaviour
{
    public ProjetilConfig config; // Configura��o do proj�til (dano, escala, etc.)
    private float alcance; // Alcance m�ximo do proj�til
    private Vector3 posicaoInicial; // Posi��o onde o proj�til foi instanciado

    void Start()
    {
        // Aplica a escala do proj�til definida no config
        transform.localScale = Vector3.one * config.escala;
        // Destroi o proj�til ap�s o tempo de vida definido
        Destroy(gameObject, config.tempoDeVida);
        // Salva a posi��o inicial pra checar o alcance
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // Verifica se o proj�til excedeu o alcance m�ximo
        if (alcance > 0 && Vector3.Distance(posicaoInicial, transform.position) >= alcance)
        {
            Destroy(gameObject); // Destroi o proj�til
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignora colis�o com outros proj�teis
        if (collision.gameObject.GetComponent<ControlarProjetil>() != null)
        {
            return; // Sai sem processar a colis�o
        }

        // Aplica o comportamento baseado no tipo do proj�til
        switch (config.tipo)
        {
            case ProjetilConfig.TipoProjetil.Normal:
                // Dano b�sico
                AplicarDano(collision.gameObject);
                Destroy(gameObject);
                break;
            case ProjetilConfig.TipoProjetil.Fragmentacao:
                // Explode em fragmentos (implementar depois)
                AplicarDano(collision.gameObject);
                Destroy(gameObject);
                break;
            case ProjetilConfig.TipoProjetil.Incendiario:
                // Causa dano ao longo do tempo (implementar depois)
                AplicarDano(collision.gameObject);
                Destroy(gameObject);
                break;
            case ProjetilConfig.TipoProjetil.Congelante:
                // Reduz velocidade do alvo (implementar depois)
                AplicarDano(collision.gameObject);
                Destroy(gameObject);
                break;
            case ProjetilConfig.TipoProjetil.Perfurante:
                // Ignora colis�o e continua (n�o destr�i agora)
                AplicarDano(collision.gameObject);
                break;
        }
    }

    void AplicarDano(GameObject alvo)
    {
        // Implementar l�gica de dano quando houver inimigos
        Debug.Log($"Causou {config.dano} de dano em {alvo.name}");
    }

    public void SetAlcance(float novoAlcance)
    {
        // Define o alcance do proj�til
        alcance = novoAlcance;
    }
}