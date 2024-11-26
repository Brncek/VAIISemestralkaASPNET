window.initMap = function() {
    const location = { lat: 32.2952269, lng: -64.7814626 };
    const map = new google.maps.Map(document.getElementById("map"), {
        zoom: 12,
        center: location,
    });
    const marker = new google.maps.Marker({
        position: location,
        map: map,
    });
}
