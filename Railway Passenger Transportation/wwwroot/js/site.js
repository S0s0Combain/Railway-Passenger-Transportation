// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById('swap-button').addEventListener('click', function () {
    let departure = document.getElementById('departure');
    let destination = document.getElementById('destination');
    let tmp = departure.value;
    departure.value = destination.value;
    destination.value = tmp;
});