EXEC delete_xml_doc_proc @user_id=1, @docname=N'books1';

declare @xdoc xml
set @xdoc = '<test><node>1</node><node>2</node><node>3</node></test>'
EXEC insert_xml_doc_proc @user_id=1, @docname=N'testowy2', @xmlcontent=@xdoc;


SELECT * FROM XMLTable
EXEC find_xml_elem_proc @docname='testowy1', @user_id=2, @xpath='test/dwa'
EXEC find_xml_elem_proc @docname='testowy1', @user_id=2, @xpath='(/test/node)[1]'



	