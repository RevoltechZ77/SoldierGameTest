using UnityEngine;

[CreateAssetMenu(fileName = "NovoProjetil", menuName = "Projeteis/ProjetilConfig")]
public class ProjetilConfig : ScriptableObject
{
    public string nomeProjetil = "Projetil"; // Nome do projétil (ex.: Normal, Incendiário)
    public float dano = 10f; // Dano causado
    public float escala = 1f; // Escala do projétil (tamanho)
    public float tempoDeVida = 3f; // Tempo até o projétil ser destruído
    public enum TipoProjetil { Normal, Fragmentacao, Incendiario, Congelante, Perfurante }
    public TipoProjetil tipo = TipoProjetil.Normal; // Tipo do projétil
}