$(document).ready(function() {
    initReadingBar();
});

function initReadingBar() {
    const $readingBarProgress =  $('#readingbar-progress');
    const $document = $(document);
    const $window = $(window);
    $(window).scroll(() => {
        const scrollPercent = 100 * $window.scrollTop() / ($document.height() - $window.height());
        $readingBarProgress.css('width', scrollPercent + '%');
    });
}