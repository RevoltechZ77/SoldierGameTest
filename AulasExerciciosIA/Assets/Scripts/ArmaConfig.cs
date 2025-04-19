using UnityEngine;

// ScriptableObject que define as configurações de uma arma
[CreateAssetMenu(fileName = "NovaArma", menuName = "Armas/Nova Arma", order = 1)]
public class ArmaConfig : ScriptableObject
{
    public string nomeArma; // Nome da arma (ex.: "Carrion 9mm", "ESP Cano Curto")
    public int tamanhoCarregador; // Quantidade máxima de balas no carregador
    public float tempoEntreTiros; // Intervalo entre disparos (cadência)
    public float tempoRecarga; // Tempo necessário pra recarregar a arma
    public float tempoAtrasoRecargaZerada; // Tempo de atraso quando a recarga é iniciada com o carregador zerado
    public float forcaRecuo; // Força do recuo visual da arma
    public float forcaRecuoBraco; // Multiplicador do recuo pro braço
    public float forcaCoice; // Força do coice aplicado ao jogador
    public bool coice; // Se a arma aplica coice ao jogador
    public float angulo; // Ângulo de dispersão dos projéteis (ex.: pra espingarda)
    public int projeteisPorTiro; // Quantidade de projéteis por disparo (ex.: 3 pra espingarda)
    public float velocidadeProjetil; // Velocidade do projétil
    public float alcance; // Distância máxima que o projétil percorre
    public float rotacaoSpriteProjetil; // Rotação adicional do sprite do projétil
    public Vector3 offsetArma; // Posição relativa da arma em relação ao braço
    public Vector3 offsetPontaArma; // Posição da ponta da arma (onde o projétil é instanciado)
    public GameObject projetilPrefab; // Prefab do projétil
    public AudioClip somDisparo; // Som do disparo
    public AudioClip somRecarga; // Som da recarga
    public Sprite spriteArma; // Sprite visual da arma
}