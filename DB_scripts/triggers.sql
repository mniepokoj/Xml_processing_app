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
