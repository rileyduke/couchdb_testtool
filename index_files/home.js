$(function(){

	$(".progress").hide();
	// $.ajax({
	// 	url: 'http://localhost:5984/bedroom/_design/files/_view/map_and_reduce?group_level=1&group_level=2',
	// 	type: 'GET',
	// 	jsonp: false,
	// 	contentType: 'application/json',
	// 	success: function(result){
	// 		alert("done");
	// 	},
	// 	dataType: 'jsonp',
	// 	error: function (jqXHR, text, errorThrown) {
	// 		console.log(jqXHR + " " + text + " " + errorThrown);
	// 	}
	// });
	$("#get-button").click(function(){
		var url = $("#get-url").val();

		$(".progress").show();
		$.get(url, function(data){

			$(".progress").hide();
			console.log(data);

			$("#result").text(JSON.stringify(data));
		});
	});
	
})