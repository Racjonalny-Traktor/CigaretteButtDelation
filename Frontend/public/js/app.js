import map from './map.js';
import fetcher from './fetcher.js';

fetcher.fetchLitter().then( data =>{
    console.log(data);
});

map.on('click', (e) =>{
    let lat = e.lngLat.lat;
    let long = e.lngLat.lng;
    let cigarettesNum = Math.floor(Math.random() * 10) %10
    let pic = new File([''], '/assets/pety.jpg');

    let data = {lat, long, cigarettesNum, pic};
    fetcher.sendLitter(data).then(() =>{

    }).catch(reason =>{
        console.log(reason);
    });
})