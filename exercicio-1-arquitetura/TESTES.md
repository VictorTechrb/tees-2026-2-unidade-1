# Registro de Testes — Arquitetura Hexagonal

Evidência do critério *"Testes existentes passando"* e da inversão de dependência.
Pasta: `projeto-es-biblioteca-2025-hexagonal/Codigo2025/Biblioteca/` · Branch: `Hexagonal` · .NET SDK 8.0.425

---

## 1. ANTES — linha de base

Medição feita sobre a cópia **intocada** do BibliotecaES, antes de qualquer alteração
arquitetural. Este é o número que precisa ser reproduzido ao final.

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

Verificação de pureza rodada **antes** da adaptação. A saída **não** é vazia, e é exatamente
esse o problema que a atividade pede para resolver.

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

`ServiceTests/AutorServiceTests.cs` precisa montar um `DbContext` para exercitar a regra de
negócio, que é uma única comparação de inteiros:

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

**Essa regra não tem nenhum teste hoje.** Nenhum dos 6 testes verifica a exceção.

---

## 3. DEPOIS — a preencher ⏳

> Preencher ao concluir a fase de integração, quando o `AutorService` já estiver
> consumindo a porta e o adaptador com Entity Framework estiver ligado.

### 3.1 Testes existentes (regressão)

```
⏳ colar aqui a saída literal de:
   dotnet test ServiceTests/ServiceTests.csproj --nologo
   dotnet test BibliotecaWebTests/BibliotecaWebTests.csproj --nologo
```

### 3.2 Testes novos — regra de negócio sem banco

```
⏳ colar aqui a saída de AutorServiceSemBancoTests
```

### 3.3 Verificação de pureza

```
⏳ $ grep -rn "EntityFrameworkCore\|MySql" Core/ Application/ --include=*.cs --include=*.csproj
   (esperado: saída vazia)
```

### 3.4 Sentido das dependências

```
⏳ $ dotnet list Core/Core.csproj reference
⏳ $ dotnet list Application/Application.csproj reference
   (esperado: Core sem nenhuma referência; Application referenciando apenas Core)
```

---

## 4. Quadro comparativo ⏳

| Verificação | Antes | Depois |
|---|---|---|
| `ServiceTests` | 6 aprovados, 0 falhas | ⏳ |
| `BibliotecaWebTests` | 9 aprovados, 0 falhas | ⏳ |
| Testes da regra de negócio sem EF | 0 (não existiam) | ⏳ |
| Ocorrências de EF em `Core/` | 7 | ⏳ (esperado: 0) |
| Pacotes NuGet em `Core.csproj` | 4 | ⏳ (esperado: 0) |
| `Assert` alterados em `AutorServiceTests` | — | ⏳ (esperado: 0) |
