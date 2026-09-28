# Última Chama

Jogo survival top-down 2D feito em Unity para o Desafio Individual Unity 3 (Jogos Digitais).

**Jogar no navegador (WebGL):** https://notsogreatdavi.github.io/ultima-chama/

## Experiência

> Quero que o jogador sinta tensão crescente ao ser cercado na escuridão, decidindo quando gastar a própria vida para sobreviver.

Você é a última chama numa masmorra tomada por sombras. A luz ao seu redor é a sua vida: a cada dano, o raio de luz encolhe e fica mais difícil enxergar quem vem.

## Controles

| Tecla | Ação |
|---|---|
| WASD / setas | Mover |
| Mouse esquerdo / Espaço | Atirar faíscas (mira no mouse) |
| Shift / Mouse direito | Dash (invulnerável durante o dash) |
| Q / E | NOVA: destrói todas as sombras ao redor, custa 1 de vida |
| ESC / P | Pausar |
| Enter | Jogar / jogar de novo |

## Requisitos atendidos

| Requisito | Onde |
|---|---|
| Menu inicial, pause e game over | `GameManager.cs` (estados + IMGUI) |
| Comportamento inteligente do inimigo | `Enemy.cs` (desvio de obstáculos por CircleCast), `ChaserEnemy.cs` (perseguição), `FlankerEnemy.cs` (prevê a posição do jogador e circula) |
| Movimentação especial de câmera (extra) | `CameraFollow.cs` (seguimento suave, antecipação na mira, shake) |
| Ação especial com custo | NOVA em `PlayerController.cs`: mata tudo no raio, custa 1 de vida e diminui a luz |
| Sistema de recompensas | Gemas que somem em 6s, combo multiplicador (zera ao levar dano), coração raro, bônus por onda limpa, recorde salvo (`PlayerPrefs`) |
| Tilemap | `ArenaBuilder.cs` monta a fase com `Tilemap` + `TilemapCollider2D` |
| Morte e reinício do jogador | Vida 0 leva ao game over, reinício reconstrói o mundo (`Bootstrap.Rebuild`) |
| Morte e respawn do inimigo | Sombras morrem com tiro/Nova e renascem em ondas (`WaveSpawner.cs`) |
| Dificuldade gradativa | Cada onda: mais sombras, mais rápidas, menor intervalo, mais caçadoras e sombras resistentes |
| Animações | `SpriteAnimator.cs`: chama parada/andando, sombras pulsando |

## Estrutura

Todo o jogo é montado por código a partir de `Bootstrap.cs` (`RuntimeInitializeOnLoadMethod`). A cena só precisa existir. Os sprites são gerados em `SpriteFactory.cs`.

Herança e polimorfismo: `Enemy` é abstrata, `ChaserEnemy` e `FlankerEnemy` sobrescrevem `Desired()`.

## Build WebGL

```
Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
```

Saída em `Builds/WebGL` (sem compressão, pronto para GitHub Pages).

## Roteiro do vídeo (até 10 min)

1. Menu: nome, proposta da experiência, controles (1 min)
2. Gameplay: movimento, tiro, dash, colisão com paredes do Tilemap (1 min)
3. IA: sombra comum perseguindo e desviando de pilares, caçadora cortando caminho (1 min)
4. Recompensas: gemas, combo subindo, perder combo ao levar dano, coração, bônus de onda (2 min)
5. NOVA: limpar um cerco e mostrar a luz diminuindo (1 min)
6. Dificuldade: comparar onda 1 com onda 4+ (1 min)
7. Pause e game over com recorde (1 min)
8. O que aprendi e dificuldades (2 min)
