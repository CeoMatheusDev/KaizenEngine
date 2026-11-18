 <div align="center">
  <img src="LogoBanner_1.png" alt="Kaizen Game Engine" width="100%">
</div>

<div align="center">

[![Versão](https://img.shields.io/badge/versão-4.8.0--beta-blue.svg)]()
[![Licença](https://img.shields.io/badge/licença-AGPL--3.0%20%2B%20Exceção%20para%20Jogos-green.svg)](LICENSE-ADDITIONAL-TERMS.md)
[![Plataformas](https://img.shields.io/badge/plataformas-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()
[![Status](https://img.shields.io/badge/status-em%20desenvolvimento-orange.svg)]()

</div>

---

## 🎮 Kaizen Game Engine

**Kaizen** (改善) é uma game engine open-source baseada na Godot Engine, desenvolvida com foco em evolução contínua, desempenho e liberdade de desenvolvimento.

A engine oferece um ambiente integrado para criação de jogos 2D e 3D, reunindo ferramentas de edição, sistemas de renderização, física, áudio e programação.

O projeto busca preservar a flexibilidade e a filosofia open-source de sua base, enquanto desenvolve modificações e funcionalidades próprias.

> *Kaizen: pequenas melhorias todos os dias levam a grandes resultados.*

**Origem:** A KaizenEngine é derivada da [Godot Engine v4.8.0-beta.1](https://godotengine.org). O código original da Godot permanece sob MIT, enquanto as modificações e implementações próprias seguem AGPL-3.0, conforme os termos aplicáveis.

---

## ✨ Recursos

- 🎨 **Editor integrado:** criação e gerenciamento visual de projetos e cenas (infraestrutura herdada da Godot, com modificações próprias em desenvolvimento).
- ⚡ **Renderização:** infraestrutura gráfica baseada na arquitetura de renderização da Godot.
- 🧩 **Sistema de cenas e nós:** organização dos objetos e comportamentos do jogo (arquitetura de cenas herdada da Godot).
- 🎯 **Física:** simulação e colisão 2D e 3D (física herdada da Godot, incluindo módulos renomeados na árvore, como `kaizen_physics_2d`/`kaizen_physics_3d` e Jolt).
- 🔊 **Áudio:** reprodução e gerenciamento de áudio (herdado da Godot).
- 🛠️ **Extensibilidade:** possibilidade de trabalhar com código nativo e sistemas de scripts.
- 📦 **Exportação multiplataforma:** infraestrutura de exportação herdada da arquitetura da Godot, sujeita às plataformas efetivamente habilitadas na Kaizen.
- 🔌 **Modularidade:** possibilidade de ampliar os sistemas da engine.

---

## 📥 Obtendo a Kaizen

Atualmente, a maneira de obter o código-fonte da Kaizen é através do repositório oficial.

```bash
git clone https://github.com/CeoMatheusDev/KaizenEngine.git

cd KaizenEngine
```

> Nota: o repositório não utiliza submódulos Git.

### Binários

Quando disponibilizados, os binários da Kaizen permitirão utilizar o editor sem precisar compilar manualmente todo o código-fonte.

Os pacotes de distribuição deverão ser específicos para cada plataforma e arquitetura suportada.

**Downloads:** ainda não há endereço de distribuição binária oficial informado neste README.

---

## 🔨 Compilando a partir do código-fonte

A base de compilação da Godot utiliza **SCons**, um sistema de build baseado em Python.

A compilação permite gerar o editor completo, builds de desenvolvimento e templates destinados à execução de jogos exportados.

### Pré-requisitos

Os requisitos variam conforme a plataforma. Mínimos exigidos pelo `SConstruct`:

| Ferramenta | Versão mínima | Finalidade |
|---|---|---|
| Python | 3.10+ | Sistema de compilação (`EnsurePythonVersion(3, 10)`) |
| SCons | 4.4+ | Configuração e build (`EnsureSConsVersion(4, 4)`) |
| SCons | 4.8+ | Obrigatório para Visual Studio 2022 |
| SCons | 4.10.1+ | Obrigatório para Visual Studio 2026 |
| GCC | 11+ | Compilador C/C++ (Linux) |
| Clang | 9+ | Compilador C/C++ (Linux/macOS) |
| Apple Clang | 16+ (Xcode 16) | Compilador C/C++ (macOS) |
| Visual Studio | 2019 16.11+ | Compilador C/C++ (Windows; VS2022/VS2026 também suportados) |
| Git | qualquer recente | Controle de versão |
| pkg-config | — | Detecção de dependências no Linux |

Também são necessárias bibliotecas de desenvolvimento específicas do sistema operacional.

As instruções de compilação da [Godot Engine](https://docs.godotengine.org/en/latest/engine_details/development/compiling/index.html) são a referência da arquitetura herdada, mas os requisitos mínimos acima são os verificados no próprio `SConstruct` da Kaizen.

### Targets de compilação

O `SConstruct` define explicitamente três targets:

```bash
# Editor (gera bin/kaizen.<plataforma>.editor.<arch>)
scons platform=linuxbsd target=editor

# Template de exportação (release)
scons platform=linuxbsd target=template_release

# Template de exportação (debug)
scons platform=linuxbsd target=template_debug
```

`target=` pode ser omitido (o padrão é `editor`), mas explicitar evita dúvidas. Substitua `linuxbsd` por `windows` ou `macos` conforme a plataforma.

### Linux

Instale as ferramentas necessárias para compilação:

```bash
# Fedora
sudo dnf install gcc-c++ clang python3 python3-pip scons \
    pkgconf-pkg-config \
    libX11-devel libXcursor-devel libXinerama-devel \
    libXi-devel libXrandr-devel \
    wayland-devel wayland-protocols-devel \
    mesa-libGL-devel \
    alsa-lib-devel pulseaudio-libs-devel
```

Na raiz do código-fonte:

```bash
# Compilar o editor
scons platform=linuxbsd target=editor

# Executar o editor compilado
./bin/kaizen.linuxbsd.editor.x86_64
```

O nome exato do executável depende da plataforma e da arquitetura da build.

### Windows

Instale:

- Python 3.10 ou superior.
- SCons 4.4 ou superior (4.8+ se usar Visual Studio 2022; 4.10.1+ se usar Visual Studio 2026).
- Visual Studio 2019 16.11+ (ou mais recente) com ferramentas de compilação C++, ou MinGW-w64.

Instale o SCons:

```powershell
python -m pip install scons
```

Na raiz do código-fonte:

```powershell
# Compilar o editor
scons platform=windows target=editor
```

O executável será gerado no diretório `bin/`.

### macOS

Instale:

- Python 3.10 ou superior.
- SCons 4.4 ou superior.
- Xcode 16 ou superior (Apple Clang 16+) ou Command Line Tools correspondentes.
- Driver gráfico: a Kaizen tem suporte a **Metal** (selecionado por padrão em builds de macOS) e a Vulkan via MoltenVK, conforme o driver habilitado na configuração gráfica — não é obrigatório instalar um Vulkan SDK para usar Metal. Consulte `platform/macos/detect.py` para as opções (`metal`, etc.).

Instale o SCons:

```bash
python3 -m pip install scons
```

Compile para Intel:

```bash
scons platform=macos target=editor arch=x86_64
```

Ou para Apple Silicon:

```bash
scons platform=macos target=editor arch=arm64
```

O resultado da compilação será disponibilizado no diretório `bin/`.

### Builds de desenvolvimento

Para trabalhar no código-fonte, é possível utilizar opções de compilação voltadas ao desenvolvimento:

```bash
# Editor com recursos adicionais de depuração (define DEV_ENABLED)
scons platform=linuxbsd target=editor dev_build=yes
```

Para builds destinados à distribuição:

```bash
scons platform=linuxbsd target=editor production=yes
```

`dev_build` e `production` são opções definidas no próprio `SConstruct` (`dev_build` ativa código de depuração exclusivo de desenvolvimento; `production` ajusta os padrões para builds de produção).

**Módulo C#/Mono:** o módulo Mono existe na árvore, mas **vem desabilitado por padrão**. Para habilitá-lo, compile com `module_mono_enabled=yes`. Sem essa opção, o suporte a C# não faz parte da build.

**Lua:** não há módulo Lua na árvore do projeto. Não há suporte a Lua para anunciar.

---

## 🚀 Utilizando o editor

Após obter ou compilar uma versão executável da Kaizen, você poderá iniciar o ambiente de desenvolvimento.

### Gerenciador de projetos

O gerenciador de projetos permite criar, importar, organizar e abrir projetos existentes.

A partir dele, é possível iniciar um novo projeto ou selecionar um projeto já existente.

### Abrindo um projeto

Projetos podem ser abertos através do editor, selecionando o diretório correspondente.

Na Kaizen, os projetos são identificados pelo arquivo **`project.kaizen`** — não por `project.godot`. O `main.cpp` reconhece esse arquivo e o gerenciador de projetos também o utiliza para identificar e abrir projetos.

O editor também pode ser iniciado diretamente pelo terminal:

```bash
# Abrir o editor apontando para o projeto
./bin/kaizen.linuxbsd.editor.x86_64 --editor --path /caminho/do/projeto

# Executar o projeto diretamente
./bin/kaizen.linuxbsd.editor.x86_64 --path /caminho/do/projeto
```

O nome do executável varia com a plataforma e a arquitetura da build.

### Executando um projeto

Depois de configurar o projeto, utilize a opção de execução do editor para iniciar o jogo e testar suas alterações.

A execução também pode ser feita por linha de comando com o binário do editor, como no exemplo acima.

---

## 📦 Exportando jogos

O código da Kaizen já possui infraestrutura de exportação (com suporte a **Linux, Windows e macOS** — `platform/macos/export/export_plugin.cpp` implementa o exportador de macOS) e parâmetros de linha de comando para exportação. A disponibilidade prática de cada plataforma depende dos templates compilados e da configuração de cada export preset. O `main.cpp` confirma `--export-release`, `--export-debug` e `--headless`.

Os templates de exportação são builds específicas da engine, compiladas a partir do próprio código-fonte e correspondentes à versão usada no desenvolvimento do projeto.

### Templates de exportação

Os templates são builds específicas da engine utilizadas para executar os projetos exportados sem incluir o editor completo. Compile-os com:

```bash
scons platform=linuxbsd target=template_release
scons platform=linuxbsd target=template_debug
```

Para preparar uma exportação:

1. Compile os templates correspondentes à Kaizen (release e/ou debug).
2. Abra o projeto no editor.
3. Acesse o menu de exportação.
4. Crie um preset para a plataforma desejada.
5. Configure as opções de exportação.
6. Gere o pacote final do jogo.

### Exportação pela linha de comando

A Kaizen oferece suporte à exportação automatizada:

```bash
./bin/kaizen.linuxbsd.editor.x86_64 \
    --headless \
    --path /caminho/do/projeto \
    --export-release "Meu preset Linux" \
    build/game
```

Observações:

- O nome do preset precisa corresponder a um preset definido em `export_presets.cfg` no projeto — `"Meu preset Linux"` é apenas ilustrativo.
- Também existe `--export-debug <preset> <path>` para exports de depuração.
- Documentação da base upstream: [exportação de projetos (Godot)](https://docs.godotengine.org/en/latest/tutorials/export/exporting_projects.html).

---

## 📚 Documentação e recursos

A documentação é essencial para aprender a utilizar a engine e compreender sua arquitetura.

Os seguintes materiais são documentação da **Godot Engine (base upstream)**, não documentação específica da Kaizen — a Kaizen ainda não possui documentação própria publicada:

- [Godot Documentation](https://docs.godotengine.org/en/latest/) — documentação da arquitetura original.
- [Godot Class Reference](https://docs.godotengine.org/en/latest/classes/) — referência das classes e APIs herdadas.
- [Godot Compilation Guide](https://docs.godotengine.org/en/latest/engine_details/development/compiling/index.html) — compilação a partir do código-fonte.
- [Godot Export Documentation](https://docs.godotengine.org/en/latest/tutorials/export/exporting_projects.html) — funcionamento do sistema de exportação.

A documentação específica da Kaizen deverá registrar as modificações, APIs próprias e diferenças em relação à Godot — ainda está por ser escrita.

---

## 🧪 Demos e exemplos

> **Seção planejada.** Ainda não há projetos de demonstração oficiais publicados para a KaizenEngine. Quando existirem, serão linkados aqui.

Enquanto isso, a melhor forma de explorar a engine é compilar o editor (ver [Compilando a partir do código-fonte](#compilando-a-partir-do-código-fonte)) e abrir um projeto novo — a arquitetura de cenas e nós herdada da Godot é totalmente utilizável.

---

## 🗺️ Roadmap

| Versão | Recurso | Status |
|---|---|---|
| **4.8.0-beta** | Base da engine (editor, renderer 2D/3D) herdada da Godot, rebranding Kaizen | 🚧 Em desenvolvimento (beta) |
| **0.2** | Física 2D/3D e áudio — infraestrutura herdada da Godot; integração e melhorias próprias em desenvolvimento | 🚧 Em desenvolvimento |
| **0.3** | Editor de materiais e shaders — a base já possui infraestrutura Godot; trabalho pendente é a integração/evolução própria | 📅 Planejado |
| **0.4** | Build para Web (WASM) | 📅 Planejado |
| **1.0** | Versão estável e documentação completa | 🎯 Objetivo |

> Nota: a Kaizen ainda não possui um ECS (Entity-Component-System) próprio implementado. A arquitetura atual é a de cenas e nós herdada da Godot.

---

## 🤝 Comunidade e contribuição

A Kaizen é um projeto open-source. Contribuições são bem-vindas, sejam correções, melhorias, documentação ou novas funcionalidades.

### Como contribuir

1. Faça um fork do projeto.
2. Crie uma branch:

   ```bash
   git checkout -b feature/minha-feature
   ```

3. Implemente suas alterações.
4. Faça um commit:

   ```bash
   git commit -m "feat: adiciona nova funcionalidade"
   ```

5. Envie as alterações:

   ```bash
   git push origin feature/minha-feature
   ```

6. Abra um Pull Request.

Antes de contribuir, consulte o arquivo `CONTRIBUTING.md`, quando disponível.

### Canais

- 💡 **Issues:** reporte problemas e sugira funcionalidades.
- 💬 **Discussões:** participe das conversas sobre o projeto.
- 📚 **Wiki:** acompanhe a documentação.
- 🔧 **Pull Requests:** contribua diretamente com o código.

---

## 📄 Licença

A Kaizen utiliza um modelo de licenciamento híbrido devido à sua origem como obra derivada da Godot Engine.

| Camada | Licença | Arquivo |
|---|---|---|
| Código original da Godot Engine | MIT | `LICENSE-MIT` |
| Modificações e código novo da KaizenEngine | AGPL-3.0 | `LICENSE.txt` |
| Exceção para jogos e termos adicionais | Permissões adicionais (Seção 7 da AGPL) | `LICENSE-ADDITIONAL-TERMS.md` |

### Resumo prático

- **Engine:** o código novo e as modificações estão sujeitos à AGPL-3.0.
- **Jogos:** a exceção adicional permite a distribuição comercial de jogos sob outras licenças, respeitando suas condições.
- **Código da Godot:** permanece sob a licença MIT original.

Para conhecer todas as condições, consulte:

- `LICENSE.txt`
- `LICENSE-MIT`
- `LICENSE-ADDITIONAL-TERMS.md`

---

<div align="center">

**Kaizen — evoluindo um commit de cada vez. 🌱**

Feito com ❤️ por desenvolvedores apaixonados por games.

</div>
