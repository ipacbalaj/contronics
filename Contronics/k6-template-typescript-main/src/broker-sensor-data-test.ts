import { check, sleep } from 'k6';
import http from 'k6/http';
import { Options } from 'k6/options';

export let options: Options = {
  vus: 100, // Number of virtual users
  duration: '20s', // Test duration
};

function getRandomSensorData() {
  return {
    SensorId: `sensor-${Math.floor(Math.random() * 1000)}`,
    Value: (Math.random() * 100).toFixed(2),
    Timestamp: new Date().toISOString(),
  };
}

export default function () {
  const url = 'http://localhost:5046/api/forward-sensor-data'; // Update this if necessary
  const payload = JSON.stringify(getRandomSensorData());
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
