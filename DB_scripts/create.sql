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

GO
IF OBJECT_ID('delete_xml_doc_proc') IS NOT NULL
	DROP PROC delete_xml_doc_proc
IF OBJECT_ID('insert_xml_doc_proc') IS NOT NULL
	DROP PROC insert_xml_doc_proc
GO

-- procedura skladowna delete_xml_doc_proc
-- usuwa dokument z bazy danych
CREATE PROC delete_xml_doc_proc
@docname AS nvarchar(40) = NULL,
@user_id AS INT = NULL
AS
DECLARE @msg AS NVARCHAR(500);
IF @docname IS NULL OR @user_id IS NULL
	BEGIN
	SET @msg = N'A value must be supplied for parameter @docname and @user_id';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
ELSE IF @docname NOT IN (SELECT name from XMLTable)
	BEGIN
	SET @msg = N'There is no document with given name in database.';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
ELSE IF @user_id NOT IN (SELECT Account.AccountId from Account)
	BEGIN
	SET @msg = N'There is no document with given name in database.';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
declare @xmlId INT;
SET @xmlId = (SELECT XmlId FROM XMLTable WHERE XMLTable.Name = @docname);
IF @user_id NOT IN (SELECT XML_account.AccountID FROM XML_account WHERE XML_account.XmlID = @xmlId)
	BEGIN
	SET @msg = N'You do not have access to this document.';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
ELSE
	DELETE from XML_account WHERE XML_account.XmlID=@xmlId;
	DELETE FROM XMLTable WHERE XMLTable.XmlId = @xmlId;
	RETURN;
GO


-- procedura skladowna insert_xml_doc_proc
-- dodaje dokument do bazy danych
CREATE PROC insert_xml_doc_proc
@docname AS nvarchar(40) = NULL,
@xmlContent AS xml = NULL,
@user_id AS INT = NULL
AS
DECLARE @msg AS NVARCHAR(500);
IF @docname IS NULL OR @xmlContent IS NULL OR @user_id IS NULL
	BEGIN
	SET @msg = N'A value must be supplied for parameter @docname, @user_id and @xmlContent';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
ELSE IF @docname IN (SELECT name from XMLTable)
	BEGIN
	SET @msg = N'Document with given name already exists in database.';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
ELSE IF @user_id NOT IN (SELECT Account.AccountId from Account)
	BEGIN
	SET @msg = N'There is no document with given name in database.';
	RAISERROR(@msg, 16, 1);
	RETURN;
	END
ELSE
	INSERT INTO XMLTable VALUES(@docname, @xmlContent);
	declare @xmlId INTEGER;
	SET @xmlId = (SELECT XMLTable.XmlId FROM XMLTable WHERE XMLTable.Name=@docname);
	INSERT INTO XML_account(XmlID, AccountID) VALUES(@xmlId, @user_id);
	RETURN;
GO


GO
CREATE TRIGGER XML_modification_trigger ON XMLTable
AFTER DELETE, INSERT
AS
BEGIN
	INSERT INTO XmlTable_log(XmlID, OperationDate, Operation)
			SELECT del.XmlID, GETDATE(), 'DELETE'
			FROM deleted del
		UNION ALL
			SELECT ins.XmlID, GETDATE(), 'INSERTED'
			FROM inserted ins
END
GO
CREATE TRIGGER XMLupdate_trigger ON XMLTable
AFTER UPDATE 
AS
BEGIN
	INSERT INTO XmlTable_log(XmlID, OperationDate, Operation)
			SELECT ins.XmlID, GETDATE(), 'UPDATE'
			FROM inserted ins
END
GO

INSERT INTO account(AccountName, Password) VALUES('user1', 'password')
INSERT INTO account(AccountName, Password) VALUES('user2', 'password')
GO