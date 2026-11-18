# Termos Adicionais e Exceção para Distribuição de Jogos

**Projeto:** KaizenEngine
**Copyright (C) 2026 Matheus A. Santos, Hardion Labs**
**Repositório:** https://github.com/CeoMatheusDev/Kaizen

O código-fonte da KaizenEngine (a "Engine") é licenciado sob a **GNU Affero General Public License, versão 3 ou posterior (AGPL-3.0-or-later)**, cujo texto completo está no arquivo `LICENSE`.

Os termos abaixo são **Permissões Adicionais** concedidas sob a **Seção 7** da AGPL-3.0, e **obrigações adicionais de aviso** decorrentes da **Seção 5(a)** da AGPL-3.0. Eles fazem parte da licença do software e se aplicam a todos que recebem a Engine. Eles não podem ser removidos ou alterados por distribuidores subsequentes.

---

## 1. Exceção para Distribuição de Jogos (Game Distribution Exception)

**Por que esta exceção existe:** Sob a AGPL, uma obra que se liga ou importa a Engine pode ser considerada uma "obra combinada", o que forçaria o jogo inteiro a ser licenciado sob a AGPL (e, devido à cláusula de rede, forçaria a divulgação do código-fonte para jogos online). Esta exceção **limita o copyleft à Engine em si**, e não aos jogos criados com ela, permitindo que você distribua seu jogo sob os termos que desejar, inclusive proprietários e comerciais.

### A Concessão (A Permissão)

**Permissão adicional sob a Seção 7 da GNU AGPL versão 3.**

Como uma exceção especial, o detentor dos direitos autorais da KaizenEngine concede permissão para que você combine a Engine com seu trabalho independente (um "Jogo") e distribua a obra combinada resultante sob os termos de sua escolha, **incluindo termos proprietários e comerciais**, **desde que**:

1. Você distribua a **própria Engine**, e quaisquer modificações que você fizer a ela, sob a **AGPL-3.0-or-later**, juntamente com estes Termos Adicionais.
2. O Jogo utilize a Engine através de suas **interfaces públicas** e não copie, reimplemente ou incorpore uma parte substancial do código-fonte da própria Engine além do necessário para usá-la.
3. Você exiba a atribuição exigida na **Seção 2** abaixo.

**Importante:** Esta exceção é **opcional**. Se você não desejar exibir a atribuição, você pode, alternativamente, licenciar seu Jogo sob os termos da AGPL-3.0. A atribuição é exigida **apenas** quando você se beneficia desta exceção para distribuir seu Jogo sob termos não-AGPL (como os proprietários/comerciais).

---

## 2. Requisitos de Atribuição (Créditos)

Para se beneficiar da Exceção para Distribuição de Jogos acima, você deve fornecer a atribuição de forma clara e visível.

- **Na Engine:** Você deve preservar os avisos de copyright e créditos originais da Engine em qualquer redistribuição da Engine (modificada ou não). Manter o arquivo `NOTICE` ou `CREDITS.md` original é a forma de cumprir este requisito.
- **No Jogo (Produto Final):** Você deve incluir uma atribuição visível de que o Jogo foi "Feito com KaizenEngine" ("Made with KaizenEngine"). Essa atribuição pode ser feita:
  - Na tela de inicialização (splash screen) do Jogo; ou
  - Na tela de créditos do Jogo; ou
  - Em um arquivo de texto `LICENSE.md` ou `CREDITS.md` distribuído junto com o Jogo.
- **Para Modificações na Engine:** Se você modificar arquivos da Engine, deverá manter os créditos originais desses arquivos e adicionar seu próprio nome como colaborador da modificação. Arquivos criados por você do zero (sem código preexistente da Engine) são de sua autoria, mas a obra como um todo (a Engine modificada) permanece sob a AGPL-3.0 com estes Termos Adicionais.

---

## 3. Aviso de Modificação nos Arquivos (Seção 5(a) da AGPL)

**Esta é uma obrigação decorrente diretamente da AGPL-3.0, não uma escolha do projeto.** A Seção 5(a) da AGPL exige que todo arquivo modificado contenha um aviso destacado informando que o arquivo foi alterado e a data da alteração.

### O que é obrigatório

Todo arquivo-fonte da Engine que for modificado deve conter, em seu cabeçalho, um aviso com:

1. A declaração de que o arquivo foi **modificado**;
2. O **nome** de quem modificou (pessoa ou organização);
3. A **data** da modificação (formato `AAAA-MM-DD`).

### Formato recomendado

```cpp
// Modified by [Nome], [Organização] on [AAAA-MM-DD].