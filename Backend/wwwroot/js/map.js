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
        "id": "cigarettes-heat",
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
                // location of the feature, with description HTML from its properties.
        map.on('click', 'cigarettes-heat', function (e) {
            var coordinates = e.features[0].geometry.coordinates.slice();
            var description = e.features[0].properties.description;
            console.log(coordinates);

            while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
            coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
            new mapboxgl.Popup()
            .setLngLat(coordinates)
            .setHTML(description)
            .addTo(map);
            }
        });

        map.on('mouseenter', 'cigarettes-heat', function () {
            map.getCanvas().style.cursor = 'pointer';
        });
             
            // Change it back to a pointer when it leaves.
            map.on('mouseleave', 'cigarettes-heat', function () {
            map.getCanvas().style.cursor = '';
        });
            
    
})

export default map;