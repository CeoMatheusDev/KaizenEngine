# Kaizen Engine — Build e Distribuição

Documento técnico da infraestrutura de build e packaging da Kaizen Engine
(fork da Godot Engine 4.8, renomeado e com modificações próprias).

- Versão da engine: `4.8.beta.mono.custom_build` (`version.py` = `4.8.0`,
  status `beta`)
- Plataforma validada: **Linux x86_64** (Fedora 44, GCC 16.2.1)
- Variante .NET: **validada**, com SDK/runtime 10 privados embarcados
- Windows: infraestrutura **preparada, não validada** neste ambiente e ainda
  no formato antigo (ver §12)

---

## 1. Princípios

1. **Build ≠ packaging.** O SCons continua sendo o único responsável por
   compilar. Os scripts dirigem o SCons e organizam artefatos; nenhuma opção
   interna do build system foi alterada.
2. **Nada é apagado implicitamente.** `build.sh` nunca executa `scons -c` nem
   remove `bin/`. `package.sh` só substitui a saída com `--force` explícito,
   via staging + troca atômica.
3. **Versão/canal são metadados explícitos.** Nunca são deduzidos de nome de
   diretório. A versão do pacote vem de `version.py` ou de `--version`; o
   canal, de `status` ou de `--channel`.
4. **Distribuição reproduzível.** O `.tar.xz` é determinístico; com o mesmo
   toolchain e `SOURCE_DATE_EPOCH`, duas execuções geram bytes idênticos.
5. **Sem intermediários na distribuição.** `.o`, `.a`, `.lib`, `obj/`, caches,
   `__pycache__`, `.sconsign*`, `.scons_env.json`, `doc/translations` e
   `tests/` nunca são copiados.
6. **Nada de vazio.** Nenhum diretório é criado sem conteúdo real.

---

## 2. Arquitetura de diretórios

### 2.1 Repositório (fonte)

```text
KaizenEngine/
├── SConstruct, methods.py, platform_methods.py   # build system (SCons)
├── core/ editor/ main/ scene/ servers/ platform/ modules/ drivers/
├── modules/mono/                                 # integração .NET
│   ├── build_scripts/build_assemblies.py
│   ├── glue/KaizenSharp/                         # fontes da API C# (glue)
│   ├── editor/Kaizen.NET.Sdk/                    # SDK MSBuild distribuído
│   └── editor/KaizenTools/                       # editor tooling C#
├── misc/dist/linux/                              # .desktop, AppData, MIME, man
├── misc/logo/                                    # ícones
├── packaging/                                    # esta infraestrutura
│
│ ── saídas de build (geradas, não fazem parte do fonte) ──
├── bin/                                          # artefatos de build
└── dist/                                         # .reference/, Kaizen/, .tar.xz
```

> O repositório está em estado **somente fonte**: `bin/` e `dist/` são
> saídas de `build.sh`/`package.sh` e foram removidos em 2026-11-17;
> reaparecem apenas após uma reconstrução completa.

### 2.2 Distribuição

| Caminho | Conteúdo | Consumidor |
|---|---|---|
| `Engine/Binaries/Linux/` | executável canônico, wrapper `kaizen`, marcador `_sc_`, symlink `KaizenSharp` | usuário/runtime |
| `ExportTemplates/Linux/{Release,Debug}/` | `linux_release.x86_64`, `linux_debug.x86_64` | plugin de exportação |
| `Templates/<FULL_CONFIG>/` | visão pronta para instalar: **somente symlinks relativos** | instalador |
| `Engine/Managed/KaizenSharp/` | `Api/{Debug,Release}`, `Tools` (inclui `nupkgs/`) | editor .NET |
| `Engine/Managed/dotnet/` | SDK + runtime privados (mono) | wrapper/editor |
| `Engine/Source/` | fonte (inclui `packaging/`) — **apenas** com `--with-source` | desenvolvimento |
| `Build/Config/` | `build-info.json`, `package-info.env`, `dotnet-info.env` | validação/instalação |
| `Build/Toolchains/` | notas de toolchain | mantenedores |
| `Config/linux/` | `.desktop`, AppStream, MIME, man page, ícones | integração desktop |
| `LICENSES/` | Kaizen + licenças do .NET embarcado | distribuição |
| `Samples/` | `hello-kaizen`, `hello-kaizen-csharp` | usuário/CI |
| `Setup/` | `install.sh`, `uninstall.sh`, `select.sh`, `check-deps.sh` | instalação |
| `README.txt` | resumo da versão/canal e instruções | usuário |
| `MANIFEST.sha256` | hash de todos os arquivos regulares | integridade |

O modo portátil usa `Engine/Binaries/Linux/editor_data/export_templates/<FULL_CONFIG>/`
com symlinks para `ExportTemplates/`. A distribuição **instalada** descarta
`_sc_`, `editor_data/`, `ExportTemplates/` e `Templates/`, pois os templates vão
para o diretório XDG e o editor deixa de ser autocontido.

### 2.3 Instalação (por usuário)

```text
~/.local/opt/kaizen/
├── <canal>/<versão>/            árvore instalada + .kaizen-receipt.env
├── <canal>/current -> <versão>
├── .dotnet/<sdk>/               .NET privado compartilhado
├── default-channel              canal padrão
├── select.sh
└── check-deps.sh

~/.local/bin/kaizen              wrapper do canal padrão
~/.local/bin/kaizen-<canal>      launcher por canal
~/.local/bin/kaizen-select       seleção
~/.local/share/kaizen/export_templates/<FULL_CONFIG>/
~/.local/share/{applications,metainfo,mime,icons,man}/
~/.local/share/kaizen/{app_userdata,...}   # dados do usuário (nunca removidos)
```

---

## 3. Versionamento e canais

### 3.1 Modelo

`version.py` fornece `major.minor.patch` e `status`:

| Campo | Exemplo |
|---|---|
| `major.minor.patch` | `4.8.0` |
| `status` | `beta` |
| `--channel-revision N` | `1` |
| Versão do pacote | `4.8.0-beta1` |
| Nome | `kaizen-x86_64-linux-4.8.0-beta1` |

Canais aceitos: `stable`, `beta`, `rc`, `dev`.

- `stable`: sem sufixo (`4.8.0`).
- `beta`/`rc`/`dev`: `X.Y.Z-<canal><N>` (N ≥ 1, padrão 1).
- `--version`/`--channel` sobrescrevem os metadados de `version.py`.
- O empacotador cruza o canal/versão com o que o binário reporta. Se
  divergirem, falha; `--allow-channel-mismatch` existe só para fixtures de
  teste.

A versão da engine (interna) não muda com o canal do pacote:
`4.8.beta.mono.custom_build`, templates em `4.8.beta.mono`.

### 3.2 Coexistência

Versões de canais diferentes convivem em `<opt-root>/<canal>/<versão>/`.
Exemplo:

```text
~/.local/opt/kaizen/stable/4.8.0/
~/.local/opt/kaizen/beta/4.8.0-beta1/
~/.local/opt/kaizen/rc/4.8.0-rc1/
~/.local/opt/kaizen/dev/4.8.0-dev1/
```

`kaizen` aponta para o canal padrão; `kaizen-beta` etc. são launchers fixos
por canal. `kaizen-select` altera o padrão e/ou o `current` de um canal.

---

## 4. Sistema de build (SCons)

### 4.1 Opções relevantes (verificadas no `SConstruct`)

| Opção | Padrão | Uso |
|---|---|---|
| `platform=` | detectado | `linuxbsd`, `windows` |
| `target=` | `editor` | `editor`, `template_debug`, `template_release` |
| `arch=` | detectado | `x86_64` |
| `module_mono_enabled=` | `no` | habilita .NET |
| `production=` | `no` | build de produção (LTO/strip) |
| `dev_build=` | `no` | build de desenvolvimento |
| `use_llvm=` | `no` | Clang em vez de GCC |
| `use_mingw=` | `no` | MinGW-w64 no Windows |
| `accesskit=` | `yes` | leitor de tela (requer deps extras) |
| `redirect_build_objects=` | `yes` | objetos em `bin/obj/` |

Nome final do executável:

```text
<PROGPREFIX>kaizen.<platform>.<target>.<arch><module_version><PROGSUFFIX>
```

Exemplos:

```text
kaizen.linuxbsd.editor.x86_64
kaizen.linuxbsd.template_release.x86_64.mono
kaizen.windows.editor.x86_64.mono.exe
```

### 4.2 Comandos reproduzíveis

```bash
scons platform=linuxbsd target=editor -j24
scons platform=linuxbsd target=editor module_mono_enabled=yes -j24
scons platform=linuxbsd target=template_release -j24
scons platform=linuxbsd target=template_release module_mono_enabled=yes -j24
scons platform=linuxbsd target=template_debug   module_mono_enabled=yes -j24
```

### 4.3 Versão e diretório de templates

`core/version.h` define:

- `KAIZEN_VERSION_FULL_CONFIG` = `4.8.beta` ou `4.8.beta.mono`
- `KAIZEN_VERSION_FULL_BUILD` = `4.8.beta.mono.custom_build`

O editor procura templates em
`<data>/export_templates/<KAIZEN_VERSION_FULL_CONFIG>/`. O `package.sh` deriva
esse nome removendo `.custom_build`/`.official` do `kaizen --version`.

---

## 5. Driver de build — `packaging/linux/build.sh`

```bash
./packaging/linux/build.sh [--target editor|template_debug|template_release|all]
                           [--mono|--no-mono] [--dev|--production]
                           [--jobs N] [--arch x86_64] [--dry-run] [-v]
```

- `--target` aceita lista separada por vírgula ou repetição; padrão `all`
  (editor → template_release → template_debug).
- Valida que o binário esperado apareceu em `bin/`.
- `--mono` adiciona `module_mono_enabled=yes` e, após o editor:
  1. gera o glue C# com `--generate-mono-glue`;
  2. compila os assemblies com `build_assemblies.py`;
  3. verifica `bin/KaizenSharp/Api/Release/KaizenSharp.dll` e
     `bin/KaizenSharp/Tools/KaizenTools.dll`.
- `--dry-run` imprime os comandos sem executar.

---

## 6. Integração .NET

### 6.1 Componentes gerados

| Componente | Origem | Destino |
|---|---|---|
| Glue C# | `--generate-mono-glue` | `modules/mono/glue/KaizenSharp/**/Generated` |
| API gerenciada | `build_assemblies.py` | `bin/KaizenSharp/Api/{Debug,Release}` |
| Ferramentas do editor | `KaizenTools.sln` | `bin/KaizenSharp/Tools` |
| SDK MSBuild | `Kaizen.NET.Sdk.sln` | `bin/KaizenSharp/Tools/nupkgs` |
| Source generators | `Kaizen.SourceGenerators*.sln` | dentro de `Tools` |

### 6.2 Localização em runtime

`modules/mono/kaizensharp_dirs.cpp` resolve `KaizenSharp` **ao lado do
executável** (`exe_dir/KaizenSharp/{Tools,Api/<Config>}`). A distribuição
mantém:

```text
Engine/Managed/KaizenSharp/         # conteúdo real
Engine/Binaries/Linux/KaizenSharp -> ../../Managed/KaizenSharp
```

Não mova o conteúdo sem manter esse link: o editor não procura em
`share/kaizen` nem em caminhos XDG.

### 6.3 SDK + runtime privados

A variante `--mono` embarca um subconjunto do .NET 10 em
`Engine/Managed/dotnet/`:

- `dotnet`, `host`, `metadata`
- `sdk/<versão>` (SDK completo, necessário para compilar C#)
- `shared/Microsoft.NETCore.App/<versão>`
- `packs/Microsoft.NETCore.App.Ref`
- `LICENSE.txt`, `ThirdPartyNotices.txt`

Não são embarcados: AspNetCore, packs de RID (Fedora), templates de projeto do
SDK, glibc, drivers gráficos/Vulkan ou bibliotecas fundamentais do sistema.

O wrapper `Engine/Binaries/Linux/kaizen` resolve o próprio caminho, define
`DOTNET_ROOT=<tree>/Engine/Managed/dotnet`, prepende esse diretório ao `PATH`
e executa o binário canônico. `KAIZEN_DOTNET_ROOT` pode sobrescrever.

Na instalação, o conteúdo é movido para `<opt-root>/.dotnet/<sdk>/` e cada
versão recebe o symlink:

```text
<versão>/Engine/Managed/dotnet -> ../../../../.dotnet/<sdk>
```

Isso evita duplicar ~300 MB por versão. A remoção só apaga o `.dotnet/<sdk>`
quando nenhum recibo restante usa o mesmo SDK.

Verificação:

```bash
<opt-root>/.dotnet/10.0.113/dotnet --info
<opt-root>/.dotnet/10.0.113/dotnet --list-sdks
```

### 6.4 NuGet e projetos C#

`build_assemblies.py` gera `KaizenSharp`, `KaizenSharpEditor`,
`Kaizen.SourceGenerators` e `Kaizen.NET.Sdk` na versão dos assemblies
(`4.8.0-beta`). `Kaizen.NET.Sdk` não existe no NuGet.org; o sample usa uma
fonte local relativa dentro da distribuição:

```xml
<add key="kaizen-local" value="../../Engine/Managed/KaizenSharp/Tools/nupkgs" />
```

Projetos criados pelo editor recebem `Sdk="Kaizen.NET.Sdk/<versão>"` e
`TargetFramework=net10.0`, com saída em `.kaizen/mono/temp/`.

O SDK privado compila e exporta C# sem SDK do sistema. O restore ainda pode
precisar de rede (NuGet.org) para dependências de terceiros; o feed local
cobre os pacotes Kaizen.

---

## 7. Packaging — `packaging/linux/package.sh`

```bash
./packaging/linux/package.sh [--mono|--no-mono]
    [--version X.Y.Z] [--channel stable|beta|rc|dev] [--channel-revision N]
    [--dotnet-bundle auto|yes|no] [--dotnet-root DIR] [--dotnet-version VER]
    [-o|--output DIR] [--tarball-out DIR] [--tarball-name NAME]
    [--no-dir] [--no-tarball] [--with-source] [--reproducible]
    [--xz-preset 0-9] [-j|--jobs N] [--force] [--dry-run]
```

Sem `--mono/--no-mono`, detecta a variante pela presença de
`bin/kaizen.linuxbsd.editor.x86_64.mono`.

Etapas:

1. Resolve binários e lê a versão real do executável (`--version`).
2. Resolve versão/canal explícitos e valida contra o binário.
3. Cria staging; nada é escrito no destino até o fim.
4. Copia o executável canônico (`0755`), gera o wrapper `kaizen` e o marcador
   `_sc_`.
5. Instala templates em `ExportTemplates/Linux/{Release,Debug}` e cria
   symlinks em `editor_data/export_templates/<FULL_CONFIG>/` e em
   `Templates/<FULL_CONFIG>/`.
6. Copia `bin/KaizenSharp` para `Engine/Managed/KaizenSharp` e cria o symlink
   ao lado do executável (mono).
7. Opcionalmente embarca o .NET privado e o valida com `dotnet --info`.
8. Copia `.desktop`, AppStream, MIME, man page, ícones e licenças.
9. Copia os samples aplicáveis, excluindo `.kaizen/`, `out/`, `export/`,
   `bin/`, `obj/`.
10. Copia `install.sh`, `uninstall.sh`, `select.sh`, `check-deps.sh` e
    `Setup/README.md`.
11. Gera `README.txt`, `Build/Config/package-info.env`, `dotnet-info.env` e
    `Build/Config/build-info.json` (schema 2, com SHA-256 de cada arquivo).
12. Gera `MANIFEST.sha256`.
13. Cria o `.tar.xz` determinístico e o `.sha256` externo.
14. Troca o staging pelo destino (backup temporário + remoção).

### 7.1 Metadados

`package-info.env`:

```ini
PRODUCT=Kaizen
PACKAGE_NAME=kaizen-x86_64-linux-4.8.0-beta1
PACKAGE_VERSION=4.8.0-beta1
PACKAGE_VERSION_BASE=4.8.0
CHANNEL=beta
CHANNEL_REVISION=1
ENGINE_VERSION=4.8.beta.mono.custom_build
TEMPLATES_CONFIG=4.8.beta.mono
MONO=yes
DOTNET_BUNDLED=yes
DOTNET_SDK=10.0.113
DOTNET_RUNTIME=10.0.13
...
```

`build-info.json` (schema 2) contém `package`, `engine`, `build`, `files` e
`symlinks`. O validador recomputa todos os hashes. O próprio
`build-info.json` **não** entra em `files` (ele ainda não existe quando a
lista é calculada); `MANIFEST.sha256` é gerado depois e cobre todos os
arquivos regulares, exceto ele mesmo. Nesta build: 3907 arquivos + 5
symlinks no JSON, 3908 entradas no `MANIFEST.sha256`.

### 7.2 Arquivo oficial

```text
kaizen-x86_64-linux-<versão>.tar.xz
kaizen-x86_64-linux-<versão>.tar.xz.sha256
```

Raiz `Kaizen/`, ordenação por nome, mtime fixo, dono 0/0, sem ACL/SELinux/
xattrs, hardlinks expandidos (`--hard-dereference`), `xz -T<jobs>`.

### 7.3 Reprodutibilidade

- `--reproducible` fixa `mtime` e `generated_at` em `946684800`
  (2000-01-01T00:00:00Z).
- `SOURCE_DATE_EPOCH` tem precedência.
- O campo `host` do manifesto e o caminho de origem do .NET são omitidos
  quando reproduzível.
- Testado: duas execuções produziram o mesmo SHA-256.

### 7.4 Modo portátil

`_sc_` ao lado do executável faz `EditorPaths` usar `exe_dir/editor_data/`.
A árvore é movível e não depende do repositório. O wrapper é POSIX sh e
funciona em qualquer shell.

---

## 8. Instalação

### 8.1 Modos e opções

```bash
./Setup/install.sh [--user|--system] [--prefix DIR]
    [--opt-root DIR] [--bin-dir DIR] [--share-dir DIR] [--data-home DIR]
    [--from-tarball FILE] [--channel NAME] [--version VER]
    [--select[=CANAL]] [--no-select] [--templates-only]
    [--no-share-dotnet] [--force] [--dry-run] [-y|--yes]
```

- `--user` (padrão): `~/.local/opt/kaizen`, `~/.local/bin`,
  `$XDG_DATA_HOME` (ou `~/.local/share`).
- `--system`: `/opt/kaizen`, `/usr/local/bin`, `/usr/share`; os templates vão
  para o XDG do usuário que executa (limitação da engine, ver §8.6).
- `--from-tarball` verifica o `.sha256` externo, inspeciona as entradas,
  extrai em diretório privado e valida o manifesto.
- `--templates-only` instala apenas os templates (útil para outros usuários).
- `--force` substitui uma versão já instalada.

### 8.2 Verificações antes de escrever

1. checksum externo do arquivo;
2. manifesto SHA-256 e cobertura completa;
3. caminhos relativos, sem `..`/absolutos, tipos de entrada seguros;
4. symlinks relativos e contidos na árvore;
5. produto, plataforma `linux`, arquitetura `x86_64`, canal e versão;
6. espaço livre (com estimativa já descontando .NET compartilhado);
7. permissão de escrita nos destinos.

### 8.3 Transação e rollback

- A árvore é copiada para `<canal>/.<versão>.staging.$$`.
- O `.NET` privado é movido para `.dotnet/<sdk>` (ou reutilizado) antes da
  troca.
- A versão existente (com `--force`) vira backup; em erro, o backup é
  restaurado e o staging/launchers são limpos.
- Launchers e seleção só são atualizados depois que a engine em staging
  responde `--version` corretamente.
- O recibo `.kaizen-receipt.env` registra canal, versão, paths e SDK.

### 8.4 Integração XDG

- `.desktop` com `Exec=<bin-dir>/kaizen %f` (caminho absoluto).
- AppStream em `metainfo/`, MIME em `mime/packages/`, man em `man/man6/`,
  ícones em `icons/hicolor/{scalable,256x256}/apps/`.
- `update-desktop-database`, `update-mime-database` e
  `gtk-update-icon-cache` são executados best-effort.
- Nenhum arquivo de shell (`~/.bashrc`, `~/.profile`, `~/.zshrc`) é editado.

### 8.5 Seleção

`kaizen` lê `<opt-root>/default-channel` e cai em ordem determinística
(`stable`, `rc`, `beta`, `dev`) se o canal não existir. `kaizen-select`:

```bash
kaizen-select --list
kaizen-select --show
kaizen-select beta
kaizen-select beta/4.8.0-beta1
```

### 8.6 Templates e instalação global

A engine só procura templates em
`$XDG_DATA_HOME/kaizen/export_templates/<FULL_CONFIG>`. Não há caminho
system-wide. Consequências:

- instalação global instala templates no XDG do usuário que rodou o
  instalador;
- outros usuários precisam extrair o arquivo e rodar
  `./Setup/install.sh --templates-only` com o próprio `HOME`;
- `--templates-only` **não** funciona a partir de uma árvore já instalada
  (ela não contém `Templates/`), apenas do arquivo/árvore portátil.

---

## 9. Desinstalação — `Setup/uninstall.sh`

```bash
./Setup/uninstall.sh --list
./Setup/uninstall.sh --channel beta --version 4.8.0-beta1
./Setup/uninstall.sh --all [--channel beta] --yes
/opt/kaizen/beta/4.8.0-beta1/Setup/uninstall.sh   # infere tudo do recibo
```

- Remove somente `<opt-root>/<canal>/<versão>` e valida o recibo antes.
- Atualiza `<canal>/current` e `default-channel` (fallback determinístico).
- Remove `.dotnet/<sdk>` apenas se nenhum recibo restante usar o SDK.
- Remove templates apenas se nenhum recibo restante compartilhar o mesmo
  `TEMPLATES_CONFIG`.
- Remove launchers/ícones/`.desktop`/MIME/man apenas quando o último canal
  some.
- Nunca remove projetos, `app_userdata`, configurações ou caches; o
  `rmdir` só atua em diretórios vazios.

---

## 10. Validação — `packaging/linux/validate.sh`

```bash
./packaging/linux/validate.sh --mono [--global] [--gui]
./packaging/linux/validate.sh --mono --dist DIR --tarball FILE
./packaging/linux/validate.sh --mono --no-install --no-tarball --skip-run
```

Blocos de checagem (cada um reporta PASS/WARN/FAIL):

1. **Layout**: árvore nova, ícones, MIME, man, licenças, samples, Setup
   executável, ausência de caminhos legados e de diretórios vazios.
2. **Metadados**: chaves obrigatórias, formato de versão, canal, nome,
   `TEMPLATES_CONFIG` × engine, `SOURCE_INCLUDED`.
3. **Binário**: ELF x86-64, permissão, wrapper `kaizen` (não symlink),
   `DOTNET_ROOT`, alvo do `exec`, `_sc_`, `--version`.
4. **Templates**: Release/Debug, caminho portátil e visão `Templates/` com
   symlinks resolvendo.
5. **.NET**: `KaizenSharp` ao lado do executável, DLLs, `.deps.json`, nupkg,
   `KaizenPlugins`; se embarcado, host, licença, SDK × metadados e execução
   isolada do `dotnet`.
6. **Manifesto JSON**: schema 2, arquivos/tamanhos/hashes, symlinks.
7. **MANIFEST.sha256**: verificação completa e cobertura de todos os arquivos.
8. **Symlinks**: relativos e contidos.
9. **Higiene**: sem `.o/.a/.lib/.sconsign/__pycache__/*.pyc`.
10. **Dependências**: `ldd` sem `not found`.
11. **Runtime**: cópia descartável, `HOME`/XDG isolados, `--version` e
    `--headless --quit`.
12. **GUI opcional**: Project Manager com `--quit-after`.
13. **Tarball**: nome, checksum, entradas dentro de `Kaizen/`, tipos seguros,
    extração, manifesto e execução da engine extraída.
14. **Instalação**: usuário em HOME/XDG isolados, launcher, `kaizen-select`,
    XDG, recibo, ausência de `_sc_`/templates duplicados, `.bashrc` intacto.
15. **Coexistência**: segundo canal (fixture), launchers, `--no-select`,
    troca de padrão.
16. **Desinstalação**: remoção seletiva, preservação da outra versão,
    templates compartilhados, reparo do padrão, remoção final e preservação
    de dados do usuário.
17. **Global simulada**: `--system` com `--opt-root/--bin-dir/--share-dir/
    --data-home` em diretório gravável, sem root; launcher e remoção.

Testes de runtime/instalação usam cópias descartáveis; a árvore empacotada
não é executada no lugar nem sujada.

---

## 11. Dependências (Linux)

Compilação:

- GCC ≥ 11 ou Clang ≥ 9
- Python 3.9+, SCons ≥ 4
- Headers: X11, Xcursor, Xinerama, XRandR, Xi, XInput2, Wayland, libudev,
  ALSA, PulseAudio, fontconfig, freetype, dbus
- Vulkan loader (runtime) e drivers de GPU
- .NET SDK ≥ 10.0.100 (somente para rebuild da variante .NET; o pacote mono
  já traz SDK/runtime privados)

Runtime do pacote:

```bash
ldd dist/Kaizen/Engine/Binaries/Linux/kaizen.linuxbsd.editor.x86_64.mono
# linux-vdso, libm, libgcc_s, libc, ld-linux (+ libdl/libpthread) e X11/GL
```

Drivers opcionais são carregados por `dlopen` (Vulkan, ALSA, PulseAudio,
DBus, fontconfig): ausência degrada funcionalidades, não impede iniciar.

`Setup/check-deps.sh` relata .NET (privado ou sistema), bibliotecas
dinâmicas e sessão gráfica; `--strict` falha se faltar dependência
obrigatória.

---

## 12. Windows

Infraestrutura em `packaging/windows/` existe, mas **não foi validada** neste
host (Fedora, sem MSVC/Mingw-w64) e ainda produz o layout antigo
(`Engine/ExportTemplates`, `Engine/Build`, sem canais/.tar.xz). Nenhum
formato Windows (MSI, ZIP, instalador) foi implementado nesta etapa.

---

## 13. Limitações conhecidas

1. **Windows não validado** e fora do escopo desta etapa.
2. Apenas **x86_64** em Linux; ARM64/ARM32 não testados.
3. Instalação global não consegue servir templates para todos os usuários
   (limitação da engine, §8.6).
4. `--templates-only` só funciona a partir do arquivo/árvore portátil.
5. NuGet de terceiros pode exigir rede; o feed local cobre só pacotes Kaizen.
6. `--with-source` não é uma distribuição de desenvolvimento completa (não
   inclui `bin/obj`); recompilar exige as dependências de build.
7. Assinatura, pacotes distro (`.deb`/`.rpm`/Flatpak) e atualização
   automática estão fora do escopo.
8. `doc/translations/` e `tests/` não são distribuídos.
9. O `.tar.xz` não é assinado criptograficamente; a integridade é garantida
   pelo `.sha256` publicado ao lado dele.

---

## 14. Resultados de validação (Linux)

> **Nota:** as medições abaixo são o registro histórico da última execução
> (artefato `kaizen-x86_64-linux-4.8.0-beta1.tar.xz`, SHA-256
> `bebd507e28e7601b0321a90cea42b2397180caf79176ff5ccc0cd92ab9a67b00`).
> Em 2026-11-17, a pedido explícito, `dist/` foi removido — inclusive o
> `.tar.xz`, a árvore `dist/Kaizen/` e `dist/.reference/`. O repositório
> hoje contém apenas o fonte; para obter uma entrega novamente é preciso
> reconstruir do zero (`build.sh` → `package.sh` → `validate.sh`), e os
> hashes/contagens podem divergir.

Medições daquela execução:

| Etapa | Resultado |
|---|---|
| Build editor mono | OK — `4.8.beta.mono.custom_build` |
| Glue C# / assemblies / SDK NuGet | OK |
| Templates mono Debug/Release | OK |
| Pacote mono com .NET privado | OK — SDK 10.0.113, runtime 10.0.13 |
| Distribuição extraída | 745 MB, 3909 files, 5 symlinks |
| `.tar.xz` | 232 MB + `.sha256` |
| SHA-256 do arquivo | `bebd507e28e7601b0321a90cea42b2397180caf79176ff5ccc0cd92ab9a67b00` |
| Reprodutibilidade | OK — duas execuções, mesmo SHA-256 |
| `validate.sh --mono --global` | **132 PASS, 0 FAIL, 0 WARN** |
| Exportação GDScript | OK — executável exportado roda e imprime |
| `--build-solutions` C# | OK com SDK privado |
| Exportação C# | OK — executável exportado roda e imprime |
| Instalação por usuário + `kaizen-select` | OK |
| Coexistência beta/rc (fixture) | OK |
| Desinstalação seletiva | OK — preserva versão e dados |
| Instalação global simulada | OK (sem root) |
| Windows | **não executado** |

---

## 15. Solução de problemas

| Sintoma | Causa / solução |
|---|---|
| “Nenhum modelo de exportação encontrado” | versão do template ≠ `KAIZEN_VERSION_FULL_CONFIG`; confira `kaizen --version` e o diretório de templates |
| `Unable to find MSBuild` | o wrapper não encontrou o .NET privado; confira o symlink `Engine/Managed/dotnet` e `<opt>/.dotnet/<sdk>` |
| Restore falha com `Kaizen.NET.Sdk` | fonte local ausente; use o `NuGet.config` do sample ou aponte para `Tools/nupkgs` |
| `KaizenSharp` não encontrado | mantenha o symlink em `Engine/Binaries/Linux`; o runtime não procura em XDG |
| Editor não acha templates no modo portátil | verifique `_sc_` e os symlinks em `editor_data/export_templates/<versão>/` |
| `sha256sum -c` falha | arquivo corrompido ou editado; reempacote ou rebaixe |
| Instalação diz que o canal não existe | o pacote tem metadados de outro canal; use o arquivo correto ou `--channel` compatível |
| `kaizen` abre a versão errada | `kaizen-select --list` e `kaizen-select <canal>` |
| Sem permissão em `/opt` | use o modo por usuário (padrão) ou `sudo --system` |
