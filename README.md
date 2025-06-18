# 🎫 Ticket2Help

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![WPF](https://img.shields.io/badge/UI-WPF-purple.svg)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red.svg)](https://www.microsoft.com/en-us/sql-server)

## 📋 Sobre o Projeto

**Ticket2Help** é um sistema completo de gestão de tickets de helpdesk desenvolvido em **WPF (.NET Framework 4.8)** com arquitetura em camadas. O sistema permite a criação, gestão e resolução de tickets de suporte técnico, oferecendo uma interface intuitiva e funcionalidades robustas para equipas de TI.

### ✨ Características Principais

- 🎯 **Sistema de Tickets Completo** - Criação, atribuição, acompanhamento e resolução
- 👥 **Gestão de Utilizadores** - Colaboradores, Técnicos e Administradores
- 📊 **Relatórios e Estatísticas** - Dashboard com métricas de desempenho
- ♿ **Totalmente Acessível** - Compatível com leitores de tela e navegação por teclado
- 🔒 **Segurança Robusta** - Autenticação e autorização
- 🎨 **Interface Moderna** - Design responsivo e intuitivo

## 🏗️ Arquitetura

O projeto segue uma **arquitetura em camadas** bem definida:

```
Ticket2Help/
├── 📁 Ticket2Help.UI/          # Interface do Utilizador (WPF)
├── 📁 Ticket2Help.BLL/         # Business Logic Layer
├── 📁 Ticket2Help.DAL/         # Data Access Layer
│   ├── Models/                 # Modelos de dados
```

### 🔧 Tecnologias Utilizadas

- **Frontend**: WPF (.NET Framework 4.8)
- **Backend**: C# com arquitetura em camadas
- **Base de Dados**: SQL Server / SQL Server Express
- **ORM**: ADO.NET (repositórios personalizados)
- **Documentação**: XML Documentation

## 🚀 Início Rápido

### 📋 Pré-requisitos

- **Visual Studio 2019+** ou **Visual Studio Code**
- **.NET Framework 4.8** ou superior
- **SQL Server** ou **SQL Server Express**
- **Windows 10+** (para WPF)

### 💾 Instalação

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/Fryxion/Ticket2Help
   cd ticket2help
   ```

2. **Configure a base de dados:**
   ```sql
   -- Execute os scripts SQL na Solução Ticket2Help.DAL
   -- 1. database.sql
   ```

3. **Configure a string de conexão:**
   ```xml
   <!-- No appsettings.json -> Ticket2Help.UI -->
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost\\SQLEXPRESS;Initial Catalog=Ticket2HelpDb;Persist Security Info=True;User ID=sa;Password=123;Trust Server Certificate=True"
      },
      "Application": {
        "Name": "Ticket2Help",
        "Version": "1.0.0",
        "DefaultAttendanceStrategy": "FIFO",
        "CreateDefaultUsers": true
      }
    }
 
    <!-- No appsettings.json -> Ticket2Help.DAL -> DatabaseContext.cs -->
    _connectionString -> colocar igual ao DefaultConnection de cima

   ```

4. **Compile e execute:**
   ```bash
   # Via Visual Studio: F5
    registar um utilizador e colocar como administrador na base de dados
   ```

## 📚 Funcionalidades

### 🎫 Gestão de Tickets

- ✅ **Criação de Tickets** - Interface intuitiva para reportar problemas
- ✅ **Categorização** - Hardware, Software, Rede, etc.
- ✅ **Prioridades** - Baixa, Normal, Alta, Crítica
- ✅ **Estados** - Por Atender, Em Progresso, Resolvido, Fechado
- ✅ **Atribuição** - Automática ou manual para técnicos
- ✅ **Comentários** - Sistema de comunicação entre utilizadores e técnicos
- ✅ **Histórico** - Registo completo de todas as alterações

### 👥 Gestão de Utilizadores

- ✅ **Tipos de Utilizador:**
  - **Colaborador** - Pode criar e acompanhar tickets
  - **Técnico** - Pode atender e resolver tickets
  - **Administrador** - Acesso total ao sistema
- ✅ **Perfis Personalizáveis** - Nome, email, departamento
- ✅ **Auditoria** - Registo de ações dos utilizadores

### 📊 Relatórios e Estatísticas

- ✅ **Dashboard Principal** - Visão geral do sistema
- ✅ **Relatórios Predefinidos:**
  - Resumo Geral
  - Tickets por Técnico
  - Estatísticas por Período
  - Desempenho do Sistema
  - Relatórios por Categoria (Hardware/Software)
- ✅ **Filtros Avançados** - Por data, estado, técnico, categoria
- ✅ **Gráficos Interativos** - Visualização de dados em tempo real

### ♿ Acessibilidade

- ✅ **Navegação por Teclado** - Tab, atalhos personalizados
- ✅ **Leitores de Tela** - Compatível com NVDA, JAWS
- ✅ **Tooltips Informativos** - Ajuda contextual
- ✅ **Tamanhos de Fonte** - Escaláveis para melhor legibilidade

### 🔒 Segurança

O sistema implementa várias camadas de segurança:

- **Autorização** - Controlo de acesso baseado em roles
- **Validação** - Sanitização de inputs do utilizador
- **Sessões** - Gestão segura de sessões de utilizador

### 🗄️ Base de Dados

#### Estrutura Principal:

```sql
-- Tabelas Principais
Users           -- Utilizadores do sistema
Tickets         -- Tickets de suporte
Categories      -- Categorias de tickets
Comments        -- Comentários dos tickets
Attachments     -- Anexos dos tickets
```

## 🔌 Integração e API

### ServiceLocator

O sistema utiliza um ServiceLocator simples para injeção de dependências:

```csharp
// Configuração inicial
ServiceLocator.RegisterService<IUserService>(userService);
ServiceLocator.RegisterService<ITicketService>(ticketService);

// Uso
var userService = ServiceLocator.GetService<IUserService>();
```

### Extensões Personalizadas

Para adicionar funcionalidades personalizadas:

```csharp
// Implementar interface
public class CustomTicketService : ITicketService
{
    // Sua implementação personalizada
}

// Registar no sistema
ServiceLocator.RegisterService<ITicketService>(new CustomTicketService());
```

## 🧪 Testes

O projeto inclui testes unitários abrangentes:

```bash
# Executar todos os testes
dotnet test

# Executar testes específicos
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Gerar relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### 📝 Padrões de Código:

- **Nomenclatura**: PascalCase para classes, camelCase para variáveis
- **Comentários**: XML Documentation para métodos públicos
- **Testes**: Mínimo 80% de cobertura para novas funcionalidades
- **Commits**: Mensagens descritivas em português
