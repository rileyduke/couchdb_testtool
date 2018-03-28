//http://localhost:5984/bedroom/_design/files/_view/relationships
var missing = [];
$(function(){

	$(".progress").hide();
	$("#get-button").click(function(){
		var getRelationshipUrl = 'http://localhost:5984/bedroom/_design/_repo/_view/relationships';
		var getDocumentUrl = 'http://localhost:5984/bedroom/_design/_repo/_view/documents?key=';

		$(".progress").show();
		$.get(getRelationshipUrl, function(data){
			data.rows.forEach(function(d){
				//relationships with parentId exists
				if(d.value.sourceId != null){
					var getUrl = getDocumentUrl + '%22' + d.value.sourceId + '%22';
					$.get(getUrl,function(x){dumpData(x,d)});
				}

				if(d.value.targetId != null){
					var getUrl = getDocumentUrl + '%22' + d.value.targetId + '%22';
					$.get(getUrl,function(x){dumpData(x,d)});
				}
			});

			$("#result").text(JSON.stringify(data));
			$(".progress").hide();
			console.log(missing);
		});
	});
	
});

function dumpData(docData,d){
	if(docData.rows.length > 0){
		console.log(docData.rows[0].value);
	}else{
		//file is missing
		missing.push(d);
	}
}