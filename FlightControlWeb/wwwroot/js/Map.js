let intervalID = window.setInterval(addOrUpdateMarker, 200);

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
async function addOrUpdateMarker() {
    let t = new Date().toISOString();
    let request = new Request('/api/Flights?relative_to=' + t + '&sync_all');
    let flightList
    let response = await fetch(request)
    response = await response.json();
    flightList = JSON.parse(JSON.stringify(response));
    for (let i = 0; i < flightList.length; i++) {
        //Add if new flight.
        if (window.markers.filter(x => x.id === flightList[i].flight_id).length == 0) {
            addMarker({
                coords: { lat: flightList[i].latitude, lng: flightList[i].longitude },
                content: flightList[i].company_name,
                id: flightList[i].flight_id
            });
        //If not new, update existing marker location.
        } else  {
            window.markers[i].setPosition({ lat: flightList[i].latitude, lng: flightList[i].longitude });
        }
    }
    removeUnactiveFlights(flightList);
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