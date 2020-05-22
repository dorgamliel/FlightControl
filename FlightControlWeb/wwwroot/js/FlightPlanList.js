document.getElementById("demo").innerHTML = "working"
let t = new Date().toISOString();

function submit() {
    let t = new Date().toISOString();
    let request = new Request('/api/Flight/' + t);
    let flightList
    fetch(request)
        // reads the body of the response as json
        .then(response => response.json())
        .then(response => flightList = JSON.parse(JSON.stringify(response)))
        .then(response => document.getElementById("flightList").innerHTML = JSON.stringify(flightList))
        .catch(error => console.log(error))
}
