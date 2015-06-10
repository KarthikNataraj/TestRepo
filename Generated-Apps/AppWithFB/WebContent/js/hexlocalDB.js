
var localDB = null;
var dbName = null;
function initializeDatabase(shortName, version, displayName, maxSize){
	//shortName="demoDatabase";
	try {
		if(!window.openDatabase){
			dbSupport = false;
			//alert("Database is not supported");
		}
		else {
			localDB = window.openDatabase(shortName, version, displayName, maxSize);
			dbSupport = true;
			updateStatus("Database initialized");
		}
	} catch(e) {
		updateStatus("Error: Unable to initialize the database. Reason: " + e + ".");
	}
}

function createTable(query,database,createTableSuccessHandler,createTableErrorHandler){
    //var query = 'CREATE TABLE IF NOT EXISTS mgenie(id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,userid VARCHAR NOT NULL,ticketid VARCHAR NOT NULL, status VARCHAR NOT NULL, comments VARCHAR NOT NULL, action VARCHAR NOT NULL);';
      //windowLog.info('createTable --> ' + query);
	updateStatus("create table called");        
	//initializeDatabase(database, "1.0", "Demo", 200000);
	//updateStatus("create after  init");     
	if(createTableSuccessHandler==null) {		  
		createTableSuccessHandler=success;
	}
	if(createTableErrorHandler==null) {
		createTableErrorHandler=error;
	}
    try {
		updateStatus("localDB"+localDB);       
        localDB.transaction(function(transaction){
		//updateStatus("inside transaction");     
        transaction.executeSql(query, [], createTableSuccessHandler, createTableErrorHandler);
            updateStatus("Info: Table is created succesfully.");            
        });        
    } catch (e) {
        updateStatus("Error: Unable to create a Table. Reason: " + e + ".");        
    }
}

function insertRecord(query,args,database,insertSuccess,insertError){
    //var query = "insert into mgenie (userid, ticketid, status, comments, action) VALUES (?,?,?,?,?);";
	//initializeDatabase(database, "1.0", "Demo", 200000);
    var result = false;
	if(insertSuccess==null) {
		insertSuccess=success;
	}
	if(insertError==null) {
		insertError=error;
	}
    try {
		updateStatus("insertRecord localDB"+localDB);
		localDB.transaction(function(transaction){
            transaction.executeSql(query, args, insertSuccess, insertError);
			updateStatus("Inside Insert Transaction");
        });
    } catch (e) {
        updateStatus("Error: Unable to execute Insert Query. Reason : " + e + ".");
    }
}

function execute(query,args,database,executeSuccess,executeError) {
    var result;
	//initializeDatabase(database, "1.0", "Demo", 200000);
	if(executeSuccess==null) {
		executeSuccess=success;
	}
	if(executeError==null) {
		executeSuccess=error;
	}
    try {
		result = localDB.transaction(function(transaction){
            transaction.executeSql(query, args, executeSuccess, executeError);
        });
    } catch (e) {        
        updateStatus("Error: Unable to execute Query. Reason : " + e + ".");
    }
}

function executeSelect(query, args,database,selectTableSuccess,selectTableError){ 
	//initializeDatabase(database, "1.0", "Demo", 200000);
    var resultSet=new Array();
	if(selectTableSuccess==null) {		
		selectTableSuccess=selectSuccess;
	}
	if(selectTableError==null) {
		selectTableError=error;
	}
    try { 
        localDB.transaction(function(transaction){
            transaction.executeSql(query, args, selectTableSuccess, selectTableError);            
        });
    } catch (e) {
        updateStatus("Error: Unable to execute Select Query. Reason : " + e + ".");
    }   
}

function dropTable(tableName,database,dropTableSucces,dropTableError){
	//initializeDatabase(database, "1.0", "Demo", 200000);
    var query = 'drop table ' + tableName + ';';
    if(dropTableSucces==null) {
		dropTableSucces=success;
	}
	if(dropTableError==null) {
		dropTableError=error;
	}
    try {
        localDB.transaction(function(transaction){
            transaction.executeSql(query, [], dropTableSucces, dropTableError);
        });
        return true;
    } catch (e) {
        updateStatus("Error: Unable to drop table. Reason: " + e + ".");
        return false;
    }
}

success=function(transaction, results) {
    updateStatus(results.rowsAffected+" rows affected.");                    
}

selectSuccess=function(transaction, results) {
    updateStatus(results.rows.length +" Records Found. ");                    
}

error=function(transaction, error) {
	updateStatus("Error:  "+error.message);
}

function updateStatus(message){
    window.status += message+'\n';
    if( document.getElementById('statusLog') ) {
		document.getElementById('statusLog').innerHTML = window.status;
    }
}

/*Offline Storage block starts*/
function storeLocal(args, methodName, dbName, onResult, onError){
	args.deviceType = platform;
	initializeDatabase(dbName, "1.0", dbName, 200000);
	var query = 'CREATE TABLE IF NOT EXISTS offlineTable(id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, method VARCHAR NOT NULL, args VARCHAR NOT NULL, status VARCHAR NOT NULL);';
	createTable(query, dbName, onCreateResult, onCreateError);
	var insertQuery = 'INSERT INTO offlineTable (method, args, status) VALUES (?, ?, ?);';
	var myArgs = new Array();
	myArgs[0] = methodName;
	myArgs[1] = JSON.stringify(args);
	myArgs[2] = "offline";
	insertRecord(insertQuery, myArgs, dbName, onInsertSuccess, onInsertError);
	alert("Device Offline: Stored locally");
}

onCreateResult=function reqOnSuccess(req) {
	//MsgDisplay("Response onCreateResult "+req.responseText);
};
onCreateError=function(req) {
	//MsgDisplay('onCreateError Error!\nStatusText='+req.statusText+'\nContents='+req.responseText);
};
onInsertSuccess=function(transaction, results) {
    updateStatus("onInsertSuccess : "+results.rowsAffected+" rows affected.");                    
}
onInsertError=function(transaction, error) {
	updateStatus("onInsertError: "+error.message);
}

function syncAll(dbName) {
	initializeDatabase(dbName, "1.0", dbName, 200000);
	var createQuery = 'CREATE TABLE IF NOT EXISTS offlineTable(id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, method VARCHAR NOT NULL, args VARCHAR NOT NULL, status VARCHAR NOT NULL);';
	if(navigator.onLine && dbSupport){
		createTable(createQuery, dbName, onCreateResult, onCreateError);
	}
	var query = 'SELECT * from offlineTable WHERE status="offline";';
	if(navigator.onLine && dbSupport){
		executeSelect(query, [], dbName, selectSyncSuccess, selectSyncError);
	} else
		updateStatus("Device may be Offline or Database may not be supported");   
}

selectSyncSuccess=function(transaction, results) {
	var rowCount = results.rows.length;
    updateStatus(rowCount+" Records Found. "); 
	var methods = '';
	var param = '';
	if(rowCount != 0) {
		for (var i = 0; i <= rowCount-1; i++) {	
			var row = results.rows.item(i);
			pos = row['id'];
			var method = row['method'];
			var args = new Object();
			args = row['args'];
			var json = JSON.parse(args);
			if(i==rowCount-1) {
				methods += method;
				param += args;
			} else {
				methods += method+'|';
				param += args+'|';
			}
		}
		invokeSyncAdapter(methods, param);
	} else {
		//MsgDisplay("No Records found Offline");
	}
}

selectSyncError=function(req){
	//MsgDisplay('selectSyncError Error!\nStatusText='+req.statusText+'\nContents='+req.responseText);
};


onResultSync=function reqOnSuccess(req)
{
	//MsgDisplay("Response onResultSync "+req.responseText);
	var query = "DELETE FROM offlineTable";
	execute(query, [], dbName, onDeleteResult, onDeleteError);
};

onErrorSync=function(req){
	MsgDisplay('onErrorSync Error!\nStatusText='+req.statusText+'\nContents='+req.responseText);
	//var query = "DELETE FROM offlineTable";
	//execute(query, [], dbName, success, error);
};

onDeleteResult=function reqOnSuccess(req) {
		//MsgDisplay("Response onDeleteResult "+req.responseText);
	};
onDeleteError=function(req) {
	//MsgDisplay('onDeleteError Error!\nStatusText='+req.statusText+'\nContents='+req.responseText);
	};
onResult=function reqOnSuccess(req)
{
	//MsgDisplay("Response onResultSync "+req.responseText);
};

onError=function(req){
	//MsgDisplay('onErrorSync Error!\nStatusText='+req.statusText+'\nContents='+req.responseText);
	//var query = "DELETE FROM offlineTable";
	//execute(query, [], dbName, success, error);
};
/*Offline Storage block ends*/