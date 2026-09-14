# Adaptação Arquitetural — BibliotecaES

**Atividade 01 — Adaptação arquitetural.** Aplicar a regra de dependência de Clean, Hexagonal ou Onion sobre um sistema que já existe, fazendo o domínio parar de depender do banco de dados.

Projeto original: https://github.com/marcosdosea/biblioteca-es/tree/master/Codigo2025/Biblioteca.

| | |
|---|---|
| **Agregado escolhido** | `Autor` (recomendado pelo professor — é o único com testes prontos) |
| **Arquitetura 1 desta entrega** | **Clean Architecture**, em `projeto-es-bibliote-2025-clean/` |
| **Arquitetura 2 desta entrega** | **Hexagonal (Ports & Adapters)** |
| **Pasta** | `projeto-es-biblioteca-2025-hexagonal/Codigo2025/Biblioteca/` |
| **Onde as duas ficam** | branch `main` — as duas arquiteturas convivem no repositório, uma pasta para cada |

> **Status:** entrega concluída — documentação, implementação, integração e verificação. Build com êxito, 23 testes verdes (14 em `ServiceTests` + 9 em `BibliotecaWebTests`) e o domínio sem nenhuma referência a Entity Framework. Números e saídas literais nas seções 7 e 8 e em [`TESTES.md`](TESTES.md).

---

## 1. Documentação da arquitetura atual

### 1.1 Estilo externo (encontro 01) — *quantos artefatos, quantos bancos?*

**Monolito não modular.**

O sistema tem **dois artefatos implantáveis** — `BibliotecaAPI` (Web API + Swagger) e `BibliotecaWeb` (MVC + Razor + Identity) — que **compartilham os mesmos projetos** `Core`, `Service` e `Util`, e **o mesmo banco MySQL** (a connection string `BibliotecaDatabase` aparece igual nos dois `appsettings.json`).

### 1.2 Estilo interno (encontro 02) — *para onde apontam as dependências?*

**Arquitetura em camadas clássica**, com dependência descendente:

```
Controller  →  Service  →  Core (entidades + BibliotecaContext + Entity Framework)
```

- `BibliotecaAPI/Controllers/AutoresController.cs` e `BibliotecaWeb/Controllers/AutorController.cs` recebem `IAutorService` por injeção. **Essa fronteira já está invertida.**
- `Service/AutorService.cs` recebe `BibliotecaContext` por injeção. **Essa não está.**
- `Core` é quem carrega o Entity Framework.

Não é Clean, não é Hexagonal, não é Onion.

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

`Core/BibliotecaContext.cs` é um `DbContext` completo, com todo o mapeamento relacional (nomes de tabela, colunas, chaves estrangeiras) no `OnModelCreating`. Ou seja: **o círculo mais interno da arquitetura é justamente o que mais conhece o banco** — exatamente a inversão do que Clean, Hexagonal e Onion exigem.

**Sintoma prático.** A única regra de negócio do agregado Autor é uma linha:

```csharp
if (autor.DataNascimento.Year < 1000)
    throw new ServiceException("O ano de nascimento de autor deve ser maior do que 1000. ...");
```

Mesmo assim, os 6 testes de `ServiceTests/AutorServiceTests.cs` precisam montar um `DbContextOptionsBuilder` com `UseInMemoryDatabase` para exercitá-la. É preciso simular o Entity Framework para testar uma comparação de inteiros. **Esse é o custo que a inversão elimina.**

### 1.4 Linha de base — testes antes de qualquer alteração

```
ServiceTests ........ Aprovado: 6, Com falha: 0, Total: 6
BibliotecaWebTests .. Aprovado: 9, Com falha: 0, Total: 9
TOTAL ............... 15 testes verdes
```

---

## 2. Escolha arquitetural e justificativa

### 2.1 A escolha

**Arquitetura Clean** e **Arquitetura Hexagonal (Ports & Adapters).**

### 2.2 Por que Clean, neste projeto

Clean é a entrega principal, feita pelo restante do grupo. A escolha se sustenta em quatro pontos:

**1. É o vocabulário que o projeto já falava.** O BibliotecaES original já tinha um projeto `Service` com `AutorService`, `EditoraService`, `LivroService` e `ItemAcervoService`. Clean não pede que nada disso seja renomeado: basta declarar o contrato de persistência em `Core/Repository/` e mover o Entity Framework para um projeto `Infrastructure`. A adaptação é de posição, não de nomenclatura — o diff fica menor e o risco de quebrar o que já funcionava, também.

**2. Ataca exatamente os dois pontos da seção 1.3.** O problema era o domínio carregar os pacotes de EF e o serviço receber `BibliotecaContext`. Clean resolve os dois de uma vez: o contrato (`IAutorRepository`) fica no círculo interno, a implementação (`AutorRepository`) no externo, e o `Core.csproj` termina sem nenhum `PackageReference`.

**3. Repete-se por agregado sem custo de conceito.** O mesmo padrão — uma interface em `Core/Repository/`, uma implementação em `Infrastructure/Repositories/` — foi aplicado a quatro agregados: `Autor`, `Editora`, `Livro` e `ItemAcervo`. Num trabalho de grupo isso pesa: é um padrão que várias pessoas aplicam em paralelo, cada uma no seu agregado, sem precisar combinar detalhes a cada passo.

**4. O compilador passa a cobrar a regra.** Com `Service.csproj` referenciando apenas `Core`, injetar um `DbContext` num serviço deixa de compilar. A regra de dependência sai da disciplina da equipe e vira erro de build.

### 2.3 Por que Hexagonal, neste projeto

**1. Metade do hexágono já existe.** `Core/Autor.cs` é POCO puro — sem `[Table]`, sem `[Key]`, sem nenhum atributo de Entity Framework; todo o mapeamento é fluente, dentro do `OnModelCreating`. Isso significa que **nenhuma entidade precisa ser alterada** para tirar o EF do domínio. E os controllers já dependem de `IAutorService`, não da implementação. Falta apenas o lado de saída.

**2. Hexagonal nomeia os dois lados da fronteira.** Clean e Onion dão nome ao contrato de persistência. Hexagonal obriga a nomear também o lado de entrada: `IAutorService` deixa de ser "uma interface de serviço" e passa a ser a **porta de entrada** (driving), e `BibliotecaAPI` e `BibliotecaWeb` passam a ser, explicitamente, **dois adaptadores de entrada plugados no mesmo núcleo**. Isso amarra no código a resposta do encontro 01 (dois artefatos, um núcleo) com a do encontro 02 (para onde apontam as dependências).

**3. Permite uma prova executável de isolamento.** Uma porta, vários adaptadores: o mesmo `IAutorRepositorioPort` recebe um adaptador Entity Framework (produção) e um adaptador em memória (teste). Com isso, a regra de negócio passa a ser testada **sem nenhuma referência ao Entity Framework** — o isolamento deixa de ser uma afirmação da documentação e vira um teste que roda.

**4. Rende o comparativo que o slide EXTRA cobra.** Como esta é a segunda arquitetura sobre o mesmo agregado, a entrega precisa separar o que mudou de verdade do que só mudou de nome (seção 7).

### 2.4 Por que não Onion

Onion propõe círculos concêntricos: Domain Model → Domain Services → Application Services → Infrastructure. Em um projeto com **um agregado, uma regra de negócio e uma interface de persistência**, o resultado seria:

| Clean (entrega do grupo) | Onion equivalente |
|---|---|
| `Core/` (entidades) | `Domain/` |
| `Core/Repository/IAutorRepository` | `Domain/IAutorRepository` |
| `Service/AutorService` | `ApplicationServices/AutorService` |
| `Infrastructure/AutorRepository` | `Infrastructure/AutorRepository` |

**As setas de dependência seriam idênticas às da Clean.** Não há nenhuma decisão estrutural que Onion force e Clean não force neste código. Como segunda arquitetura, o comparativo resultante seria honesto mas vazio: "mudaram os nomes das pastas".

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

`Core/Ports/Saida/IAutorRepositorioPort.cs` — projeto `Core`, que após a adaptação **não referencia nenhum projeto e nenhum pacote NuGet**.

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

**Porta mínima, por decisão.** A porta declara **apenas os 7 métodos que `IAutorService` expõe** — não os 11 métodos públicos da classe `AutorService` original. Os outros quatro (`GetAllOrderByNome`, `GetCountAutores`, `GetByName`, `GetOrderByDescending`) não são chamados por nenhum controller, view ou teste do projeto, e não migraram. O princípio: **a porta declara o que o domínio precisa, não o que o banco sabe fazer.** A entrega Clean do grupo tomou a decisão oposta e levou os 11 — o contraste está na seção 7.

### 4.2 O adaptador com Entity Framework, fora do domínio

`Adapters/Persistencia/EntityFramework/AutorRepositorioEF.cs` — projeto `Adapters`, que concentra os pacotes `MySql.EntityFrameworkCore` e `Microsoft.EntityFrameworkCore.*` e referencia `Core`.

As queries LINQ vêm **copiadas** do `AutorService` original, sem alteração de comportamento. As validações de regra de negócio **não** vêm junto: elas ficam no hexágono.

### 4.3 O segundo adaptador da mesma porta

`ServiceTests/Fakes/AutorRepositorioEmMemoria.cs` — implementação sobre `List<Autor>`, **sem nenhum `using` de Entity Framework**. É o que torna a regra de negócio testável sem banco.

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
| `ServiceTests` | testes | regressão (com adaptador EF) e isolamento (com adaptador em memória) | `Application`, `Service` e, por transitividade, `Core` e `Adapters` |
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

**A seta que importa:** `Adapters → Core`, nunca `Core → Adapters`. A implementação conhece o contrato; o contrato não conhece a implementação.

---

## 6. O que mudou e o que permaneceu

### Mudou

1. O Entity Framework saiu do `Core` — `Core.csproj` fica sem nenhum `PackageReference`.
2. `BibliotecaContext` e o Identity mudaram de projeto (`Core` → `Adapters`). **Mudaram de arquivo, não de conteúdo**: o `OnModelCreating` é o mesmo.
3. Nasceu a porta de saída `IAutorRepositorioPort`, declarada no domínio.
4. `AutorService` mudou de projeto (`Service` → `Application`) e passou a depender da porta em vez do `DbContext`. O antigo `Service/AutorService.cs` foi **removido** na integração: restou uma única classe `AutorService` no projeto, em `Application/`.
5. `IAutorService` foi reclassificada como porta de entrada (`Core/Ports/Entrada/`).
6. Os dois `Program.cs` passaram a resolver `IAutorService` para `Application.AutorService` e `IAutorRepositorioPort` para `AutorRepositorioEF` — a ligação porta/adaptador acontece só no composition root.
7. A regra de negócio ganhou testes que rodam sem banco — ela não tinha nenhum.

### Permaneceu igual

- `Core/Autor.cs` e todas as demais entidades — nenhuma linha alterada.
- As assinaturas de `IAutorService`.
- O mapeamento relacional e o comportamento das queries.
- A lógica dos controllers — muda apenas a linha de `using`.
- Views `.cshtml`, `Util`, AutoMapper, migrations, `appsettings.json`, o banco.
- Os 9 testes de `BibliotecaWebTests`.
- **Os 6 testes de `AutorServiceTests`** — mudou uma linha só, a que monta o serviço (`new AutorService(new AutorRepositorioEF(context))`); nenhum `Assert` foi tocado.

---

## 7. Comparativo: Clean × Hexagonal no agregado Autor

### 7.1 O que é igual nas duas

**As duas entregas cumprem a regra de dependência, e cumprem do mesmo jeito.** Em nenhuma delas o domínio conhece o banco; em nenhuma delas o serviço conhece o `DbContext`. Isso precisa ser dito com todas as letras antes de qualquer diferença:

| Critério da atividade | Clean (entrega principal) | Hexagonal (entrega extra) |
|---|---|---|
| Pacotes NuGet no projeto de domínio | **0** | **0** |
| Contrato de persistência declarado no domínio | `Core/Repository/IAutorRepository` | `Core/Ports/Saida/IAutorRepositorioPort` |
| Implementação com Entity Framework fora do domínio | `Infrastructure/Repositories/AutorRepository` | `Adapters/Persistencia/EntityFramework/AutorRepositorioEF` |
| `AutorService` depende de | a abstração (`IAutorRepository`) | a abstração (`IAutorRepositorioPort`) |
| Projeto onde vive o `AutorService` | `Service.csproj` — 0 pacotes, referencia só `Core` | `Application.csproj` — 0 pacotes, referencia só `Core` |
| `Autor.cs` alterado | não | não |
| Testes existentes passando | 15 (6 + 9) | 23 (14 + 9) |
| Linhas alteradas em `AutorServiceTests` | 1 (a construção do serviço) | 1 (a construção do serviço) |
| `Assert` alterados | 0 | 0 |

Duas observações que valem por si:

- **A inversão é completa nas duas.** Em ambas, o projeto que hospeda o `AutorService` referencia exclusivamente `Core`: injetar um `DbContext` num serviço deixaria de compilar nas duas entregas.
- **O custo de adaptação nos testes foi idêntico.** Nas duas, `AutorServiceTests` mudou uma única linha — `new AutorService(new AutorRepository(context))` lá, `new AutorService(new AutorRepositorioEF(context))` aqui — e nenhum `Assert` foi tocado.

### 7.2 O que efetivamente mudou

**1. O alcance da adaptação — e aqui a Clean foi mais longe.**

A Clean inverteu **quatro agregados**: `Core/Repository/` declara `IAutorRepository`, `IEditoraRepository`, `ILivroRepository` e `IItemAcervoRepository`, com as quatro implementações correspondentes em `Infrastructure/Repositories/`. O resultado é que na pasta da Clean, procurar `BibliotecaContext` dentro de `Service/` não devolve nenhuma ocorrência: o projeto de serviços inteiro ficou livre do Entity Framework.

Esta entrega inverteu **um agregado**. Foi decisão de escopo, declarada na seção 3 — *fatia vertical*: `Editora`, `Livro` e `ItemAcervo` continuam no projeto `Service` legado, recebendo `BibliotecaContext` por injeção. Em quantidade de código coberto, a Clean vai mais longe.

| | Clean | Hexagonal |
|---|---|---|
| Agregados invertidos | 4 (`Autor`, `Editora`, `Livro`, `ItemAcervo`) | 1 (`Autor`) |
| Serviços que ainda recebem `DbContext` | 0 | 3 (`Editora`, `Livro`, `ItemAcervo`) |

**2. A largura do contrato.**

`IAutorRepository` declara **11 métodos**; `IAutorRepositorioPort`, **7**. A diferença não é de estilo: quatro dos onze — `GetAllOrderByNome`, `GetCountAutores`, `GetByName` e `GetOrderByDescending` — **não têm nenhum chamador no projeto**. Procurar por cada um deles na pasta da Clean devolve exatamente duas linhas: a declaração na interface e a implementação no repositório. São métodos que já existiam na classe `AutorService` original e foram levados junto na migração.

A porta desta entrega declara só os 7 métodos que `IAutorService` expõe. **A porta descreve a necessidade do domínio; um contrato de 11 métodos descreve as capacidades do banco.** É a mesma regra de dependência com contratos de larguras diferentes.

**3. A forma das assinaturas.**

Oito dos onze métodos de `IAutorRepository` são assíncronos (`Task<>`); `Create`, `Edit` e `Delete` continuaram síncronos. Como `IAutorService` permaneceu **síncrona e idêntica à original** nas duas entregas, a Clean resolve essa diferença dentro do próprio serviço:

```csharp
// Clean — Service/AutorService.cs
public Autor? Get(uint id) => _autorRepository.Get(id).Result;
public IEnumerable<Autor> GetAll() => _autorRepository.GetAll().Result;
```

`IAutorRepositorioPort` manteve as 7 assinaturas síncronas, iguais às das queries originais, e o hexágono chama a porta direto, sem intermediação.

**4. Quantos adaptadores a porta aceita — e o que isso permite testar.**

| | Clean | Hexagonal |
|---|---|---|
| Implementações do contrato | 1 — `AutorRepository` (EF) | 2 — `AutorRepositorioEF` (EF) e `AutorRepositorioEmMemoria` (`List<Autor>`, zero EF) |
| Testes da regra de negócio (ano < 1000) | 0 | 2 |
| Testes que rodam sem Entity Framework | 0 | 8 |

Este é o ponto onde as duas entregas mais divergem. O segundo adaptador não é enfeite: é ele que torna possível `ServiceTests/AutorServiceSemBancoTests.cs`, onde o `AutorService` é construído sobre uma `List<Autor>` e a regra de negócio — a única do agregado — é finalmente exercitada por teste. **No projeto original essa regra não tinha nenhum teste**, justamente porque exercitá-la exigia levantar um `DbContext`. Aqui o isolamento do domínio deixou de ser afirmação de documentação e virou teste que roda; na Clean ele continua igualmente verdadeiro, mas segue demonstrável apenas por leitura do código.

### 7.3 O que só mudou de nome

Boa parte do que parece diferença entre as duas é vocabulário:

| Clean | Hexagonal | O que muda de fato |
|---|---|---|
| `Infrastructure` | `Adapters` | nada — mesmo papel, mesma posição no grafo de dependências |
| `Repository` | porta de saída (*driven port*) | nada — mesmo conceito, tradições diferentes |
| `Core/Service/IAutorService` | `Core/Ports/Entrada/IAutorService` | nada no contrato: **as 7 assinaturas são as mesmas, na mesma ordem**; muda o papel declarado |
| círculos concêntricos | hexágono | a metáfora do desenho |
| `Core` como camada mais interna | `Core` como interior do hexágono | a figura, não a seta |

E o que é literalmente idêntico nas duas:

- `Core/Autor.cs` — nenhuma linha alterada em nenhuma das duas entregas.
- A regra de negócio: a mesma comparação `autor.DataNascimento.Year < 1000` lançando `ServiceException`, com a mesma mensagem.
- A seta que a atividade cobra: `implementação → contrato`, com o contrato dentro do domínio.
- Os 9 testes de `BibliotecaWebTests`, intocados nas duas.

### 7.4 Conclusão

**As duas arquiteturas satisfazem a regra de dependência de formas equivalentes** — o domínio parou de depender do banco nas duas, pelo mesmo mecanismo, e a troca de nomes entre `Infrastructure` e `Adapters`, ou entre *repository* e *porta*, não muda nenhuma seta.

As diferenças reais são três, e nenhuma delas é sobre qual arquitetura é melhor:

1. **Alcance** — a Clean inverteu quatro agregados; esta entrega, um, por escolha de escopo.
2. **Largura do contrato** — 11 métodos (4 sem chamador) contra 7: a diferença entre um contrato que descreve o banco e um que descreve a necessidade do domínio.
3. **Demonstrabilidade** — dois adaptadores na mesma porta transformam o isolamento em teste executável; com um adaptador só, ele permanece correto, mas verificável apenas por leitura.

Em uma frase: **a Clean do grupo aplicou a regra em mais lugares; esta entrega aplicou em um lugar só e a tornou verificável por teste.**

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

### Resultado

Medido em 13/09/2026, nesta pasta, com .NET SDK 8.0.425. As saídas literais estão em [`TESTES.md`](TESTES.md).

| Verificação | Antes | Depois |
|---|---|---|
| Compilação da solução | êxito | **êxito, 0 erros** |
| `ServiceTests` | 6 aprovados, 0 falhas | **14 aprovados, 0 falhas** (6 originais + 8 novos) |
| `BibliotecaWebTests` | 9 aprovados, 0 falhas | **9 aprovados, 0 falhas** |
| Testes da regra de negócio sem EF | 0 (não existiam) | **2** |
| Testes que rodam sem EF | 0 | **8** |
| EF em `Core/` + `Application/` | 7 ocorrências em `Core/` | **nenhuma — saída vazia** |
| Pacotes NuGet em `Core.csproj` | 4 | **0** |
| Referências de projeto de `Core` | — | **nenhuma** |
| Referências de projeto de `Application` | — | **uma: `Core`** |
| `Assert` alterados em `AutorServiceTests` | — | **0** |

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

As connection strings `BibliotecaDatabase` e `IdentityDatabase` ficam nos `appsettings.json` de cada host.

---

## 10. Como esta entrega se relaciona com a do grupo

As duas arquiteturas ficam lado a lado na branch `main`, **uma pasta para cada** — que é o que o slide EXTRA pede ao aceitar *"uma branch ou pasta própria"*. Nenhuma das duas depende da outra para compilar ou rodar os testes.

| | Entrega principal | Entrega extra |
|---|---|---|
| Arquitetura | Clean Architecture | Hexagonal (Ports & Adapters) |
| Pasta | `projeto-es-bibliote-2025-clean/` | `projeto-es-biblioteca-2025-hexagonal/` |
| Papel | **entrega principal** (1,0 ponto da unidade) | **extra** (pontuação registrada à parte) |
| Agregado | Autor | Autor — o mesmo, como o slide EXTRA pede |

Como as duas pastas estão no mesmo lugar, os comandos da seção 8 podem ser rodados numa e noutra sem trocar de branch — é assim que os números do comparativo da seção 7 foram levantados.

---

## Documentos

- [`TESTES.md`](TESTES.md) — saídas literais do `dotnet test`, da verificação de pureza e do sentido das dependências, antes e depois da adaptação