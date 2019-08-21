if (!window.location.search) {
	window.location = Globals.databaseUrl + "?command=getComments&subjectId=" + Globals.blogPostId
		+ "&returnBaseUrl=" + window.location.origin + window.location.pathname;
} else {
	if (window.location.search.indexOf('result') !== -1) {
		let search = window.location.search;
		search = search
			.substr("?result=".length)
			.replace(/%22/g, "\"");
		Globals.comments = JSON.parse(search);
		window.history.replaceState("", "", "?");
	}
}

