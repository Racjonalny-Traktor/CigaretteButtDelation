import map from './map.js';
import fetcher from './fetcher.js';


fetcher.fetchLitter().then(data =>{
    console.log(data);
})