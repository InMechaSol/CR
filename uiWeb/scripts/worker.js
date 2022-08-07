self.addEventListener('message', function(e){
    // we got a message / array buffer from the main thread
    ;
  }, false);

var intervalCounter = 0;

let myVar2 = setInterval(myTimer ,100);


function myTimer() {
  const d = new Date();
  if(intervalCounter%10==0){
    // once per second    
    ;
  }
  // ten times per second
  
  intervalCounter++;
}

