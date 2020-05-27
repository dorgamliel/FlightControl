let polylines = [];



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
    window.map.addListener('click', function() {
        resetFlightPlanView();
        removeExistingPaths();
        resetPlaneMarker();
        removeHighlight();
    });
    
}

//Add Marker function.
function addMarker(props, newFlight) {
    var planeMarker = {
        url: 'resources/plane.png',
        anchor: new google.maps.Point(20, 20),
        scaledSize: new google.maps.Size(40, 40),
    }
    let marker = new google.maps.Marker({
        id: props.id,
        position: props.coords,
        content: props.content,
        map: window.map,
    });
    marker.setIcon(planeMarker)
    //Adding a marker to list of map markers.
    window.markers.push(marker);
    var infoWindow = new google.maps.InfoWindow({
        content: props.id
    });
    //flightPath.setPath(flightPlanCoordinates);
    marker.addListener('click', function () {
        flightClicked(newFlight);
    });
    marker.addListener('mouseover', function () {
        infoWindow.open(window.map, marker)
    })
    marker.addListener('mouseout', function () {
        infoWindow.close();
    })

}

function printSegments(flightPlan) {
    var lineSymbol = {
        path: 'M 0,-1 0,1',
        strokeOpacity: 1,
        strokeColor: 'black',
        scale: 2
    };
    let flightPath = new google.maps.Polyline({
        geodesic: true,
        icons: [{
            icon: lineSymbol,
            offset: '0',
            repeat: '20px'
        }],
        strokeOpacity: 0,
    });
    let arr = [];
    let coupl = {
        "lat": flightPlan.initial_location.latitude,
        "lng": flightPlan.initial_location.longitude
    };
    arr.push(coupl);
    for (let i = 0; i < flightPlan.segments.length; i++) {
        let segPoint = flightPlan.segments[i];
        let couple = { "lat": segPoint.latitude, "lng": segPoint.longitude };
        arr.push(couple);
    }
    flightPath.setPath(arr);
    polylines.push(flightPath);
    flightPath.setMap(window.map);
}


function changeFlightMarker(flightPlan) {
    var planeMarker = {
        url: 'resources/plane.png',
        anchor: new google.maps.Point(20, 20),
        scaledSize: new google.maps.Size(40, 40),
    }
    var clickedPlaneMarker = {
        url: 'resources/plane-yellow.png',
        anchor: new google.maps.Point(30, 30),
        scaledSize: new google.maps.Size(60, 60),
    }
    for (marker of window.markers) {
        if (marker.id == flightPlan.flight_id) {
            marker.setIcon(clickedPlaneMarker);
        } else {
            marker.setIcon(planeMarker);
        }
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
        //content: flight.company_name,
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

function removeExistingPaths() {
    for (polyline of polylines) {
        polyline.setMap(null);
    }
    polylines = [];
}

function resetPlaneMarker() {
    var planeMarker = {
        url: 'resources/plane.png',
        anchor: new google.maps.Point(20, 20),
        scaledSize: new google.maps.Size(40, 40),
    }
    for (marker of window.markers) {
        marker.setIcon(planeMarker);
    }
}