# Registro de Testes — Clean e Hexagonal

Evidência do critério *"Testes existentes passando"* e da inversão de dependência, nas duas entregas. Pastas: `projeto-es-bibliote-2025-clean/Biblioteca/` e `projeto-es-biblioteca-2025-hexagonal/Codigo2025/Biblioteca/` (branch `main`) · .NET SDK 8.0.425

---

## 1. ANTES — linha de base

Medição feita sobre a cópia **intocada** do BibliotecaES, antes de qualquer alteração arquitetural. Este é o número que precisa ser reproduzido ao final.

### 1.1 `ServiceTests` — os testes do agregado Autor

```
$ dotnet test ServiceTests/ServiceTests.csproj --nologo

Execução de teste para ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:     6, Ignorado:     0, Total:     6, Duração: 1 s - ServiceTests.dll (net8.0)
```

Os 6 testes de `ServiceTests/AutorServiceTests.cs`:

| # | Teste | O que exercita |
|---|---|---|
| 1 | `CreateTest` | inserção e leitura de volta |
| 2 | `DeleteTest` | remoção |
| 3 | `EditTest` | atualização |
| 4 | `GetTest` | busca por id |
| 5 | `GetAllTest` | listagem completa |
| 6 | `GetByNomeTest` | busca por prefixo de nome (retorna `AutorDto`) |

### 1.2 `BibliotecaWebTests` — testes dos controllers

```
$ dotnet test BibliotecaWebTests/BibliotecaWebTests.csproj --nologo

Execução de teste para ...\BibliotecaWebTests\bin\Debug\net8.0\BibliotecaWebTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:     9, Ignorado:     0, Total:     9, Duração: 383 ms - BibliotecaWebTests.dll (net8.0)
```

Arquivos: `Controllers/AutorControllerTests.cs` e `Controllers/EditoraControllerTests.cs`.

### 1.3 Total

| Projeto | Aprovados | Com falha | Total |
|---|---|---|---|
| `ServiceTests` | 6 | 0 | 6 |
| `BibliotecaWebTests` | 9 | 0 | 9 |
| **TOTAL** | **15** | **0** | **15** |

---

## 2. ANTES — o domínio depende do banco

Verificação de pureza rodada **antes** da adaptação. A saída **não** é vazia, e é exatamente esse o problema que a atividade pede para resolver.

```
$ grep -rn "EntityFrameworkCore\|MySql" Core/ --include=*.cs --include=*.csproj

Core/BibliotecaContext.cs:1:using Microsoft.EntityFrameworkCore;
Core/Core.csproj:10:    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.23" />
Core/Core.csproj:11:    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.23">
Core/Core.csproj:15:    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.23">
Core/Core.csproj:19:    <PackageReference Include="MySql.EntityFrameworkCore" Version="8.0.20" />
Core/Identity/Data/IdentityContext.cs:2:using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
Core/Identity/Data/IdentityContext.cs:3:using Microsoft.EntityFrameworkCore;
```

**7 ocorrências dentro do projeto de domínio**, sendo 4 pacotes NuGet declarados no `Core.csproj`.

### Sintoma nos testes

`ServiceTests/AutorServiceTests.cs` precisa montar um `DbContext` para exercitar a regra de negócio, que é uma única comparação de inteiros:

```csharp
var builder = new DbContextOptionsBuilder<BibliotecaContext>();
builder.UseInMemoryDatabase("Biblioteca");
context = new BibliotecaContext(builder.Options);
...
autorService = new AutorService(context);     // o serviço só existe se houver um DbContext
```

E a regra em si, em `Service/AutorService.cs`:

```csharp
if (autor.DataNascimento.Year < 1000)
    throw new ServiceException("O ano de nascimento de autor deve ser maior do que 1000. ...");
```

**No projeto original, essa regra não tem nenhum teste.** Nenhum dos 6 testes verifica a exceção.

---

## 3. DEPOIS — a entrega Hexagonal

Medido em **13/09/2026**, nesta pasta, com a integração concluída: `AutorService` vive no projeto `Application`, consome `IAutorRepositorioPort` e recebe `AutorRepositorioEF` pelo composition root. As saídas abaixo são literais, com dois cortes de formatação: os caminhos absolutos aparecem encurtados como `...` e as linhas de restauração e de aviso do compilador (`CS8602`/`CS8618`, herdados do projeto original) foram omitidas.

### 3.0 Compilação da solução

```
$ dotnet build Biblioteca.sln --nologo

Compilação com êxito.
    0 Erro(s)
```

### 3.1 Testes existentes (regressão)

```
$ dotnet test ServiceTests/ServiceTests.csproj --nologo

  Core -> ...\Core\bin\Debug\net8.0\Core.dll
  Application -> ...\Application\bin\Debug\net8.0\Application.dll
  Adapters -> ...\Adapters\bin\Debug\net8.0\Adapters.dll
  Service -> ...\Service\bin\Debug\net8.0\Service.dll
  ServiceTests -> ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll
Execução de teste para ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:    14, Ignorado:     0, Total:    14, Duração: 2 s - ServiceTests.dll (net8.0)
```

```
$ dotnet test BibliotecaWebTests/BibliotecaWebTests.csproj --nologo

  Adapters -> ...\Adapters\bin\Debug\net8.0\Adapters.dll
  Service -> ...\Service\bin\Debug\net8.0\Service.dll
  BibliotecaWeb -> ...\BibliotecaWeb\bin\Debug\net8.0\BibliotecaWeb.dll
  BibliotecaWebTests -> ...\BibliotecaWebTests\bin\Debug\net8.0\BibliotecaWebTests.dll
Execução de teste para ...\BibliotecaWebTests\bin\Debug\net8.0\BibliotecaWebTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:     9, Ignorado:     0, Total:     9, Duração: 646 ms - BibliotecaWebTests.dll (net8.0)
```

Os **14** de `ServiceTests` são os 6 originais de `AutorServiceTests.cs` mais os 8 novos de `AutorServiceSemBancoTests.cs`. Os 9 de `BibliotecaWebTests` são exatamente os mesmos da linha de base — nenhum arquivo desse projeto foi tocado.

**O que mudou nos 6 testes originais:** uma linha, a que constrói o serviço.

```diff
- autorService = new AutorService(context);
+ autorService = new AutorService(new AutorRepositorioEF(context));
```

Nenhum `Assert` foi alterado. O `DbContextOptionsBuilder` com `UseInMemoryDatabase` continua ali de propósito: é o teste de regressão do caminho real, com Entity Framework.

### 3.2 Testes novos — a regra de negócio sem banco

```
$ dotnet test ServiceTests/ServiceTests.csproj --nologo --filter "FullyQualifiedName~AutorServiceSemBancoTests"

  ServiceTests -> ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll
Execução de teste para ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:     8, Ignorado:     0, Total:     8, Duração: 131 ms - ServiceTests.dll (net8.0)
```

Os 8 testes de `ServiceTests/AutorServiceSemBancoTests.cs`:

| # | Teste | O que exercita |
|---|---|---|
| 1 | `CreateTest` | inserção e leitura de volta |
| 2 | `DeleteTest` | remoção |
| 3 | `EditTest` | atualização |
| 4 | `GetTest` | busca por id |
| 5 | `GetAllTest` | listagem completa |
| 6 | `GetByNomeTest` | busca por prefixo de nome (retorna `AutorDto`) |
| 7 | **`CreateComAnoDeNascimentoAnteriorA1000LancaServiceException`** | **regra de negócio na criação** |
| 8 | **`EditComAnoDeNascimentoAnteriorA1000LancaServiceException`** | **regra de negócio na edição** |

Os seis primeiros repetem os cenários dos testes originais — mas contra o outro adaptador da mesma porta. Os dois últimos são os que **não existiam no projeto**: a regra do ano de nascimento nunca tinha sido exercitada por teste nenhum.

O arquivo inteiro não tem uma linha de Entity Framework. O `Arrange` é uma lista:

```csharp
using Application.Tests.Fakes;
using Core;
using Core.Ports.Entrada;
using Core.Ports.Saida;
using Core.Service;

// Nenhum using de Entity Framework, nenhum DbContext, nenhum banco em memória do EF.

repositorio  = new AutorRepositorioEmMemoria();   // adaptador sobre List<Autor>
autorService = new AutorService(repositorio);     // o hexágono, ligado na porta
```

E a regra, finalmente sob teste:

```csharp
[TestMethod()]
public void CreateComAnoDeNascimentoAnteriorA1000LancaServiceException()
{
    var autor = new Autor() { Id = 5, Nome = "Autor Invalido", DataNascimento = new DateTime(999, 1, 1) };
    Assert.ThrowsException<ServiceException>(() => autorService.Create(autor));
    // o autor nao pode ter sido persistido
    Assert.AreEqual(3, autorService.GetAll().Count());
}
```

**É esta a prova de isolamento da entrega.** A seção 2 mostrou que, antes, testar uma comparação de inteiros exigia levantar um `DbContext`. Agora não exige mais — e o `AutorRepositorioEmMemoria` só é possível porque o contrato de persistência é uma interface do domínio.

### 3.3 Verificação de pureza — o domínio não conhece o banco

```
$ grep -rn "EntityFrameworkCore\|MySql" Core/ Application/ --include=*.cs --include=*.csproj
$
```

**Saída vazia.** Contra as **7 ocorrências** registradas na seção 2, todas dentro do `Core`. O `Core.csproj` também ficou sem nenhum pacote:

```
$ grep -c PackageReference Core/Core.csproj
0
```

Os quatro pacotes (`MySql.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`) não sumiram: foram para o `Adapters.csproj`, que é onde a tecnologia deve ficar. O `BibliotecaContext` mudou de projeto sem mudar de conteúdo — o `OnModelCreating` é o mesmo.

### 3.4 Sentido das dependências

```
$ dotnet list Core/Core.csproj reference

Não há nenhuma referência Projeto para Projeto no projeto Core/Core.csproj.
```

```
$ dotnet list Application/Application.csproj reference

Referências de projeto
----------------------
..\Core\Core.csproj
```

```
$ dotnet list Adapters/Adapters.csproj reference

Referências de projeto
----------------------
..\Core\Core.csproj
```

Lidas juntas, as três saídas são a regra de dependência da atividade:

- `Core` não referencia **nada** — é o interior do hexágono, e não tem para onde apontar.
- `Application` (o serviço, com a regra de negócio) referencia **só** `Core`. Não tem como injetar um `DbContext` aqui: não compilaria.
- `Adapters` (Entity Framework + MySQL) referencia `Core`. **A seta vai do concreto para o abstrato**, e não o contrário.

---

## 4. DEPOIS — a entrega Clean

Mesma verificação, agora na pasta `projeto-es-bibliote-2025-clean/Biblioteca/`. A linha de base é a mesma das seções 1 e 2: as duas entregas partiram da mesma cópia do BibliotecaES, com os mesmos 15 testes e as mesmas 7 ocorrências de Entity Framework dentro do `Core`.

### 4.1 Compilação e testes

```
$ dotnet build Biblioteca.sln --nologo

Compilação com êxito.
    0 Erro(s)
```

```
$ dotnet test ServiceTests/ServiceTests.csproj --nologo

  Core -> ...\Core\bin\Debug\net8.0\Core.dll
  Service -> ...\Service\bin\Debug\net8.0\Service.dll
  Infrastructure -> ...\Infrastructure\bin\Debug\net8.0\Infrastructure.dll
  ServiceTests -> ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll
Execução de teste para ...\ServiceTests\bin\Debug\net8.0\ServiceTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:     6, Ignorado:     0, Total:     6, Duração: 3 s - ServiceTests.dll (net8.0)
```

```
$ dotnet test BibliotecaWebTests/BibliotecaWebTests.csproj --nologo

  Util -> ...\Util\bin\Debug\net8.0\Util.dll
  Service -> ...\Service\bin\Debug\net8.0\Service.dll
  Infrastructure -> ...\Infrastructure\bin\Debug\net8.0\Infrastructure.dll
  BibliotecaWeb -> ...\BibliotecaWeb\bin\Debug\net8.0\BibliotecaWeb.dll
  BibliotecaWebTests -> ...\BibliotecaWebTests\bin\Debug\net8.0\BibliotecaWebTests.dll
Execução de teste para ...\BibliotecaWebTests\bin\Debug\net8.0\BibliotecaWebTests.dll (.NETCoreApp,Version=v8.0)
1 arquivos de teste no total corresponderam ao padrão especificado.

Aprovado!  – Com falha:     0, Aprovado:     9, Ignorado:     0, Total:     9, Duração: 2 s - BibliotecaWebTests.dll (net8.0)
```

São os **15 testes da linha de base, todos verdes** — o critério de "testes existentes passando" está cumprido também aqui. E o custo de adaptação nos testes foi o mesmo da entrega Hexagonal: o `AutorServiceTests.cs` mudou **uma linha**, a que constrói o serviço, mais as linhas de `using`. Nenhum `Assert` foi tocado.

```diff
- autorService = new AutorService(context);
+ autorService = new AutorService(new AutorRepository(context));
```

### 4.2 Verificação de pureza — o domínio não conhece o banco

```
$ grep -rn "EntityFrameworkCore\|MySql" Core/ Service/ --include=*.cs --include=*.csproj
$
```

**Saída vazia**, contra as 7 ocorrências registradas na seção 2. A verificação aqui cobre `Core/` **e** `Service/`, porque na Clean o serviço vive num projeto separado do domínio — e nenhum dos dois conhece o Entity Framework. O `Core.csproj` também ficou sem pacote nenhum:

```
$ grep -c PackageReference Core/Core.csproj
0
```

Os quatro pacotes de EF e MySQL foram para o `Infrastructure.csproj`, junto com o `BibliotecaContext` (em `Infrastructure/Data/`), o Identity e os repositórios.

### 4.3 Sentido das dependências

```
$ dotnet list Core/Core.csproj reference

Não há nenhuma referência Projeto para Projeto no projeto Core/Core.csproj.
```

```
$ dotnet list Service/Service.csproj reference

Referências de projeto
----------------------
..\Core\Core.csproj
```

```
$ dotnet list Infrastructure/Infrastructure.csproj reference

Referências de projeto
----------------------
..\Core\Core.csproj
```

As três saídas juntas são a regra de dependência da Clean: `Core` não aponta para ninguém; `Service` (o serviço, com a regra de negócio) e `Infrastructure` (Entity Framework + MySQL) apontam os dois para `Core`, e não um para o outro. Injetar um `DbContext` num serviço não compilaria, porque `Service` não enxerga `Infrastructure`.

### 4.4 Cobertura da regra de negócio

A regra do agregado — `DataNascimento.Year < 1000` lançando `ServiceException` — continua **sem teste** na Clean: os 6 testes de `AutorServiceTests.cs` são os originais, e todos montam um `DbContextOptionsBuilder` com `UseInMemoryDatabase`. Nenhum teste da pasta roda sem Entity Framework.

Isso não é falha da arquitetura, e sim consequência de existir **um único adaptador** do contrato de persistência: sem uma segunda implementação de `IAutorRepository`, testar o serviço exige o repositório que fala com o EF. É exatamente o ponto comparado na seção 7 do [`README.md`](README.md).

---

## 5. Quadro comparativo — antes × Clean × Hexagonal

| Verificação | Antes | Clean | Hexagonal |
|---|---|---|---|
| `ServiceTests` | 6 aprovados, 0 falhas | **6 aprovados, 0 falhas** | **14 aprovados, 0 falhas** |
| `BibliotecaWebTests` | 9 aprovados, 0 falhas | **9 aprovados, 0 falhas** | **9 aprovados, 0 falhas** |
| Total de testes verdes | 15 | **15** | **23** |
| Testes da regra de negócio sem EF | 0 (não existiam) | **0** | **2** |
| Testes que rodam sem nenhum EF | 0 | **0** | **8** |
| Ocorrências de EF no projeto de domínio | 7 | **0** | **0** |
| Pacotes NuGet em `Core.csproj` | 4 | **0** | **0** |
| Referências de projeto de `Core` | — | **nenhuma** | **nenhuma** |
| Projeto que hospeda o `AutorService` | `Service`, com o `DbContext` injetado | `Service` — referencia só `Core` | `Application` — referencia só `Core` |
| Implementações do contrato de persistência | 1 (`BibliotecaContext` direto no serviço) | **1** (`AutorRepository`) | **2** (`AutorRepositorioEF`, `AutorRepositorioEmMemoria`) |
| `Assert` alterados em `AutorServiceTests` | — | **0** | **0** |
| Linhas alteradas em `AutorServiceTests` | — | **1** (a construção do serviço) | **1** (a construção do serviço) |
| Entidades alteradas | — | **0** | **0** |

As duas entregas cumprem a regra de dependência e mantêm os 15 testes originais verdes. A diferença mensurável está nas três linhas do meio: quantos adaptadores o contrato aceita e, por consequência, quanto do isolamento é demonstrável por teste.
