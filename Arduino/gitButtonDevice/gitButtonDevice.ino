int combuffp = 0;
char combuff[21];

int blinking = 0;

int pin13 = LOW;

void setup() {
  combuff[20] = 0;
  Serial.begin(9600);

  pinMode(3, OUTPUT);
  pinMode(13, INPUT);
}

void loop() {

  // Check command from PC.
  int inputchar = Serial.read();
  if (inputchar == 10 || inputchar == 13) {
    combuff[combuffp] = 0;
    combuffp = 0;
    if (strcmp(combuff, "BB") == 0) {
      blinking = 1;
    }
    else if (strcmp(combuff, "EB") == 0) {
      blinking = 0;
    }
  }
  else if (inputchar != -1) {
    combuff[combuffp] = inputchar;
    combuffp = (combuffp + 1) % 20;
  }

  // Blink LED
  if (blinking == 1) {
    unsigned long def = millis() % 2000;
    def = ( def <= 1000 ? def : 2000 - def );
    analogWrite( 3, (int)( def / 1000.0 * 255 ) );
  }
  else {
    analogWrite( 3, 0 );
  }

  // Check button pressed or not.
  int neoPin13 = digitalRead(13) == 0 ? LOW : HIGH;
  if (pin13 != neoPin13) {
    pin13 = neoPin13;
    if (pin13 == HIGH) {
      Serial.println("GIT COMMIT");
    }
    
    // Avoid chuttering.
    delay(100);
  }
}

