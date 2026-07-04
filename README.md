# Knight Run

Runner infinito medieval inspirado em Temple Run / Subway Surfers — cavaleiro correndo por fases com cenários que mudam conforme a distância percorrida.

## Conceito

| Fase | Distância | Cenário placeholder | Mecânica |
|------|-----------|---------------------|----------|
| **Floresta** | 0 – 150 m | Chão verde, árvores (cubos) | Correr, trocar faixa, pular, deslizar |
| **Caverna** | 150 – 300 m | Pedra escura, estalactites | Mais obstáculos, ambiente escuro |
| **Trilho da Mina** | 300 m+ | Trilhos, carrinho de mina | Visual de carrinho no cavaleiro, trilhos por faixa |

## Como jogar (placeholder)

1. Abra o projeto no Unity 6 e pressione **Play** em qualquer cena (ex: `SampleScene`)
2. O jogo monta tudo automaticamente via `GameBootstrap`
3. **Enter** — iniciar corrida
4. **A/D** ou setas — trocar faixa (3 faixas)
5. **Espaço / W** — pular (obstáculos baixos)
6. **S** — deslizar (obstáculos altos)
7. **R** — reiniciar após game over

## Estrutura do código

```
Assets/KnightRun/Scripts/
├── Bootstrap/GameBootstrap.cs    # Monta cena inteira em runtime
├── Core/
│   ├── GameManager.cs            # Score, distância, velocidade, estados
│   ├── RunPhase.cs               # Enum das fases
│   ├── RunPhaseManager.cs        # Troca floresta → caverna → mina
│   └── RunPhaseSettings.cs       # Cores, velocidade, spawn por fase
├── Player/
│   ├── RunnerController.cs       # Movimento estilo endless runner
│   └── MineCartVisual.cs         # Carrinho placeholder na fase 3
├── World/
│   ├── TrackSegment.cs           # Pedaço de trilha + decoração
│   ├── TrackSegmentSpawner.cs    # Gera trilha infinita
│   └── PlaceholderMaterial.cs    # Materiais coloridos simples
├── Gameplay/
│   ├── Obstacle.cs               # Troncos, pedras, rochas
│   └── Collectible.cs            # Moedas douradas
├── Camera/CameraFollow.cs
└── UI/RunUI.cs                   # HUD placeholder
```

## Próximos passos sugeridos

### Curto prazo (prototype)
- [ ] Trocar cubos/cápsulas por modelos low-poly medievais
- [ ] Animações: corrida, pulo, deslize, idle
- [ ] Partículas na transição de fase (fade / névoa)
- [ ] Som placeholder (passos, moeda, colisão)

### Médio prazo (game feel)
- [ ] Input touch/swipe para mobile
- [ ] Curvas na trilha (desvio lateral suave)
- [ ] Power-ups (escudo, ímã de moedas, dash)
- [ ] Sequência scripted na fase do carrinho (QTE ou obstáculos exclusivos)

### Longo prazo (produto)
- [ ] Sistema de fases/missões em vez de só distância
- [ ] Menu principal, loja de skins de cavaleiro
- [ ] Persistência (recorde, moedas totais)
- [ ] Publicação Android/iOS

## Ajustar fases

Edite `RunPhaseDefaults.All` em `Assets/KnightRun/Scripts/Core/RunPhaseSettings.cs`:

- `distanceStart` / `distanceEnd` — quando cada fase começa
- `groundColor`, `wallColor`, `accentColor` — paleta placeholder
- `speedMultiplier` — dificuldade por fase
- `obstacleChance` — frequência de obstáculos (0–1)

## Instalação do Unity

Ver instruções em `install-unity.sh` ou na seção de instalação do README original.
