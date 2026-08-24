-- ============================================================================
-- EImece Database Creation Script
-- ============================================================================
-- Database: eimece
-- Purpose : Creates the `eimece` database and full schema (tables, indexes,
--           constraints, functions, stored procedures, etc.) from scratch.
--           Run this script FIRST on a fresh SQL Server / LocalDB instance.
--
-- Usage   : 1) Open SQL Server Management Studio (SSMS) or sqlcmd.
--           2) Connect to your local instance (e.g. YUCE\SQLEXPRESS or
--              (LocalDB)\MSSQLLocalDB).
--           3) Open and execute this file. It will:
--              - Create database [eimece] (or reuse if it already exists)
--              - Create all tables, keys, indexes and programmability objects
--           4) Then run App_Data\02_SeedAdminUsers.sql to create admin logins.
--           5) Update your connection string:
--                Data Source=YOUR_SERVER;Initial Catalog=eimece;...;
--              Example (existing dev box):
--                Data Source=YUCE\SQLEXPRESS;Initial Catalog=eimece;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;
--              Or for LocalDB / Integrated Security:
--                Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=eimece;Integrated Security=True;
--           6) Run the app (IIS http://localhost:81/ or IIS Express) and log in:
--                Admin    : http://localhost:81/account/adminlogin/  (admin@eimece.test / Admin123!)
--                Customer : http://localhost:81/account/login/
--
-- Notes   : - This script was generated from SSMS (Script Date 2026-08-24) and
--             renamed to `eimece` for local development (all occurrences updated).
--           - File paths for MDF/LDF use the default SQL Server DATA folder.
--             If that folder does not exist, SQL Server will create the files
--             in the instance default location (remove FILENAME clause if needed).
--           - The script is idempotent where possible: CREATE DATABASE is guarded
--             by IF DB_ID() IS NULL, and the sqluser DB user is created only if
--             the login exists and the user does not already exist.
--           - Original SSMS script used UTF-16LE; this version is saved as UTF-8.
--           - Tested against SQL Server 2019/2022 and SQLEXPRESS / LocalDB.
--
-- Order   : 01_CreateDatabase.sql (this file) -> 02_SeedAdminUsers.sql
-- ============================================================================

USE [master]
GO
-- ----------------------------------------------------------------------------
-- Create database [eimece] if it does not already exist.
-- The FILENAME paths are defaults for a default SQLEXPRESS install; if your
-- instance uses a different DATA folder, omit the FILENAME clauses or edit
-- them to match your instance's default (SELECT SERVERPROPERTY('InstanceDefaultDataPath')).
-- ----------------------------------------------------------------------------
IF DB_ID(N'eimece') IS NULL
BEGIN
    PRINT N'Creating database [eimece]...';
    CREATE DATABASE [eimece]
     CONTAINMENT = NONE
     ON  PRIMARY 
    ( NAME = N'eimece', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\eimece.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
     LOG ON 
    ( NAME = N'eimece_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\eimece_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
     WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF;
    PRINT N'Database [eimece] created.';
END
ELSE
    PRINT N'Database [eimece] already exists - skipping CREATE DATABASE.';
GO
ALTER DATABASE [eimece] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [eimece].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [eimece] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [eimece] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [eimece] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [eimece] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [eimece] SET ARITHABORT OFF 
GO
ALTER DATABASE [eimece] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [eimece] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [eimece] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [eimece] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [eimece] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [eimece] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [eimece] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [eimece] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [eimece] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [eimece] SET  ENABLE_BROKER 
GO
ALTER DATABASE [eimece] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [eimece] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [eimece] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [eimece] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [eimece] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [eimece] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [eimece] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [eimece] SET RECOVERY FULL 
GO
ALTER DATABASE [eimece] SET  MULTI_USER 
GO
ALTER DATABASE [eimece] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [eimece] SET DB_CHAINING OFF 
GO
ALTER DATABASE [eimece] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [eimece] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [eimece] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [eimece] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [eimece] SET QUERY_STORE = OFF
GO
USE [eimece]
GO
-- ----------------------------------------------------------------------------
-- Ensure DB user [sqluser] exists for login [sqluser] (used by legacy
-- connection string: Data Source=YUCE\SQLEXPRESS;Initial Catalog=eimece;User ID=sqluser;...).
-- If the server login [sqluser] does not exist, this is skipped - create the
-- login manually or use Integrated Security / another login.
-- ----------------------------------------------------------------------------
IF SUSER_ID(N'sqluser') IS NOT NULL AND DATABASE_PRINCIPAL_ID(N'sqluser') IS NULL
BEGIN
    PRINT N'Creating user [sqluser] for login [sqluser]...';
    CREATE USER [sqluser] FOR LOGIN [sqluser] WITH DEFAULT_SCHEMA=[dbo];
    PRINT N'User [sqluser] created.';
END
ELSE IF DATABASE_PRINCIPAL_ID(N'sqluser') IS NOT NULL
    PRINT N'User [sqluser] already exists - skipping.';
ELSE
    PRINT N'Login [sqluser] does not exist on this server - skipping CREATE USER [sqluser]. Create the login first if you need SQL auth.';
GO
/****** Object:  Schema [ewsiste]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE SCHEMA [ewsiste]
GO
/****** Object:  UserDefinedTableType [dbo].[ei_tpt_Filter]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE TYPE [dbo].[ei_tpt_Filter] AS TABLE(
	[FieldName] [nvarchar](max) NULL,
	[ValueFirst] [nvarchar](max) NULL,
	[ValueLast] [nvarchar](max) NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[GetRandomNumber]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetRandomNumber](@lowerLimit BIGINT, @upperLimit BIGINT, @GuidValue UNIQUEIDENTIFIER)
RETURNS BIGINT
AS
BEGIN
    RETURN
    (
    SELECT ABS(CAST(CAST(@GuidValue AS VARBINARY(8)) AS BIGINT)) % (@upperLimit-@lowerLimit)+@lowerLimit
    )
END
GO
/****** Object:  UserDefinedFunction [dbo].[ProductRating]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ProductRating](@ProductId INT)
RETURNS float
AS
BEGIN 
	declare @rating float;
SELECT
  @rating =   ISNULL( CAST(AVG(Cast(Rating as float)) AS DECIMAL(10,2)),0)
FROM
   [dbo].[ProductComments] where ProductId=@ProductId and IsActive = 1;

   
   RETURN @rating;
END 
 -- select  dbo.ProductRating(8888)

  
GO
/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Addresses]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Addresses](
	[Id] [int] IDENTITY(2000,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[Street] [nvarchar](500) NULL,
	[District] [nvarchar](500) NULL,
	[ZipCode] [nvarchar](500) NULL,
	[City] [nvarchar](500) NOT NULL,
	[Country] [nvarchar](500) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[AddressType] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NOT NULL,
 CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AppLogs]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventDateTime] [nvarchar](max) NULL,
	[EventLevel] [nvarchar](max) NULL,
	[UserName] [nvarchar](max) NULL,
	[MachineName] [nvarchar](max) NULL,
	[EventMessage] [nvarchar](max) NULL,
	[ErrorSource] [nvarchar](max) NULL,
	[ErrorClass] [nvarchar](max) NULL,
	[ErrorMethod] [nvarchar](max) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[InnerErrorMessage] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_AppLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[FirstName] [nvarchar](256) NULL,
	[LastName] [nvarchar](256) NULL,
	[TwoFactorAuthenticatorEnabled] [bit] NOT NULL,
	[AuthenticatorKey] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Brands]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Brands](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[MainPage] [bit] NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageState] [bit] NULL,
	[MainImageId] [int] NULL,
	[Lang] [int] NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Brands] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Coupons]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coupons](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Code] [nvarchar](255) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[DiscountPercentage] [int] NOT NULL,
	[Discount] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NOT NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Coupon] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_Coupons] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NULL,
	[CustomerType] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Surname] [nvarchar](50) NULL,
	[Company] [nvarchar](500) NULL,
	[Email] [nvarchar](100) NULL,
	[GsmNumber] [nvarchar](100) NULL,
	[IdentityNumber] [nvarchar](100) NULL,
	[Ip] [nvarchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[Position] [int] NULL,
	[Lang] [int] NULL,
	[IsPermissionGranted] [bit] NULL,
	[Gender] [int] NULL,
	[City] [nvarchar](500) NULL,
	[Town] [nvarchar](500) NULL,
	[District] [nvarchar](500) NULL,
	[Street] [nvarchar](500) NULL,
	[ZipCode] [nvarchar](500) NULL,
	[Country] [nvarchar](500) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[db_error_LearningErrorLog]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[db_error_LearningErrorLog](
	[ErrorID] [bigint] IDENTITY(1,1) NOT NULL,
	[ErrorNumber] [nvarchar](50) NOT NULL,
	[ErrorDescription] [nvarchar](4000) NULL,
	[ErrorProcedure] [nvarchar](100) NULL,
	[ErrorState] [int] NULL,
	[ErrorSeverity] [int] NULL,
	[ErrorLine] [int] NULL,
	[ErrorTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ErrorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Faqs]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Faqs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NOT NULL,
	[Question] [nvarchar](max) NULL,
	[Answer] [nvarchar](max) NULL,
	[AddUserId] [nvarchar](100) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Faqs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FileStorages]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileStorages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[FileName] [nvarchar](500) NOT NULL,
	[FileUrl] [nvarchar](500) NULL,
	[MimeType] [nvarchar](50) NOT NULL,
	[FileSize] [int] NOT NULL,
	[Width] [int] NULL,
	[Height] [int] NULL,
	[Type] [nvarchar](50) NULL,
	[Lang] [int] NOT NULL,
	[IsFileExist] [bit] NULL,
 CONSTRAINT [PK_FileStorages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FileStorageTags]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileStorageTags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileStorageId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
 CONSTRAINT [PK_FileStorageTags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ListItems]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ListItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ListId] [int] NULL,
	[Name] [nvarchar](500) NULL,
	[Value] [nvarchar](500) NULL,
	[Position] [int] NULL,
	[IsActive] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_ListItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Lists]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lists](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[IsService] [bit] NULL,
	[IsValues] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[Position] [int] NULL,
	[IsActive] [bit] NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_Lists] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MailTemplates]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailTemplates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Position] [int] NULL,
	[Lang] [int] NULL,
	[Subject] [nvarchar](500) NULL,
	[Body] [nvarchar](max) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
	[TrackWithBitly] [bit] NULL,
	[TrackWithMlnk] [bit] NULL,
 CONSTRAINT [PK_MailTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MainPageImages]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MainPageImages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageState] [bit] NULL,
	[Link] [nvarchar](500) NULL,
	[MainImageId] [int] NULL,
	[Lang] [int] NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_MainPageImages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MenuFiles]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MenuFiles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MenuId] [int] NULL,
	[FileStorageId] [int] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_MenuFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Menus]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Menus](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageState] [bit] NULL,
	[MainPage] [bit] NULL,
	[LinkIsActive] [bit] NULL,
	[Link] [nvarchar](500) NULL,
	[MainImageId] [int] NULL,
	[MenuLink] [nvarchar](500) NOT NULL,
	[PageTheme] [nvarchar](50) NULL,
	[Lang] [int] NOT NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[AddUserId] [nvarchar](100) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Menus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderProducts]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderProducts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[TotalPrice] [money] NOT NULL,
	[ProductSpecItems] [nvarchar](4000) NULL,
	[ProductSalePrice] [money] NULL,
	[ProductName] [nvarchar](500) NULL,
	[ProductCode] [nvarchar](500) NULL,
	[CategoryName] [nvarchar](500) NULL,
 CONSTRAINT [PK_Order_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderType] [int] NULL,
	[BillingAddressId] [int] NULL,
	[ShippingAddressId] [int] NULL,
	[OrderNumber] [nvarchar](50) NULL,
	[CargoPrice] [money] NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[OrderGuid] [nvarchar](100) NULL,
	[Name] [nvarchar](500) NOT NULL,
	[OrderComments] [nvarchar](4000) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[OrderStatus] [int] NULL,
	[AdminOrderNote] [nvarchar](4000) NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NULL,
	[DeliveryDate] [datetime] NOT NULL,
	[CouponDiscount] [nvarchar](100) NULL,
	[Coupon] [nvarchar](100) NULL,
	[Token] [nvarchar](100) NULL,
	[Price] [nvarchar](100) NULL,
	[PaidPrice] [nvarchar](100) NULL,
	[Installment] [nvarchar](100) NULL,
	[Currency] [nvarchar](50) NULL,
	[PaymentId] [nvarchar](100) NULL,
	[PaymentStatus] [nvarchar](100) NULL,
	[FraudStatus] [int] NULL,
	[MerchantCommissionRate] [nvarchar](100) NULL,
	[MerchantCommissionRateAmount] [nvarchar](100) NULL,
	[IyziCommissionRateAmount] [nvarchar](100) NULL,
	[IyziCommissionFee] [nvarchar](100) NULL,
	[CardType] [nvarchar](100) NULL,
	[CardAssociation] [nvarchar](100) NULL,
	[CardFamily] [nvarchar](100) NULL,
	[CardToken] [nvarchar](200) NULL,
	[CardUserKey] [nvarchar](100) NULL,
	[BinNumber] [nvarchar](100) NULL,
	[LastFourDigits] [nvarchar](100) NULL,
	[BasketId] [nvarchar](100) NULL,
	[ConversationId] [nvarchar](100) NULL,
	[ConnectorName] [nvarchar](100) NULL,
	[AuthCode] [nvarchar](100) NULL,
	[HostReference] [nvarchar](100) NULL,
	[Phase] [nvarchar](100) NULL,
	[Status] [nvarchar](100) NULL,
	[ErrorCode] [nvarchar](100) NULL,
	[ErrorMessage] [nvarchar](500) NULL,
	[Locale] [nvarchar](100) NULL,
	[SystemTime] [bigint] NULL,
	[ShipmentTrackingNumber] [nvarchar](200) NULL,
	[ShipmentCompanyName] [nvarchar](200) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductCategories]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[ShortDescription] [nvarchar](4000) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageState] [bit] NULL,
	[MainPage] [bit] NULL,
	[MainImageId] [int] NULL,
	[Lang] [int] NOT NULL,
	[TemplateId] [int] NULL,
	[DiscountPercantage] [float] NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductComments]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductComments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](50) NULL,
	[Subject] [nvarchar](50) NULL,
	[Review] [nvarchar](4000) NOT NULL,
	[Rating] [int] NULL,
	[UserId] [nvarchar](128) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NOT NULL,
 CONSTRAINT [PK_ProductComments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductFiles]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductFiles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NULL,
	[FileStorageId] [int] NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_ProductFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BrandId] [int] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[NameLong] [nvarchar](1000) NULL,
	[NameShort] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ShortDescription] [nvarchar](4000) NULL,
	[MainPage] [bit] NOT NULL,
	[ImageState] [bit] NOT NULL,
	[MainImageId] [int] NULL,
	[ProductCategoryId] [int] NOT NULL,
	[Price] [money] NULL,
	[Discount] [money] NULL,
	[ProductCode] [nvarchar](255) NULL,
	[Lang] [int] NOT NULL,
	[VideoUrl] [nvarchar](1000) NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[IsCampaign] [bit] NULL,
	[AddUserId] [nvarchar](100) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[Rating]  AS ([dbo].[ProductRating]([Id])),
	[ProductColorOptions] [nvarchar](1000) NULL,
	[ProductSizeOptions] [nvarchar](1000) NULL,
	[State] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductsOlive]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductsOlive](
	[stockCode] [nvarchar](50) NULL,
	[label] [nvarchar](255) NULL,
	[status] [int] NULL,
	[brand] [nvarchar](100) NULL,
	[brandId] [int] NULL,
	[barcode] [nvarchar](50) NULL,
	[mainCategory] [nvarchar](255) NULL,
	[category] [nvarchar](255) NULL,
	[subCategory] [nvarchar](255) NULL,
	[buyingPrice] [decimal](18, 3) NULL,
	[price1] [decimal](18, 3) NULL,
	[price2] [decimal](18, 3) NULL,
	[price3] [decimal](18, 3) NULL,
	[price4] [decimal](18, 3) NULL,
	[price5] [decimal](18, 3) NULL,
	[tax] [int] NULL,
	[currencyAbbr] [nvarchar](10) NULL,
	[stockAmount] [int] NULL,
	[stockType] [nvarchar](50) NULL,
	[warranty] [int] NULL,
	[picture1Path] [nvarchar](500) NULL,
	[picture2Path] [nvarchar](500) NULL,
	[picture3Path] [nvarchar](500) NULL,
	[picture4Path] [nvarchar](500) NULL,
	[dm3] [decimal](18, 4) NULL,
	[details] [nvarchar](max) NULL,
	[rebate] [decimal](18, 5) NULL,
	[rebateType] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductSpecifications]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductSpecifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NULL,
	[Name] [nvarchar](1000) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Value] [nvarchar](1000) NOT NULL,
	[Unit] [nvarchar](1000) NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_ProductSpecifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductTags]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TagId] [int] NULL,
	[ProductId] [int] NULL,
 CONSTRAINT [PK_ProductTags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[SettingKey] [nvarchar](255) NOT NULL,
	[SettingValue] [nvarchar](4000) NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShoppingCarts]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCarts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderGuid] [nvarchar](100) NULL,
	[UserId] [nvarchar](150) NULL,
	[ShoppingCartJson] [nvarchar](max) NULL,
	[Name] [nvarchar](100) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Position] [int] NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_ShoppingCarts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShortUrls]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShortUrls](
	[Id] [int] IDENTITY(100,1) NOT NULL,
	[Name] [nvarchar](2000) NULL,
	[UrlKey] [nvarchar](100) NULL,
	[Url] [nvarchar](2000) NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[RequestCount] [int] NULL,
	[IsActive] [bit] NULL,
	[Position] [int] NULL,
	[Lang] [nvarchar](10) NULL,
 CONSTRAINT [PK_ShortUrls] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stories]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StoryCategoryId] [int] NOT NULL,
	[AuthorName] [nvarchar](500) NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[MainPage] [bit] NOT NULL,
	[IsFeaturedStory] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ShortDescription] [nvarchar](2000) NULL,
	[ImageState] [bit] NULL,
	[MainImageId] [int] NULL,
	[Lang] [int] NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Stories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoryCategories]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoryCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageState] [bit] NOT NULL,
	[MainImageId] [int] NULL,
	[Lang] [int] NOT NULL,
	[PageTheme] [nvarchar](50) NULL,
	[MetaKeywords] [nvarchar](1000) NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_StoryCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoryFiles]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoryFiles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StoryId] [int] NULL,
	[FileStorageId] [int] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NULL,
 CONSTRAINT [PK_StoryFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoryTags]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoryTags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StoryId] [int] NULL,
	[TagId] [int] NULL,
 CONSTRAINT [PK_StoryTags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Subscribers]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subscribers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Email] [nvarchar](500) NULL,
	[Lang] [int] NULL,
	[Note] [nvarchar](max) NULL,
 CONSTRAINT [PK_Subscribers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TagCategories]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TagCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NOT NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TagCategoryId] [int] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[Lang] [int] NOT NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Templates]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Templates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Position] [int] NOT NULL,
	[TemplateXml] [nvarchar](max) NULL,
	[Lang] [int] NOT NULL,
	[UpdateUserId] [nvarchar](100) NULL,
	[AddUserId] [nvarchar](100) NULL,
 CONSTRAINT [PK_Templates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TwoFactorTokens]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TwoFactorTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[Token] [nvarchar](128) NOT NULL,
	[ExpiresUtc] [datetime2](7) NOT NULL,
	[IsUsed] [bit] NOT NULL,
 CONSTRAINT [PK_TwoFactorTokens] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Brands_IsActive_Lang_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Brands_IsActive_Lang_Position] ON [dbo].[Brands]
(
	[IsActive] ASC,
	[Lang] ASC,
	[Position] ASC
)
INCLUDE([Id],[Name],[MainPage],[Description],[ImageState],[MainImageId],[MetaKeywords],[UpdateUserId],[AddUserId],[CreatedDate],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Brands_Lang_IsActive_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Brands_Lang_IsActive_Position] ON [dbo].[Brands]
(
	[Lang] ASC,
	[IsActive] ASC,
	[Position] ASC
)
INCLUDE([Name],[MainImageId],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FileStorages_Id_Covering]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_FileStorages_Id_Covering] ON [dbo].[FileStorages]
(
	[Id] ASC
)
INCLUDE([FileName],[FileUrl],[MimeType],[FileSize],[Width],[Height],[Type],[IsFileExist],[Name],[CreatedDate],[UpdatedDate],[IsActive],[Position],[Lang]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MainPageImages_Lang_IsActive_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_MainPageImages_Lang_IsActive_Position] ON [dbo].[MainPageImages]
(
	[Lang] ASC,
	[IsActive] ASC,
	[Position] ASC
)
INCLUDE([Id],[Name],[Link],[Description],[ImageState],[MetaKeywords],[MainImageId],[UpdateUserId],[AddUserId],[CreatedDate],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MenuFiles_MenuId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_MenuFiles_MenuId] ON [dbo].[MenuFiles]
(
	[MenuId] ASC
)
INCLUDE([FileStorageId],[Name],[IsActive],[Position],[Lang]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Menus_Lang_IsActive_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Menus_Lang_IsActive_Position] ON [dbo].[Menus]
(
	[Lang] ASC,
	[IsActive] ASC,
	[Position] ASC
)
INCLUDE([Id],[ParentId],[Name],[MainPage],[MenuLink],[Link],[PageTheme],[LinkIsActive],[Description],[ImageState],[MetaKeywords],[MainImageId],[UpdateUserId],[AddUserId],[CreatedDate],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderProducts_OrderId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderProducts_OrderId] ON [dbo].[OrderProducts]
(
	[OrderId] ASC
)
INCLUDE([ProductId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderProducts_ProductId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderProducts_ProductId] ON [dbo].[OrderProducts]
(
	[ProductId] ASC
)
INCLUDE([OrderId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_CreatedDate_UserId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_CreatedDate_UserId] ON [dbo].[Orders]
(
	[CreatedDate] ASC,
	[UserId] ASC
)
INCLUDE([Id],[OrderType],[OrderStatus],[PaymentStatus],[FraudStatus],[PaidPrice],[Price],[Currency],[Coupon],[Installment],[ConnectorName],[CardType],[CardAssociation],[CardFamily],[BinNumber],[ErrorCode],[ErrorMessage],[Locale],[Phase],[Status],[Lang]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_FraudStatus_CreatedDate]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_FraudStatus_CreatedDate] ON [dbo].[Orders]
(
	[FraudStatus] ASC,
	[CreatedDate] ASC
)
INCLUDE([Id],[PaidPrice],[CardType],[CardAssociation],[BinNumber]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_OrderGuid]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_OrderGuid] ON [dbo].[Orders]
(
	[OrderGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_OrderNumber]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Orders_OrderNumber] ON [dbo].[Orders]
(
	[OrderNumber] ASC
)
WHERE ([OrderNumber] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_PaymentStatus_CreatedDate]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_PaymentStatus_CreatedDate] ON [dbo].[Orders]
(
	[PaymentStatus] ASC,
	[CreatedDate] ASC
)
INCLUDE([Id],[PaidPrice],[ConnectorName],[CardType]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_UserId_CreatedDate]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_UserId_CreatedDate] ON [dbo].[Orders]
(
	[UserId] ASC,
	[CreatedDate] ASC
)
INCLUDE([PaidPrice],[Price],[OrderStatus],[PaymentStatus]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_UserId_UpdatedDate]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_UserId_UpdatedDate] ON [dbo].[Orders]
(
	[UserId] ASC,
	[UpdatedDate] DESC
)
INCLUDE([OrderNumber],[OrderGuid],[OrderStatus],[PaidPrice],[PaymentStatus]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductCategories_IsActive_Lang_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductCategories_IsActive_Lang_Position] ON [dbo].[ProductCategories]
(
	[IsActive] ASC,
	[Lang] ASC,
	[Position] ASC
)
INCLUDE([Id],[ParentId],[Name],[MainPage],[ShortDescription],[TemplateId],[DiscountPercantage],[Description],[ImageState],[MetaKeywords],[MainImageId],[UpdateUserId],[AddUserId],[CreatedDate],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductCategories_Lang_IsActive_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductCategories_Lang_IsActive_Position] ON [dbo].[ProductCategories]
(
	[Lang] ASC,
	[IsActive] ASC,
	[Position] ASC
)
INCLUDE([Name],[ParentId],[MainPage],[MainImageId],[TemplateId],[DiscountPercantage]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductCategories_MainPage_IsActive_Lang]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductCategories_MainPage_IsActive_Lang] ON [dbo].[ProductCategories]
(
	[MainPage] ASC,
	[IsActive] ASC,
	[Lang] ASC
)
INCLUDE([Name],[ParentId],[Position],[MainImageId],[TemplateId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductCategories_ParentId_IsActive_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductCategories_ParentId_IsActive_Position] ON [dbo].[ProductCategories]
(
	[ParentId] ASC,
	[IsActive] ASC,
	[Position] ASC
)
INCLUDE([Name],[Lang],[MainPage],[MainImageId],[TemplateId],[DiscountPercantage],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductComments_ProductId_IsActive]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductComments_ProductId_IsActive] ON [dbo].[ProductComments]
(
	[ProductId] ASC,
	[IsActive] ASC
)
INCLUDE([Rating]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductComments_ProductId_Lang_IsActive]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductComments_ProductId_Lang_IsActive] ON [dbo].[ProductComments]
(
	[ProductId] ASC,
	[Lang] ASC,
	[IsActive] ASC
)
INCLUDE([Rating],[CreatedDate],[Subject],[Email],[UserId],[Position]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductFiles_FileStorageId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductFiles_FileStorageId] ON [dbo].[ProductFiles]
(
	[FileStorageId] ASC
)
INCLUDE([ProductId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductFiles_ProductId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductFiles_ProductId] ON [dbo].[ProductFiles]
(
	[ProductId] ASC
)
INCLUDE([FileStorageId],[Position],[IsActive]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_BrandId_Lang]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_BrandId_Lang] ON [dbo].[Products]
(
	[BrandId] ASC,
	[Lang] ASC
)
INCLUDE([Name],[IsActive],[ProductCategoryId],[Price],[Position],[MainImageId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_IsActive_Lang_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_IsActive_Lang_Position] ON [dbo].[Products]
(
	[IsActive] ASC,
	[Lang] ASC,
	[Position] DESC
)
INCLUDE([Name],[NameShort],[NameLong],[Price],[Discount],[ProductCode],[ProductCategoryId],[BrandId],[MainImageId],[MainPage],[IsCampaign],[State],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_IsActive_MainPage_Lang_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_IsActive_MainPage_Lang_Position] ON [dbo].[Products]
(
	[IsActive] ASC,
	[MainPage] ASC,
	[Lang] ASC,
	[Position] DESC
)
INCLUDE([Name],[Price],[Discount],[ProductCategoryId],[MainImageId],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_IsCampaign_IsActive_Lang]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_IsCampaign_IsActive_Lang] ON [dbo].[Products]
(
	[IsCampaign] ASC,
	[IsActive] ASC,
	[Lang] ASC
)
INCLUDE([ProductCategoryId],[Name],[Price],[Position],[MainImageId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_MainImageId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_MainImageId] ON [dbo].[Products]
(
	[MainImageId] ASC
)
WHERE ([MainImageId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_ProductCategoryId_IsActive_Lang_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_ProductCategoryId_IsActive_Lang_Position] ON [dbo].[Products]
(
	[ProductCategoryId] ASC,
	[IsActive] ASC,
	[Lang] ASC,
	[Position] DESC
)
INCLUDE([Name],[NameShort],[NameLong],[Price],[Discount],[ProductCode],[BrandId],[MainImageId],[MainPage],[IsCampaign],[State],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_ProductCategoryId_Lang]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_ProductCategoryId_Lang] ON [dbo].[Products]
(
	[ProductCategoryId] ASC,
	[Lang] ASC
)
INCLUDE([Name],[IsActive],[BrandId],[Price],[Position],[UpdatedDate],[State],[MainPage],[IsCampaign],[MainImageId],[ProductCode]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Products_ProductCode]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_ProductCode] ON [dbo].[Products]
(
	[ProductCode] ASC
)
INCLUDE([Name],[Lang],[IsActive],[ProductCategoryId],[BrandId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Products_State_IsActive_Lang]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_State_IsActive_Lang] ON [dbo].[Products]
(
	[State] ASC,
	[IsActive] ASC,
	[Lang] ASC
)
INCLUDE([ProductCategoryId],[Name],[Price],[BrandId],[Position]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductSpecifications_ProductId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductSpecifications_ProductId] ON [dbo].[ProductSpecifications]
(
	[ProductId] ASC
)
INCLUDE([Name],[Value],[Unit],[Position],[IsActive]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductTags_ProductId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductTags_ProductId] ON [dbo].[ProductTags]
(
	[ProductId] ASC
)
INCLUDE([Id],[TagId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductTags_ProductId_TagId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductTags_ProductId_TagId] ON [dbo].[ProductTags]
(
	[ProductId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProductTags_TagId_ProductId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_ProductTags_TagId_ProductId] ON [dbo].[ProductTags]
(
	[TagId] ASC,
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Settings_SettingKey]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Settings_SettingKey] ON [dbo].[Settings]
(
	[SettingKey] ASC
)
INCLUDE([Id],[Name],[SettingValue],[Description],[IsActive],[Position],[Lang],[CreatedDate],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Stories_IsActive_IsFeaturedStory_Lang_Position]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Stories_IsActive_IsFeaturedStory_Lang_Position] ON [dbo].[Stories]
(
	[IsActive] ASC,
	[IsFeaturedStory] ASC,
	[Lang] ASC,
	[Position] ASC
)
INCLUDE([Id],[StoryCategoryId],[AuthorName],[Name],[MainPage],[ShortDescription],[Description],[ImageState],[MetaKeywords],[MainImageId],[UpdateUserId],[AddUserId],[CreatedDate],[UpdatedDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_StoryFiles_StoryId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_StoryFiles_StoryId] ON [dbo].[StoryFiles]
(
	[StoryId] ASC
)
INCLUDE([FileStorageId],[Name],[IsActive],[Position],[Lang]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_StoryTags_StoryId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_StoryTags_StoryId] ON [dbo].[StoryTags]
(
	[StoryId] ASC
)
INCLUDE([Id],[TagId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Tags_Id_Covering]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Tags_Id_Covering] ON [dbo].[Tags]
(
	[Id] ASC
)
INCLUDE([TagCategoryId],[Name],[CreatedDate],[UpdatedDate],[IsActive],[Position],[Lang]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_TwoFactorTokens_UserId]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_TwoFactorTokens_UserId] ON [dbo].[TwoFactorTokens]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_TwoFactorTokens_Token]    Script Date: 8/24/2026 12:02:50 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_TwoFactorTokens_Token] ON [dbo].[TwoFactorTokens]
(
	[Token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AppLogs] ADD  CONSTRAINT [DF_AppLogs_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  CONSTRAINT [DF_AspNetUsers_TwoFactorAuthenticatorEnabled]  DEFAULT ((0)) FOR [TwoFactorAuthenticatorEnabled]
GO
ALTER TABLE [dbo].[FileStorages] ADD  CONSTRAINT [DF_FileStorages_IsFileExist]  DEFAULT ((1)) FOR [IsFileExist]
GO
ALTER TABLE [dbo].[ListItems] ADD  CONSTRAINT [DF_ListItems_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ListItems] ADD  CONSTRAINT [DF_ListItems_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Menus] ADD  CONSTRAINT [DF_Menus_ParentId]  DEFAULT ((0)) FOR [ParentId]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Position]  DEFAULT ((0)) FOR [Position]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_ParentId]  DEFAULT ((0)) FOR [ParentId]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_Position]  DEFAULT ((1)) FOR [Position]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_ImageState]  DEFAULT ((0)) FOR [ImageState]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_MainPage]  DEFAULT ((0)) FOR [MainPage]
GO
ALTER TABLE [dbo].[ProductCategories] ADD  CONSTRAINT [DF_ProductCategories_Lang]  DEFAULT ((1)) FOR [Lang]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_Position]  DEFAULT ((1)) FOR [Position]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_MainPage]  DEFAULT ((1)) FOR [MainPage]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ImageState]  DEFAULT ((0)) FOR [ImageState]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ProductCategoryId]  DEFAULT ((0)) FOR [ProductCategoryId]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_Discount]  DEFAULT ((0)) FOR [Discount]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_Lang]  DEFAULT ((1)) FOR [Lang]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsCampaign]  DEFAULT ((0)) FOR [IsCampaign]
GO
ALTER TABLE [dbo].[Stories] ADD  CONSTRAINT [DF_Stories_IsFeaturedStory]  DEFAULT ((1)) FOR [IsFeaturedStory]
GO
ALTER TABLE [dbo].[TwoFactorTokens] ADD  CONSTRAINT [DF_TwoFactorTokens_IsUsed]  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[ListItems]  WITH CHECK ADD  CONSTRAINT [FK_ListItems_Lists] FOREIGN KEY([ListId])
REFERENCES [dbo].[Lists] ([Id])
GO
ALTER TABLE [dbo].[ListItems] CHECK CONSTRAINT [FK_ListItems_Lists]
GO
ALTER TABLE [dbo].[OrderProducts]  WITH CHECK ADD  CONSTRAINT [FK_Order_Products_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[OrderProducts] CHECK CONSTRAINT [FK_Order_Products_Products]
GO
ALTER TABLE [dbo].[OrderProducts]  WITH CHECK ADD  CONSTRAINT [FK_OrderProducts_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
GO
ALTER TABLE [dbo].[OrderProducts] CHECK CONSTRAINT [FK_OrderProducts_Orders]
GO
ALTER TABLE [dbo].[ProductComments]  WITH CHECK ADD  CONSTRAINT [FK_ProductComments_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductComments] CHECK CONSTRAINT [FK_ProductComments_Products]
GO
ALTER TABLE [dbo].[ProductFiles]  WITH CHECK ADD  CONSTRAINT [FK_ProductFiles_FileStorages] FOREIGN KEY([FileStorageId])
REFERENCES [dbo].[FileStorages] ([Id])
GO
ALTER TABLE [dbo].[ProductFiles] CHECK CONSTRAINT [FK_ProductFiles_FileStorages]
GO
ALTER TABLE [dbo].[ProductFiles]  WITH CHECK ADD  CONSTRAINT [FK_ProductFiles_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductFiles] CHECK CONSTRAINT [FK_ProductFiles_Products]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_ProductCategories] FOREIGN KEY([ProductCategoryId])
REFERENCES [dbo].[ProductCategories] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_ProductCategories]
GO
ALTER TABLE [dbo].[ProductSpecifications]  WITH CHECK ADD  CONSTRAINT [FK_ProductSpecifications_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductSpecifications] CHECK CONSTRAINT [FK_ProductSpecifications_Products]
GO
ALTER TABLE [dbo].[ProductTags]  WITH CHECK ADD  CONSTRAINT [FK_ProductTags_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductTags] CHECK CONSTRAINT [FK_ProductTags_Products]
GO
ALTER TABLE [dbo].[Stories]  WITH CHECK ADD  CONSTRAINT [FK_Stories_StoryCategories] FOREIGN KEY([StoryCategoryId])
REFERENCES [dbo].[StoryCategories] ([Id])
GO
ALTER TABLE [dbo].[Stories] CHECK CONSTRAINT [FK_Stories_StoryCategories]
GO
ALTER TABLE [dbo].[TwoFactorTokens]  WITH CHECK ADD  CONSTRAINT [FK_TwoFactorTokens_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TwoFactorTokens] CHECK CONSTRAINT [FK_TwoFactorTokens_AspNetUsers]
GO
/****** Object:  StoredProcedure [dbo].[db_error_Learning_Insert_ErrorLog]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE 
  PROCEDURE [dbo].[db_error_Learning_Insert_ErrorLog]
AS
BEGIN
SET NOCOUNT ON 
        
         INSERT INTO [db_error_LearningErrorLog]  
             (
             ErrorNumber 
            ,ErrorDescription 
            ,ErrorProcedure 
            ,ErrorState 
            ,ErrorSeverity 
            ,ErrorLine 
            ,ErrorTime 
           )
           VALUES
           (
             ERROR_NUMBER()
            ,ERROR_MESSAGE()
            ,ERROR_PROCEDURE()
            ,ERROR_STATE()
            ,ERROR_SEVERITY()
            ,ERROR_LINE()
            ,GETDATE()  
           );
    
SET NOCOUNT OFF    
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DeleteAllData]
	 @test int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
TRUNCATE TABLE ProductTags
TRUNCATE TABLE ProductSpecifications
TRUNCATE TABLE ProductFiles
TRUNCATE TABLE Products
TRUNCATE TABLE ProductCategories



TRUNCATE TABLE FileStorages
TRUNCATE TABLE FileStorageTags
TRUNCATE TABLE ListItems
TRUNCATE TABLE Lists
TRUNCATE TABLE MainPageImages
TRUNCATE TABLE MenuFiles
TRUNCATE TABLE Menus




--TRUNCATE TABLE Settings
TRUNCATE TABLE StoryFiles
TRUNCATE TABLE StoryTags
TRUNCATE TABLE Stories
TRUNCATE TABLE StoryCategories

TRUNCATE TABLE Subscribers
TRUNCATE TABLE TagCategories
TRUNCATE TABLE Tags
--TRUNCATE TABLE MailTemplates

--TRUNCATE TABLE Templates

END

GO
/****** Object:  StoredProcedure [dbo].[DeleteAllOrders]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 

CREATE   PROCEDURE [dbo].[DeleteAllOrders]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Delete all order products
        DELETE FROM [dbo].[OrderProducts];

        -- 2. Delete all orders
        DELETE FROM [dbo].[Orders];

        -- 3. Delete all addresses that are no longer referenced by any order
        DELETE FROM [dbo].[Addresses]
        WHERE Id NOT IN (
            SELECT BillingAddressId FROM [dbo].[Orders] WHERE BillingAddressId IS NOT NULL
            UNION
            SELECT ShippingAddressId FROM [dbo].[Orders] WHERE ShippingAddressId IS NOT NULL
        );

        -- Note: Customers table is intentionally left untouched.

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteOrder]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteOrder]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @OrderId IS NULL OR @OrderId <= 0
    BEGIN
        RAISERROR('Valid @OrderId is required.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @BillingAddressId INT;
        DECLARE @ShippingAddressId INT;

        -- Get the address IDs of this order before deleting it
        SELECT 
            @BillingAddressId = BillingAddressId,
            @ShippingAddressId = ShippingAddressId
        FROM [dbo].[Orders]
        WHERE Id = @OrderId;

        -- 1. Delete related order products
        DELETE FROM [dbo].[OrderProducts]
        WHERE OrderId = @OrderId;

        -- 2. Delete the order
        DELETE FROM [dbo].[Orders]
        WHERE Id = @OrderId;

        -- 3. Delete Billing Address only if it is no longer used by any other order
        IF @BillingAddressId IS NOT NULL
        BEGIN
            IF NOT EXISTS (
                SELECT 1 
                FROM [dbo].[Orders] 
                WHERE BillingAddressId = @BillingAddressId 
                   OR ShippingAddressId = @BillingAddressId
            )
            BEGIN
                DELETE FROM [dbo].[Addresses]
                WHERE Id = @BillingAddressId;
            END
        END

        -- 4. Delete Shipping Address only if it is no longer used by any other order
        --    (and different from the billing address)
        IF @ShippingAddressId IS NOT NULL 
           AND @ShippingAddressId <> ISNULL(@BillingAddressId, -1)
        BEGIN
            IF NOT EXISTS (
                SELECT 1 
                FROM [dbo].[Orders] 
                WHERE BillingAddressId = @ShippingAddressId 
                   OR ShippingAddressId = @ShippingAddressId
            )
            BEGIN
                DELETE FROM [dbo].[Addresses]
                WHERE Id = @ShippingAddressId;
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteOrders]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 

CREATE PROCEDURE [dbo].[DeleteOrders]
    @userId NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    IF @userId IS NULL OR LTRIM(RTRIM(@userId)) = ''
    BEGIN
        RAISERROR('@userId is required.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Delete OrderProducts belonging to this user's orders
        DELETE op
        FROM [dbo].[OrderProducts] op
        INNER JOIN [dbo].[Orders] o ON op.OrderId = o.Id
        WHERE o.UserId = @userId;

        -- 2. Delete the orders of this user
        DELETE FROM [dbo].[Orders]
        WHERE UserId = @userId;

        -- Note:
        -- - Customers table is NOT deleted
        -- - Addresses table is NOT deleted
        --   (Addresses may still be used by other orders or customers)

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteProduct]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DeleteProduct] 
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	  delete from [dbo].[ProductSpecifications] where ProductId =@Id
	    delete from [dbo].[ProductFiles] where ProductId =@Id
		  delete from [dbo].[ProductTags] where ProductId =@Id
		  delete from dbo.ProductComments where ProductId =@Id    
  delete from [dbo].[Products] where Id =@Id

END

GO
/****** Object:  StoredProcedure [dbo].[DeleteProducts]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DeleteProducts] 
 
AS
BEGIN
	
   Declare @Keys Table (Id integer Primary Key Not Null)
   Insert @Keys(Id)
select top 1000 Id  FROM [dbo].[Products] where [ProductCategoryId] IN (
SELECT [ProductCategoryId]
  FROM [dbo].[Products]
  group by [ProductCategoryId]
  )
  order by NEWID()
   -- -------------------------------------------
   Declare @Id Integer
   While Exists (Select * From @Keys)
     Begin
         Select @Id = Max(Id) From @Keys
         EXEC DeleteProduct @Id
         Delete @Keys Where Id = @Id
     End

END
GO
/****** Object:  StoredProcedure [dbo].[ei_sp_SearchProducts]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
/*

  DECLARE @filter as ei_tpt_Filter
 
 -- INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('company','raymarine','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Included Qty x Type','USB adapter - 5 pin Micro-USB Type B ( female ) - Apple Dock connector ( male ) USB cable - 5 pin Micro-USB Type B ( male ) - 4 pin USB Type A ( male )','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Warranty','1 Year','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Part Numbers','CW13632','') 
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Depth','1.5','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast)  VALUES ('Low Voltage Power','20','')
 

exec [dbo].[ei_sp_SearchProducts] @search = '', @top = 100, @skip = 0, @filter= @filter



 
 select * from  dbo.Products where ProductID = 5521
 
 
 select * from   dbo.ProductsSpecValues psv
	    
			WHERE  ProductSpecID=152 AND ValueString= 'USB adapter - 5 pin Micro-USB Type B ( female ) - Apple Dock connector ( male ) USB cable - 5 pin Micro-USB Type B ( male ) - 4 pin USB Type A ( male )'
 

*/
CREATE PROCEDURE [dbo].[ei_sp_SearchProducts]
	@search nvarchar(200)='',
	@top int=20,
	@skip int=0,
	@language int=1,
	@filter AS dbo.ei_tpt_Filter Readonly 
AS
BEGIN

	SET NOCOUNT ON;
	
/*	
IF EXISTS(select name from tempdb..sysobjects  where name like '#recordFound') drop table #recordFound
 */
 
CREATE TABLE #recordFound (ProductId int, ProductCategoryId int, FiltersMatched int)
CREATE INDEX IDX_Product_RecordFound ON #recordFound(ProductId,ProductCategoryId)

 
INSERT INTO #recordFound (ProductId,ProductCategoryId, FiltersMatched)
SELECT Id,[ProductCategoryId], 0
FROM dbo.Products as p
 where 	p.IsActive=1 and p.Lang = @language
 



DECLARE cFilters CURSOR FOR  
SELECT FieldName, ValueFirst,ValueLast
FROM @filter

DECLARE @FieldName VARCHAR(max)
DECLARE @ValueFirst VARCHAR(max)
DECLARE @ValueLast VARCHAR(max)

OPEN cFilters  
FETCH NEXT FROM cFilters INTO @FieldName,  @ValueFirst, @ValueLast 

 
WHILE @@FETCH_STATUS = 0  
BEGIN  

	-- 
    
	
 

	IF @FieldName='Category'
	BEGIN
		UPDATE rf
			SET FiltersMatched=FiltersMatched+1
		FROM 
		 #recordFound rf 
		 INNER JOIN 
		 (
			SELECT DISTINCT   b.Id ProductId
			FROM         Products b INNER JOIN
						 ProductsCategories cb ON b.[ProductCategoryId] = cb.Id
			WHERE cb.Name=@ValueFirst and 	b.IsActive=1
		  ) m
		  ON rf.ProductId = m.ProductId
	END
	
	
 
 
	
	FETCH NEXT FROM cFilters INTO @FieldName,  @ValueFirst, @ValueLast 

END  

CLOSE cFilters  
DEALLOCATE cFilters 

  --select * from  #recordFound where FiltersMatched > 0 

 DELETE FROM #recordFound  WHERE FiltersMatched<(SELECT COUNT(*) FROM @filter)






CREATE TABLE #recordFound2 (ProductID int,  ProductCategoryId int,  rowNumber int)
CREATE INDEX IDX_Product_RecordFound2 ON #recordFound2(ProductId,ProductCategoryId)



  
		IF (LEN (ISNULL(@search,''))>0)  
		BEGIN
				INSERT INTO  #recordFound2 (ProductID ,ProductCategoryId, rowNumber )
				SELECT rf.ProductID , rf.ProductCategoryId, 	ROW_NUMBER() OVER (ORDER BY [RANK] DESC, rf.ProductID) AS rowNumber 
				FROM #recordFound rf INNER JOIN
				(
					SELECT s.ItemId as ProductID, ft.[RANK] as [RANK] 
						FROM  ProductSearch s  
						INNER JOIN 	FREETEXTTABLE (dbo.ProductSearch, *, @search) ft ON s.[SearchId]=ft.[Key]
					WHERE s.ItemType=2 
				) fts
				ON rf.ProductID=fts.ProductID
			
				order by rowNumber
		END
		ELSE
		BEGIN
			INSERT INTO  #recordFound2 (ProductID,ProductCategoryId , rowNumber )
			SELECT c.Id, rf.ProductCategoryId, 	ROW_NUMBER() OVER (ORDER BY c.Name) AS rowNumber 
				FROM #recordFound rf
				INNER join Products c
					ON rf.ProductID=c.Id
					where  c.isActive=1
			order by rowNumber
		END
		 
		 
		 		
		IF EXISTS(select name from tempdb..sysobjects  where name like '#recordFound') drop table #recordFound
      
	  
	  
	    SELECT DISTINCT p.*
			 FROM #recordFound2 rf
				INNER join ProductCategories p ON rf.ProductCategoryId=p.Id
			 WHERE rf.rowNumber BETWEEN @skip+1 AND @skip+@top
		
	  
	    SELECT p.*
			 FROM #recordFound2 rf
				INNER join Products p ON rf.ProductId=p.Id
			 WHERE rf.rowNumber BETWEEN @skip+1 AND @skip+@top
		 
		
		DECLARE @RC INT
		SET @RC=@@RowCount
 
		 
			SELECT * 
			FROM
			(	
				SELECT  'Category' as FieldName,c.Name  as ValueFirst,'' as ValueLast
				,t.cnt,10 ord   
				FROM	
				(  
					SELECT ct.[ProductCategoryId],Count(*) cnt
					FROM        dbo.Products ct
					INNER JOIN #recordFound2 rf on rf.ProductId=ct.Id
					where ct.IsActive=1
					group by ct.[ProductCategoryId]
				) t inner join [dbo].[ProductCategories] c
				ON t.[ProductCategoryId] = c.id 
				where c.IsActive=1
			    --group by c.Category
					
			 
			 
	 		 
			 UNION
  
 
		 
		 SELECT  ct.Name as FieldName,ct.Value as ValueFirst,'' as ValueLast, Count(*) cnt, 60 ord 
					FROM        [dbo].[ProductSpecifications] ct
					INNER JOIN #recordFound2 rf on rf.ProductId=ct.ProductId
					where ct.value <> ''  
				
					group by ct.Name,ct.Value
		--			 	order by ct.Name

	) AS E ORDER BY ord
 


		
		SELECT COUNT(*) RecordsTotal, @skip+1 RecordFirst,  @skip+@top RecordLast, @RC RecordCount
		FROM #recordFound2 
		
		

 	/*
IF EXISTS(select name from tempdb..sysobjects  where name like '#recordFound%') drop table #recordFound
 IF EXISTS(select name from tempdb..sysobjects  where name like '#recordFound2') drop table #recordFound2
*/
 
END

GO
/****** Object:  StoredProcedure [dbo].[GetCargoCostAnalysis]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCargoCostAnalysis]
AS
BEGIN
    SELECT 
        ShipmentCompanyName,
        COUNT(Id) AS OrderCount,
        SUM(CAST(CargoPrice AS DECIMAL(18,2))) AS TotalCargoCost,
        AVG(CAST(CargoPrice AS DECIMAL(18,2))) AS AvgCargoCostPerOrder
    FROM [dbo].[Orders]
    WHERE IsActive = 1
        AND CargoPrice IS NOT NULL
    GROUP BY ShipmentCompanyName
    ORDER BY TotalCargoCost DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetCommentsActivityReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. SP for Comments Activity Report
CREATE PROCEDURE [dbo].[GetCommentsActivityReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @Lang INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DATEADD(DAY, DATEDIFF(DAY, 0, pc.CreatedDate), 0) as CommentDate,
        pc.Lang,
        COUNT(*) as CommentsCount,
        AVG(CAST(pc.Rating AS FLOAT)) as AverageRating,
        SUM(CASE WHEN pc.IsActive = 1 THEN 1 ELSE 0 END) as ActiveComments
    FROM [dbo].[ProductComments] pc
    WHERE 
        (@StartDate IS NULL OR pc.CreatedDate >= @StartDate)
        AND (@EndDate IS NULL OR pc.CreatedDate <= @EndDate)
        AND (@Lang IS NULL OR pc.Lang = @Lang)
    GROUP BY 
        DATEADD(DAY, DATEDIFF(DAY, 0, pc.CreatedDate), 0),
        pc.Lang
    ORDER BY CommentDate DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetCommentsRatingAnalysis]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. SP for Comments Rating Analysis
CREATE PROCEDURE [dbo].[GetCommentsRatingAnalysis]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @ProductId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pc.ProductId,
        pc.Rating,
        COUNT(*) as RatingCount,
        AVG(CAST(pc.Rating AS FLOAT)) as AverageRating,
        COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (PARTITION BY pc.ProductId) as RatingPercentage
    FROM [dbo].[ProductComments] pc
    WHERE 
        (@StartDate IS NULL OR pc.CreatedDate >= @StartDate)
        AND (@EndDate IS NULL OR pc.CreatedDate <= @EndDate)
        AND (@ProductId IS NULL OR pc.ProductId = @ProductId)
        AND pc.Rating IS NOT NULL
    GROUP BY pc.ProductId, pc.Rating
    ORDER BY pc.ProductId, pc.Rating DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetCommentsSummaryReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. SP for Comments Summary Report
CREATE PROCEDURE [dbo].[GetCommentsSummaryReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @IsActive BIT = NULL,
    @ProductId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pc.ProductId,
        COUNT(*) as TotalComments,
        AVG(CAST(pc.Rating AS FLOAT)) as AverageRating,
        SUM(CASE WHEN pc.IsActive = 1 THEN 1 ELSE 0 END) as ActiveComments,
        MIN(pc.CreatedDate) as FirstCommentDate,
        MAX(pc.CreatedDate) as LastCommentDate
    FROM [dbo].[ProductComments] pc
    WHERE 
        (@StartDate IS NULL OR pc.CreatedDate >= @StartDate)
        AND (@EndDate IS NULL OR pc.CreatedDate <= @EndDate)
        AND (@IsActive IS NULL OR pc.IsActive = @IsActive)
        AND (@ProductId IS NULL OR pc.ProductId = @ProductId)
    GROUP BY pc.ProductId
    ORDER BY TotalComments DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetCouponUsageReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCouponUsageReport]
AS
BEGIN
    SELECT 
        Coupon,
        COUNT(Id) AS UsageCount,
        SUM(CAST(CouponDiscount AS DECIMAL(18,2))) AS TotalDiscount
    FROM [dbo].[Orders]
    WHERE IsActive = 1
        AND Coupon IS NOT NULL
    GROUP BY Coupon
    ORDER BY UsageCount DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetDeliveryTimeAnalysis]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDeliveryTimeAnalysis]
AS
BEGIN
    SELECT 
        OrderNumber,
        CreatedDate,
        DeliveryDate,
        DATEDIFF(DAY, CreatedDate, DeliveryDate) AS DeliveryDays,
        ShipmentCompanyName,
        PaidPrice,
        Currency
    FROM [dbo].[Orders]
    WHERE IsActive = 1
        AND DeliveryDate IS NOT NULL
    ORDER BY DeliveryDays DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetDetailedCommentsReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. SP for Detailed Comments Report
CREATE PROCEDURE [dbo].[GetDetailedCommentsReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @ProductId INT = NULL,
    @MinRating INT = NULL,
    @MaxRating INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pc.Id,
        pc.ProductId,
        pc.Name,
        pc.Email,
        pc.Subject,
        pc.Review,
        pc.Rating,
        pc.UserId,
        pc.CreatedDate,
        pc.UpdatedDate,
        pc.IsActive,
        pc.Lang,
        p.Name as ProductName
    FROM [dbo].[ProductComments] pc
    INNER JOIN [dbo].[Products] p ON pc.ProductId = p.Id
    WHERE 
        (@StartDate IS NULL OR pc.CreatedDate >= @StartDate)
        AND (@EndDate IS NULL OR pc.CreatedDate <= @EndDate)
        AND (@ProductId IS NULL OR pc.ProductId = @ProductId)
        AND (@MinRating IS NULL OR pc.Rating >= @MinRating)
        AND (@MaxRating IS NULL OR pc.Rating <= @MaxRating)
    ORDER BY pc.CreatedDate DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetFraudAnalysisReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFraudAnalysisReport]
AS
BEGIN
    SELECT 
        OrderNumber,
        CreatedDate,
        UserId,
        PaidPrice,
        Currency,
        FraudStatus,
        PaymentStatus,
        ErrorMessage
    FROM [dbo].[Orders]
    WHERE FraudStatus IS NOT NULL
        AND FraudStatus > 0 -- Fraud tespit edilmiş siparişler
    ORDER BY CreatedDate DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetImages]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetImages] 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    --- Entity main images
SELECT top(0)  t.[Name] CategoryName
       ,p.[Name] 
       , p.[ImagePath]
       , p.[ImagePath2]
       ,'ProductMainImage' EntityImageType
  FROM [TestEY_Horizon].[dbo].[Products] p INNER JOIN
  [TestEY_Horizon].[dbo].[Catalog]  t
			ON p.[Catalog_ID]=t.[ID]
				 
  where p.ImagePath<>''


  -- Entity Media images and files
  SELECT [File_Type]
      ,[Modul_Name]
      ,[Mod]
      ,p.Name
	  ,t.[Name] CategoryName
      ,[File_Path]
      ,[File_Desc]
      ,[File_Name]
      ,[File_Format]
  FROM [TestEY_Horizon].[dbo].[Media] m 
  INNER JOIN [TestEY_Horizon].[dbo].[Products] p 
  ON m.Modul_ID = p.Products_ID
   INNER JOIN  [TestEY_Horizon].[dbo].[Catalog]  t
			ON p.[Catalog_ID]=t.[ID]


END

GO
/****** Object:  StoredProcedure [dbo].[GetMonthlySalesTrend]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMonthlySalesTrend]
    @Year INT
AS
BEGIN
    SELECT 
        DATEPART(MONTH, CreatedDate) AS MonthNumber,
        DATENAME(MONTH, CreatedDate) AS MonthName,
        COUNT(Id) AS OrderCount,
        SUM(CAST(PaidPrice AS DECIMAL(18,2))) AS TotalRevenue,
        Currency
    FROM [dbo].[Orders]
    WHERE DATEPART(YEAR, CreatedDate) = @Year
        AND IsActive = 1
        AND PaymentStatus = 'SUCCESS'
    GROUP BY 
        DATEPART(MONTH, CreatedDate),
        DATENAME(MONTH, CreatedDate),
        Currency
    ORDER BY MonthNumber
END
GO
/****** Object:  StoredProcedure [dbo].[GetOrdersByUser]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrdersByUser]
    @UserId NVARCHAR(128)
AS
BEGIN
    SELECT 
        OrderNumber,
        CreatedDate,
        PaidPrice,
        Currency,
        OrderStatus,
        PaymentStatus,
        ShipmentTrackingNumber,
        ShipmentCompanyName
    FROM [dbo].[Orders]
    WHERE UserId = @UserId
        AND IsActive = 1
    ORDER BY CreatedDate DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetOrderStatusDistribution]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderStatusDistribution]
AS
BEGIN
    SELECT 
        OrderStatus,
        COUNT(Id) AS OrderCount,
        SUM(CAST(PaidPrice AS DECIMAL(18,2))) AS TotalAmount,
        Currency
    FROM [dbo].[Orders]
    WHERE IsActive = 1
    GROUP BY OrderStatus, Currency
    ORDER BY OrderCount DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetPaymentMethodReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPaymentMethodReport]
AS
BEGIN
    SELECT 
        CardType,
        CardAssociation,
        Installment,
        COUNT(Id) AS OrderCount,
        SUM(CAST(PaidPrice AS DECIMAL(18,2))) AS TotalAmount,
        Currency
    FROM [dbo].[Orders]
    WHERE IsActive = 1
        AND PaymentStatus = 'SUCCESS'
    GROUP BY CardType, CardAssociation, Installment, Currency
    ORDER BY OrderCount DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetPaymentStatusReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPaymentStatusReport]
AS
BEGIN
    SELECT 
        PaymentStatus,
        COUNT(Id) AS OrderCount,
        SUM(CAST(PaidPrice AS DECIMAL(18,2))) AS TotalAmount,
        Currency
    FROM [dbo].[Orders]
    WHERE IsActive = 1
    GROUP BY PaymentStatus, Currency
    ORDER BY OrderCount DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetPriceAnalysisReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 2. SP for Price Analysis Report
CREATE PROCEDURE [dbo].[GetPriceAnalysisReport]
    @MinPrice MONEY = NULL,
    @MaxPrice MONEY = NULL,
    @ProductCategoryId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.ProductCategoryId,
        COUNT(*) as ProductCount,
        AVG(p.Price) as AveragePrice,
        MIN(p.Price) as MinimumPrice,
        MAX(p.Price) as MaximumPrice,
        SUM(p.Discount) as TotalDiscount,
        AVG(p.Rating) as AverageRating
    FROM [dbo].[Products] p
    WHERE 
        p.IsActive = 1
        AND (@MinPrice IS NULL OR p.Price >= @MinPrice)
        AND (@MaxPrice IS NULL OR p.Price <= @MaxPrice)
        AND (@ProductCategoryId IS NULL OR p.ProductCategoryId = @ProductCategoryId)
    GROUP BY p.ProductCategoryId
    HAVING COUNT(*) > 0
    ORDER BY AveragePrice DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetProductDetailsReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. SP for Product Details Report
CREATE PROCEDURE [dbo].[GetProductDetailsReport]
    @ProductId INT = NULL,
    @ProductCode NVARCHAR(255) = NULL,
    @Lang INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        p.Name,
        p.NameLong,
        p.NameShort,
        p.Description,
        p.ShortDescription,
        p.Price,
        p.Discount,
        p.ProductCode,
        p.ProductCategoryId,
        p.CreatedDate,
        p.UpdatedDate,
        p.AddUserId,
        p.UpdateUserId,
        p.ProductColorOptions,
        p.ProductSizeOptions,
        p.VideoUrl,
        p.MetaKeywords,
        p.Rating
    FROM [dbo].[Products] p
    WHERE 
        (@ProductId IS NULL OR p.Id = @ProductId)
        AND (@ProductCode IS NULL OR p.ProductCode = @ProductCode)
        AND (@Lang IS NULL OR p.Lang = @Lang)
    ORDER BY p.Id;
END
GO
/****** Object:  StoredProcedure [dbo].[GetProductInventoryReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. SP for Product Inventory Report
CREATE PROCEDURE [dbo].[GetProductInventoryReport]
    @State VARCHAR(50) = NULL,
    @IsCampaign BIT = NULL,
    @MainPage BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.State,
        p.IsCampaign,
        p.MainPage,
        COUNT(*) as ProductCount,
        SUM(CASE WHEN p.IsActive = 1 THEN 1 ELSE 0 END) as ActiveProducts,
        SUM(CASE WHEN p.ImageState = 1 THEN 1 ELSE 0 END) as ProductsWithImages,
        AVG(p.Price) as AveragePrice
    FROM [dbo].[Products] p
    WHERE 
        (@State IS NULL OR p.State = @State)
        AND (@IsCampaign IS NULL OR p.IsCampaign = @IsCampaign)
        AND (@MainPage IS NULL OR p.MainPage = @MainPage)
    GROUP BY 
        p.State,
        p.IsCampaign,
        p.MainPage
    ORDER BY ProductCount DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetProductStatsByDateRange]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5. SP for Product Statistics by Date Range
CREATE PROCEDURE [dbo].[GetProductStatsByDateRange]
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DATEADD(DAY, DATEDIFF(DAY, 0, p.CreatedDate), 0) as Date,
        COUNT(*) as ProductsCreated,
        SUM(CASE WHEN p.IsActive = 1 THEN 1 ELSE 0 END) as ActiveProducts,
        AVG(p.Price) as AveragePrice,
        SUM(p.Discount) as TotalDiscount
    FROM [dbo].[Products] p
    WHERE 
        p.CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY 
        DATEADD(DAY, DATEDIFF(DAY, 0, p.CreatedDate), 0)
    ORDER BY Date;
END
GO
/****** Object:  StoredProcedure [dbo].[GetProductSummaryReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. SP for Product Summary Report
CREATE PROCEDURE [dbo].[GetProductSummaryReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @IsActive BIT = NULL,
    @ProductCategoryId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        p.Name,
        p.ProductCategoryId,
        p.Price,
        p.Discount,
        p.CreatedDate,
        p.IsActive,
        p.ProductCode,
        p.Rating,
        p.State,
        COUNT(*) OVER () as TotalRecords
    FROM [dbo].[Products] p
    WHERE 
        (@StartDate IS NULL OR p.CreatedDate >= @StartDate)
        AND (@EndDate IS NULL OR p.CreatedDate <= @EndDate)
        AND (@IsActive IS NULL OR p.IsActive = @IsActive)
        AND (@ProductCategoryId IS NULL OR p.ProductCategoryId = @ProductCategoryId)
    ORDER BY p.CreatedDate DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetRandomNumber2]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
  CREATE PROCEDURE  [dbo].[GetRandomNumber2]
  (
@lowerLimit INT, 
@upperLimit INT)
as 
Begin
Declare @randomNo int
 set  @randomNo = (select round(rand(checksum(newid()))*(@lowerLimit)+@upperLimit,0) as [GetRandomNumber])
 return @randomNo
End
GO
/****** Object:  StoredProcedure [dbo].[GetRegionalSalesReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
-- GetRegionalSalesReport: PaymentStatus is supplied by the caller (nullable = all statuses).
-- Addresses are in dbo.Addresses (Orders.ShippingAddressId FK).
CREATE PROCEDURE [dbo].[GetRegionalSalesReport]
    @PaymentStatus NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sa.City,
        COUNT(o.Id) AS OrderCount,
        SUM(CAST(o.PaidPrice AS DECIMAL(18, 2))) AS TotalRevenue,
        o.Currency
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[Addresses] sa ON o.ShippingAddressId = sa.Id
    WHERE o.IsActive = 1
      AND (
            @PaymentStatus IS NULL
            OR LTRIM(RTRIM(@PaymentStatus)) = N''
            OR o.PaymentStatus = @PaymentStatus
          )
    GROUP BY sa.City, o.Currency
    ORDER BY TotalRevenue DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[GetSalesReportByDateRange]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSalesReportByDateRange]
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SELECT 
        COUNT(Id) AS OrderCount,
        SUM(CAST(PaidPrice AS DECIMAL(18,2))) AS TotalRevenue,
        AVG(CAST(PaidPrice AS DECIMAL(18,2))) AS AverageOrderValue,
        Currency
    FROM [dbo].[Orders]
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
        AND IsActive = 1
        AND PaymentStatus = 'SUCCESS' -- Ödeme başarılı olanlar
    GROUP BY Currency
    ORDER BY TotalRevenue DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetShipmentCompanyReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetShipmentCompanyReport]
AS
BEGIN
    SELECT 
        ShipmentCompanyName,
        COUNT(Id) AS OrderCount,
        SUM(CAST(CargoPrice AS DECIMAL(18,2))) AS TotalCargoCost
    FROM [dbo].[Orders]
    WHERE IsActive = 1
        AND ShipmentCompanyName IS NOT NULL
    GROUP BY ShipmentCompanyName
    ORDER BY OrderCount DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetSubscribersStats]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
   exec dbo.GetSubscribersStats @BrowserNotificationId=5

*/
CREATE PROCEDURE [dbo].[GetSubscribersStats]
     @BrowserNotificationId int= null 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT n.[Id]
				  ,[Name]
		  ,[NotificationType]
		  ,[CreatedDate] [DateSent]
		  ,stats.NotTracked
	      ,stats.SWUpdated
		  ,stats.DebuggingSingal
		  ,stats.SWUnregister
		  ,stats.Delivered
		  ,stats.Clicked
	FROM [dbo].[BrowserNotifications] n
		   INNER JOIN 
				  (SELECT [BrowserNotificationId], 
				  ISNULL([-1],0) NotTracked,
				  ISNULL([4],0) DebuggingSingal,  
				  ISNULL([1],0) SWUpdated, 
				  ISNULL([2],0) SWUnregister , 
				  ISNULL([8],0) Delivered, 
				  ISNULL([16],0) Clicked
						 FROM
						 (
						 SELECT  [BrowserNotificationId]
								 ,[NotificationStatus]
									  ,COUNT(*) Cnt
						   FROM [dbo].[BrowserNotificationFeedBacks]
						   GROUP BY [BrowserNotificationId],[NotificationStatus]
						   ) t
						 PIVOT
						 (
						 MAX(Cnt)
						 FOR [NotificationStatus] IN ([-1], [1], [2], [4], [8],[16])
						 ) AS PivotTable
						 ) stats
				  ON stats.[BrowserNotificationId]=n.[Id]
				  where n.[Id]=ISNULL(@BrowserNotificationId,n.[Id])
	ORDER BY [CreatedDate] desc 
 



END

GO
/****** Object:  StoredProcedure [dbo].[GetTopActiveCustomers]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTopActiveCustomers]
    @TopN INT = 50
AS
BEGIN
    SELECT TOP (@TopN)
        UserId,
        COUNT(Id) AS OrderCount,
        SUM(CAST(PaidPrice AS DECIMAL(18,2))) AS TotalSpent,
        Currency,
        MAX(CreatedDate) AS LastOrderDate
    FROM [dbo].[Orders]
    WHERE IsActive = 1
        AND PaymentStatus = 'SUCCESS'
    GROUP BY UserId, Currency
    ORDER BY OrderCount DESC
END
GO
/****** Object:  StoredProcedure [dbo].[GetUserCommentsReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5. SP for User Comments Report
CREATE PROCEDURE [dbo].[GetUserCommentsReport]
    @UserId NVARCHAR(128) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pc.UserId,
        pc.Name,
        pc.Email,
        COUNT(*) as TotalComments,
        AVG(CAST(pc.Rating AS FLOAT)) as AverageRating,
        MAX(pc.CreatedDate) as LastCommentDate,
        MIN(pc.CreatedDate) as FirstCommentDate,
        SUM(CASE WHEN pc.IsActive = 1 THEN 1 ELSE 0 END) as ActiveComments
    FROM [dbo].[ProductComments] pc
    WHERE 
        (@UserId IS NULL OR pc.UserId = @UserId)
        AND (@StartDate IS NULL OR pc.CreatedDate >= @StartDate)
        AND (@EndDate IS NULL OR pc.CreatedDate <= @EndDate)
    GROUP BY 
        pc.UserId,
        pc.Name,
        pc.Email
    HAVING COUNT(*) > 0
    ORDER BY TotalComments DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[InsertProductImage]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InsertProductImage]
    @ProductId INT,
    @ProductName NVARCHAR(250),
    @EntityImageType NVARCHAR(250),
    @ImageId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @EntityImageType = N'ProductMainImage'
    BEGIN
        UPDATE [dbo].[Products] 
        SET MainImageId = @ImageId 
        WHERE Id = @ProductId;
    END
    ELSE
    BEGIN
        -- Check if ImageId exists in FileStorages
        IF EXISTS (SELECT 1 FROM [dbo].[FileStorages] WHERE Id = @ImageId)
        BEGIN
            INSERT INTO [dbo].[ProductFiles]
                ([ProductId], [FileStorageId], [Name], [CreatedDate], [UpdatedDate], [IsActive], [Position], [Lang])
            VALUES
                (@ProductId, @ImageId, @ProductName, GETDATE(), GETDATE(), 1, 0, 1);
        END
        ELSE
        BEGIN
            -- Handle the missing FileStorage record (return an error or insert a placeholder)
            RAISERROR('Error: @ImageId does not exist in FileStorages table.', 16, 1);
            RETURN;
        END
    END
END
GO
/****** Object:  StoredProcedure [dbo].[MathCalculation]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MathCalculation]
(
    @Dividend INT, 
    @Divisor INT
)
AS
BEGIN
SET NOCOUNT ON;
    BEGIN TRY
      SELECT @Dividend/@Divisor as Quotient;
    END TRY
    BEGIN CATCH
     PRINT Error_message();
	  EXEC [dbo].[db_error_Learning_Insert_ErrorLog] --To log Stored procedure errors
    END CATCH
SET NOCOUNT OFF;
END  

GO
/****** Object:  StoredProcedure [dbo].[Migrate]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Migrate]
	 @Lang int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	exec MigrateMenuData @Lang
	exec [dbo].[MigrateProductCategoryData] @Lang
    exec MigrateProductData @Lang

	delete from  [dbo].[Subscribers]
	INSERT INTO [dbo].[Subscribers]
           ([Name]
           ,[CreatedDate]
           ,[UpdatedDate]
           ,[IsActive]
           ,[Position]
           ,[Email]
           ,[EntityHash]
           ,[Lang]
           ,[Note])
	SELECT case when  [Name]<>[Email]  THEN [Name] ELSE '' END
	,getdate()
	,getdate()
	,1
	,1
      ,[Email]
    ,''
	,@Lang
	,''
  FROM [TestEY_Horizon].[dbo].[EmailList]
  
    update [dbo].[ProductCategories] set [TemplateId]=1
END

GO
/****** Object:  StoredProcedure [dbo].[MigrateChildMenuData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[MigrateChildMenuData]
      @ParentId int=0,
	   @Lang int = 1 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF @ParentId <> 0
	BEGIN
			declare @parentId2 int=0

			select @parentId2 = m.Id from 
			[Menus] m  INNER JOIN 
			[TestEY_Horizon].[dbo].[Navigation] t
			ON Name=[PageName]
				where t.Id=@ParentId

		  
	 			INSERT INTO [dbo].[Menus]
				 ([ParentId]
				   ,[Name]
				   ,[CreatedDate]
				   ,[UpdatedDate]
				   ,[IsActive]
				   ,[Position]
				   ,[Description]
				   ,[ImageState]
				   ,[MainPage]
				   ,[LinkIsActive]
				   ,[Link]
				   ,[Static]
				   ,[MainImageId]
				   ,[MenuLink]
				   ,[PageTheme]
				   ,[Lang]
				   ,[EntityHash]
				   ,[MetaKeywords])

     SELECT   @parentId2,
				t.[PageName],getdate(),getdate(),1,t.[NavigationOrdering],t.[PageDescription],0,0,0,'',0,0,'pages-index','T1',@Lang,'',''

			 FROM [TestEY_Horizon].[dbo].[Navigation] t
			where ParentId=@ParentId



			 Declare @Keys Table (ID integer Primary Key Not Null)
		 Insert @Keys(ID)
		SELECT  t.ID 
			 FROM [TestEY_Horizon].[dbo].[Navigation] t
			where ParentId=@ParentId
  
  
	 
    
     
			Declare @Key Integer
		   While Exists (Select * From @Keys)
			 Begin
				 Select @Key = Max(ID) From @Keys
				 EXEC MigrateChildMenuData @Key,@Lang
				 print @Key
				 Delete @Keys Where ID = @Key
			 End 

	 END
 



     
END

GO
/****** Object:  StoredProcedure [dbo].[MigrateChildProductCategoryData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[MigrateChildProductCategoryData]
      @ParentId int=0,
	   @Lang int = 1 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF @ParentId <> 0
	BEGIN
			declare @parentId2 int=0

			select @parentId2 = m.Id from 
			[ProductCategories] m  INNER JOIN 
			[TestEY_Horizon].[dbo].[Catalog]  t
			ON m.Name=t.[Name]
				where t.Id=@ParentId

		  
	 		INSERT INTO [dbo].[ProductCategories]
           ([ParentId]
           ,[Name]
           ,[CreatedDate]
           ,[UpdatedDate]
           ,[IsActive]
           ,[Position]
           ,[Description]
           ,[ImageState]
           ,[MainPage]
           ,[MainImageId]
           ,[Lang]
           ,[TemplateId]
           ,[EntityHash]
           ,[DiscountPercantage]
           ,[MetaKeywords])

     SELECT  @parentId2,t.[Name],getdate(),getdate(),1,t.Ordering,'',0,0,0,@Lang,0,'',0,''

			 FROM [TestEY_Horizon].[dbo].[Catalog] t
			where Parent_Id=@ParentId



			 Declare @Keys Table (ID integer Primary Key Not Null)
		 Insert @Keys(ID)
		SELECT  t.ID 
			 FROM [TestEY_Horizon].[dbo].[Catalog] t
			where Parent_Id=@ParentId
  
  
	 
    
     
			Declare @Key Integer
		   While Exists (Select * From @Keys)
			 Begin
				 Select @Key = Max(ID) From @Keys
				 EXEC MigrateChildProductCategoryData @Key,@Lang
				 print @Key
				 Delete @Keys Where ID = @Key
			 End 

	 END
 



     
END

GO
/****** Object:  StoredProcedure [dbo].[MigrateDataFromOldDatabase]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[MigrateDataFromOldDatabase]
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

     SELECT TOP 1000 [ID]
      ,[ParentID]
      ,[Static]
      ,[PageName]
      ,[PageTitle]
      ,[PageShortDesc]
      ,[PageDescription]
      ,[PageLayout]
      ,[ImagePath]
      ,[Form]
      ,[Modul]
      ,[Mod]
      ,[NavigationOrdering]
      ,[ImageState]
      ,[State]
      ,[MainPage]
      ,[Lang]
      ,[Link]
      ,[LinkState]
      ,[Created_Date]
      ,[PageMetaKeys]
  FROM [TestEY_Horizon].[dbo].[Navigation]

END

GO
/****** Object:  StoredProcedure [dbo].[MigrateMenuData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[MigrateMenuData]
	 @Lang int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	 delete  from [dbo].[Menus]
     
	INSERT INTO [dbo].[Menus]
           ([ParentId]
           ,[Name]
           ,[CreatedDate]
           ,[UpdatedDate]
           ,[IsActive]
           ,[Position]
           ,[Description]
           ,[ImageState]
           ,[MainPage]
           ,[LinkIsActive]
           ,[Link]
           ,[Static]
           ,[MainImageId]
           ,[MenuLink]
           ,[PageTheme]
           ,[Lang]
           ,[EntityHash]
           ,[MetaKeywords])

     SELECT  0,t.[PageName],getdate(),getdate(),1,t.[NavigationOrdering],t.[PageDescription],0,0,0,'',0,0,'pages-index','T1',@Lang,'',''
     FROM [TestEY_Horizon].[dbo].[Navigation] t
    where ParentId=0

 

	
 Declare @Keys Table (ID integer Primary Key Not Null)
 Insert @Keys(ID)
SELECT  t.ID 
     FROM [TestEY_Horizon].[dbo].[Navigation] t
    where ParentId=0
  
  
 -- select * from @Keys
    
     
    Declare @Key Integer
   While Exists (Select * From @Keys)
     Begin
         Select @Key = Max(ID) From @Keys
         EXEC MigrateChildMenuData @Key,@Lang
         print @Key
         Delete @Keys Where ID = @Key
     End 
END

GO
/****** Object:  StoredProcedure [dbo].[MigrateProductCategoryData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[MigrateProductCategoryData]
	 @Lang int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	 delete  from [dbo].[ProductCategories]
     
	INSERT INTO [dbo].[ProductCategories]
           ([ParentId]
           ,[Name]
           ,[CreatedDate]
           ,[UpdatedDate]
           ,[IsActive]
           ,[Position]
           ,[Description]
           ,[ImageState]
           ,[MainPage]
           ,[MainImageId]
           ,[Lang]
           ,[TemplateId]
           ,[EntityHash]
           ,[DiscountPercantage]
           ,[MetaKeywords])

     SELECT  0,t.[Name],getdate(),getdate(),1,t.Ordering,'',0,0,0,@Lang,0,'',0,''
     FROM [TestEY_Horizon].[dbo].[Catalog] t
    where [Parent_ID]=0

	
 Declare @Keys Table (ID integer Primary Key Not Null)
 Insert @Keys(ID)
SELECT  t.ID 
     FROM [TestEY_Horizon].[dbo].[Catalog] t
    where [Parent_ID]=0
  
  
 -- select * from @Keys
    
     
    Declare @Key Integer
   While Exists (Select * From @Keys)
     Begin
         Select @Key = Max(ID) From @Keys
         EXEC [dbo].[MigrateChildProductCategoryData] @Key,@Lang
         print @Key
         Delete @Keys Where ID = @Key
     End 
END

GO
/****** Object:  StoredProcedure [dbo].[MigrateProductData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[MigrateProductData]
	@Lang int=1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	delete from [dbo].[Products]

	INSERT INTO [dbo].[Products]
           ([Name]
           ,[NameShort]
           ,[CreatedDate]
           ,[UpdatedDate]
           ,[IsActive]
           ,[Position]
           ,[Description]
           ,[MainPage]
           ,[ImageState]
           ,[MainImageId]
           ,[ProductCategoryId]
           ,[Price]
           ,[Discount]
           ,[ProductCode]
           ,[Lang]
           ,[VideoUrl]
           ,[EntityHash]
           ,[MetaKeywords])

  SELECT 
      p.[Name]
	  , p.NameExp
	  ,getdate()
	  ,getdate()
	  ,1
	  ,p.Ordering
	  ,p.[Detail]
	  ,0
	  ,0
	  ,0
	,pc.Id
	,Price
	,0
	,Code
	,@Lang
	,''
	,''
	,AnahtarKelimeler
  FROM [TestEY_Horizon].[dbo].[Products] p
  INNER JOIN  [TestEY_Horizon].[dbo].[Catalog] c ON p.[Catalog_ID] = c.Id
   INNER JOIN  [dbo].[ProductCategories]  pc ON c.[Name] = pc.Name

  -- SELECT c.Name
  --    ,p.[Name]
	 -- ,pc.Name
	 -- ,pe.Name
  --    ,[Renk]
  --    ,[KafaSekli]
  --    ,[Hacim]
  --    ,[Cap]
  --    ,[Yukseklik]
  --    ,[Agirlik]
  --    ,[KoliAdedi]
  --    ,[PaketAdedi]
  --    ,[PaletAdedi]
  --    ,[Stok]
  --FROM [TestEY_Horizon].[dbo].[Products] p
  --INNER JOIN  [TestEY_Horizon].[dbo].[Catalog] c ON p.[Catalog_ID] = c.Id
  -- INNER JOIN  [dbo].[ProductCategories]  pc ON c.[Name] = pc.Name
  --   INNER JOIN  [dbo].[Products]  pe ON p.[Name] = pe.Name


	   
--  INSERT INTO [dbo].[ProductSpecifications]
--           ([ProductId]
--           ,[Name]
--           ,[CreatedDate]
--           ,[UpdatedDate]
--           ,[IsActive]
--           ,[Position]
--           ,[Value]
--           ,[Unit]
--           ,[EntityHash]
--           ,[Lang])
--SELECT pe.Id
--      ,'Stok'
--       ,getdate()
--	  ,getdate()
--	  ,1
--	  ,p.Ordering
--	  ,[Stok]
--	  ,''
--	  ,''
--	  ,1
--  FROM [TestEY_Horizon].[dbo].[Products] p
--  INNER JOIN  [TestEY_Horizon].[dbo].[Catalog] c ON p.[Catalog_ID] = c.Id
--   INNER JOIN  [dbo].[ProductCategories]  pc ON c.[Name] = pc.Name
--     INNER JOIN  [dbo].[Products]  pe ON p.[Name] = pe.Name


END

GO
/****** Object:  StoredProcedure [dbo].[pn_GetSubscribersStats]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[pn_GetSubscribersStats]
 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT n.[Id]
				  ,[Name]
		  ,[NotificationType]
		  ,[CreatedDate] [DateSent]
			 ,stats.NotTracked
			 ,stats.SWUpdated
			 		 ,stats.DebuggingSingal
			  ,stats.SWUnregister
			   ,stats.Delivered
			      ,stats.Clicked
	FROM [dbo].[BrowserNotifications] n
		   INNER JOIN 
				  (SELECT [BrowserNotificationId], ISNULL([-1],0) NotTracked,ISNULL([4],0) DebuggingSingal,  ISNULL([1],0) SWUpdated, ISNULL([2],0) SWUnregister , ISNULL([8],0) Delivered, ISNULL([16],0) Clicked
						 FROM
						 (
						 SELECT  [BrowserNotificationId]
								 ,[NotificationStatus]
									  ,COUNT(*) Cnt
						   FROM [dbo].[BrowserNotificationFeedBacks]
						   GROUP BY [BrowserNotificationId],[NotificationStatus]
						   ) t
						 PIVOT
						 (
						 MAX(Cnt)
						 FOR [NotificationStatus] IN ([-1], [1], [2], [4], [8],[16])
						 ) AS PivotTable
						 ) stats
				  ON stats.[BrowserNotificationId]=n.[Id]

	ORDER BY [CreatedDate] desc 
 



END

GO
/****** Object:  StoredProcedure [dbo].[ProductImageExternalUrl]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[ProductImageExternalUrl] 
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT        ProductId, ProductName, ImageFullPath, EntityImageType
FROM            (SELECT        p.Id AS ProductId, po.label AS ProductName, po.picture1Path AS ImageFullPath, 'ProductMainImage' AS EntityImageType
                          FROM            dbo.Products p INNER JOIN
                                                    dbo.ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
                          WHERE        (po.picture1Path IS NOT NULL)
                          UNION
                          SELECT        p.Id AS ProductId, po.label AS ProductName, po.picture2Path AS ImageFullPath, 'ProductGallery' AS EntityImageType
                          FROM            dbo.Products p INNER JOIN
                                                   dbo.ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
                          WHERE        (po.picture2Path IS NOT NULL)
                          UNION
                          SELECT        p.Id AS ProductId, po.label AS ProductName, po.picture3Path AS ImageFullPath, 'ProductGallery' AS EntityImageType
                          FROM            dbo.Products p INNER JOIN
                                                   dbo.ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
                          WHERE        (po.picture3Path IS NOT NULL)
                          UNION
                          SELECT        p.Id AS ProductId, po.label AS ProductName, po.picture4Path AS ImageFullPath, 'ProductGallery' AS EntityImageType
                          FROM            dbo.Products p INNER JOIN
                                                   dbo.ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
                          WHERE        (po.picture4Path IS NOT NULL)) P_1

END
GO
/****** Object:  StoredProcedure [dbo].[RandNumber]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[RandNumber]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT floor(Rand() * (100 + 1))
END
GO
/****** Object:  StoredProcedure [dbo].[ReturnAllTableData]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE  [dbo].[ReturnAllTableData]
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   declare @tablename varchar(500)
declare @sql varchar(5000)
declare @tableNameSql varchar(500)
declare @idname varchar(50)
declare @tablearchive varchar(500)

--Select all the tables which you want to make in archive
declare tableCursor cursor FAST_FORWARD FOR
SELECT table_name FROM INFORMATION_SCHEMA.TABLES
-- where table_name

--Put your condition, if you want to filter the tables
--like '%TRN_%' and charindex('Archive',table_name) = 0 and charindex('ErrorLog',table_name) = 0

--Open the cursor and iterate till end
OPEN tableCursor
FETCH NEXT FROM tableCursor INTO @tablename WHILE @@FETCH_STATUS = 0
          BEGIN
                  set @tableNameSql = 'select '''+@tablename +''' as TableName'
                  SET @sql = 'select *  from '+ @tablename +''
				   EXEC(@tableNameSql)
                                      EXEC(@sql)
                    
          FETCH NEXT FROM tableCursor INTO @tablename
END
CLOSE tableCursor
DEALLOCATE tableCursor


END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetCustomerSegmentationReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetCustomerSegmentationReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to all time if no dates provided
    IF @StartDate IS NULL 
        SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL 
        SET @EndDate = GETDATE();

    -- User Order Frequency
    WITH UserOrderStats AS (
        SELECT 
            UserId,
            COUNT(*) AS OrderCount,
            SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalSpend,
            MIN(CreatedDate) AS FirstOrderDate,
            MAX(CreatedDate) AS LastOrderDate
        FROM Orders
        WHERE CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY UserId
    )
    SELECT 
        CASE 
            WHEN OrderCount = 1 THEN 'One-time Buyers'
            WHEN OrderCount BETWEEN 2 AND 5 THEN 'Occasional Buyers'
            ELSE 'Frequent Buyers'
        END AS CustomerSegment,
        COUNT(*) AS CustomerCount,
        AVG(OrderCount) AS AvgOrdersPerCustomer,
        AVG(TotalSpend) AS AvgTotalSpend
    FROM UserOrderStats
    GROUP BY 
        CASE 
            WHEN OrderCount = 1 THEN 'One-time Buyers'
            WHEN OrderCount BETWEEN 2 AND 5 THEN 'Occasional Buyers'
            ELSE 'Frequent Buyers'
        END;

    -- Language Distribution
    SELECT 
        Lang,
        COUNT(DISTINCT UserId) AS UniqueCustomers,
        COUNT(*) AS TotalOrders,
        SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalRevenue
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY Lang
    ORDER BY UniqueCustomers DESC;

    -- Active vs Inactive Users
    WITH UserActivity AS (
        SELECT 
            UserId,
            MAX(CreatedDate) AS LastOrderDate,
            DATEDIFF(DAY, MAX(CreatedDate), GETDATE()) AS DaysSinceLastOrder
        FROM Orders
        WHERE CreatedDate BETWEEN @StartDate AND @EndDate
        GROUP BY UserId
    )
    SELECT 
        CASE 
            WHEN DaysSinceLastOrder <= 90 THEN 'Active (Last 3 Months)'
            WHEN DaysSinceLastOrder BETWEEN 91 AND 365 THEN 'Dormant (3-12 Months)'
            ELSE 'Inactive (Over 1 Year)'
        END AS UserActivityStatus,
        COUNT(*) AS UserCount
    FROM UserActivity
    GROUP BY 
        CASE 
            WHEN DaysSinceLastOrder <= 90 THEN 'Active (Last 3 Months)'
            WHEN DaysSinceLastOrder BETWEEN 91 AND 365 THEN 'Dormant (3-12 Months)'
            ELSE 'Inactive (Over 1 Year)'
        END;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetFinancialReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetFinancialReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to all time if no dates provided
    IF @StartDate IS NULL 
        SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL 
        SET @EndDate = GETDATE();

    -- Revenue Analysis
    SELECT 
        Currency,
        COUNT(*) AS TotalOrders,
        SUM(CAST(ISNULL(NULLIF(Price, ''), '0') AS DECIMAL(18,2))) AS TotalOriginalPrice,
        SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalPaidPrice,
        SUM(CAST(ISNULL(NULLIF(CouponDiscount, ''), '0') AS DECIMAL(18,2))) AS TotalDiscount,
        AVG(CAST(ISNULL(NULLIF(Price, ''), '0') AS DECIMAL(18,2))) AS AverageOrderValue,
        AVG(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS AveragePaidValue
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY Currency;

    -- Coupon Impact Analysis
    SELECT 
        CASE WHEN Coupon IS NOT NULL THEN 'With Coupon' ELSE 'Without Coupon' END AS CouponType,
        COUNT(*) AS OrderCount,
        SUM(CAST(ISNULL(NULLIF(Price, ''), '0') AS DECIMAL(18,2))) AS TotalOriginalPrice,
        SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalPaidPrice,
        SUM(CAST(ISNULL(NULLIF(CouponDiscount, ''), '0') AS DECIMAL(18,2))) AS TotalDiscount,
        AVG(CAST(ISNULL(NULLIF(CouponDiscount, ''), '0') AS DECIMAL(18,2))) AS AverageDiscount
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY CASE WHEN Coupon IS NOT NULL THEN 'With Coupon' ELSE 'Without Coupon' END;

    -- Installment Analysis
    SELECT 
        Installment,
        COUNT(*) AS OrderCount,
        SUM(CAST(ISNULL(NULLIF(Price, ''), '0') AS DECIMAL(18,2))) AS TotalPrice,
        AVG(CAST(ISNULL(NULLIF(Price, ''), '0') AS DECIMAL(18,2))) AS AverageOrderValue
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY Installment
    ORDER BY OrderCount DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetFraudRiskReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetFraudRiskReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to all time if no dates provided
    IF @StartDate IS NULL 
        SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL 
        SET @EndDate = GETDATE();

    -- Fraud Status Detailed Analysis
    SELECT 
        FraudStatus,
        COUNT(*) AS OrderCount,
        SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalAmount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage,
        AVG(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS AverageOrderValue
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY FraudStatus
    ORDER BY OrderCount DESC;

    -- Card-Related Fraud Indicators
    SELECT 
        CardType,
        CardAssociation,
        CardFamily,
        COUNT(*) AS OrderCount,
        SUM(CASE WHEN FraudStatus = 1 THEN 1 ELSE 0 END) AS FraudulentOrders,
        ROUND(SUM(CASE WHEN FraudStatus = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) AS FraudPercentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY CardType, CardAssociation, CardFamily
    ORDER BY FraudulentOrders DESC;

    -- Error Code Analysis
    SELECT 
        ErrorCode,
        ErrorMessage,
        COUNT(*) AS OccurrenceCount,
        SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalAmount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE ErrorCode IS NOT NULL 
      AND CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY ErrorCode, ErrorMessage
    ORDER BY OccurrenceCount DESC;

    -- Bin Number Fraud Risk
    SELECT 
        LEFT(BinNumber, 6) AS BinPrefix,
        COUNT(*) AS OrderCount,
        SUM(CASE WHEN FraudStatus = 1 THEN 1 ELSE 0 END) AS FraudulentOrders,
        ROUND(SUM(CASE WHEN FraudStatus = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) AS FraudPercentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY LEFT(BinNumber, 6)
    ORDER BY FraudPercentage DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetOrderVolumeReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetOrderVolumeReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to all time if no dates provided
    IF @StartDate IS NULL 
        SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL 
        SET @EndDate = GETDATE();

    -- Total Order Volume
    SELECT 
        COUNT(*) AS TotalOrders,
        COUNT(DISTINCT UserId) AS UniqueUsers
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate;

    -- Orders by Type
    SELECT 
        OrderType, 
        COUNT(*) AS OrderCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY OrderType
    ORDER BY OrderCount DESC;

    -- Orders by Status
    SELECT 
        OrderStatus, 
        COUNT(*) AS OrderCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY OrderStatus
    ORDER BY OrderCount DESC;

    -- Monthly Order Trend
    SELECT 
        YEAR(CreatedDate) AS OrderYear,
        MONTH(CreatedDate) AS OrderMonth,
        COUNT(*) AS MonthlyOrderCount
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY YEAR(CreatedDate), MONTH(CreatedDate)
    ORDER BY OrderYear, OrderMonth;

    -- Language Distribution
    SELECT 
        Lang, 
        COUNT(*) AS OrderCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY Lang
    ORDER BY OrderCount DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetPaymentTransactionReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetPaymentTransactionReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to all time if no dates provided
    IF @StartDate IS NULL 
        SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL 
        SET @EndDate = GETDATE();

    -- Payment Status Distribution
    SELECT 
        PaymentStatus, 
        COUNT(*) AS OrderCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY PaymentStatus
    ORDER BY OrderCount DESC;

    -- Card Type Analysis
    SELECT 
        CardType, 
        CardAssociation,
        CardFamily,
        COUNT(*) AS OrderCount,
        SUM(CAST(ISNULL(NULLIF(PaidPrice, ''), '0') AS DECIMAL(18,2))) AS TotalAmount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY CardType, CardAssociation, CardFamily
    ORDER BY OrderCount DESC;

    -- Merchant Commission Analysis
    SELECT 
        ConnectorName,
        COUNT(*) AS OrderCount,
        SUM(CAST(ISNULL(NULLIF(MerchantCommissionRateAmount, ''), '0') AS DECIMAL(18,2))) AS TotalMerchantCommission,
        SUM(CAST(ISNULL(NULLIF(IyziCommissionRateAmount, ''), '0') AS DECIMAL(18,2))) AS TotalIyziCommission,
        AVG(CAST(ISNULL(NULLIF(MerchantCommissionRate, ''), '0') AS DECIMAL(18,2))) AS AvgMerchantCommissionRate
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY ConnectorName
    ORDER BY OrderCount DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetPerformanceSystemReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Performance and System Report Stored Procedure
CREATE PROCEDURE [dbo].[sp_GetPerformanceSystemReport]
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Default to all time if no dates provided
    IF @StartDate IS NULL 
        SET @StartDate = '1900-01-01';
    IF @EndDate IS NULL 
        SET @EndDate = GETDATE();

    -- Order Processing Time Analysis
    SELECT 
        ConnectorName,
        COUNT(*) AS TotalOrders,
        AVG(DATEDIFF(MINUTE, CreatedDate, UpdatedDate)) AS AvgProcessingTimeMinutes,
        MIN(DATEDIFF(MINUTE, CreatedDate, UpdatedDate)) AS MinProcessingTimeMinutes,
        MAX(DATEDIFF(MINUTE, CreatedDate, UpdatedDate)) AS MaxProcessingTimeMinutes
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY ConnectorName
    ORDER BY AvgProcessingTimeMinutes DESC;

    -- Payment Gateway Performance
    SELECT 
        PaymentStatus,
        Phase,
        Status,
        COUNT(*) AS OrderCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage,
        AVG(CAST(PaidPrice AS DECIMAL(18,2))) AS AverageOrderValue
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY PaymentStatus, Phase, Status
    ORDER BY OrderCount DESC;

    -- System Error Analysis
    SELECT 
        ErrorCode,
        ErrorMessage,
        Phase,
        COUNT(*) AS ErrorCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage
    FROM Orders
    WHERE ErrorCode IS NOT NULL
      AND CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY ErrorCode, ErrorMessage, Phase
    ORDER BY ErrorCount DESC;

    -- Locale and System Distribution
    SELECT 
        Locale,
        COUNT(*) AS OrderCount,
        ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) AS Percentage,
        AVG(CAST(PaidPrice AS DECIMAL(18,2))) AS AverageOrderValue
    FROM Orders
    WHERE CreatedDate BETWEEN @StartDate AND @EndDate
    GROUP BY Locale
    ORDER BY OrderCount DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[SqlProfilerReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SqlProfilerReport]
AS
BEGIN
	SELECT CONVERT(NVARCHAR(max),[TextData]), COUNT(*) Cnt
  FROM [dbo].[SqlProfiler]
    where CONVERT(NVARCHAR(max),[TextData]) NOT LIKE '%AppLogs%'
  GROUP BY CONVERT(NVARCHAR(max),[TextData])
  HAVING  COUNT(*)>2
  order by COUNT(*) desc
END

 
GO
/****** Object:  StoredProcedure [dbo].[SqlQueryReport]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SqlQueryReport] 
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT   cast([TextData] as nvarchar(max)) SQLQuery 
		,Count(*) Cnt 
		,Max([RowNumber]) rowNumber
  FROM  [dbo].[SqlReport]
    where [TextData] is not null
  group by cast([TextData] as nvarchar(max))
  order by Count(*) desc

END
GO
/****** Object:  StoredProcedure [dbo].[test_SearchProducts]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
/*

  DECLARE @filter as ei_tpt_Filter
 
 -- INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('company','raymarine','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Included Qty x Type','USB adapter - 5 pin Micro-USB Type B ( female ) - Apple Dock connector ( male ) USB cable - 5 pin Micro-USB Type B ( male ) - 4 pin USB Type A ( male )','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Warranty','1 Year','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Part Numbers','CW13632','') 
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast) VALUES ('Depth','1.5','')
  --INSERT INTO @filter (FieldName, ValueFirst, ValueLast)  VALUES ('Low Voltage Power','20','')
 

exec [dbo].[test_SearchProducts] @search = '', @top = 100, @skip = 0, @filter= @filter



 
 select * from  dbo.Products where ProductID = 5521
 
 
 select * from   dbo.ProductsSpecValues psv
	    
			WHERE  ProductSpecID=152 AND ValueString= 'USB adapter - 5 pin Micro-USB Type B ( female ) - Apple Dock connector ( male ) USB cable - 5 pin Micro-USB Type B ( male ) - 4 pin USB Type A ( male )'
 

*/
CREATE PROCEDURE [dbo].[test_SearchProducts]
	@search nvarchar(200)='',
	@top int=20,
	@skip int=0,
	@language int=1,
	@filter AS dbo.ei_tpt_Filter Readonly 
AS
BEGIN

	SET NOCOUNT ON;
	
SELECT p.*
FROM dbo.Products as p
 where 	p.IsActive=1 and p.Lang = @language
 
SELECT p.*
FROM dbo.ProductCategories as p
 where 	p.IsActive=1 and p.Lang = @language
 

END

GO
/****** Object:  StoredProcedure [dbo].[updateAllLang]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[updateAllLang] 
	@Lang Int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   update  [dbo].[Menus]  set Lang=1

Update  Products  set Lang=1

Update  ProductSpecifications  set Lang=1

Update  ProductFiles  set Lang=1

Update  Products  set Lang=1

Update  ProductCategories  set Lang=1




Update  FileStorages  set Lang=1


Update  ListItems  set Lang=1

Update  Lists  set Lang=1

Update  MainPageImages  set Lang=1

Update  MenuFiles set Lang=1

Update  Menus  set Lang=1





--Update  Settings
Update  Stories  set Lang=1

Update  StoryCategories  set Lang=1

Update  StoryFiles  set Lang=1


Update  Subscribers  set Lang=1

Update  TagCategories  set Lang=1

Update  Tags set Lang=1

Update  MailTemplates set Lang=1

END
GO
/****** Object:  StoredProcedure [dbo].[UpdateProductPrice]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[UpdateProductPrice]
	 
AS
BEGIN
	 Declare @Keys Table (ID nvarchar(500))
 
 Insert @Keys(ID)
   SELECT  Id
     FROM Products
 
   
    Declare @Key  nvarchar(500)
   While Exists (Select * From @Keys)
     Begin
         Select @Key = Max(ID) From @Keys
		-- select floor(rand() * (8000 + 1)) ,@Key 
		 update Products set Price=floor(rand() * (10000 + 1)) where Id=@Key
         Delete @Keys Where ID = @Key
     End 
	
	
    
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateProductPrice2]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[UpdateProductPrice2]
	 
AS
BEGIN
	
  UPDATE  P
SET     Price = T.NewPrice
FROM    [dbo].[Products]    P  INNER JOIN
(
	SELECT  Id,dbo.GetRandomNumber(0,10000,NEWID()) as NewPrice
  FROM [dbo].[Products]

)  as  T   ON  P.Id = T.Id 
	
    
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateProductPrices]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateProductPrices]
    @PercentageOfIncreaseOrDecrease DECIMAL(18, 2), -- e.g., 10.00 for 10% increase, -5.00 for 5% decrease
    @ProductId INT = NULL,                          -- Optional: Update a specific product
    @CategoryId INT = NULL,                        -- Optional: Update products in a specific category
    @BrandId INT = NULL,                          -- Optional: Update products by brand
    @TagId INT = NULL                            -- Optional: Update products with specific tag
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate percentage input
    IF @PercentageOfIncreaseOrDecrease IS NULL
    BEGIN
        RAISERROR ('Percentage cannot be null.', 16, 1);
        RETURN;
    END

    -- Declare variable to store total affected rows
    DECLARE @TotalAffectedRows INT = 0;

   

    -- Update 2: Products by TagId
    IF @TagId IS NOT NULL
    BEGIN
        UPDATE p
        SET price = CASE 
                        WHEN price < 10 THEN ROUND(price * (1 + @PercentageOfIncreaseOrDecrease / 100) * 4, 0) / 4
                        WHEN price < 100 THEN ROUND(price * (1 + @PercentageOfIncreaseOrDecrease / 100) * 2, 0) / 2
                        ELSE ROUND(price * (1 + @PercentageOfIncreaseOrDecrease / 100) / 5, 0) * 5
                    END
        FROM [dbo].[Products] p
        INNER JOIN [dbo].[ProductTags] pt ON p.Id = pt.ProductId
        WHERE pt.TagId = @TagId;
        
        SET @TotalAffectedRows = @TotalAffectedRows + @@ROWCOUNT;
    END
	ELSE
	BEGIN

		 -- Update 1: Products by ProductId, CategoryId, or BrandId
		UPDATE [dbo].[Products]
		SET price = CASE 
						-- For small prices (< 10), round to nearest 0.25
						WHEN price < 10 THEN ROUND(price * (1 + @PercentageOfIncreaseOrDecrease / 100) * 4, 0) / 4
						-- For medium prices (< 100), round to nearest 0.50
						WHEN price < 100 THEN ROUND(price * (1 + @PercentageOfIncreaseOrDecrease / 100) * 2, 0) / 2
						-- For larger prices (>= 100), round to nearest 5.00
						ELSE ROUND(price * (1 + @PercentageOfIncreaseOrDecrease / 100) / 5, 0) * 5
					END
		WHERE (@ProductId IS NULL OR Id = @ProductId)       
		  AND (@CategoryId IS NULL OR ProductCategoryId = @CategoryId)
		  AND (@BrandId IS NULL OR BrandId = @BrandId);
    
		SET @TotalAffectedRows = @TotalAffectedRows + @@ROWCOUNT;


	END

    -- Return total affected rows
    SELECT @TotalAffectedRows AS AffectedRows;
END
GO
/****** Object:  StoredProcedure [dbo].[zeytin_insertImageFiles]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[zeytin_insertImageFiles] 
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

 
 INSERT INTO [dbo].[FileStorages] 
    ([Name], [CreatedDate], [UpdatedDate], [IsActive], [Position], 
     [FileName], [FileUrl], [MimeType], [FileSize], [Width], [Height], 
     [Type], [Lang], [IsFileExist])
SELECT  
    po.label AS Name, 
    GETDATE() AS CreatedDate,
    GETDATE() AS UpdatedDate,
    1 AS IsActive, 
    ROW_NUMBER() OVER (PARTITION BY p.Id ORDER BY p.Id) AS Position,
    COALESCE(PARSENAME(po.picture1Path, 1), 'no-file-name') AS FileName,  -- Handle NULL or empty picture1Path
    po.picture1Path AS FileUrl,
    LOWER(RIGHT(po.picture1Path, CHARINDEX('.', REVERSE(po.picture1Path)) - 1)) AS MimeType, -- Extract extension
    0 AS FileSize,  -- Set default; update later if size is available
    NULL AS Width, 
    NULL AS Height,
    'ProductMainImage' AS Type, 
    1 AS Lang,
    1 AS IsFileExist
FROM [dbo].[Products] p
INNER JOIN ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
WHERE po.picture1Path IS NOT NULL AND po.picture1Path <> ''

UNION ALL

SELECT  
    po.label AS Name, 
    GETDATE(), GETDATE(), 1, 
    ROW_NUMBER() OVER (PARTITION BY p.Id ORDER BY p.Id),
    COALESCE(PARSENAME(po.picture2Path, 1), 'no-file-name') AS FileName,
    po.picture2Path AS FileUrl,
    LOWER(RIGHT(po.picture2Path, CHARINDEX('.', REVERSE(po.picture2Path)) - 1)),
    0, NULL, NULL, 'ProductGallery', 1, 1
FROM [dbo].[Products] p
INNER JOIN ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
WHERE po.picture2Path IS NOT NULL AND po.picture2Path <> ''

UNION ALL

SELECT  
    po.label AS Name, 
    GETDATE(), GETDATE(), 1, 
    ROW_NUMBER() OVER (PARTITION BY p.Id ORDER BY p.Id),
    COALESCE(PARSENAME(po.picture3Path, 1), 'no-file-name') AS FileName,
    po.picture3Path AS FileUrl,
    LOWER(RIGHT(po.picture3Path, CHARINDEX('.', REVERSE(po.picture3Path)) - 1)),
    0, NULL, NULL, 'ProductGallery', 1, 1
FROM [dbo].[Products] p
INNER JOIN ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
WHERE po.picture3Path IS NOT NULL AND po.picture3Path <> ''

UNION ALL

SELECT  
    po.label AS Name, 
    GETDATE(), GETDATE(), 1, 
    ROW_NUMBER() OVER (PARTITION BY p.Id ORDER BY p.Id),
    COALESCE(PARSENAME(po.picture4Path, 1), 'no-file-name') AS FileName,
    po.picture4Path AS FileUrl,
    LOWER(RIGHT(po.picture4Path, CHARINDEX('.', REVERSE(po.picture4Path)) - 1)),
    0, NULL, NULL, 'ProductGallery', 1, 1
FROM [dbo].[Products] p
INNER JOIN ProductsOlive po ON LTRIM(RTRIM(p.Name)) = LTRIM(RTRIM(po.label))
WHERE po.picture4Path IS NOT NULL AND po.picture4Path <> '';


END
GO
/****** Object:  StoredProcedure [dbo].[zeytin_ProductCategoriesSave]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[zeytin_ProductCategoriesSave] 
	 @Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

-- Insert distinct main categories
INSERT INTO ProductCategories (ParentId, Name, CreatedDate, UpdatedDate, IsActive, Position, Lang)
SELECT DISTINCT 
    0 AS ParentId, -- Root categories have no parent
    mainCategory AS Name,
    GETDATE() AS CreatedDate,
    GETDATE() AS UpdatedDate,
    1 AS IsActive,
    1 AS Position,
    1 AS Lang
FROM ProductsOlive
WHERE mainCategory IS NOT NULL;

-- Insert distinct categories under main categories
INSERT INTO ProductCategories (ParentId, Name, CreatedDate, UpdatedDate, IsActive, Position, Lang)
SELECT DISTINCT 
    mc.Id AS ParentId,
    po.category AS Name,
    GETDATE() AS CreatedDate,
    GETDATE() AS UpdatedDate,
    1 AS IsActive,
    1 AS Position,
    1 AS Lang
FROM ProductsOlive po
JOIN ProductCategories mc ON po.mainCategory = mc.Name
WHERE po.category IS NOT NULL;

-- Insert distinct subcategories under categories
INSERT INTO ProductCategories (ParentId, Name, CreatedDate, UpdatedDate, IsActive, Position, Lang)
SELECT DISTINCT 
    c.Id AS ParentId,
    po.subCategory AS Name,
    GETDATE() AS CreatedDate,
    GETDATE() AS UpdatedDate,
    1 AS IsActive,
    1 AS Position,
    1 AS Lang
FROM ProductsOlive po
JOIN ProductCategories c ON po.category = c.Name
WHERE po.subCategory IS NOT NULL;


END
GO
/****** Object:  StoredProcedure [dbo].[zeytin_tagInsert]    Script Date: 8/24/2026 12:02:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[zeytin_tagInsert]
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
INSERT INTO Tags (TagCategoryId, Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, UpdateUserId, AddUserId)
SELECT DISTINCT 2020, REPLACE(brand, 'TlosFARM-', ''), GETDATE(), GETDATE(), 1, 1, 1, NULL, NULL
FROM ProductsOlive
WHERE brand LIKE 'TlosFARM-%'
AND NOT EXISTS (
    SELECT 1 FROM Tags WHERE Name = REPLACE(brand, 'TlosFARM-', '') AND TagCategoryId = 2020
);

INSERT INTO Tags (TagCategoryId, Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, UpdateUserId, AddUserId)
SELECT DISTINCT 2019, REPLACE(brand, 'ZeytinHANIM-', ''), GETDATE(), GETDATE(), 1, 1, 1, NULL, NULL
FROM ProductsOlive
WHERE brand LIKE 'ZeytinHANIM-%'
AND NOT EXISTS (
    SELECT 1 FROM Tags WHERE Name = REPLACE(brand, 'ZeytinHANIM-', '') AND TagCategoryId = 2019
);


END
GO
USE [master]
GO
ALTER DATABASE [eimece] SET  READ_WRITE 
GO
