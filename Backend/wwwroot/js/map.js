import fetcher from './fetcher.js';

mapboxgl.accessToken = 'pk.eyJ1Ijoia3Bla2FsYSIsImEiOiJjazBqcDl0dXIwYzJxM2dzdmVoMGdsbmx3In0.LUI-WrcAUM_It4GHMo9hvQ';
var map = new mapboxgl.Map({
    container: 'mapContainer',
    style: 'mapbox://styles/mapbox/streets-v11',
    center: [20.830443, 52.113],
     zoom: 16
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
        "heatmap-weight": [
            "interpolate",
            ["linear"],
            ["get", "mag"],
            0, 0,
            6, 1
        ],
        "heatmap-intensity": [
            "interpolate",
            ["linear"],
            ["zoom"],
            0, 1,
            maxZoom, 3
        ],
   
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
        "heatmap-radius": [
            "interpolate",
            ["linear"],
            ["zoom"],
            0, 2,
            maxZoom, 20
        ],
        }}, 
        'waterway-label');
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

        map.dragRotate.disable();
        map.touchZoomRotate.disableRotation();
            
    
})

export function getNewCigarettesNumber(cigarettes){
    const canvas = map.getCanvas()
    const w = canvas.width
    const h = canvas.height
    const cUL = map.unproject ([0,0]).toArray()
    const cUR = map.unproject ([w,0]).toArray()
    const cLR = map.unproject ([w,h]).toArray()
    const cLL = map.unproject ([0,h]).toArray()
    const coordinates = [cUL,cUR,cLR,cLL]
    let number = 0;

    for(let cig of cigarettes){
        if(cig.lat > cUL[0] && cig.lat < cUR[0] && cig.long < cUL[1] && cig.long < cUR[1]){
            number += cig.cigarettesNum;
        }
    }

    return number;
}
export function getZoom(){
    return Math.round(map.getZoom());
}
export default map;