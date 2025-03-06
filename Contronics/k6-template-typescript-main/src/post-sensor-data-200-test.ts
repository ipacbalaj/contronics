import { check, sleep } from 'k6';
import http from 'k6/http';
import { Options } from 'k6/options';

// Define a list of 100 `deveui` values (node1 to node100)
const devEuiList = Array.from({ length: 100 }, (_, i) => `node${i + 1}`);

export let options: Options = {
  vus: 10, // Number of virtual users
  duration: '5s', // Test duration
};

function generateLoRaMessage(
  analogue1: number, 
  analogue2: number, 
  digital1: boolean, 
  digital2: boolean, 
  state: number, 
  voltageState: number, 
  hasSettingsBeenUpdated: boolean
): string {
    
    const _BYTE_MSG_SIZE = 9; // Adjust size as per your actual payload
    let generatedMsg = new Uint8Array(_BYTE_MSG_SIZE);

    // Custom Epoch Time: 2023-01-01 00:00:00 UTC
    const epochTimeOfDay = 1672531200;
    
    // Get current timestamp
    let now = Math.floor(Date.now() / 1000); // Convert milliseconds to seconds
    let timeSinceEpoch = now - epochTimeOfDay;

    // Encode analogue sensor values
    addAnalogueIntToArray(generatedMsg, analogue1, digital1, 0);
    addAnalogueIntToArray(generatedMsg, analogue2, digital2, 2);

    // Encode timestamp (big-endian format)
    generatedMsg[4] = (timeSinceEpoch >> 24) & 0xFF;
    generatedMsg[5] = (timeSinceEpoch >> 16) & 0xFF;
    generatedMsg[6] = (timeSinceEpoch >> 8) & 0xFF;
    generatedMsg[7] = timeSinceEpoch & 0xFF;

    // Encode state, voltage, and settings flag
    generatedMsg[8] = (state << 4) | (voltageState << 2) | (hasSettingsBeenUpdated ? 1 : 0);

    // Convert byte array to hex string
    return Array.from(generatedMsg).map(byte => byte.toString(16).padStart(2, "0")).join("").toUpperCase();
}

// Function to encode analogue values into byte array
function addAnalogueIntToArray(buffer: Uint8Array, value: number, sixBit: boolean, startIndex: number) {
    let isNegative = value < 0;
    let absValue = Math.abs(value);

    buffer[startIndex] = ((absValue >> 8) & 0x3F) | (sixBit ? 0x40 : 0) | (isNegative ? 0x80 : 0);
    buffer[startIndex + 1] = absValue & 0xFF;
}

// Example Usage
const encodedLoRaMessage = generateLoRaMessage(300, -150, true, false, 2, 3, true);
console.log("Encoded LoRa Message:", encodedLoRaMessage);

console.log(generateLoRaMessage(300, -150, true, false, 2, 3, true));

function getRandomLoRaPayload() {
  return {
    tmst: Math.floor(Math.random() * 1000000000),
    chan: Math.floor(Math.random() * 10),
    rfch: Math.floor(Math.random() * 2),
    freq: (Math.random() * (868 - 863) + 863).toFixed(1),
    stat: 1,
    modu: "LORA",
    datr: "SF7BW125",
    codr: "4/5",
    lsnr: (Math.random() * 20 - 10).toFixed(1),
    rssi: Math.floor(Math.random() * (-30 - (-130)) + (-130)),
    opts: "",
    size: Math.floor(Math.random() * 100),
    fcnt: Math.floor(Math.random() * 1000),
    cls: 0,
    port: Math.floor(Math.random() * 256),
    mhdr: "404209ce0180fe01",
    data: generateLoRaMessage(300, -150, true, false, 2, 3, true),
    appeui: "a8-40-41-00-00-00-01-00",
    deveui: "node1",//devEuiList[Math.floor(Math.random() * devEuiList.length)], // Select a random deveui
    name: null,
    devaddr: "01ce0942",
    ack: false,
    adr: true,
    gweui: "00-80-00-00-00-01-f9-c7",
    seqn: Math.floor(Math.random() * 1000),
    time: getUTCTimeWithMicroseconds()
  };
}

function getUTCTimeWithMicroseconds(): string {
  const now = new Date();
  const pad = (num: number, size: number) => num.toString().padStart(size, '0');

  const year = now.getUTCFullYear();
  const month = pad(now.getUTCMonth() + 1, 2);
  const day = pad(now.getUTCDate(), 2);
  const hours = pad(now.getUTCHours(), 2);
  const minutes = pad(now.getUTCMinutes(), 2);
  const seconds = pad(now.getUTCSeconds(), 2);
  const milliseconds = pad(now.getUTCMilliseconds(), 3);
  
  // Generate 6-digit microseconds by appending extra random digits
  const microseconds = milliseconds + Math.floor(Math.random() * 1000).toString().padStart(3, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}.${microseconds}Z`;
}

// Example usage
console.log(getUTCTimeWithMicroseconds());


export default function () {
  const url = 'http://10.80.16.199/api/v1/lorawan/APPUITEST/DEVUITEST/ups'; // Update this if necessary
  const payload = JSON.stringify(getRandomLoRaPayload());
  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(url, payload, params);

  check(res, {
    'status is 200': () => res.status === 200
  });

  sleep(1);
}
