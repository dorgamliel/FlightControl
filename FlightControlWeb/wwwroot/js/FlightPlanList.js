document.getElementById("demo").innerHTML = "working"
let t = new Date().toISOString();
updateFlightList();

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
    let HTMLFlightList = document.getElementById("flightList");
    HTMLFlightList.innerHTML = '';
    for (flight of flightList) {
        let newFlight = document.createElement('li');
        newFlight.setAttribute('onclick', 'displayCurrentFlightPlan(this)');
        let onlyFlightID = document.createElement('p');
        onlyFlightID.style.display = 'none';
        onlyFlightID.id = 'onlyFlightID';
        let flightID = document.createElement('p');
        let airline = document.createElement('p');
        onlyFlightID.innerHTML = JSON.stringify(flight.flight_id)
        flightID.innerHTML = 'Flight ID: ' + JSON.stringify(flight.flight_id);
        airline.innerHTML = 'Airline: ' + JSON.stringify(flight.company_name);
        newFlight.append(onlyFlightID);
        newFlight.append(flightID);
        newFlight.append(airline);
        HTMLFlightList.append(newFlight);
        addOrUpdateMarker(flight, newFlight);
    }
}

async function displayCurrentFlightPlan(flight) {
    let flightID = JSON.parse(flight.children[0].innerHTML);
    let request = new Request('/api/FlightPlan/' + flightID);
    let response = await fetch(request);
    response = await response.json();
    let flightPlan = JSON.parse(JSON.stringify(response));
    try {
        assignFlightPathView(flightPlan);
        document.getElementById('errorMsg').style.display = 'initial';
    }
    catch (err) {
        document.getElementById('currentFlightPlan').style.display = 'none';
        let error = document.getElementById('errorMsg');
        error.innerHTML = 'ERROR: could not find flight details.';
        error.style.display = 'initial';
    }
}


function assignFlightPathView(flightPlan) {
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
    document.getElementById("flightID").innerHTML = 'Flight: ' + flightID;
    document.getElementById("airline").innerHTML = 'Airline: ' + airline;
    document.getElementById("passengers").innerHTML = 'passengers: ' + passengers;
    document.getElementById("originLongitude").innerHTML = 'Longitude: ' + origin.longitude;
    document.getElementById("originLatitude").innerHTML = 'Latitude: ' + origin.latitude;
    document.getElementById("destLongitude").innerHTML = 'Longitude: ' + destination.longitude;
    document.getElementById("destLatitude").innerHTML = 'Latitude: ' + destination.latitude;
    document.getElementById("departureTime").innerHTML = "DEPARTURE TIME: " + depTime.toUTCString();
    document.getElementById("arrivalTime").innerHTML = "ARRIVAL TIME: " + arrivalTime.toUTCString();
    currentFlightPlan.style.display = 'initial'
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
    var str = 'hi';
    let car = (JSON.stringify(fileData));
    await xhttp.send(fileData);
}

