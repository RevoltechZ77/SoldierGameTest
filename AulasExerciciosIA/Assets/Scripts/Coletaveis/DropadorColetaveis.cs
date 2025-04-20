using UnityEngine;

// Script para testes: dropa um colet�vel ao pressionar "P"
public class DropadorColetavel : MonoBehaviour
{
    public GameObject coletavelPrefab; // Prefab do colet�vel a ser dropado
    public Coletavel.TipoColetavel tipoColetavel; // Tipo do colet�vel a ser dropado
    public int quantidade; // Quantidade (usado apenas para muni��o)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            DroparColetavel();
        }
    }

    private void DroparColetavel()
    {
        Vector3 posicaoDrop = transform.position + new Vector3(2f, 0f, 0f); // Dropa � direita do jogador
        GameObject coletavelObj = Instantiate(coletavelPrefab, posicaoDrop, Quaternion.identity);
        Coletavel coletavel = coletavelObj.GetComponent<Coletavel>();
        if (coletavel != null)
        {
            coletavel.tipoColetavel = tipoColetavel;
            coletavel.quantidade = quantidade;
        }
        else
        {
            Debug.LogError("Coletavel component n�o encontrado no prefab dropado!");
            Destroy(coletavelObj);
        }
    }
}