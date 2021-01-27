import { sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import {
  randomString,
  makeTrackedCrudRequestsForEntity,
} from '../common/utils.js';

const failedRequests = new Counter('failed_requests');
const endpointResponseTime = new Trend('response_time', true);

export const makePersonApi = (
  requestParams,
  endpointResponseTime,
  failedRequests
) => makeTrackedCrudRequestsForEntity(
  'ef_person',
  `/api/v1/ef-person`,
  requestParams,
  endpointResponseTime,
  failedRequests
);

export let options = {
  // soak test load
  stages: [
    { duration: '2m', target: 4 }, // ramp up
    { duration: '6m', target: 4 }, // hold
    { duration: '2m', target: 0 } // ramp down
  ],

  thresholds: {
    // datadog monitors

    // failed requests

    failed_requests: [ 'count < 1' ],

    'failed_requests{tag:get_ef_person_paged}': ['count < 1'],
    'failed_requests{tag:get_ef_person_all}': ['count < 1'],
    'failed_requests{tag:get_ef_person_single}': ['count < 1'],
    'failed_requests{tag:get_ef_person_changes}': ['count < 1'],
    'failed_requests{tag:post_ef_person_single}': ['count < 1'],
    'failed_requests{tag:post_ef_person_multiple}': ['count < 1'],
    'failed_requests{tag:put_ef_person}': ['count < 1'],
    'failed_requests{tag:patch_ef_person}': ['count < 1'],
    'failed_requests{tag:delete_ef_person}': ['count < 1'],
    'failed_requests{tag:export_ef_person_csv}': ['count < 1'],
    'failed_requests{tag:export_ef_person_excel}': ['count < 1'],

    // response times

    http_req_duration: [ 'p(99) < 1500' ],

    'response_time{tag:get_ef_person_paged}': ['p(99) < 1500'],
    'response_time{tag:get_ef_person_all}': ['p(99) < 1500'],
    'response_time{tag:get_ef_person_single}': ['p(99) < 1500'],
    'response_time{tag:get_ef_person_changes}': ['p(99) < 1500'],
    'response_time{tag:post_ef_person_single}': ['p(99) < 1500'],
    'response_time{tag:post_ef_person_multiple}': ['p(99) < 1500'],
    'response_time{tag:put_ef_person}': ['p(99) < 1500'],
    'response_time{tag:patch_ef_person}': ['p(99) < 1500'],
    'response_time{tag:delete_ef_person}': ['p(99) < 1500'],
    'response_time{tag:export_ef_person_csv}': ['p(99) < 1500'],
    'response_time{tag:export_ef_person_excel}': ['p(99) < 1500'],
  },

  setupTimeout: '1m'
};

// setup runs once before any virtual users are provisioned.
// put login step here to prevent auth on every iteration
export const setup = () => {
  const requestParams = { headers: { 'Content-Type': 'application/json' }};

  return { requestParams };
};

// test
export default (data) => {
  const { requestParams } = data;

  const personApi = makePersonApi(
    requestParams,
    endpointResponseTime,
    failedRequests
  );

  let person = personApi.postOne({
    firstName: `Load tester`,
    lastName: randomString()
  });

  sleep(1);

  let updated = personApi.put(person.json('id'), { firstName: 'Load testerrrrr', lastName: randomString() });

  sleep(1);

  let changes = personApi.getChanges(person.json('id'), null, 1, 20);

  sleep(1);

  let people = personApi.getPaged(null, 1, 20);

  sleep(1);

  let newPeople = personApi.postMultiple([
    { firstName: 'Load tester 111', lastName: randomString() },
    { firstName: 'Load tester 222', lastName: randomString() }
  ]);

  sleep(1);

  people = personApi.getPaged(null, 1, 20);

  sleep(1);

  let csv = personApi.export('test.csv');

  sleep(1);
};

