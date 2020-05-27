document.getElementById("demo").innerHTML
updateFlightList();
let currentlyHighlightedFlight = null;

async function updateFlightList() {
    let t = new Date().toISOString();
    let request = new Request('/api/Flights?relative_to=' + t + '&sync_all');
    let flightList
    let response = await fetch(request)
    response = await response.json();
    flightList = JSON.parse(JSON.stringify(response));
    printFlightList(flightList);
    removeUnactiveFlights(flightList);
    setTimeout(updateFlightList, 3000);
}


function printFlightList(flightList) {
    let internalFlighList = document.getElementById("internalFlighList");
    let externalFlightList = document.getElementById('externalFlightList');
    internalFlighList.innerHTML = '';
    externalFlightList.innerHTML = '';
    for (flight of flightList) {
        let newFlight = document.createElement('div');
        newFlight.id = flight.flight_id;
        newFlight.setAttribute('onclick', 'flightClicked(this)');
        let onlyFlightID = document.createElement('p');
        onlyFlightID.style.display = 'none';
        onlyFlightID.id = 'onlyFlightID';
        let flightID = document.createElement('p');
        let airline = document.createElement('p');
        onlyFlightID.innerHTML = JSON.stringify(flight.flight_id);
        flightID.innerHTML = flight.flight_id;
        airline.innerHTML = flight.company_name;
        if (currentlyHighlightedFlight != null) {
            if (flight.flight_id == currentlyHighlightedFlight.flight_id) {
                newFlight.style.border = 'solid';
            }
        }
        newFlight.append(onlyFlightID);
        newFlight.append(flightID);
        newFlight.append(airline);
        if (flight.is_external) {
            externalFlightList.appendChild(newFlight);
        } else {
            internalFlighList.appendChild(newFlight);
            let deleteButton = document.createElement('button');
            deleteButton.setAttribute('onclick', 'deleteFlight(this, event)');
            newFlight.appendChild(deleteButton);
        }
        addOrUpdateMarker(flight, newFlight);
    }
}


async function flightClicked(flight) {
    let flightID = JSON.parse(flight.children[0].innerHTML);
    let request = await new Request('/api/FlightPlan/' + flightID);
    let response = await fetch(request);
    response = await response.json();
    let flightPlan = JSON.parse(JSON.stringify(response));
    displayCurrentFlightPlan(flightPlan);
    removeExistingPaths();
    printSegments(flightPlan);
    changeFlightMarker(flightPlan);
    highlightFlight(flightPlan);
}

async function displayCurrentFlightPlan(flightPlan) {
    let airline = flightPlan.company_name;
    let passengers = flightPlan.passengers;
    let origin = flightPlan.initial_location;
    let destination = flightPlan.segments[flightPlan.segments.length - 1];
    let currentFlightPlan = document.getElementById('currentFlightPlan');
    let flightID = flightPlan.flight_id;
    let depTime = new Date(origin.date_time);
    let flightTime = getArrivalTime(flightPlan.segments);
    let arrivalTimeMs = depTime.getTime() + (flightTime * 1000);
    let arrivalTime = new Date(arrivalTimeMs);
    document.getElementById("flightID").innerHTML = flightID;
    document.getElementById("airline").innerHTML = airline;
    document.getElementById("passengers").innerHTML = passengers;
    document.getElementById("originLongitude").innerHTML = origin.longitude;
    document.getElementById("originLatitude").innerHTML =origin.latitude;
    document.getElementById("destLongitude").innerHTML = destination.longitude;
    document.getElementById("destLatitude").innerHTML = destination.latitude;
    document.getElementById("departureTime").innerHTML = depTime.toUTCString();
    document.getElementById("arrivalTime").innerHTML = arrivalTime.toUTCString();
    currentFlightPlan.style.display = 'initial'
}


function resetFlightPlanView() {
    let currentFlightPlan = document.getElementById('currentFlightPlan');
    currentFlightPlan.style.display = 'none';
}



function getArrivalTime(segments) {
    let flightTime = 0;
    for (seg of segments) {
        flightTime += seg.timespan_seconds;
    }
    return flightTime;
}


function allowDrop(event) {
    event.preventDefault();
}

function drop(event) {
    event.preventDefault();
    let files = event.dataTransfer.files;
    let reader = new FileReader();
    reader.onload = function () {
        postFlightFromFile(reader.result);
    }
    for (file of files) {
        reader.readAsText(file);
        var x = 0;
    }
}

async function postFlightFromFile(fileData) {
    var xhttp = new XMLHttpRequest();
    await xhttp.open("POST", "api/FlightPlan", true);
    xhttp.setRequestHeader("Content-Type", "application/json")
    await xhttp.send(fileData);
}

function removeErrorMsg (button) {
    button.pare
}


function highlightFlight(flightPlan) {
    let FlightElement = document.getElementById(flightPlan.flight_id);
    FlightElement.style.border = 'solid'
    if (currentlyHighlightedFlight != null) {
        FlightElement = document.getElementById(currentlyHighlightedFlight.flight_id);
        FlightElement.style.border = 'none';
    }
    currentlyHighlightedFlight = flightPlan;
}


function removeHighlight() {
    if (currentlyHighlightedFlight == null) {
        return;
    }
    let FlightElement = document.getElementById(currentlyHighlightedFlight.flight_id);
    FlightElement.style.border = 'none';
    currentlyHighlightedFlight = null;
}


async function deleteFlight(deleteButton, event) {
    event.stopPropagation();
    let flightElement = deleteButton.parentElement;
    let flightID = flightElement.id;
    var xhttp = new XMLHttpRequest();
    await xhttp.open("DELETE", "api/Flights/" + flightID, true);
    xhttp.setRequestHeader("Content-Type", "application/json")
    await xhttp.send();
}

