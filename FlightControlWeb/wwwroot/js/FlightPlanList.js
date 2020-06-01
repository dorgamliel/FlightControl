document.getElementById("demo").innerHTML
updateFlightList();
let currentlyHighlightedFlight = null;


// update flight list every 3 seconds
async function updateFlightList() {
    try {
        let t = new Date().toISOString();
        // get list of all active flights
        let request = new Request('/api/Flights?relative_to=' + t + '&sync_all');
        let flightList
        let response = await fetch(request)
        response = await response.json();
        flightList = JSON.parse(JSON.stringify(response));
        printFlightList(flightList);
        // remove markers of unactive flights from map
        removeUnactiveFlights(flightList);
        setTimeout(updateFlightList, 3000);
    }
    catch {
        let errormsg = document.getElementById('errorTxt');
        errormsg.innerHTML = 'error updating flight list';
        errormsg.parentElement.style.display = 'initial';
    }
}

// insert all the flights from 'flightList' to appropriate displayed list
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
        let flightID = document.createElement('div');
        let airline = document.createElement('div');
        onlyFlightID.innerHTML = JSON.stringify(flight.flight_id);
        flightID.innerHTML = flight.flight_id;
        airline.innerHTML = flight.company_name;
        flightID.className = 'flightInList'
        airline.className = 'airlineInList'
        flightID.style.float = 'left';
        airline.style.float = 'right';
        if (currentlyHighlightedFlight != null &&
            flight.flight_id == currentlyHighlightedFlight.flight_id) {
            //TODO: different and better style
            newFlight.style.border = '0.5px solid';
            newFlight.style.fontWeight = 'bold'
            newFlight.style.backgroundColor = '#87CEEB';
            newFlight.style.height = '30px';
        } else {
            newFlight.style.height = '29px';
            newFlight.style.borderBottom = '0.5px dashed #D0D0D0';
        }
        newFlight.style.cursor = 'pointer';
        newFlight.append(onlyFlightID);
        if (flight.is_external) {
            flightID.className = 'extflightInList';
            externalFlightList.appendChild(newFlight);
        } else {
            internalFlighList.appendChild(newFlight);
            let deleteButton = document.createElement('button');
            deleteButton.className = 'btn btn-danger btn-sm rounded-0';
            let deleteIcon = document.createElement('i');
            deleteIcon.className = 'fa fa-trash';
            deleteButton.appendChild(deleteIcon);
            deleteButton.setAttribute('onclick', 'deleteFlight(this, event)');
            newFlight.appendChild(deleteButton);
            deleteButton.style.height = '28px';
            deleteButton.style.width = '28px';
            deleteButton.style.float = 'left';
            deleteButton.style.marginLeft = '0.4px';
        }
        addOrUpdateMarker(flight, newFlight);
        newFlight.append(flightID);
        newFlight.append(airline);
    }
}


// event of a flight clicked from list or from marker
async function flightClicked(flight) {
    let flightID = JSON.parse(flight.children[0].innerHTML);
    let request = await new Request('/api/FlightPlan/' + flightID);
    let response = await fetch(request);
    response = await response.json();
    let flightPlan = JSON.parse(JSON.stringify(response));
    // display clicked flight filght details
    displayCurrentFlightPlan(flightPlan);
    // remove any printed path from the map
    removeExistingPaths();
    // print the path of the flight on the map
    printSegments(flightPlan);
    // highlight the clicked flight marker
    changeFlightMarker(flightPlan);
    // highlight the clicked flight in the list
    highlightFlight(flightPlan);
}


// show flight details
async function displayCurrentFlightPlan(flightPlan) {
    try {
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
        document.getElementById("originLatitude").innerHTML = origin.latitude;
        document.getElementById("destLongitude").innerHTML = destination.longitude;
        document.getElementById("destLatitude").innerHTML = destination.latitude;
        document.getElementById("departureTime").innerHTML = depTime.toUTCString();
        document.getElementById("arrivalTime").innerHTML = arrivalTime.toUTCString();
        currentFlightPlan.style.display = 'initial'
    }
    catch {
        let errormsg = document.getElementById('errorTxt');
        errormsg.innerHTML = 'error displaying flight details';
        errormsg.parentElement.style.display = 'initial';
    }
}

// remove flight details from the view
function resetFlightPlanView() {
    try {
        let currentFlightPlan = document.getElementById('currentFlightPlan');
        currentFlightPlan.style.display = 'none';
    }
    catch {
        let errormsg = document.getElementById('errorTxt');
        errormsg.innerHTML = 'error reseting flight plan view';
        errormsg.parentElement.style.display = 'initial';
    }
}


// calculate the flight time of flight
function getArrivalTime(segments) {
    let flightTime = 0;
    for (seg of segments) {
        flightTime += seg.timespan_seconds;
    }
    return flightTime;
}


function allowDrop(event) {
    event.preventDefault();
    displayDragImage();
}


function displayDragImage() {
    let flightListElement = document.getElementById("flightList");
    let children = flightListElement.children;
    for (i = 0; i < children.length; i++) {
        children[i].style.display = 'none';
    }
    flightListElement.style.backgroundImage = "url(resources/dropzone.jpg)";
    flightListElement.style.backgroundPosition = 'center';
    flightListElement.style.backgroundSize = '190px 220px';
    flightListElement.style.backgroundRepeat = 'no-repeat';
}


function dragLeave(event) {
    event.preventDefault();
    let flightListElement = document.getElementById("flightList");
    let children = flightListElement.children;
    for (i = 0; i < children.length; i++) {
        children[i].style.display = 'initial';
    }
    flightListElement.style.backgroundImage = "initial";
    flightListElement.style.backgroundPosition = 'center';
    flightListElement.style.backgroundSize = '190px 220px';
    flightListElement.style.backgroundRepeat = 'no-repeat';
}


// event when a file is dropped on flight list
function drop(event) {
    // try to read the file and create a new flight (or flights) from it
    try {
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
    catch {
        let errormsg = document.getElementById('errorTxt');
        errormsg.innerHTML = 'error loading new flight';
        errormsg.parentElement.style.display = 'initial';
    }
}

// attemp to post a new flight from the files data using ajax
async function postFlightFromFile(fileData) {
    try {
        let xhttp = new XMLHttpRequest();
        await xhttp.open("POST", "api/FlightPlan", true);
        xhttp.setRequestHeader("Content-Type", "application/json")
        xhttp.onload = () => {
            if (xhttp.status != 201) {
                let errormsg = document.getElementById('errorTxt');
                errormsg.innerHTML = 'error loading new flight';
                errormsg.parentElement.style.display = 'initial';
            }
        }
        await xhttp.send(fileData);
        
    }
    catch(err) {
        let errormsg = document.getElementById('errorMsg');
        errormsg.innerHTML = 'error loading new flight';
    }
}

// remove the error message from screen
function removeErrorMsg(button) {
    document.getElementById('errorMsg').style.display = 'none';
    ///This is how you resume animation:
    /*document.getElementById('errorMsg').style.animation = 'none';
    setTimeout(function () {
        document.getElementById('errorMsg').style.animation = '';
    }, 10);*/
    ///Plus, we need to change back display to whatever it should be, do all of that in a different fucntion.
}


// highlight the given flight in the flight list
function highlightFlight(flightPlan) {
    let FlightElement = document.getElementById(flightPlan.flight_id);
    FlightElement.style.border = '0.5px solid'
    FlightElement.style.fontWeight = 'bold'
    FlightElement.style.backgroundColor = '#87CEEB';
    FlightElement.style.height = '30px';
    if (currentlyHighlightedFlight != null) {
        FlightElement = document.getElementById(currentlyHighlightedFlight.flight_id);
        FlightElement.style.backgroundColor = 'none';
        FlightElement.style.border = 'none';
        FlightElement.style.fontWeight = 'none'
        FlightElement.style.height = 'none';
    }
    currentlyHighlightedFlight = flightPlan;
}

// removes the highlight of of the flight that is currently highlighted
function removeHighlight() {
    if (currentlyHighlightedFlight == null) {
        return;
    }
    let FlightElement = document.getElementById(currentlyHighlightedFlight.flight_id);
    FlightElement.style.backgroundColor = 'none';
    FlightElement.style.border = 'none';
    FlightElement.style.fontWeight = 'none'
    FlightElement.style.height = 'none';
    currentlyHighlightedFlight = null;
}

// sends a DELETE request of a flight from flight list using ajax
async function deleteFlight(deleteButton, event) {
    try {
        event.stopPropagation();
        let flightElement = deleteButton.parentElement;
        let flightID = flightElement.id;
        var xhttp = new XMLHttpRequest();
        await xhttp.open("DELETE", "api/Flights/" + flightID, true);
        xhttp.setRequestHeader("Content-Type", "application/json")
        await xhttp.send();
    }
    catch {
        let errormsg = document.getElementById('errorTxt');
        errormsg.innerHTML = 'error deleting flight from server';
        errormsg.parentElement.style.display = 'initial';
    }
}

