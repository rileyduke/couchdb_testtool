//http://localhost:5984/bedroom/_design/files/_view/relationships
var missing = [];
$(function(){

	$(".progress").hide();
	$("#get-button").click(function(){
		var getRelationshipUrl = 'http://localhost:5984/bedroom/_design/_repo/_view/relationships';
		var getRelationshipCountUrl = 'http://localhost:5984/bedroom/_design/files/_view/relationships_count';
		var getDocumentUrl = 'http://localhost:5984/bedroom/_design/_repo/_view/documents?key=';

		var relationshipCount = 0;
		var pageSize = 2;

		var getSequence = [];

		$(".progress").show();
		$(".progress-bar").css("width","0%")
		$.get(getRelationshipCountUrl)
		.done(function(count){
			relationshipCount = count.rows[0].value;
			for(i=0; i<=relationshipCount; i+=pageSize){
				$.get(getRelationshipUrl + "?skip=" + i + "&take=" + pageSize)
				.done(function(data){
					$(".progress-bar").css("width", (100*(i/relationshipCount)) + "%")
					data.rows.forEach(function(d){
						//relationships with sourceId exists
						if(d.value.sourceId != null){
							var getUrl = getDocumentUrl + '%22' + d.value.sourceId + '%22';
							$.get(getUrl,function(x){dumpData(x,d)});
						}

						//relationship has targetId
						if(d.value.targetId != null){
							var getUrl = getDocumentUrl + '%22' + d.value.targetId + '%22';
							$.get(getUrl,function(x){dumpData(x,d)});
						}
					});
				});
				
			}
		})
		// $.get(getRelationshipUrl, function(data){
		// 	data.rows.forEach(function(d){
		// 		//relationships with parentId exists
		// 		if(d.value.sourceId != null){
		// 			var getUrl = getDocumentUrl + '%22' + d.value.sourceId + '%22';
		// 			$.get(getUrl,function(x){dumpData(x,d)});
		// 		}

		// 		if(d.value.targetId != null){
		// 			var getUrl = getDocumentUrl + '%22' + d.value.targetId + '%22';
		// 			$.get(getUrl,function(x){dumpData(x,d)});
		// 		}
		// 	});

		// 	$("#result").text(JSON.stringify(data));
		// 	$(".progress").hide();
		// 	console.log(missing);
		// });
	});
	
});

function dumpData(docData,d){
	if(docData.rows.length > 0){
		console.log(d.id, docData.rows[0].value);
	}else{
		//file is missing
		missing.push(d);
	}
}