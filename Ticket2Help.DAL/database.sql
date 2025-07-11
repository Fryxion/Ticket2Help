USE [master]
GO
/****** Object:  Database [Ticket2HelpDb]    Script Date: 18/06/2025 07:05:37 ******/
CREATE DATABASE [Ticket2HelpDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Ticket2HelpDb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\Ticket2HelpDb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Ticket2HelpDb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\Ticket2HelpDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Ticket2HelpDb] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Ticket2HelpDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Ticket2HelpDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Ticket2HelpDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Ticket2HelpDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Ticket2HelpDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Ticket2HelpDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Ticket2HelpDb] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [Ticket2HelpDb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Ticket2HelpDb] SET  MULTI_USER 
GO
ALTER DATABASE [Ticket2HelpDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Ticket2HelpDb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Ticket2HelpDb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Ticket2HelpDb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Ticket2HelpDb] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Ticket2HelpDb] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [Ticket2HelpDb] SET QUERY_STORE = ON
GO
ALTER DATABASE [Ticket2HelpDb] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Ticket2HelpDb]
GO
/****** Object:  User [ticket]    Script Date: 18/06/2025 07:05:37 ******/
CREATE USER [ticket] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ticket]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 18/06/2025 07:05:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [nvarchar](50) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](500) NOT NULL,
	[Nome] [nvarchar](200) NOT NULL,
	[Email] [nvarchar](255) NULL,
	[TipoUtilizador] [int] NOT NULL,
	[Ativo] [bit] NOT NULL,
	[DataCriacao] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tickets]    Script Date: 18/06/2025 07:05:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tickets](
	[TicketId] [int] IDENTITY(1,1) NOT NULL,
	[DataCriacao] [datetime2](7) NOT NULL,
	[ColaboradorId] [nvarchar](50) NOT NULL,
	[EstadoTicket] [int] NOT NULL,
	[TipoTicket] [int] NOT NULL,
	[DataAtendimento] [datetime2](7) NULL,
	[EstadoAtendimento] [int] NULL,
	[TecnicoId] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[TicketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HardwareTickets]    Script Date: 18/06/2025 07:05:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HardwareTickets](
	[TicketId] [int] NOT NULL,
	[Equipamento] [nvarchar](500) NULL,
	[Avaria] [nvarchar](1000) NULL,
	[DescricaoReparacao] [nvarchar](2000) NULL,
	[Pecas] [nvarchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[TicketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SoftwareTickets]    Script Date: 18/06/2025 07:05:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SoftwareTickets](
	[TicketId] [int] NOT NULL,
	[Software] [nvarchar](500) NULL,
	[DescricaoNecessidade] [nvarchar](2000) NULL,
	[DescricaoIntervencao] [nvarchar](2000) NULL,
PRIMARY KEY CLUSTERED 
(
	[TicketId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_TicketsCompletos]    Script Date: 18/06/2025 07:05:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_TicketsCompletos] AS
SELECT 
    t.TicketId,
    t.DataCriacao,
    t.EstadoTicket,
    t.TipoTicket,
    t.DataAtendimento,
    t.EstadoAtendimento,
    
    -- Informações do colaborador
    c.Nome AS ColaboradorNome,
    c.Email AS ColaboradorEmail,
    
    -- Informações do técnico
    tec.Nome AS TecnicoNome,
    tec.Email AS TecnicoEmail,
    
    -- Campos específicos de Hardware
    ht.Equipamento,
    ht.Avaria,
    ht.DescricaoReparacao,
    ht.Pecas,
    
    -- Campos específicos de Software
    st.Software,
    st.DescricaoNecessidade,
    st.DescricaoIntervencao,
    
    -- Tempo de atendimento em minutos
    CASE 
        WHEN t.DataAtendimento IS NOT NULL 
        THEN DATEDIFF(MINUTE, t.DataCriacao, t.DataAtendimento)
        ELSE NULL 
    END AS TempoAtendimentoMinutos

FROM Tickets t
INNER JOIN Users c ON t.ColaboradorId = c.UserId
LEFT JOIN Users tec ON t.TecnicoId = tec.UserId
LEFT JOIN HardwareTickets ht ON t.TicketId = ht.TicketId
LEFT JOIN SoftwareTickets st ON t.TicketId = st.TicketId;
GO
ALTER TABLE [dbo].[Tickets] ADD  DEFAULT (getdate()) FOR [DataCriacao]
GO
ALTER TABLE [dbo].[Tickets] ADD  DEFAULT ((0)) FOR [EstadoTicket]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [Ativo]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [DataCriacao]
GO
ALTER TABLE [dbo].[HardwareTickets]  WITH CHECK ADD  CONSTRAINT [FK_HardwareTickets_Ticket] FOREIGN KEY([TicketId])
REFERENCES [dbo].[Tickets] ([TicketId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[HardwareTickets] CHECK CONSTRAINT [FK_HardwareTickets_Ticket]
GO
ALTER TABLE [dbo].[SoftwareTickets]  WITH CHECK ADD  CONSTRAINT [FK_SoftwareTickets_Ticket] FOREIGN KEY([TicketId])
REFERENCES [dbo].[Tickets] ([TicketId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SoftwareTickets] CHECK CONSTRAINT [FK_SoftwareTickets_Ticket]
GO
ALTER TABLE [dbo].[Tickets]  WITH CHECK ADD  CONSTRAINT [FK_Tickets_Colaborador] FOREIGN KEY([ColaboradorId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Tickets] CHECK CONSTRAINT [FK_Tickets_Colaborador]
GO
ALTER TABLE [dbo].[Tickets]  WITH CHECK ADD  CONSTRAINT [FK_Tickets_Tecnico] FOREIGN KEY([TecnicoId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Tickets] CHECK CONSTRAINT [FK_Tickets_Tecnico]
GO
ALTER TABLE [dbo].[Tickets]  WITH CHECK ADD  CONSTRAINT [CK_Tickets_EstadoAtendimento] CHECK  (([EstadoAtendimento] IS NULL OR ([EstadoAtendimento]=(2) OR [EstadoAtendimento]=(1) OR [EstadoAtendimento]=(0))))
GO
ALTER TABLE [dbo].[Tickets] CHECK CONSTRAINT [CK_Tickets_EstadoAtendimento]
GO
ALTER TABLE [dbo].[Tickets]  WITH CHECK ADD  CONSTRAINT [CK_Tickets_EstadoTicket] CHECK  (([EstadoTicket]=(2) OR [EstadoTicket]=(1) OR [EstadoTicket]=(0)))
GO
ALTER TABLE [dbo].[Tickets] CHECK CONSTRAINT [CK_Tickets_EstadoTicket]
GO
ALTER TABLE [dbo].[Tickets]  WITH CHECK ADD  CONSTRAINT [CK_Tickets_TipoTicket] CHECK  (([TipoTicket]=(1) OR [TipoTicket]=(0)))
GO
ALTER TABLE [dbo].[Tickets] CHECK CONSTRAINT [CK_Tickets_TipoTicket]
GO
USE [master]
GO
ALTER DATABASE [Ticket2HelpDb] SET  READ_WRITE 
GO
