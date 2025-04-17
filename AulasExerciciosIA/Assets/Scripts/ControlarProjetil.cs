using UnityEngine;

public class ControlarProjetil : MonoBehaviour
{
    public ProjetilConfig config; // Configuração do projétil (dano, escala, etc.)
    private float alcance; // Alcance máximo do projétil
    private Vector3 posicaoInicial; // Posição onde o projétil foi instanciado

    void Start()
    {
        // Aplica a escala do projétil definida no config
        transform.localScale = Vector3.one * config.escala;
        // Destroi o projétil após o tempo de vida definido
        Destroy(gameObject, config.tempoDeVida);
        // Salva a posição inicial pra checar o alcance
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // Verifica se o projétil excedeu o alcance máximo
        if (alcance > 0 && Vector3.Distance(posicaoInicial, transform.position) >= alcance)
        {
            Destroy(gameObject); // Destroi o projétil
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignora colisão com outros projéteis
        if (collision.gameObject.GetComponent<ControlarProjetil>() != null)
        {
            return; // Sai sem processar a colisão
        }

        // Aplica o comportamento baseado no tipo do projétil
        switch (config.tipo)
        {
            case ProjetilConfig.TipoProjetil.Normal:
                // Dano básico
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
                // Ignora colisão e continua (não destrói agora)
                AplicarDano(collision.gameObject);
                break;
        }
    }

    void AplicarDano(GameObject alvo)
    {
        // Implementar lógica de dano quando houver inimigos
        Debug.Log($"Causou {config.dano} de dano em {alvo.name}");
    }

    public void SetAlcance(float novoAlcance)
    {
        // Define o alcance do projétil
        alcance = novoAlcance;
    }
}