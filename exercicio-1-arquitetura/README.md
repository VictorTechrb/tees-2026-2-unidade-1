# Adaptação Arquitetural — BibliotecaES

**Atividade 01 — Adaptação arquitetural.** Aplicar a regra de dependência de Clean, Hexagonal ou
Onion sobre um sistema que já existe, fazendo o domínio parar de depender do banco de dados.

Sistema de gestão de uma biblioteca acadêmica, reaproveitado das disciplinas de Engenharia de
Software I e II. O projeto original está no repositório oficial da disciplina.

| | |
|---|---|
| **Agregado escolhido** | `Autor` (recomendado pelo professor — é o único com testes prontos) |
| **Arquitetura desta branch** | **Hexagonal (Ports & Adapters)** |
| **Pasta** | `projeto-es-biblioteca-2025-hexagonal/Codigo2025/Biblioteca/` |
| **Branch** | `Hexagonal` |
| **Entrega principal do grupo** | **Clean Architecture**, na branch `main`, em `projeto-es-bibliote-2025-clean/` |
| **Natureza desta entrega** | slide **EXTRA** — *"Implementar o mesmo agregado em uma segunda ou terceira arquitetura"* |

> **Status:** documentação da arquitetura atual, escolha e justificativa — concluídas.
> Implementação — em andamento.
> As seções marcadas com ⏳ são preenchidas ao final da implementação.

---

## 1. Documentação da arquitetura atual

### 1.1 Estilo externo (encontro 01) — *quantos artefatos, quantos bancos?*

**Monolito não modular.**

O sistema tem **dois artefatos implantáveis** — `BibliotecaAPI` (Web API + Swagger) e
`BibliotecaWeb` (MVC + Razor + Identity) — que **compartilham os mesmos projetos**
`Core`, `Service` e `Util`, e **o mesmo banco MySQL** (a connection string `BibliotecaDatabase`
aparece igual nos dois `appsettings.json`).

Não é SOA, não é microsserviço, não é serverless. E **continua não sendo depois desta adaptação**:
esta atividade mexe na pergunta do encontro 02, não na do encontro 01.

### 1.2 Estilo interno (encontro 02) — *para onde apontam as dependências?*

**Arquitetura em camadas clássica**, com dependência descendente:

```
Controller  →  Service  →  Core (entidades + BibliotecaContext + Entity Framework)
```

- `BibliotecaAPI/Controllers/AutoresController.cs` e `BibliotecaWeb/Controllers/AutorController.cs`
  recebem `IAutorService` por injeção. **Essa fronteira já está invertida.**
- `Service/AutorService.cs` recebe `BibliotecaContext` por injeção. **Essa não está.**
- `Core` é quem carrega o Entity Framework.

Não é Clean, não é Hexagonal, não é Onion — e o professor deixa claro que não precisava ser.

### 1.3 Onde a regra de dependência ainda não se aplica

Dois pontos concretos, ambos verificáveis no código original:

**(a) O serviço depende do framework de persistência.**

```csharp
// Service/AutorService.cs, linhas 14-20
using Microsoft.EntityFrameworkCore;

public class AutorService : IAutorService
{
    private readonly BibliotecaContext context;          // dependência concreta

    public AutorService(BibliotecaContext context)
    {
        this.context = context;
    }
}
```

**(b) O domínio é o projeto que mais depende de tecnologia externa.**

```xml
<!-- Core/Core.csproj -->
<PackageReference Include="MySql.EntityFrameworkCore" Version="8.0.20" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.23" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.23" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.23" />
```

`Core/BibliotecaContext.cs` é um `DbContext` completo, com todo o mapeamento relacional
(nomes de tabela, colunas, chaves estrangeiras) no `OnModelCreating`. Ou seja: **o círculo mais
interno da arquitetura é justamente o que mais conhece o banco** — exatamente a inversão do que
Clean, Hexagonal e Onion exigem.

**Sintoma prático.** A única regra de negócio do agregado Autor é uma linha:

```csharp
if (autor.DataNascimento.Year < 1000)
    throw new ServiceException("O ano de nascimento de autor deve ser maior do que 1000. ...");
```

Mesmo assim, os 6 testes de `ServiceTests/AutorServiceTests.cs` precisam montar um
`DbContextOptionsBuilder` com `UseInMemoryDatabase` para exercitá-la. É preciso simular o Entity
Framework para testar uma comparação de inteiros. **Esse é o custo que a inversão elimina.**

### 1.4 Linha de base — testes antes de qualquer alteração

Medido nesta pasta, antes de tocar em qualquer arquivo:

```
ServiceTests ........ Aprovado: 6, Com falha: 0, Total: 6
BibliotecaWebTests .. Aprovado: 9, Com falha: 0, Total: 9
TOTAL ............... 15 testes verdes
```

Este é o critério objetivo de que nada quebrou.

---

## 2. Escolha arquitetural e justificativa

### 2.1 A escolha

**Arquitetura Hexagonal (Ports & Adapters).**

O critério não foi "qual arquitetura é mais moderna". As três opções do professor resolvem a regra
de dependência. O critério foi: **qual delas demonstra a inversão neste código com o menor número
de mudanças desnecessárias.**

### 2.2 Por que Hexagonal, neste projeto

**1. Metade do hexágono já existe.** `Core/Autor.cs` é POCO puro — sem `[Table]`, sem `[Key]`,
sem nenhum atributo de Entity Framework; todo o mapeamento é fluente, dentro do `OnModelCreating`.
Isso significa que **nenhuma entidade precisa ser alterada** para tirar o EF do domínio. E os
controllers já dependem de `IAutorService`, não da implementação. Falta apenas o lado de saída.

**2. Hexagonal nomeia os dois lados da fronteira.** Clean e Onion dão nome ao contrato de
persistência. Hexagonal obriga a nomear também o lado de entrada: `IAutorService` deixa de ser
"uma interface de serviço" e passa a ser a **porta de entrada** (driving), e `BibliotecaAPI` e
`BibliotecaWeb` passam a ser, explicitamente, **dois adaptadores de entrada plugados no mesmo
núcleo**. Isso amarra no código a resposta do encontro 01 (dois artefatos, um núcleo) com a do
encontro 02 (para onde apontam as dependências).

**3. Permite uma prova executável de isolamento.** Uma porta, vários adaptadores: o mesmo
`IAutorRepositorioPort` recebe um adaptador Entity Framework (produção) e um adaptador em memória
(teste). Com isso, a regra de negócio passa a ser testada **sem nenhuma referência ao Entity
Framework** — o isolamento deixa de ser uma afirmação da documentação e vira um teste que roda.

**4. Rende o comparativo que o slide EXTRA cobra.** Como esta é a segunda arquitetura sobre o mesmo
agregado, a entrega precisa separar o que mudou de verdade do que só mudou de nome (seção 7).

### 2.3 Por que não Onion

Onion propõe círculos concêntricos: Domain Model → Domain Services → Application Services →
Infrastructure. Em um projeto com **um agregado, uma regra de negócio e uma interface de
persistência**, o resultado seria:

| Clean (entrega do grupo) | Onion equivalente |
|---|---|
| `Core/` (entidades) | `Domain/` |
| `Core/Repository/IAutorRepository` | `Domain/IAutorRepository` |
| `Service/AutorService` | `ApplicationServices/AutorService` |
| `Infrastructure/AutorRepository` | `Infrastructure/AutorRepository` |

**As setas de dependência seriam idênticas às da Clean.** Não há nenhuma decisão estrutural que
Onion force e Clean não force neste código. Como segunda arquitetura, o comparativo resultante
seria honesto mas vazio: "mudaram os nomes das pastas".

### 2.4 Por que não repetir Clean

É a entrega principal do grupo, já em andamento na branch `main`. Repeti-la não produziria nem a
segunda implementação nem o comparativo pedidos pelo slide EXTRA.

---

## 3. Escopo — uma fatia vertical

Conforme o slide *"O ESCOPO É UMA FATIA VERTICAL"*: não migramos o sistema inteiro.

**Dentro do escopo** — agregado `Autor`, de ponta a ponta:

```
AutoresController (API)          ─┐
AutorController (Web)            ─┴→ IAutorService (porta de entrada)
                                        → AutorService (hexágono, regra de negócio)
                                            → IAutorRepositorioPort (porta de saída)
                                                → AutorRepositorioEF (adaptador, EF + MySQL)
                                                → AutorRepositorioEmMemoria (adaptador, testes)
```

**Fora do escopo, e por quê:**

| Item | Situação |
|---|---|
| `Editora`, `Livro`, `ItemAcervo`, `Pessoa`, `Emprestimo`, `Devolucao`, `Doacao` | permanecem no projeto `Service` legado, acessando o contexto diretamente |
| Identity / autenticação | muda de projeto (sai do `Core`), não muda de arquitetura |
| Views `.cshtml`, `Util`, AutoMapper, `BibliotecaWebTests` | intocados |
| Banco, migrations, `appsettings.json` | intocados |
| Estilo externo (monolito) | intocado — é a pergunta do encontro 01 |

---

## 4. A porta e o adaptador

### 4.1 A porta de persistência, declarada no domínio

`Core/Ports/Saida/IAutorRepositorioPort.cs` — projeto `Core`, que após a adaptação **não referencia
nenhum projeto e nenhum pacote NuGet**.

```csharp
namespace Core.Ports.Saida;

public interface IAutorRepositorioPort
{
    uint Create(Autor autor);
    void Edit(Autor autor);
    void Delete(uint id);
    Autor? Get(uint id);
    IEnumerable<Autor> GetAll();
    IEnumerable<AutorDto> GetByNome(string nome);
    DatatableResponse<Autor> GetDataPage(DatatableRequest request);
}
```

**Porta mínima, por decisão.** A porta declara **apenas os 7 métodos que `IAutorService` expõe** —
não os 11 métodos públicos que a classe `AutorService` tem hoje. Os outros quatro
(`GetAllOrderByNome`, `GetCountAutores`, `GetByName`, `GetOrderByDescending`) não são chamados por
nenhum controller, view ou teste do projeto, e não migram. O princípio: **a porta declara o que o
domínio precisa, não o que o banco sabe fazer.**

### 4.2 O adaptador com Entity Framework, fora do domínio

`Adapters/Persistencia/EntityFramework/AutorRepositorioEF.cs` — projeto `Adapters`, que concentra
os pacotes `MySql.EntityFrameworkCore` e `Microsoft.EntityFrameworkCore.*` e referencia `Core`.

As queries LINQ vêm **copiadas** do `AutorService` original, sem alteração de comportamento.
As validações de regra de negócio **não** vêm junto: elas ficam no hexágono.

### 4.3 O segundo adaptador da mesma porta

`ServiceTests/Fakes/AutorRepositorioEmMemoria.cs` — implementação sobre `List<Autor>`, **sem
nenhum `using` de Entity Framework**. É o que torna a regra de negócio testável sem banco.

---

## 5. Distribuição das responsabilidades

| Projeto | Papel | Responsabilidade | Depende de |
|---|---|---|---|
| `Core` | **hexágono — domínio** | entidades, DTOs, `ServiceException`, porta de entrada, porta de saída | nada |
| `Application` | **hexágono — aplicação** | orquestra o caso de uso e aplica a regra de negócio (ano de nascimento) | `Core` |
| `Adapters` | **adaptador de saída** | `BibliotecaContext`, mapeamento relacional, queries LINQ, Identity | `Core`, EF, MySQL |
| `BibliotecaAPI` | **adaptador de entrada** | HTTP/REST, Swagger, AutoMapper, filtros de exceção, **composition root** | todos |
| `BibliotecaWeb` | **adaptador de entrada** | MVC, Razor, Identity, **composition root** | todos |
| `Service` | legado | agregados não migrados | `Core`, `Adapters` |
| `ServiceTests` | testes | regressão (com adaptador EF) e isolamento (com adaptador em memória) | `Core`, `Application`, `Adapters` |
| `Util` | utilitário | validadores de CPF, CEP, telefone | — |

### Regra de dependência

```
        ADAPTADORES DE ENTRADA (driving)
        BibliotecaAPI        BibliotecaWeb
              │                    │
              └────────┬───────────┘
                       ▼
        ╔══════════════════════════════════════╗
        ║  PORTA DE ENTRADA                    ║
        ║  Core.Ports.Entrada.IAutorService    ║
        ║              │                       ║
        ║              ▼                       ║
        ║  Application.AutorService            ║   HEXÁGONO
        ║  (regra: ano de nascimento > 1000)   ║   zero EF
        ║              │                       ║
        ║              ▼                       ║
        ║  PORTA DE SAÍDA                      ║
        ║  Core.Ports.Saida.                   ║
        ║       IAutorRepositorioPort          ║
        ╚══════════════════════════════════════╝
                       ▲
              ┌────────┴────────┐
              │                 │
    AutorRepositorioEF   AutorRepositorioEmMemoria
    (Adapters, EF+MySQL)  (ServiceTests, zero EF)
         ADAPTADORES DE SAÍDA (driven)
```

| Projeto | PODE referenciar | NÃO PODE referenciar |
|---|---|---|
| `Core` | nada | EF, MySQL, ASP.NET, `Application`, `Adapters` |
| `Application` | `Core` | EF, MySQL, `Adapters`, `Service`, ASP.NET |
| `Adapters` | `Core`, EF, MySQL | `Application`, `Service`, ASP.NET MVC |
| `Service` (legado) | `Core`, `Adapters` | `Application` |
| `BibliotecaAPI` / `BibliotecaWeb` | todos | — (é o composition root) |

**A seta que importa:** `Adapters → Core`, nunca `Core → Adapters`. A implementação conhece o
contrato; o contrato não conhece a implementação.

---

## 6. O que mudou e o que permaneceu

### Mudou

1. O Entity Framework saiu do `Core` — `Core.csproj` fica sem nenhum `PackageReference`.
2. `BibliotecaContext` e o Identity mudaram de projeto (`Core` → `Adapters`). **Mudaram de
   arquivo, não de conteúdo**: o `OnModelCreating` é o mesmo.
3. Nasceu a porta de saída `IAutorRepositorioPort`, declarada no domínio.
4. `AutorService` mudou de projeto (`Service` → `Application`) e passou a depender da porta
   em vez do `DbContext`.
5. `IAutorService` foi reclassificada como porta de entrada (`Core/Ports/Entrada/`).
6. A regra de negócio ganhou testes que rodam sem banco — ela não tinha nenhum.

### Permaneceu igual

- `Core/Autor.cs` e todas as demais entidades — nenhuma linha alterada.
- As assinaturas de `IAutorService`.
- O mapeamento relacional e o comportamento das queries.
- A lógica dos controllers — muda apenas a linha de `using`.
- Views `.cshtml`, `Util`, AutoMapper, migrations, `appsettings.json`, o banco.
- Os 9 testes de `BibliotecaWebTests`.
- **Os 6 `Assert` de `AutorServiceTests`** — muda só a linha que monta o serviço.

---

## 7. Comparativo: Clean × Hexagonal no agregado Autor

> Esta seção atende ao slide **EXTRA**: *"com um comparativo do que efetivamente mudou entre elas,
> e do que só mudou de nome"*.

| | Clean (branch `main`) | Hexagonal (branch `Hexagonal`) |
|---|---|---|
| Interface de persistência | `Core/Repository/IAutorRepository` | `Core/Ports/Saida/IAutorRepositorioPort` |
| Largura do contrato | 11 métodos (4 sem nenhum chamador) | 7 métodos — só o que `IAutorService` expõe |
| Projeto que implementa | `Infrastructure` | `Adapters` |
| Onde vive o `AutorService` | `Service`, que **referencia** `Infrastructure` | `Application`, cuja **única** referência é `Core` |
| Lado de entrada | não nomeado | `Ports/Entrada` — controllers são adaptadores explícitos |
| Adaptadores por porta | 1 (Entity Framework) | 2 (Entity Framework + memória) |
| Teste da regra de negócio | exige `UseInMemoryDatabase` | roda sem nenhum Entity Framework |

### O que efetivamente mudou

**A separação do serviço em um projeto que não referencia infraestrutura.** Na Clean,
`Service.csproj` referencia `Infrastructure.csproj`: nada impede, amanhã, que alguém volte a
injetar o `DbContext` em um serviço. Em Hexagonal, `Application.csproj` referencia **somente**
`Core` — a violação deixa de ser uma questão de disciplina da equipe e passa a ser **erro de
compilação**.

**O segundo adaptador.** Duas implementações da mesma porta transformam o isolamento do domínio de
afirmação em documentação para teste que roda. É a diferença entre dizer que a regra de negócio
não depende do banco e demonstrá-lo.

**A largura do contrato.** Portar 11 métodos, quatro deles sem chamador, faz a porta descrever as
capacidades do banco. Portar 7 faz a porta descrever a necessidade do domínio. É a mesma regra de
dependência, com contratos de qualidade diferente.

### O que só mudou de nome

- `Infrastructure` × `Adapters` — mesmo papel, mesma posição no grafo de dependências.
- `Repository` × `porta de saída (driven port)` — mesmo conceito, vocabulários diferentes.
- `Entities` / `Core` × `hexágono` — mesma coisa, metáforas diferentes (círculos × hexágono).
- `Autor.cs` é idêntico nas duas; a validação do ano de nascimento é idêntica nas duas;
  a seta `implementação → contrato` é idêntica nas duas.

### Conclusão

**As duas arquiteturas satisfazem a regra de dependência de formas equivalentes.** A diferença real
não está no desenho, e sim em **quanto da regra o compilador consegue fazer valer sozinho** — e em
quanto do isolamento é demonstrável por teste, em vez de descrito em prosa. Boa parte do resto é
vocabulário.

---

## 8. Como verificar

```bash
cd projeto-es-biblioteca-2025-hexagonal/Codigo2025/Biblioteca
```

**O domínio não conhece o banco** — a saída tem de ser vazia:

```bash
grep -rn "EntityFrameworkCore\|MySql" Core/ Application/ --include=*.cs --include=*.csproj
```

**Sentido das dependências:**

```bash
dotnet list Core/Core.csproj reference
dotnet list Application/Application.csproj reference
```

**Compilação e testes:**

```bash
dotnet build Biblioteca.sln --nologo
dotnet test ServiceTests/ServiceTests.csproj --nologo
dotnet test BibliotecaWebTests/BibliotecaWebTests.csproj --nologo
```

### Resultado ⏳

| Verificação | Antes | Depois |
|---|---|---|
| `ServiceTests` | 6 aprovados, 0 falhas | ⏳ |
| `BibliotecaWebTests` | 9 aprovados, 0 falhas | ⏳ |
| Testes da regra sem EF | 0 (não existiam) | ⏳ |
| EF em `Core/` + `Application/` | 4 pacotes + 1 `DbContext` | ⏳ (esperado: nenhum) |
| Referências de `Core` | 4 pacotes NuGet | ⏳ (esperado: nenhuma) |

---

## 9. Pré-requisitos e execução

**Pré-requisitos**

- .NET SDK 8.0 ou superior (validado com 8.0.425)
- MySQL para executar a aplicação (os testes usam banco em memória e não precisam de MySQL)

**Executar**

```bash
cd projeto-es-biblioteca-2025-hexagonal/Codigo2025/Biblioteca
dotnet run --project BibliotecaWeb    # aplicação MVC
dotnet run --project BibliotecaAPI    # API + Swagger
```

As connection strings `BibliotecaDatabase` e `IdentityDatabase` ficam nos `appsettings.json`
de cada host.

---

## 10. Como esta entrega se relaciona com a do grupo

| | Branch `main` | Branch `Hexagonal` |
|---|---|---|
| Arquitetura | Clean Architecture | Hexagonal (Ports & Adapters) |
| Pasta | `projeto-es-bibliote-2025-clean/` | `projeto-es-biblioteca-2025-hexagonal/` |
| Papel | **entrega principal** (1,0 ponto da unidade) | **extra** (pontuação registrada à parte) |
| Agregado | Autor | Autor — o mesmo, como o slide EXTRA pede |

---

## Documentos

- [`TESTES.md`](TESTES.md) — saídas literais do `dotnet test`; linha de base já registrada, coluna "depois" pendente

O enunciado da atividade e o plano de execução interno da dupla não são versionados.
