mapboxgl.accessToken = 'pk.eyJ1Ijoia3Bla2FsYSIsImEiOiJjazBqcDl0dXIwYzJxM2dzdmVoMGdsbmx3In0.LUI-WrcAUM_It4GHMo9hvQ';
var map = new mapboxgl.Map({
    container: 'mapContainer',
    style: 'mapbox://styles/mapbox/streets-v11',
    center: [20.829425, 52.1119135],
     zoom: 13
});
const maxZoom = 22;
map.on('load', () =>{
    map.addSource('earthquakes', {
        "type": "geojson",
        "data": "https://fajeczky.azurewebsites.net/api/Litter/geojson"
        });
         
        map.addLayer({
        "id": "earthquakes-heat",
        "type": "heatmap",
        "source": "earthquakes",
        "maxzoom": maxZoom,
        "paint": {
        // Increase the heatmap weight based on frequency and property magnitude
        "heatmap-weight": [
            "interpolate",
            ["linear"],
            ["get", "mag"],
            0, 0,
            6, 1
        ],
        // Increase the heatmap color weight weight by zoom level
        // heatmap-intensity is a multiplier on top of heatmap-weight
        "heatmap-intensity": [
            "interpolate",
            ["linear"],
            ["zoom"],
            0, 1,
            maxZoom, 3
        ],
        // Color ramp for heatmap.  Domain is 0 (low) to 1 (high).
        // Begin color ramp at 0-stop with a 0-transparancy color
        // to create a blur-like effect.
        "heatmap-color": [
            "interpolate",
            ["linear"],
            ["heatmap-density"],
            0, "rgba(33,102,172,0)",
            0.2, "rgb(103,169,207)",
            0.4, "rgb(209,229,240)",
            0.6, "rgb(253,219,199)",
            0.8, "rgb(239,138,98)",
            1, "rgb(178,24,43)"
        ],
        // Adjust the heatmap radius by zoom level
        "heatmap-radius": [
            "interpolate",
            ["linear"],
            ["zoom"],
            0, 2,
            maxZoom, 20
        ],
        // Transition from heatmap to circle layer by zoom level
        /*"heatmap-opacity": [
            "interpolate",
            ["linear"],
            ["zoom"],
            7, 1,
            maxZoom, 0
            ]*/
        }}, 
        'waterway-label');
         
        /*map.addLayer({
        "id": "earthquakes-point",
        "type": "circle",
        "source": "earthquakes",
        "minzoom": 7,
        "paint": {
        // Size circle radius by earthquake magnitude and zoom level
        "circle-radius": [
        "interpolate",
        ["linear"],
        ["zoom"],
        7, [
        "interpolate",
        ["linear"],
        ["get", "mag"],
        1, 1,
        6, 4
        ],
        16, [
        "interpolate",
        ["linear"],
        ["get", "mag"],
        1, 5,
        6, 50
        ]
        ],
        // Color circle by earthquake magnitude
        "circle-color": [
        "interpolate",
        ["linear"],
        ["get", "mag"],
        1, "rgba(33,102,172,0)",
        2, "rgb(103,169,207)",
        3, "rgb(209,229,240)",
        4, "rgb(253,219,199)",
        5, "rgb(239,138,98)",
        6, "rgb(178,24,43)"
        ],
        "circle-stroke-color": "white",
        "circle-stroke-width": 1,
        // Transition from heatmap to circle layer by zoom level
        "circle-opacity": [
        "interpolate",
        ["linear"],
        ["zoom"],
        7, 0,
        8, 1
        ]
        }
        }, 'waterway-label');*/
})

export default map;