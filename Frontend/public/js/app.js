import map from './map.js';
import {getNewCigarettesNumber} from './map.js';
import fetcher from './fetcher.js';
import './jquery.js';

var cigarettes = [];
var numOfCigarettes = 0;

fetcher.fetchLitter().then( data =>{
    cigarettes = data;
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
$(document).ready(() =>{
    console.log('ready!');
    updateCigarettesText('0');

    setInterval(findNumberOfCigarettesInArea, 3000)
});

function findNumberOfCigarettesInArea(){
    let number = getNewCigarettesNumber(cigarettes);
    console.log(number);
    updateCigarettesText(number.toString());
}

function updateCigarettesText(numOfCigarettes){
    $('.numOfCigarettes').text(`Na tym obszarze jest ${numOfCigarettes} petów`)
}



