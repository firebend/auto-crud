import http from 'k6/http';
import { check, group, sleep, fail } from 'k6';
import { Counter, Trend } from 'k6/metrics';
export const options = {
  vus: 100,
  duration: '30s',
  thresholds: {
    'http_req_duration{status:200}': ['max>=0'],
    'http_req_duration{status:500}': ['max>=0'],
  },
  'summaryTrendStats': ['min', 'med', 'avg', 'p(90)', 'p(95)', 'max', 'count'],
};

export const randomString = (length = 10, chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789') => {
  let str = '';
  for (let i = 0; i < length; i++) {
    str += chars.charAt(Math.floor(Math.random() * chars.length));
  }

  return str;
};

const failedRequests = new Counter('failed_requests');
const endpointResponseTime = new Trend('response_time', true);
const tag = 'ef_person';

const checkRequest = (response) => {
  if (endpointResponseTime) {
    endpointResponseTime.add(response.timings.duration, { tag });
  }

  if (failedRequests) {
    if (!check(response, { 'successful request': (res) => res.status >= 200 && res.status <= 300 })) {
      failedRequests.add(1, { 'status': response.status, tag });
    }
  }
}

export default function () {
  const root = 'http://localhost:5020/api/v1/ef-person';

  var headers = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBdXRvQ3J1ZEpXVFNlcnZpY2VBY2Nlc3NUb2tlbiIsImp0aSI6IjhhMGFiMmExLThiYzUtNDNjMC1iZmMzLWI5Y2E4ZjZhNTkxOCIsImlhdCI6MTcwOTA2MzY0MiwiVXNlcklkIjoiNDE4ZDY0ZjItZmY4ZS00YjYxLTk1M2UtNmQ3ZjkxOGNlZDYzIiwiZW1haWwiOiJkZXZlbG9wZXJAdGVzdC5jb20iLCJuYmYiOjE3MDkwNjM2NDIsImV4cCI6MTcwOTA2NzI0MiwiaXNzIjoiQXV0b0NydWQiLCJhdWQiOiJBdXRvQ3J1ZENsaWVudCJ9.XWHKTVhRtbs0IE1-nGwaXQGghXGqezQz9vxR5xjUuc8',
    }
  };

  var res = http.post(root,JSON.stringify({ firstName: randomString(), lastName: randomString(), dataAuth: {} }),  headers);
  checkRequest(res);

  var created = res.json();
  var id = created.id;
  var firstname = created.firstName;
  sleep(1);

  checkRequest(http.get(`${root}/${id}`, headers));
  sleep(1);

  checkRequest(http.put(`${root}/${id}`,
    JSON.stringify({ firstName: randomString(), lastName: randomString(), dataAuth: {} }),  headers));

  checkRequest(http.get(`${root}?pageSize=10&pageNumber=1`, headers));
  sleep(1);

  checkRequest(http.get(`${root}?search=${firstname}&pageSize=10&pageNumber=1`, headers));
  sleep(1);

  checkRequest(http.get(`${root}/export/csv?filename=export&pageNumber=1&pageSize=10`, headers));
  sleep(1);

  checkRequest(http.get(`${root}/export/spreadSheet?filename=export&pageSize=50&pageNumber=1`, headers));
  sleep(1);
}
