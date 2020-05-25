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
function addMarker(props, newFlight) {
    let marker = new google.maps.Marker({
        id: props.id,
        position: props.coords,
        content: props.content,
        map: window.map
    });
    //Adding a marker to list of map markers.
    window.markers.push(marker);
    let clicked = false;
    let infoWindow;
    var flightPlanCoordinates = [
        { lat: 32.0055, lng: 34.8854 },
        { lat: 32.005859, lng: 34.855610 },
        { lat: -18.142, lng: 178.431 },
        { lat: -27.467, lng: 153.027 }
    ];
    var flightPath = new google.maps.Polyline({
        path: flightPlanCoordinates,
        geodesic: true,
        strokeColor: '#FF0000',
        strokeOpacity: 1.0,
        strokeWeight: 2
    });
    marker.addListener('click', function () {
            //Buttons first click.
            if (clicked == false) {
                clicked = true;
                infoWindow.open(map, marker);
                displayCurrentFlightPlan(newFlight, flightPath);
                marker.setIcon('https://www.google.com/mapfiles/marker_green.png');
                
                //flightPath.setMap(map);
                //Button second click ("disable").
            } else {
                clicked = false;
                infoWindow.close(map, marker);
                //displayCurrentFlightPlan(newFlight);
                marker.setIcon('https://www.google.com/mapfiles/marker_yellow.png');
                printSegments(null, flightPath);

            }
    });

    //Adds content if exists.
    if (props.content) {
        infoWindow = new google.maps.InfoWindow({
            content: props.content
        });
    }

}

function printSegments(flightPlan, flightPath) {
    
    if (flightPlan == null) {
        flightPath.setMap(null);
    } else {
        flightPath.setMap(window.map);
    }
}

//Adding/Updating markers in map.
function addOrUpdateMarker(flight, newFlight) {
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
    }, newFlight);
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