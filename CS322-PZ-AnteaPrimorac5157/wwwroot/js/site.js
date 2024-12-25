// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const alertMessage = 'Are you sure you want to delete this confession? This action cannot be undone.';


// Podnosi zahtjev za brisanje ispovijesti putem skrivene forme (u skladu sa HTTP specifikacijama)
function confirmDelete(confessionId) {
    if (confirm(alertMessage)) {
        document.getElementById('deleteForm-' + confessionId).submit();
    }
}

function confirmDeleteComment(confessionId, commentId) {
    if (confirm(alertMessage)) {
        document.getElementById('deleteCommentForm-' + confessionId + '-' + commentId).submit();
    }
}