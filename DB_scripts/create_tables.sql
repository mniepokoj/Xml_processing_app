USE [xml_projectDB]
GO

/****** Object:  Table [dbo].[XMLTable]    Script Date: 25.05.2022 19:50:31 ******/
CREATE TABLE [dbo].[Account](
	[AccountId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[AccountName] [varchar](255) NOT NULL UNIQUE,
	[Password] [varchar](255) NOT NULL)

CREATE TABLE [dbo].[XMLTable](
	[XmlId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name] [varchar](255) NOT NULL UNIQUE,
	[XmlColumn] [xml] NULL)

CREATE TABLE [dbo].[XmlTable_log]
(
	[XmlID] [int] NULL,
	[OperationDate] [date] NULL,
	[Operation] [varchar](255) NOT NULL
)


CREATE TABLE [dbo].[XML_account]
(
	[AccountID] [int] NOT NULL FOREIGN KEY REFERENCES Account(AccountId),
	[XmlID] [int] NOT NULL FOREIGN KEY REFERENCES XMLTable(XmlId)
)