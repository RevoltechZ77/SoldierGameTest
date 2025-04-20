using UnityEngine;

// Script para testes: dropa um coletável ao pressionar "P"
public class DropadorColetavel : MonoBehaviour
{
    public GameObject coletavelPrefab; // Prefab do coletável a ser dropado
    public Coletavel.TipoColetavel tipoColetavel; // Tipo do coletável a ser dropado
    public int quantidade; // Quantidade (usado apenas para munição)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            DroparColetavel();
        }
    }

    private void DroparColetavel()
    {
        Vector3 posicaoDrop = transform.position + new Vector3(2f, 0f, 0f); // Dropa à direita do jogador
        GameObject coletavelObj = Instantiate(coletavelPrefab, posicaoDrop, Quaternion.identity);
        Coletavel coletavel = coletavelObj.GetComponent<Coletavel>();
        if (coletavel != null)
        {
            coletavel.tipoColetavel = tipoColetavel;
            coletavel.quantidade = quantidade;
        }
        else
        {
            Debug.LogError("Coletavel component não encontrado no prefab dropado!");
            Destroy(coletavelObj);
        }
    }
}