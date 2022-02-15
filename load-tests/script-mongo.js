import http from 'k6/http';
import { sleep } from 'k6';
export const options = {
  vus: 100,
  duration: '30s',
  thresholds: {
    'http_req_duration{status:200}': ['max>=0'],
    'http_req_duration{status:500}': ['max>=0'],
  },
  'summaryTrendStats': ['min', 'med', 'avg', 'p(90)', 'p(95)', 'max', 'count'],
};
export default function () {
  http.get('http://localhost:5000/api/v1/mongo-person?pageSize=10&pageNumber=1');
  sleep(1);
  http.get('http://localhost:5000/api/v1/mongo-person?search=Fox&pageSize=10&pageNumber=1&modifiedStartDate=2021-01-12T15:24:22.285552-06:00');
  sleep(1);
}
