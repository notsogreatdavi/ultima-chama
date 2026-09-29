# Última Chama

Jogo survival top-down 2D em pixel art feito em Unity para o Desafio Individual Unity 3 (Jogos Digitais).

**Jogar no navegador (WebGL):** https://notsogreatdavi.github.io/ultima-chama/

## Experiência

> Quero que o jogador sinta tensão crescente ao ser cercado na escuridão, decidindo quando gastar a própria vida para sobreviver.

Pavio é a última chaminha acesa no Castelo Apagado, e os Breus querem apagá-lo. A luz ao redor dele é a sua vida: a cada dano o raio de luz encolhe, e com 1 chama Pavio mal enxerga e fica com medo.

## Controles

| Tecla | Ação |
|---|---|
| WASD / setas | Andar |
| Mouse esquerdo / Espaço | Faísca (tiro, mira no mouse) |
| Shift / Mouse direito | Sopro (dash, invulnerável durante) |
| Q / E | Labareda: destrói os Breus ao redor, custa 1 chama |
| ESC / P | Pausar |
| Enter | Jogar / acender de novo |

## Nomes no jogo

| Elemento | Nome |
|---|---|
| Jogador | Pavio |
| Inimigos | Breus (Breu, Caçadora, Brutamontes) |
| Coletável de pontos | Lumen |
| Coletável de vida | Vela |
| Ação especial | Labareda |
| Combo | Calor (x4 deixa a chama azul) |

## Requisitos atendidos

| Requisito | Onde |
|---|---|
| Menu inicial, pause e game over | `GameManager.cs` + `UiKit.cs` (IMGUI com painéis 9-slice e fontes pixel) |
| Comportamento inteligente do inimigo | `Enemy.cs` (desvio de obstáculos por CircleCast), `ChaserEnemy.cs` (perseguição), `FlankerEnemy.cs` (prevê a posição do jogador, circula e dá o bote), `BruteEnemy.cs` |
| Movimentação especial de câmera (extra) | `CameraFollow.cs` (seguimento suave, antecipação na mira, shake) + câmera pixel perfect |
| Ação especial com custo | Labareda em `PlayerController.cs`: mata tudo no raio, custa 1 chama e diminui a luz |
| Sistema de recompensas | Lumens que somem em 6 s, Calor multiplicador (zera ao levar dano), Vela rara, bônus por onda limpa, recorde salvo (`PlayerPrefs`) |
| Tilemap | `ArenaBuilder.cs` monta a fase com `Tilemap` + `TilemapCollider2D` (piso variado, paredes em falso 3D) |
| Morte e reinício do jogador | Chama 0 leva ao game over, reinício reconstrói o mundo (`Bootstrap.Rebuild`) |
| Morte e respawn do inimigo | Breus se dissolvem com Faísca/Labareda e renascem em ondas (`WaveSpawner.cs`) |
| Dificuldade gradativa | Cada onda: mais Breus, mais rápidos, menor intervalo, mais Caçadoras e Brutamontes |
| Animações | `SpriteAnimator.cs`: Pavio com 6 expressões e animações de parado, correr, Faísca, Sopro, dano e apagar; Breus flutuam, sofrem dano e se dissolvem |

## Arte

Pixel art original, com estilo inspirado em [penusbmic](https://penusbmic.itch.io/): fundo escuro e dessaturado, acentos que brilham, contorno no tom mais escuro de cada material e luz em faixas. Pavio tem 32x32 px, os tiles 16x16 px e a tela nativa é 320x180.

Todos os sprites saem de `tools/sprites/` (Python + Pillow): corpos e animações são rasterizados por parâmetros, rostos, tiles e UI são grades de texto. Para regenerar:

```
python3 tools/sprites/build_sprites.py --unity Assets/Resources/Sprites
```

O `Assets/Editor/PixelArtImporter.cs` configura os PNGs sozinho (filtro Point, 16 px por unidade). Se precisar, use o menu **Tools → Reimportar sprites**.

Iluminação: luz 2D do URP (`LightRig.cs`). A luz de Pavio encolhe com a vida e fica azul com Calor x4.

Fontes: [Silkscreen](https://fonts.google.com/specimen/Silkscreen) e [Pixelify Sans](https://fonts.google.com/specimen/Pixelify+Sans), licença SIL OFL (arquivos em `Assets/Resources/Fonts/`).

## Estrutura

Todo o jogo é montado por código a partir de `Bootstrap.cs` (`RuntimeInitializeOnLoadMethod`). A cena só precisa existir.

Herança e polimorfismo: `Enemy` é abstrata, e `ChaserEnemy`, `FlankerEnemy` e `BruteEnemy` sobrescrevem `Desired()` e a aparência.

## Build WebGL

No editor, menu **Build → WebGL** (ou `Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL`). Saída em `Builds/WebGL`.

## Roteiro do vídeo (até 10 min)

1. Menu: nome, proposta da experiência, controles (1 min)
2. Gameplay: Pavio andando, Faísca, Sopro, colisão com as paredes do Tilemap, expressões mudando (1 min)
3. IA: Breu perseguindo e desviando de pilares, Caçadora cortando caminho e dando o bote (1 min)
4. Recompensas: Lumens, Calor subindo até a chama azul, perder o Calor ao apanhar, Vela, bônus de onda (2 min)
5. Labareda: limpar um cerco e mostrar a luz diminuindo (1 min)
6. Dificuldade: comparar a onda 1 com a onda 4+ (Brutamontes) (1 min)
7. Pausa e game over com recorde (1 min)
8. O que aprendi e dificuldades (2 min)
