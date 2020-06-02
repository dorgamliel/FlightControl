let polylines = []

function initMap () {
  try {
    // Map properties.
    const properties = {
      zoom: 13,
      center: { lat: 32.0055, lng: 34.8854 }
    }
    // Map object.
    window.map = new google.maps.Map(document.getElementById('map'), properties)
    // Array of markers.
    window.markers = []
    window.map.addListener('click', function () {
      resetFlightPlanView()
      removeExistingPaths()
      resetPlaneMarker()
      removeHighlight()
    })
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error initializing map'
    errormsg.parentElement.style.display = 'initial'
  }
}

// Add Marker function.
function addMarker (props, newFlight) {
  try {
    var planeMarker = {
      url: 'resources/plane.png',
      anchor: new google.maps.Point(20, 20),
      scaledSize: new google.maps.Size(40, 40),
    }
    const marker = new google.maps.Marker({
      id: props.id,
      position: props.coords,
      content: props.content,
      map: window.map
    })
    marker.setIcon(planeMarker)
    // Adding a marker to list of map markers.
    window.markers.push(marker)
    var infoWindow = new google.maps.InfoWindow({
      content: props.id
    })
    marker.addListener('click', function () {
      flightClicked(newFlight)
    })
    marker.addListener('mouseover', function () {
      infoWindow.open(window.map, marker)
    })
    marker.addListener('mouseout', function () {
      infoWindow.close()
    })
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error adding new flight to map'
    errormsg.parentElement.style.display = 'initial'
  }
}

// print flight path on map
function printSegments (flightPlan) {
  try {
    var lineSymbol = {
      path: 'M 0,-1 0,1',
      strokeOpacity: 1,
      strokeColor: 'black',
      scale: 2
    }
    const flightPath = new google.maps.Polyline({
      geodesic: true,
      icons: [{
        icon: lineSymbol,
        offset: '0',
        repeat: '20px'
      }],
      strokeOpacity: 0
    })
    const arr = []
    const coupl = {
      lat: flightPlan.initial_location.latitude,
      lng: flightPlan.initial_location.longitude
    }
    arr.push(coupl)
    for (let i = 0; i < flightPlan.segments.length; i++) {
      const segPoint = flightPlan.segments[i]
      const couple = { lat: segPoint.latitude, lng: segPoint.longitude }
      arr.push(couple)
    }
    flightPath.setPath(arr)
    polylines.push(flightPath)
    flightPath.setMap(window.map)
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error printing flight path on map'
    errormsg.parentElement.style.display = 'initial'
  }
}

// highlight flight marker (larger marker and yellow color)
function changeFlightMarker (flightPlan) {
  try {
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
      if (marker.id === flightPlan.flight_id) {
        marker.setIcon(clickedPlaneMarker)
      } else {
        marker.setIcon(planeMarker)
      }
    }
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error highlighting flight marker'
    errormsg.parentElement.style.display = 'initial'
  }
}

// Adding/Updating markers in map.
function addOrUpdateMarker (flight, newFlight) {
  try {
    for (marker of window.markers) {
      if (marker.id === flight.flight_id) {
        marker.setPosition({ lat: flight.latitude, lng: flight.longitude })
        return
      }
    }
    addMarker({
      coords: { lat: flight.latitude, lng: flight.longitude },
      // content: flight.company_name,
      id: flight.flight_id
    }, newFlight)
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error updating flight markers'
    errormsg.parentElement.style.display = 'initial'
  }
}

// Removing unactive flights from map.
function removeUnactiveFlights (flightList) {
  try {
    // Remove unactive flights from map.
    for (let i = 0; i < window.markers.length; i++) {
      if (flightList.filter(x => x.flight_id === window.markers[i].id).length == 0) {
        window.markers[i].setMap(null)
        window.markers[i].setPosition(null)
        window.markers.splice(i, 1)
        i--
      }
    }
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error removing unactive flight markers'
    errormsg.parentElement.style.display = 'initial'
}
}

// delete all existing flight paths from map
function removeExistingPaths() {
  try {
    for (polyline of polylines) {
      polyline.setMap(null)
    }
    polylines = []
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error removing flight paths from map'
    errormsg.parentElement.style.display = 'initial'
  }
}

// change back to original marker
function resetPlaneMarker () {
  try {
    var planeMarker = {
      url: 'resources/plane.png',
      anchor: new google.maps.Point(20, 20),
      scaledSize: new google.maps.Size(40, 40),
    }
    for (marker of window.markers) {
      marker.setIcon(planeMarker)
    }
  } catch {
    const errormsg = document.getElementById('errorTxt')
    errormsg.innerHTML = 'error reseting flight marker'
    errormsg.parentElement.style.display = 'initial'
  }
}
