
$(document).ready(function() {
    // adds hover effect to cards
    $('.card').hover(
        function() {
            $(this).addClass('shadow');
        },
        function() {
            $(this).removeClass('shadow');
        }
    );

    // adds click effect to buttons
    $('.btn-primary').click(function() {
        $(this).addClass('active');
        setTimeout(() => {
            $(this).removeClass('active');
        }, 200);
    });
});
