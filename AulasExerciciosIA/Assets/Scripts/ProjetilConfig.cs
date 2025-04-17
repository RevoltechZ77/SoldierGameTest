using UnityEngine;

[CreateAssetMenu(fileName = "NovoProjetil", menuName = "Projeteis/ProjetilConfig")]
public class ProjetilConfig : ScriptableObject
{
    public string nomeProjetil = "Projetil"; // Nome do proj�til (ex.: Normal, Incendi�rio)
    public float dano = 10f; // Dano causado
    public float escala = 1f; // Escala do proj�til (tamanho)
    public float tempoDeVida = 3f; // Tempo at� o proj�til ser destru�do
    public enum TipoProjetil { Normal, Fragmentacao, Incendiario, Congelante, Perfurante }
    public TipoProjetil tipo = TipoProjetil.Normal; // Tipo do proj�til
}