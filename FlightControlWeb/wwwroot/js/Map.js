function initMap() {
    var options = {
        zoom: 13,
        center: { lat: 32.0055, lng: 34.8854 }
    }
    //Map object
    var map = new google.maps.Map(document.getElementById('map'), options);
    //Array of markers.
    var markers = [
        {
            coords: { lat: 32.0055, lng: 34.8854 },
            content: 'hello<h1>world</h1>'
        }
    ];
    //Loop markers.
    for (var i = 0; i < markers.length; i++) {
        addMarker(markers[i]);
    }
    //Add Marker function.
    function addMarker(props) {
        var marker = new google.maps.Marker({
            position: props.coords,
            content: props.content,
            map: map
        });
        //Adds content if exists.
        if (props.content) {
            var infoWindow = new google.maps.InfoWindow({
                content: props.content
            });
            marker.addListener('click', function () {
                infoWindow.open(map, marker);
            });
        }
    }
}