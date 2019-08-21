$(document).ready(function() {
	initActiveMenuItem();
	initButtons();
	//fetchNumComments();
	renderComments();
});

function renderComments() {
	if (Globals.comments !== undefined) {
		for (const i in Globals.comments) {
			if (Globals.comments.hasOwnProperty(i)) {
				const c = Globals.comments[i];
				console.log(c);
				$('#comments').append(
					`<p class="comment outset">
						<span class="date">${new Date(c.date * 1000).toDateString()}</span>
						<b>${c.author || 'Anonymous'}</b> <br><br> <span>${c.comment}</span>
					</p>`);
			}
		}
	}
}

function initActiveMenuItem() {
	let hasFoundItem = false;
	$('#hmenu li a, #vmenu.li a').each((i, a) => {
		const pageId = $(a).text().replace(" ", "-").toLowerCase();
		
		if (window.location.href.indexOf(pageId) !== -1) {
			$(a).addClass("active");
			hasFoundItem = true;
		}
	});
	if (!hasFoundItem) {
		$('#hmenu li:first-child a, #vmenu li:first-child a').addClass("active");
	}
}

function initButtons() {
	$('#toggle_vmenu').click(() => {
		$('#vmenu').toggle();
	});
	
	$('#add_comment_button').click((e) => {
		e.preventDefault();
		
		const comment = $('#input_comment').val();
		
		if (comment.trim().length === 0) {
			$('#modal-message').text('Empty comments are not allowed!');
			$('#myModal').modal('show');
			return;
		}
		
		const name = $('#input_name').val();
		
		//console.log(name, comment);
		window.location = Globals.databaseUrl + '?command=saveComment'
			+ `&subjectId=${Globals.blogPostId}`
			+ `&author=${encodeURIComponent(name)}`
			+ `&comment=${encodeURIComponent(comment)}`
			+ `&returnBaseUrl=${encodeURIComponent(window.location.origin + window.location.pathname)}`;
	});
}

function fetchNumComments() {
	const articleIds = [];
	$('article').each((i, article) => {
		const articleId = $(article).attr('itemid');
		if (articleId) {
			articleIds.push(articleId);
		}
	});
	if (articleIds.length === 0) {
		return;
	}
	
	const baseUrl = Globals.databaseUrl + '?command=numComments&subjects[]=';
	const fetchUrl = baseUrl + articleIds.join('&subjects[]=');
	$.get(fetchUrl)
		.then(response => {
			for (const key in response.result) {
				const numComments = response.result[key];
				$(`article[itemid=${key}] .comments`)
					.html(numComments + " " + (numComments === 1 ? "Comment" : "Comments"));
			}
		});
}
