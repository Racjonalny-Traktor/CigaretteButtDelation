import map from './map.js';
import {getNewCigarettesNumber, getZoom} from './map.js';
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
    let zoom = getZoom();
    let factor = (number/100000) * (zoom*zoom*zoom);
    let percent = Math.round((factor * 100) / 80);
    updateCigarettesText(number.toString());
    updatePollutionText(percent.toString());
}

function updateCigarettesText(numOfCigarettes){
    $('.numOfCigarettes').text(`${numOfCigarettes} butts in this area`);
}

function updatePollutionText(percent){
    if (percent > 50){
        $('.pollutionLevel').addClass('red');
        $('.pollutionLevel').removeClass('green');
    }else{
        $('.pollutionLevel').addClass('green');
        $('.pollutionLevel').removeClass('red');
    }
    $('.pollutionLevel').text(`${percent}%`);
}



