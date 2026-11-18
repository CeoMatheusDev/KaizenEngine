# Kaizen Engine — Build & Packaging

Infraestrutura de build e empacotamento da Kaizen Engine. Os scripts são
**externos ao build system**: eles apenas dirigem o SCons existente e organizam
os artefatos. Nenhuma opção do SCons foi alterada.

Documentação técnica completa: [`docs/BUILD_AND_PACKAGING.md`](../docs/BUILD_AND_PACKAGING.md).

## Estrutura

```text
packaging/
├── common.sh              # helpers compartilhados (Linux)
├── common.ps1             # helpers compartilhados (Windows)
├── linux/
│   ├── build.sh           # driver de build (editor / templates / .NET)
│   ├── package.sh         # árvore + .tar.xz determinístico
│   ├── install.sh         # instalador (usuário/global) — vai para Setup/
│   ├── uninstall.sh       # desinstalador por canal/versão — vai para Setup/
│   ├── select.sh          # seleção do canal/versão padrão — vai para Setup/
│   ├── check-deps.sh      # relatório de dependências — vai para Setup/
│   └── validate.sh        # validação completa (tarball, instalação, coexistência)
├── windows/               # equivalente Windows (NÃO validado neste host)
└── samples/
    ├── hello-kaizen/          # projeto GDScript
    └── hello-kaizen-csharp/   # projeto C# (.NET)
```

## Fluxo Linux

```bash
# 1. Build (editor + templates + assemblies .NET)
./packaging/linux/build.sh --mono --target all --jobs 24

# 2. Distribuição + arquivo oficial
./packaging/linux/package.sh --mono
#   dist/Kaizen/
#   dist/kaizen-x86_64-linux-4.8.0-beta1.tar.xz
#   dist/kaizen-x86_64-linux-4.8.0-beta1.tar.xz.sha256

# 3. Validação completa (tarball + instalação + coexistência + global simulada)
./packaging/linux/validate.sh --mono --global

# 4. Instalação
dist/Kaizen/Setup/install.sh                 # por usuário (padrão)
sudo dist/Kaizen/Setup/install.sh --system   # global
```

## Versão e canal

A versão do **pacote** é metadado explícito, nunca derivada de nome de
diretório:

| Origem | Exemplo |
|---|---|
| `version.py` (`major.minor.patch`) | `4.8.0` |
| `version.py` (`status`) | `beta` |
| `--channel-revision N` | `1` |
| Versão final | `4.8.0-beta1` |
| Nome do arquivo | `kaizen-x86_64-linux-4.8.0-beta1.tar.xz` |

- Canais: `stable`, `beta`, `rc`, `dev`.
- Stable não tem sufixo (`4.8.0`); os demais têm `-<canal><N>`.
- `--version X.Y.Z` e `--channel NAME` podem sobrescrever os metadados.
- `package.sh` recusa canal/versão que contradigam o binário compilado, a
  menos que `--allow-channel-mismatch` seja usado (apenas para testes).
- A versão da engine continua `4.8.beta.mono.custom_build`; o diretório de
  templates continua `4.8.beta.mono`.

## Layout da distribuição

```text
Kaizen/
├── Engine/
│   ├── Binaries/Linux/          kaizen.linuxbsd.editor.x86_64.mono, wrapper kaizen, _sc_
│   ├── Managed/KaizenSharp/     Api/{Debug,Release}, Tools/ (mono)
│   ├── Managed/dotnet/          SDK + runtime privados (quando embarcados)
│   └── Source/                  somente com --with-source
├── ExportTemplates/Linux/       Release/ e Debug/
├── Templates/<versão>/          visão pronta para instalar (somente symlinks)
├── Build/Config/                build-info.json, package-info.env, dotnet-info.env
├── Build/Toolchains/            notas de toolchain
├── Config/linux/                .desktop, AppStream, MIME, man page, ícones
├── LICENSES/                    Kaizen + licenças do .NET embarcado
├── Samples/                     hello-kaizen, hello-kaizen-csharp
├── Setup/                       install.sh, uninstall.sh, select.sh, check-deps.sh
├── README.txt
└── MANIFEST.sha256
```

- `Templates/<versão>/` contém **apenas symlinks relativos** para
  `ExportTemplates/`; não há duplicação de binários.
- `Engine/Binaries/Linux/editor_data/export_templates/<versão>/` também usa
  symlinks, mantendo o modo portátil (`_sc_`).
- Nenhum diretório vazio é criado.

## Metadados e integridade

- `Build/Config/package-info.env`: produto, versão do pacote, canal, revisão,
  versão da engine, config de templates, mono, .NET, arquitetura.
- `Build/Config/build-info.json` (schema 2): metadados + SHA-256 e tamanho de
  cada arquivo + alvos de symlink.
- `Build/Config/dotnet-info.env`: SDK/runtime embarcados (quando aplicável).
- `MANIFEST.sha256`: hash de todos os arquivos regulares (symlinks são
  validados estruturalmente).
- `*.tar.xz.sha256`: checksum externo do arquivo oficial.

O instalador verifica o checksum externo, o manifesto, a arquitetura, o
canal/versão, os symlinks (relativos e contidos) e o espaço livre antes de
escrever qualquer coisa.

## Reprodutibilidade

```bash
./packaging/linux/package.sh --mono --reproducible
SOURCE_DATE_EPOCH=1700000000 ./packaging/linux/package.sh --mono --reproducible
```

Com o mesmo toolchain e `SOURCE_DATE_EPOCH`, duas execuções produzem o mesmo
`.tar.xz` byte a byte. O arquivo usa `tar --sort=name --mtime --owner=0
--group=0 --numeric-owner --hard-dereference` e `xz -T`.

## .NET

A variante `--mono` embarca um SDK e um runtime privados em
`Engine/Managed/dotnet/` (subconjunto do .NET 10: host, SDK, runtime
`Microsoft.NETCore.App`, reference pack e licenças). O wrapper `kaizen` injeta
`DOTNET_ROOT`/`PATH` antes de executar a engine, sem alterar o .NET do sistema.

Na instalação, esse conteúdo é compartilhado em `<opt>/.dotnet/<sdk>/` e cada
versão aponta para ele por symlink; assim várias versões não duplicam ~300 MB.

O SDK privado compila e exporta projetos C#. O restore de NuGet ainda pode
precisar de rede ou de uma fonte local (`Tools/nupkgs`).

## Instalação

```bash
./Setup/install.sh                       # ~/.local (padrão)
./Setup/install.sh --from-tarball FILE   # verifica checksum e extrai
./Setup/install.sh --select              # torna o canal instalado o padrão
sudo ./Setup/install.sh --system         # /opt/kaizen + /usr/local + /usr/share
```

Layout por usuário:

```text
~/.local/opt/kaizen/<canal>/<versão>/    árvore instalada + recibo
~/.local/opt/kaizen/<canal>/current -> <versão>
~/.local/opt/kaizen/.dotnet/<sdk>/       .NET privado compartilhado
~/.local/opt/kaizen/default-channel      canal padrão
~/.local/bin/kaizen                      launcher do canal padrão
~/.local/bin/kaizen-<canal>              launcher por canal
~/.local/bin/kaizen-select               seleção
~/.local/share/kaizen/export_templates/  templates
```

- `kaizen` é um wrapper gerado (não um symlink), para localizar o .NET privado.
- Trocar de canal/versão: `kaizen-select --list`, `kaizen-select beta`,
  `kaizen-select beta/4.8.0-beta1`.
- O instalador **nunca** edita `~/.bashrc`/`~/.profile`; apenas informa o
  `PATH` necessário.
- Sobrescrever uma instalação existente exige `--force`.

Limitação: a engine só procura templates em
`$XDG_DATA_HOME/kaizen/export_templates/<versão>`. Portanto uma instalação
global instala os templates no XDG do usuário que executou o instalador. Para
outros usuários, extraia o arquivo e rode `./Setup/install.sh --templates-only`
com o `HOME` de cada um.

## Desinstalação

```bash
./Setup/uninstall.sh --list
./Setup/uninstall.sh --channel beta --version 4.8.0-beta1
./Setup/uninstall.sh --all --yes
```

Remove apenas os arquivos daquele canal/versão, atualiza ponteiros e mantém
templates/.NET compartilhados enquanto outra versão os usar. Projetos,
configurações e caches nunca são removidos. A seleção padrão é reavaliada.

## Validação

```bash
./packaging/linux/validate.sh --mono [--global] [--gui]
./packaging/linux/validate.sh --mono --dist DIR --tarball FILE
```

O validador mantém as checagens estruturais e de runtime e acrescenta:

- layout novo, metadados, manifesto JSON (schema 2) e `MANIFEST.sha256`;
- symlinks relativos/contidos e ausência de diretórios vazios;
- `.tar.xz`: nome, checksum externo, entradas dentro de `Kaizen/`, tipos
  seguros, extração e execução da engine extraída;
- instalação por usuário em `HOME`/XDG isolados, launcher, seleção, XDG;
- coexistência de dois canais e desinstalação seletiva com preservação de
  dados;
- instalação global simulada (sem root) com `--opt-root/--bin-dir/...`.

A árvore empacotada nunca é usada em execução: os testes de runtime e
instalação rodam em cópias descartáveis.

## Windows

```powershell
.\packaging\windows\build.ps1   -Target all -Mono -Jobs 16
.\packaging\windows\package.ps1 -Mono yes -Output dist\Kaizen -Force
.\packaging\windows\validate.ps1 -Dist dist\Kaizen -Mono yes
```

> **Não validado.** Este host é Fedora, sem MSVC/Mingw-w64. O fluxo Windows
> antecede a nova estrutura de canais e do `.tar.xz`; ele será atualizado
> quando houver um host Windows. Nenhum formato Windows (MSI, ZIP) foi
> implementado nesta etapa.

## Convenções

- Alvos SCons: `editor`, `template_debug`, `template_release`.
- Binário: `kaizen.<platform>.<target>.<arch>[.mono]`.
- Templates Linux: `linux_release.x86_64`, `linux_debug.x86_64`.
- Templates em `KAIZEN_VERSION_FULL_CONFIG` (`4.8.beta[.mono]`).
- Projeto: `project.kaizen`; dados em `.kaizen/`.

## O que nunca é incluído

`bin/obj/`, `.sconsign*.dblite`, `.scons_env.json`, `__pycache__/`, `*.pyc`,
`*.o`, `*.a`, `*.lib`, `doc/translations/`, `tests/`, resíduos `obj/`.

## Referência

Uma cópia de referência dos binários de um build anterior chegou a ser
preservada em `dist/.reference/` com `SHA256SUMS`, mas foi **removida**
juntamente com o diretório `dist/` (que também continha a entrega oficial),
por decisão explícita: o repositório hoje contém apenas o fonte. Esses
arquivos não faziam parte de nenhuma distribuição gerada. Para obter um
artefato novamente é necessário reconstruir tudo.
