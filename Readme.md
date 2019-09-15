*HackYeah 2019 project*

---
# Cigarette Butt Delation
### Report those cigarette-butt-throwers, street litterers and world-polluters
---


The app is live on https://fajeczky.scm.azurewebsites.net/. At the index website we can see the map of pollution in the world. 

Rest API documentation is available at https://fajeczky.scm.azurewebsites.net/swagger/.

All chatbot logic is set up in DialogFlow - the tool from Google to support chatbot development. The chatbot communicates with backend server using `/api/google/try` endpoint, where data gets processed and response to user is being sent.
