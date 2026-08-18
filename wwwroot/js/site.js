document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () { new bootstrap.Alert(alert).close(); }, 5000);
    });
});