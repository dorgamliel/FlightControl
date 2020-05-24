function initMap() {
    //Map properties.
    let properties = {
        zoom: 13,
        center: { lat: 32.0055, lng: 34.8854 }
    }
    //Map object.
    window.map = new google.maps.Map(document.getElementById('map'), properties);
    //Array of markers.
    window.markers = [];
}

//Add Marker function.
function addMarker(props) {
    let marker = new google.maps.Marker({
        id: props.id,
        position: props.coords,
        content: props.content,
        map: window.map
    });
    //Adding a marker to list of map markers.
    window.markers.push(marker);

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

//Adding/Updating markers in map.
function addOrUpdateMarker(flight) {
    for (marker of window.markers) {
        if (marker.id == flight.flight_id) {
            marker.setPosition({ lat: flight.latitude, lng: flight.longitude });
            return;
        }
    }
    addMarker({
        coords: { lat: flight.latitude, lng: flight.longitude },
        content: flight.company_name,
        id: flight.flight_id
    });
}

//Removing unactive flights from map.
function removeUnactiveFlights(flightList) {
    //Remove unactive flights from map.
    for (let i = 0; i < window.markers.length; i++) {
        if (flightList.filter(x => x.flight_id === window.markers[i].id).length == 0) {
            window.markers[i].setMap(null);
            window.markers[i].setPosition(null);
            window.markers.splice(i, 1);
            i--;
        }
    }
}

function f1() {
    let x = 0;
}