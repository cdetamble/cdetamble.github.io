$(document).ready(function() {
	function findGetParameter(parameterName) {
		var result = null, tmp = [];
		location.search
			.substr(1)
			.split("&")
			.forEach(function (item) {
			  tmp = item.split("=");
			  if (tmp[0] === parameterName) result = decodeURIComponent(tmp[1]);
			});
		return result;
	}
	function validateEmail(email) {
		var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
		return re.test(String(email).toLowerCase());
	}

	const message = findGetParameter('modal_message');
	if (message) {
		$('#myModal').on('hidden.bs.modal', function () {
			window.location.href = "https://mouthlessgames.com";
		});
		const title = findGetParameter('modal_title');
		if (title) {
			$('#modal-title').html(title);
		}
		$('#modal-message').html(message);
		$('#myModal').modal('show');
	}

	$('#myForm').submit(function(e){
		e.preventDefault();
		$('#subscribe').click();
	});
	$('#subscribe').click(function () {
		const mail = $('#mail').val();
		if (mail && mail.length > 0) {
			if (validateEmail(mail)) {
				const subscribedFor = "Evelyn von Wolkenstein Demo Release";
				const successMessage = "Thank you for subscribing!<br><br> On release day, you'll receive <b>a private mail</b> containing instructions on how to download your free copy the game.<br><br> Have a great summer until then!";
				
				window.location.href = "http://therefactory.bplaced.net/api/subscribe.php?mail_address=" + encodeURIComponent(mail)
					+ "&subscribed_for=" + encodeURIComponent(subscribedFor)
					+ "&success_message=" + encodeURIComponent(successMessage);
			}
		}
	});
});