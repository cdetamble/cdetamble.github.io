﻿$(document).ready(function() {
	initActiveMenuItem();
	initButtons();
    initFancyBox();
    initMaterialRipple();
    initReadingBar();
    initNews();

    //fetchNumComments();
    renderComments();

    $('#home_image').click(() => {
		window.location.href = Globals.baseUrl + 'blog';
	})
});

function initNews() {
    $('.blogpost-card').click((event) => {
       window.location.href = Globals.baseUrl + 'blog/' + $(event.currentTarget).attr('itemid');
    });
}

function initReadingBar() {
    const $readingBarProgress =  $('#readingbar-progress');
    const $document = $(document);
    const $window = $(window);
    $(window).scroll(() => {
        const scrollPercent = 100 * $window.scrollTop() / ($document.height() - $window.height());
        $readingBarProgress.css('width', scrollPercent + '%');
    });
}

function initFancyBox() {
    const $fancyBoxes = $('.fancybox');
    if ($fancyBoxes.length > 0) {
        $fancyBoxes.fancybox({
            padding: 2,
            closeClick: true,
            openEffect: 'elastic',
            closeEffect: 'elastic',
            helpers: {
                thumbs: {
                    width: 75,
                    height: 50,
                    position: 'bottom'
                }
            },
            overlay : {
                closeClick : true
            },
            afterClose: function() {
                $fancyBoxes.each(function() {
                    $(this).find('img').css('visibility', "visible");
                });
            },
            onUpdate: function() {
                for (var i = 0; i < this.group.length; i++) {
                    $(this.group[i].element).find('img').css('visibility', "visible");
                }
                $(this.element[0]).find('img').css('visibility', "hidden");
            }
        });
        $fancyBoxes.click(function() {
            $(this).find('img').css('visibility', "hidden");
        });
    }
    const $fancyBoxesMedia = $('.fancybox-media');
    if ($fancyBoxesMedia.length > 0) {
        $fancyBoxesMedia.fancybox({
            padding: 2,
            openEffect: 'elastic',
            closeEffect: 'elastic',
            helpers : {
                media : {}
            }
        });
    }
}

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
	let subpage = window.location.pathname.replace(Globals.baseUrl, "");
	if (subpage.endsWith("/")) {
        subpage = subpage.substr(0, subpage.length - 1);
	}
	$('#hmenu li a, #vmenu li a').each((i, a) => {
		const pageId = $(a).text().replace(" ", "-").toLowerCase();
		if (subpage.indexOf(pageId) !== -1) {
			$(a).parent().addClass("active");
			hasFoundItem = true;
		}
	});
	if (!hasFoundItem) {
		$('#hmenu li:first-child, #vmenu li:first-child').addClass("active");
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

function initMaterialRipple() {
    if (!mdc.ripple.util.supportsCssVariables(window)) {
        document.documentElement.classList.add('unsupported');
    }

    [].forEach.call(document.querySelectorAll('.mdc-ripple-surface:not([data-demo-no-js])'), function(surface) {
        mdc.ripple.MDCRipple.attachTo(surface);
    });
}