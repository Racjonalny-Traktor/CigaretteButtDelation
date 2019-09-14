mapboxgl.accessToken = 'pk.eyJ1Ijoia3Bla2FsYSIsImEiOiJjazBqcDl0dXIwYzJxM2dzdmVoMGdsbmx3In0.LUI-WrcAUM_It4GHMo9hvQ';
var map = new mapboxgl.Map({
    container: 'mapContainer',
    style: 'mapbox://styles/mapbox/streets-v11',
    center: [20.831121, 52.109050],
    zoom: 11
});

export default map;