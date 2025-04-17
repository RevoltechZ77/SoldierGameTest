using UnityEngine;

[CreateAssetMenu(fileName = "NovaArma", menuName = "ArmaConfig")]
public class ArmaConfig : ScriptableObject
{
    public int tamanhoCarregador = 10; // Capacidade do carregador
    public float tempoRecarga = 2f; // Tempo (em segundos) pra recarregar a arma
    public GameObject projetilPrefab; // Prefab do projétil
    public float rotacaoSpriteProjetil = 0f; // Ajuste de rotação do sprite do projétil (em graus)
    public float velocidadeProjetil = 10f; // Velocidade fixa do projétil
    public float alcance = 10f; // Distância máxima do projétil
    public float tempoEntreTiros = 0.5f; // Cadência de tiro
    public int projeteisPorTiro = 1; // Número de projéteis por tiro (1 pra pistola, 3 pra espingarda)
    public float angulo = 5f; // Ângulo de espalhamento (ex.: -5°, 0°, +5° pra espingarda)
    public Vector3 offsetArma = new Vector3(0.5f, 0f, 0f); // Offset da arma em relação ao Braco
    public Vector3 offsetPontaArma = new Vector3(0.6f, 0.2f, 0f); // Offset da ponta da arma pra spawn do projétil
    public bool coice = false; // Se a arma aplica coice no Soldado Player
    public float forcaCoice = 0f; // Força do coice aplicado ao Soldado Player
    public float forcaRecuo = 0f; // Força do recuo visual da arma
    [Range(0f, 10f)] // Permite valores de 0 a 10 (0% a 1000%)
    public float forcaRecuoBraco = 0.5f; // Multiplicador do recuo da arma aplicado ao Braco
    public AudioClip somDisparo; // Som de disparo da arma
    public AudioClip somRecarga; // Som de recarga da arma
    public bool recargaManual = false; // Se a recarga é manual (um som por bala) ou automática (um som só)
}