document.getElementById('swap-button').addEventListener('click', function () {
    let departure = document.getElementById('departure');
    let destination = document.getElementById('destination');
    let tmp = departure.value;
    departure.value = destination.value;
    destination.value = tmp;
});
