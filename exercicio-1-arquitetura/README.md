## Adaptação Arquitetural — BibliotecaES

Sistema de gestão de uma biblioteca acadêmica.
O objetivo deste trabalho consiste em aplicar a regra de dependência de Clean, Hexagonal ou Onion sobre um sistema que já existe, o projeto usado para esse trabalho consiste no BibliotecaES, projeto referente a disciplina de Engenharia de Software I e II. 

O projeto original pode ser encontrado no repositório oficial da disciplina.

---

## 1. Documentação da Arquitetura Atual

### Estilo Externo (Encontro 01)

O sistema de biblioteca se caracteriza como um **Monolito não modular** já que o mesmo possui 2 artefatos, sendo eles: **BibliotecaAPI** e **BibliotecaWeb**, e ambos compartilham  o mesmo banco de dados.
  

### Estilo Interno (Encontro 02)
Na estrutura original, o sistema utiliza uma **Arquitetura em Camadas Clássica**, seguindo o fluxo onde o *Controller* chama o *Service*, e o *Service* chama o *Core*, que já importa diretamente o Entity Framework.

  
### Onde a regra de dependência não se aplica?
A regra de dependência é violada no escopo de `Autor`, onde o `AutorService.cs` injeta diretamente o `BibliotecaContext`. Dessa forma, a camada do Service possui uma dependência a uma tecnologia externa, fazendo com que o serviço de negócio se comunique diretamente com o Entity Framework para buscar e salvar dados. É por isso que a regra de dependência ainda não se aplica, já que o domínio da aplicação está acoplado ao Entity Framework.

---
## 2. Escolha Arquitetural e Justificativa

O trabalho proposto consistem em inverter essa dependência usando a Clean Architecture. Os principais motivos técnicos para essa escolha são:
- **Terminologia intuitiva**: Utiliza termos e conceitos diretos e de fácil compreensão
- **Material em .NET abundante**: Conta com amplo acervo de documentação, tutoriais e suporte da comunidade no ecossistema .NET
- **Aplicação já em andamento no projeto**: Trata-se da estrutura cuja refatoração já está sendo executada no código da Biblioteca.


## Sumário

- [1. Pré-requisitos](#1-pré-requisitos)
- [2. Como executar](#2-como-executar)
- [3. Estrutura do projeto](#3-estrutura-do-projeto)
- [4. O que observar](#4-o-que-observar)

## 1. Pré-requisitos:
* .NET SDK 8.0 (ou superior)
* Banco de dados MySQL (ou banco em memória configurado para testes)


## 2. Como executar:


## 3. Estrutura do projeto:


## 4. O que observar:

[⬆ Voltar ao topo](#adaptação-arquitetural--bibliotecaes)<br>
