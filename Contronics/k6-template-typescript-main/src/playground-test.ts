export default function generateLoRaMessage(
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

function addAnalogueIntToArray(buffer: Uint8Array, value: number, sixBit: boolean, startIndex: number) {
  let isNegative = value < 0;
  let absValue = Math.abs(value);

  buffer[startIndex] = ((absValue >> 8) & 0x3F) | (sixBit ? 0x40 : 0) | (isNegative ? 0x80 : 0);
  buffer[startIndex + 1] = absValue & 0xFF;
}

// Example Usage
const encodedLoRaMessage = generateLoRaMessage(300, -150, true, false, 2, 3, true);
console.log("Encoded LoRa Message:", encodedLoRaMessage);
