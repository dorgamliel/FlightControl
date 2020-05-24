function initMap() {
    //Map properties.
    let properties = {
        zoom: 13,
        center: { lat: 32.0055, lng: 34.8854 }
    }
    //Map object.
    let map = new google.maps.Map(document.getElementById('map'), properties);
    //Array of markers.
    let markers = [
        {
            coords: { lat: 32.0055, lng: 34.8854 },
            content: 'hello<h1>world</h1>'
        }
    ];
    //Loop markers.
    for (let i = 0; i < markers.length; i++) {
        addMarker(markers[i], map);
    }
}

//Add Marker function.
function addMarker(props, map) {
    let marker = new google.maps.Marker({
        position: props.coords,
        content: props.content,
        map: map
    });

    //Adds content if exists.
    if (props.content) {
        let infoWindow = new google.maps.InfoWindow({
            content: props.content
        });
        marker.addListener('click', function () {
            infoWindow.open(map, marker);
        });
    }
}
