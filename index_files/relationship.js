//http://localhost:5984/bedroom/_design/files/_view/relationships
$(function(){

	$(".progress").hide();
	$("#get-button").click(function(){
		var getRelationshipUrl = 'http://localhost:5984/bedroom/_design/files/_view/relationships';
		var getDocumentUrl = 'http://localhost:5984/bedroom/_design/files/_view/docs?key=';

		$(".progress").show();
		$.get(getRelationshipUrl, function(data){
			data.rows.forEach(function(d){
				//relationships with parentId exists
				if(d.value.parentId != null){
					console.log(d);
					var getUrl = getDocumentUrl + '%22' + '%22';
					$.get(getUrl,function(docData){
						console.log(docData);
					});
				}
			});

			$("#result").text(JSON.stringify(data));
			$(".progress").hide();
		});
	});
	
})