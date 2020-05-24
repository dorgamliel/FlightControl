let intervalID = window.setInterval(updateFlightList, 200);
document.getElementById("demo").innerHTML = "working"
let t = new Date().toISOString();

async function updateFlightList() {
    let t = new Date().toISOString();
    let request = new Request('/api/Flights?relative_to=' + t + '&sync_all');
    let flightList
    let response = await fetch(request)
    response = await response.json();
    flightList = JSON.parse(JSON.stringify(response));
    printFlightList(flightList);
}


function printFlightList(flightList) {
    
    let HTMLFlightList = document.getElementById("flightList");
    HTMLFlightList.innerHTML = '';
    for (flight of flightList) {
        let newFlight = document.createElement('li');
        let flightID = document.createElement('p');
        let airline = document.createElement('p');
        flightID.innerHTML = 'Flight ID: ' + JSON.stringify(flight.flight_id);
        airline.innerHTML = 'Airline: ' + JSON.stringify(flight.company_name);
        newFlight.append(flightID);
        newFlight.append(airline);
        HTMLFlightList.append(newFlight);
    }
}

function displayCurrentFlightPlan(flight) {
    let currentFlightPlan = document.getElementById("currentFlightPlan");
    currentFlightPlan.innerHTML = '';
}
