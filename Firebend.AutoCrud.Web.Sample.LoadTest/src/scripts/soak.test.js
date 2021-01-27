import { sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import {
  makeRequestParams,
  getDatadogMonitorEvents,
  randomString,
  randomElement,
  randomInt,
} from '../common/utils.js';
import {
  makeManufacturersApi,
  makeBrandsApi,
  makeCategoriesApi,
  makeItemsApi,
  makeItemAttributesApi,
  makeItemMeasurementsApi,
} from '../common/item-catalog.js';

const failedRequests = new Counter('failed_requests');
const endpointResponseTime = new Trend('response_time', true);
const cpuAlerts = new Counter('datadog_cpu_usage_alerts');
const restartAlerts = new Counter('datadog_pod_restart_alerts');
const memoryAlerts = new Counter('datadog_memory_usage_alerts');

export let options = {
  // soak test load
  stages: [
    { duration: '2m', target: 4 }, // ramp up
    { duration: '6m', target: 4 }, // hold
    { duration: '2m', target: 0 } // ramp down
  ],

  thresholds: {
    // datadog monitors

    cpuAlerts: [ 'count < 1 ' ],
    restartAlerts: [ 'count < 1' ],
    memoryAlerts: [ 'count < 1' ],

    // failed requests

    failed_requests: [ 'count < 1' ],

    'failed_requests{tag:get_manufacturers_paged}': ['count < 1'],
    'failed_requests{tag:get_manufacturers_all}': ['count < 1'],
    'failed_requests{tag:get_manufacturers_single}': ['count < 1'],
    'failed_requests{tag:get_manufacturers_changes}': ['count < 1'],
    'failed_requests{tag:post_manufacturers_single}': ['count < 1'],
    'failed_requests{tag:post_manufacturers_multiple}': ['count < 1'],
    'failed_requests{tag:put_manufacturers}': ['count < 1'],
    'failed_requests{tag:patch_manufacturers}': ['count < 1'],
    'failed_requests{tag:delete_manufacturers}': ['count < 1'],
    'failed_requests{tag:export_manufacturers_csv}': ['count < 1'],
    'failed_requests{tag:export_manufacturers_excel}': ['count < 1'],

    'failed_requests{tag:get_brands_paged}': ['count < 1'],
    'failed_requests{tag:get_brands_all}': ['count < 1'],
    'failed_requests{tag:get_brands_single}': ['count < 1'],
    'failed_requests{tag:get_brands_changes}': ['count < 1'],
    'failed_requests{tag:post_brands_single}': ['count < 1'],
    'failed_requests{tag:post_brands_multiple}': ['count < 1'],
    'failed_requests{tag:put_brands}': ['count < 1'],
    'failed_requests{tag:patch_brands}': ['count < 1'],
    'failed_requests{tag:delete_brands}': ['count < 1'],
    'failed_requests{tag:export_brands_csv}': ['count < 1'],
    'failed_requests{tag:export_brands_excel}': ['count < 1'],

    'failed_requests{tag:get_items_paged}': ['count < 1'],
    'failed_requests{tag:get_items_all}': ['count < 1'],
    'failed_requests{tag:get_items_single}': ['count < 1'],
    'failed_requests{tag:get_items_changes}': ['count < 1'],
    'failed_requests{tag:post_items_single}': ['count < 1'],
    'failed_requests{tag:post_items_multiple}': ['count < 1'],
    'failed_requests{tag:put_items}': ['count < 1'],
    'failed_requests{tag:patch_items}': ['count < 1'],
    'failed_requests{tag:delete_items}': ['count < 1'],
    'failed_requests{tag:export_items_csv}': ['count < 1'],
    'failed_requests{tag:export_items_excel}': ['count < 1'],

    'failed_requests{tag:get_item_attributes_paged}': ['count < 1'],
    'failed_requests{tag:get_item_attributes_all}': ['count < 1'],
    'failed_requests{tag:get_item_attributes_single}': ['count < 1'],
    'failed_requests{tag:get_item_attributes_changes}': ['count < 1'],
    'failed_requests{tag:post_item_attributes_single}': ['count < 1'],
    'failed_requests{tag:post_item_attributes_multiple}': ['count < 1'],
    'failed_requests{tag:put_item_attributes}': ['count < 1'],
    'failed_requests{tag:patch_item_attributes}': ['count < 1'],
    'failed_requests{tag:delete_item_attributes}': ['count < 1'],
    'failed_requests{tag:export_item_attributes_csv}': ['count < 1'],
    'failed_requests{tag:export_item_attributes_excel}': ['count < 1'],

    'failed_requests{tag:get_item_measurements_paged}': ['count < 1'],
    'failed_requests{tag:get_item_measurements_all}': ['count < 1'],
    'failed_requests{tag:get_item_measurements_single}': ['count < 1'],
    'failed_requests{tag:get_item_measurements_changes}': ['count < 1'],
    'failed_requests{tag:post_item_measurements_single}': ['count < 1'],
    'failed_requests{tag:post_item_measurements_multiple}': ['count < 1'],
    'failed_requests{tag:put_item_measurements}': ['count < 1'],
    'failed_requests{tag:patch_item_measurements}': ['count < 1'],
    'failed_requests{tag:delete_item_measurements}': ['count < 1'],
    'failed_requests{tag:export_item_measurements_csv}': ['count < 1'],
    'failed_requests{tag:export_item_measurements_excel}': ['count < 1'],

    // response times

    http_req_duration: [ 'p(99) < 1500' ],

    'response_time{tag:get_manufacturers_paged}': ['p(99) < 1500'],
    'response_time{tag:get_manufacturers_all}': ['p(99) < 1500'],
    'response_time{tag:get_manufacturers_single}': ['p(99) < 1500'],
    'response_time{tag:get_manufacturers_changes}': ['p(99) < 1500'],
    'response_time{tag:post_manufacturers_single}': ['p(99) < 1500'],
    'response_time{tag:post_manufacturers_multiple}': ['p(99) < 1500'],
    'response_time{tag:put_manufacturers}': ['p(99) < 1500'],
    'response_time{tag:patch_manufacturers}': ['p(99) < 1500'],
    'response_time{tag:delete_manufacturers}': ['p(99) < 1500'],
    'response_time{tag:export_manufacturers_csv}': ['p(99) < 1500'],
    'response_time{tag:export_manufacturers_excel}': ['p(99) < 1500'],

    'response_time{tag:get_brands_paged}': ['p(99) < 1500'],
    'response_time{tag:get_brands_all}': ['p(99) < 1500'],
    'response_time{tag:get_brands_single}': ['p(99) < 1500'],
    'response_time{tag:get_brands_changes}': ['p(99) < 1500'],
    'response_time{tag:post_brands_single}': ['p(99) < 1500'],
    'response_time{tag:post_brands_multiple}': ['p(99) < 1500'],
    'response_time{tag:put_brands}': ['p(99) < 1500'],
    'response_time{tag:patch_brands}': ['p(99) < 1500'],
    'response_time{tag:delete_brands}': ['p(99) < 1500'],
    'response_time{tag:export_brands_csv}': ['p(99) < 1500'],
    'response_time{tag:export_brands_excel}': ['p(99) < 1500'],

    'response_time{tag:get_items_paged}': ['p(99) < 1500'],
    'response_time{tag:get_items_all}': ['p(99) < 1500'],
    'response_time{tag:get_items_single}': ['p(99) < 1500'],
    'response_time{tag:get_items_changes}': ['p(99) < 1500'],
    'response_time{tag:post_items_single}': ['p(99) < 1500'],
    'response_time{tag:post_items_multiple}': ['p(99) < 1500'],
    'response_time{tag:put_items}': ['p(99) < 1500'],
    'response_time{tag:patch_items}': ['p(99) < 1500'],
    'response_time{tag:delete_items}': ['p(99) < 1500'],
    'response_time{tag:export_items_csv}': ['p(99) < 1500'],
    'response_time{tag:export_items_excel}': ['p(99) < 1500'],

    'response_time{tag:get_item_attributes_paged}': ['p(99) < 1500'],
    'response_time{tag:get_item_attributes_all}': ['p(99) < 1500'],
    'response_time{tag:get_item_attributes_single}': ['p(99) < 1500'],
    'response_time{tag:get_item_attributes_changes}': ['p(99) < 1500'],
    'response_time{tag:post_item_attributes_single}': ['p(99) < 1500'],
    'response_time{tag:post_item_attributes_multiple}': ['p(99) < 1500'],
    'response_time{tag:put_item_attributes}': ['p(99) < 1500'],
    'response_time{tag:patch_item_attributes}': ['p(99) < 1500'],
    'response_time{tag:delete_item_attributes}': ['p(99) < 1500'],
    'response_time{tag:export_item_attributes_csv}': ['p(99) < 1500'],
    'response_time{tag:export_item_attributes_excel}': ['p(99) < 1500'],

    'response_time{tag:get_item_measurements_paged}': ['p(99) < 1500'],
    'response_time{tag:get_item_measurements_all}': ['p(99) < 1500'],
    'response_time{tag:get_item_measurements_single}': ['p(99) < 1500'],
    'response_time{tag:get_item_measurements_changes}': ['p(99) < 1500'],
    'response_time{tag:post_item_measurements_single}': ['p(99) < 1500'],
    'response_time{tag:post_item_measurements_multiple}': ['p(99) < 1500'],
    'response_time{tag:put_item_measurements}': ['p(99) < 1500'],
    'response_time{tag:patch_item_measurements}': ['p(99) < 1500'],
    'response_time{tag:delete_item_measurements}': ['p(99) < 1500'],
    'response_time{tag:export_item_measurements_csv}': ['p(99) < 1500'],
    'response_time{tag:export_item_measurements_excel}': ['p(99) < 1500'],
  },

  setupTimeout: '1m'
};

// setup runs once before any virtual users are provisioned.
// put login step here to prevent auth on every iteration
export const setup = () => {
  const time = Date.now(); // record the test start time for datadog
  const requestParams = makeRequestParams();

  return { requestParams, time };
};

// test
export default (data) => {
  const { requestParams } = data;

  const manufacturersApi = makeManufacturersApi(
    requestParams,
    endpointResponseTime,
    failedRequests
  );
  
  const brandsApi = makeBrandsApi(
    requestParams,
    endpointResponseTime,
    failedRequests
  );

  const categoriesApi = makeCategoriesApi(
    requestParams,
    endpointResponseTime,
    failedRequests
  );
  
  const itemsApi = makeItemsApi(
    requestParams,
    endpointResponseTime,
    failedRequests
  );

  // create a manufacturer

  let manufacturer = manufacturersApi.postOne({
    name: `Load test manufacturer ${randomString()}`,
    code: randomString(3),
  });

  sleep(1);

  const manufacturers = manufacturersApi.getPaged();

  sleep(1);

  // create a brand

  let brand = brandsApi.postOne({
    name: `Load test brand ${randomString()}`,
    code: randomString(3),
    manufacturerId: manufacturer.json('id')
  });

  sleep(1);

  const brands = brandsApi.getPaged();

  sleep(1);

  // create a category

  let category = categoriesApi.postOne({
    name: `Load test brand ${randomString()}`,
    code: randomString(3),
  });

  sleep(1);

  const categories = categoriesApi.getPaged();

  sleep(1);

  // create an item

  const items = [];
  const numItems = 20;

  for (let i = 0; i < numItems; i++) {
    items.push(
      {
        name: `Load test item ${randomString()}`,
        code: randomString(3),
        description: 'I have, like, a billion of these',
        hasInstances: false,
        modelNumber: randomString(),
        packSize: 0,
        partNumber: randomString(),
        isBreakPack: false,
        maxGroupSize: 1,
        brandId: brand.json('id'),
        parentItemId: null
      }
    );
  };

  let createdItems = itemsApi.postMultiple(items).json('created');

  sleep(1);

  // create attributes and measurements for each item, then fetch the changes

  if (createdItems && createdItems.length > 0) {
    createdItems.forEach(item => {
      const attributesApi = makeItemAttributesApi(
        item.id,
        requestParams,
        endpointResponseTime,
        failedRequests
      );

      const measurementsApi = makeItemMeasurementsApi(
        item.id,
        requestParams,
        endpointResponseTime,
        failedRequests
      );

      // attributesApi.postMultiple([
      //   { name: 'color', value: randomElement(['red', 'green', 'blue', 'orange', 'purple']) },
      //   { name: 'material', value: randomElement(['stainless steel', 'plush', 'pvc', 'aluminum', 'wood']) },
      // ]);

      attributesApi.postOne({
        name: 'color',
        value: randomElement(['red', 'green', 'blue', 'orange', 'purple'])
      });

      // measurementsApi.postMultiple([
      //   { name: 'width', value: randomInt(1,10), unitOfMeasure: 'inches' },
      //   { name: 'length', value: randomInt(1,10), unitOfMeasure: 'inches' },
      //   { name: 'height', value: randomInt(1,10), unitOfMeasure: 'inches' },
      //   { name: 'weight', value: randomInt(1,10), unitOfMeasure: 'pounds' },
      // ]);

      measurementsApi.postOne({
        name: 'width',
        value: randomInt(1,10),
        unitOfMeasure: 'inches'
      });

      sleep(1);

      const changes = itemsApi.getChanges(item.id);
    });
  }

  sleep(1);

  let fetchedItems = itemsApi.getPaged();

  sleep(1);

  // now, fetch and export a bunch of data

  let exportedManufacturers = manufacturersApi.export('manufacturers.csv');

  sleep(1);

  let exportedBrands = brandsApi.export('brands.csv');

  sleep(1);

  let exportedCategories = categoriesApi.export('categories.csv');

  sleep(1);

  let exportedItems = itemsApi.export('items.csv');

  sleep(1);
};

export function teardown(data) {
  const { time } = data;
  const monitorTags = ['kube_container_name:platform-item-catalog'];

  const events = getDatadogMonitorEvents(monitorTags, time);

  if (events && events.length > 0) {
    events.forEach((event) => {
      console.log(event.title);
      if (event.title.includes('CPU')) {
        cpuAlerts.add(1);
      } else if (event.title.includes('restarted')) {
        restartAlerts.add(1);
      } else if (event.title.includes('memory')) {
        memoryAlerts.add(1);
      }
    });
  }
}
