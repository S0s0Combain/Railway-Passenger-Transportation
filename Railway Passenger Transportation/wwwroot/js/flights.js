document.getElementById('route').addEventListener('click', function () {
    let routeId = document.getElementById('route').data;
    let routeList = document.findElementById('route-' + routeId);
    routeList.style.visibility = 'visible';
}));