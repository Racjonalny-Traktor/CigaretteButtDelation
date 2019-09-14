import { baseUrl } from "./utils.js";

const headers = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': '*',
    'Origin': '*',
    'Access-Control-Request-Headers': '*',
    'Content-type': 'application/json'
};
class Fetcher{

    postData(url, data){
        return fetch(url, {
            method:  'POST',
            body: JSON.stringify(data),
            headers:{'Content-Type': 'application/json'}
        })
    }

    fetchLitter(){  
        const url = `${baseUrl}/api/Litter`;
        return fetch(url, headers).then(response => response.json());
    }
}

export default new Fetcher();