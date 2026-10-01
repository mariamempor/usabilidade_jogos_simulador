# Clean Slate — Simulador de Lavagem de Dinheiro

> **Projeto acadêmico e ficcional.** O tema serve exclusivamente para discutir fluxos financeiros, indicadores de risco, tomada de decisão e UX em um simulador. Não use os conceitos apresentados como orientação para atividades reais.

Protótipo 2D para Unity com dois loops: um dashboard de trabalho corporativo e uma transição de rotina em pixel art. A estrutura privilegia UI legível, código modular e arte de baixo custo de produção.

## Requisitos

- Unity **2022.3 LTS** ou superior (template **2D Core**).
- TextMeshPro pode ser usado nos prefabs finais, mas os scripts de exemplo usam `UnityEngine.UI.Text` para funcionar sem migração adicional.
- Não há dependências de tweening: os fades e microanimações usam corrotinas nativas, `CanvasGroup.alpha`, `Transform.localScale` e tempo não escalado do Unity.


## Executar o protótipo agora

Não há um `.exe` dentro de `Assets`: no Unity, o teste é iniciado pelo botão **Play** do Editor. Para este protótipo, abra a pasta raiz no Unity Hub e pressione **Play**, inclusive na cena vazia `Untitled` mostrada pelo Editor. `RuntimeDemoBootstrap` é inicializado automaticamente após o carregamento da cena e cria o Canvas de tutorial e o dashboard de teste sem precisar arrastar objetos no Inspector. Ele usa a fonte interna `LegacyRuntime.ttf`, compatível com Unity 2022.3.

No dashboard, clique em **INICIAR SIMULAÇÃO**, depois em **PROCESSAR R$ 8.000** e **ENCERRAR O DIA**. Para gerar um executável posteriormente, crie/salve uma cena em `Assets/Scenes/Main.unity`, adicione-a em **File > Build Settings > Scenes In Build** e escolha **Build**.

## Estrutura de pastas

```text
Assets/
  Art/Sprites                 # personagens, quarto e ícones de empresas
  Prefabs/Popups              # Popup_Noticia e Popup_NotificacaoOficial
  Prefabs/PixelArt            # quarto e objetos da rotina
  Scenes/Main.unity           # cena principal a criar
  ScriptableObjects/Companies # Lavanderia, Bar, Boate, Concessionária
  ScriptableObjects/Events    # eventos News e Official
  Scripts/Core                # GameManager, TurnManager, PixelRoutinePlayer
  Scripts/Finance             # empresas e cálculo de operação/risco
  Scripts/Popups              # dados e views dos dois modelos de alerta
  Scripts/UI                  # binding do dashboard
```

## Hierarquia exata da cena `Main`

```text
Main
├── _Systems
│   ├── GameManager (GameManager)
│   ├── FinancialManager (FinancialManager)
│   ├── TurnManager (TurnManager)
│   └── PopupManager (PopupManager)
├── EventSystem
├── Main Camera
├── PixelRoomRoot [inativo ao iniciar]
│   ├── RoomBackground (SpriteRenderer)
│   ├── Character (SpriteRenderer)
│   └── Monitor (SpriteRenderer)
└── UIRoot (Canvas: Screen Space - Overlay; Canvas Scaler: Scale With Screen Size, 1920×1080)
    ├── WorkCanvas (CanvasGroup)
    │   ├── TopBar / MetricCards (saldo sujo, saldo limpo, risco, dias)
    │   ├── Sidebar / Tabs (Visão geral, Empresas, Transferências, Alertas)
    │   ├── ContentArea / CompanyCards
    │   └── EndDayButton
    ├── TutorialModal (CanvasGroup)
    │   ├── Step01_Placement
    │   ├── Step02_Layering
    │   ├── Step03_Integration
    │   └── StartButton → GameManager.CompleteTutorial
    ├── PopupLayer (stretch total; ordenação acima do dashboard)
    └── BlackOverlay (Image preta + CanvasGroup, alpha 0)
        └── DayLabel (Text central, “DIA X - 07:00 AM”)
```

### Ligações no Inspector

1. No objeto `_Systems`, arraste `WorkCanvas`, `PixelRoomRoot`, `BlackOverlay`, `DayLabel`, `PixelRoutinePlayer` e `FinancialManager` aos campos do `TurnManager`.
2. Em `PixelRoutinePlayer`, associe os `SpriteRenderer` de personagem/monitor e adicione sprites ordenados: noite = levantar, apagar luz, deitar; manhã = acordar, café, cadeira. A sequência é executada por coroutine, sem Animator.
3. Crie quatro assets **Create > Clean Slate > Finance > Facade Company**. Sugestão: Lavanderia (capacidade 8.000, perda 12%, fator 0,0004), Bar (15.000, 22%, 0,0008), Boate (25.000, 30%, 0,0012) e Concessionária (40.000, 18%, 0,0015). Arraste-os para `FinancialManager.companies`.
4. Para cada card de empresa, crie um botão que chama `FinancialManager.TryPlace` via um componente de formulário próprio (ou botão intermediário que fornece o asset e valor digitado). O método bloqueia saldo insuficiente e capacidade excedida.
5. No botão **Encerrar o dia**, registre `TurnManager.EndDayButtonPressed`.

## Pop-ups realistas

### Prefab `Popup_Noticia`

Faça um painel 900×520, sombra, faixa vermelha superior com o texto **URGENTE / BREAKING NEWS**, `Image` para a matéria, título em negrito, corpo editorial e botão de fechar. Adicione `NewsPopupView` e associe os quatro campos. Não use um diálogo genérico: trate-o como uma página curta de portal jornalístico, com hierarquia tipográfica e imagem.

### Prefab `Popup_NotificacaoOficial`

Faça um painel branco institucional com faixa verde-escura/azul, área para brasão fictício, instituição, protocolo, assunto, corpo jurídico, uma barra de prazo e três ações: **Pagar taxa**, **Ocultar registros** e **Aceitar autuação**. Adicione `OfficialPopupView`, associe textos e botões. Os nomes são ficcionais para a apresentação acadêmica; não use marcas, logotipos ou documentos oficiais reais.

Crie assets pelos menus **Clean Slate > Events > News Event** e **Official Notification**, preencha as faixas de suspeita e arraste-os para as listas do `PopupManager`. Ao iniciar um novo dia, ele seleciona um evento compatível com `Suspicion`; notícias têm impacto direto e notificações exigem escolha.

## Fórmula e fluxo de turno

Ao processar uma empresa, `FinancialManager` limita o valor à capacidade diária, desconta do saldo sujo e credita o resultado líquido. No fim do dia aplica:

```text
suspeita aplicada = Σ(valor processado × fator de risco da empresa) × (1 − redutor ativo)
```

Em seguida, `TurnManager` faz fade do PC, toca a rotina noturna, mostra o cartão do próximo dia, toca a manhã, liga o monitor e restaura o Canvas. `GameManager` impede operações fora de `Work` e encerra o jogo ao chegar a 100% de suspeita ou ao fim do prazo.

## Animações UI nativas (sem dependências)

`UiTransitionUtility` implementa `Fade` para `CanvasGroup` e `ScaleIn` para `Transform` usando `IEnumerator`, `Time.unscaledDeltaTime` e `Mathf`. Por isso, as transições continuam mesmo se a equipe pausar `Time.timeScale`, sem instalar pacotes de animação nem configurar um Animator. Para um novo painel, mantenha a animação no controlador da view e use este padrão:

```csharp
panel.alpha = 0f;
yield return UiTransitionUtility.Fade(panel, 1f, 0.25f);
popup.transform.localScale = Vector3.zero;
yield return UiTransitionUtility.ScaleIn(popup.transform, 0.25f);
```

As corrotinas terminam automaticamente quando o objeto é destruído. Evite Animator para microinterações de dashboard; reserve sprites para a rotina em pixel art.

## Referências visuais para moodboard (não copiar assets)

- **Papers, Please**: densidade de informações, carimbos, documentos e consequência clara de escolhas.
- **Not Tonight**: fluxo de inspeção sob pressão e layout de interface ocupacional.
- **The Sims**: leitura instantânea de rotina doméstica para o intervalo dia/noite.
- **Dashboard corporativo dark mode**: superfícies em grafite, cartões com bordas discretas, uma cor de semântica por estado (verde=limpo, âmbar=atenção, vermelho=risco).

Use essas referências apenas como linguagem visual. Produza sprites, textos, ícones e marcas próprios para evitar confusão com obras e órgãos reais.

## Expansões sugeridas

1. **Rede de confiança**: contatos fictícios têm favores limitados; cada ajuda baixa risco agora, mas cria uma obrigação em dias posteriores.
2. **Auditoria explicável**: um painel “Por que meu risco subiu?” mostra um gráfico de contribuição por empresa e por evento, transformando derrota em aprendizado.
3. **Modo apresentação**: um botão reproduz uma semana pré-configurada em 90 segundos, com destaques guiados para tutorial, operação, alerta e ciclo pixel art.

## Correção do erro `CS0246` de namespace ausente

Esta versão não contém referências a bibliotecas externas de animação. Se o Console ainda informar um namespace externo ausente em `TurnManager.cs` ou `PopupManager.cs`, o Editor está compilando uma cópia anterior dos arquivos ou um cache local.

1. Feche o Unity.
2. Confirme que `Assets/Scripts/Core/TurnManager.cs` não possui uma diretiva `using` de pacote externo e que `Assets/Scripts/Popups/PopupManager.cs` também não possui essa diretiva. Estas são as versões presentes neste repositório.
3. Apague a pasta `Library` do **projeto local** (não a pasta `Assets` e não arquivos versionados). O Unity a recriará ao abrir o projeto.
4. Abra o projeto novamente; se necessário, use **Assets > Reimport All**. Só saia do Safe Mode quando o Console não apresentar erros.

Não instale nenhum pacote de animação para resolver esse erro: os scripts usam somente as APIs padrão do Unity (`IEnumerator`, `CanvasGroup`, `Transform`, `Mathf` e `Time`).
