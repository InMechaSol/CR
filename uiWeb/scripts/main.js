



if (typeof(Worker) !== "undefined") {
    var worker = new Worker('worker.js');
    worker.addEventListener('message',function(e){
      // we got a message / array buffer from the worker thread
    },false);
    var ab = new ArrayBuffer(1);
    worker.postMessage(ab, [ab]);
    if (ab.byteLength) {
        alert('Transferables are not supported in your browser!');
    } else {
        // Transferables are supported.
    }
  } else {
    alert('Web Workers are not supported in your browser!');
}

var LT = document.getElementById("localtime");
var IC = document.getElementById("intervals");
var intervalCounter = 0;

let myVar = setInterval(myTimer ,100);
clearInterval(myVar);

function startTimer() {
  myVar = setInterval(myTimer ,100);
}
  
function stopTimer() {
  clearInterval(myVar);
}

function myTimer() {
  const d = new Date();
  if(intervalCounter%10==0){
    LT.innerHTML = d.toLocaleTimeString();    
  } 
    
  IC.innerHTML = intervalCounter.toString();
  
  intervalCounter++;
}





