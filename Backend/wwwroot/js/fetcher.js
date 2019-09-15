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
            body: data
        })
    }

    fetchLitter(){  
        const url = `${baseUrl}/api/Litter`;
        return fetch(url, headers).then(response => response.json());
    }

    sendLitter(data){
        let formData = new FormData();
        formData.append('lat', data.long);
        formData.append('long', data.lat);
        formData.append('cigarettesNum', data.cigarettesNum);
        formData.append('pic', data.pic);

        return this.postData(`${baseUrl}/api/Litter`, formData);
    }
}

export default new Fetcher();