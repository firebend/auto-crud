import { check, group, sleep, fail } from 'k6';
import http from 'k6/http';

export const makeTrackedGetRequest = (
  tag,
  path,
  requestParams,
  endpointResponseTime = null,
  failedRequests = null
) => group(tag, () => {
  const response = http.get(`${__ENV.BASE_URL}${path}`, requestParams);

  if (endpointResponseTime) {
    endpointResponseTime.add(response.timings.duration, { tag });
  }

  if (failedRequests) {
    if (!check(response, { 'successful request': (res) => res.status === 200 && res.status <= 300 })) {
      console.log(`${tag} request failed`, response.body);
      failedRequests.add(1, { 'status': response.status, tag });
    }
  }

  return response;
});

export const makeTrackedPostRequest = (
  tag,
  path,
  body,
  requestParams,
  endpointResponseTime = null,
  failedRequests = null
) => group(tag, () => {
  const response = http.post(`${__ENV.BASE_URL}${path}`, JSON.stringify(body), requestParams);

  if (endpointResponseTime) {
    endpointResponseTime.add(response.timings.duration, { tag });
  }

  if (failedRequests) {
    if (!check(response, { 'successful request': (res) => res.status >= 200 && res.status <= 300 })) {
      console.log(`${tag} request failed`, JSON.stringify(body), response.body);
      failedRequests.add(1, { 'status': response.status, tag });
    }
  }

  return response;
});

export const makeTrackedPutRequest = (
  tag,
  path,
  body,
  requestParams,
  endpointResponseTime = null,
  failedRequests = null
) => group(tag, () => {
  const response = http.put(`${__ENV.BASE_URL}${path}`, JSON.stringify(body), requestParams);

  if (endpointResponseTime) {
    endpointResponseTime.add(response.timings.duration, { tag });
  }

  if (failedRequests) {
    if (!check(response, { 'successful request': (res) => res.status >= 200 && res.status <= 300 })) {
      console.log(`${tag} request failed`, JSON.stringify(body), response.body);
      failedRequests.add(1, { 'status': response.status, tag });
    }
  }

  return response;
});

export const makeTrackedPatchRequest = (
  tag,
  path,
  body,
  requestParams,
  endpointResponseTime = null,
  failedRequests = null
) => group(tag, () => {
  const response = http.patch(`${__ENV.BASE_URL}${path}`, JSON.stringify(body), requestParams);

  if (endpointResponseTime) {
    endpointResponseTime.add(response.timings.duration, { tag });
  }

  if (failedRequests) {
    if (!check(response, { 'successful request': (res) => res.status >= 200 && res.status <= 300 })) {
      console.log(`${tag} request failed`, JSON.stringify(body), response.body);
      failedRequests.add(1, { 'status': response.status, tag });
    }
  }

  return response;
});

export const makeTrackedDeleteRequest = (
  tag,
  path,
  body,
  requestParams,
  endpointResponseTime = null,
  failedRequests = null
) => group(tag, () => {
  const response = http.del(`${__ENV.BASE_URL}${path}`, JSON.stringify(body), requestParams);

  if (endpointResponseTime) {
    endpointResponseTime.add(response.timings.duration, { tag });
  }

  if (failedRequests) {
    if (!check(response, { 'successful request': (res) => res.status >= 200 && res.status <= 300 })) {
      console.log(`${tag} request failed`, JSON.stringify(body), response.body);
      failedRequests.add(1, { 'status': response.status, tag });
    }
  }

  return response;
});

export const makeTrackedCrudRequestsForEntity = (
  tag,
  entityBasePath,
  requestParams,
  endpointResponseTime,
  failedRequests
) => ({
  getPaged: (search = null, page = 1, pageSize = 25) => makeTrackedGetRequest(
    `get_${tag}_paged`,
    `${entityBasePath}?search=${search}&pageNumber=${page}&pageSize=${pageSize}`,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  getAll: () => makeTrackedGetRequest(
    `get_${tag}_all`,
    `${entityBasePath}/all`,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  getOne: (id) => makeTrackedGetRequest(
    `get_${tag}_single`,
    `${entityBasePath}/${id}`,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  getChanges: (id, search = null, page = 1, pageSize = 25) => makeTrackedGetRequest(
    `get_${tag}_changes`,
    `${entityBasePath}/${id}/changes?search=${search}&pageNumber=${page}&pageSize=${pageSize}`,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  postOne: (body) => makeTrackedPostRequest(
    `post_${tag}_single`,
    `${entityBasePath}`,
    body,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  postMultiple: (body) => makeTrackedPostRequest(
    `post_${tag}_multiple`,
    `${entityBasePath}/multiple`,
    body,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  put: (id, body) => makeTrackedPutRequest(
    `put_${tag}`,
    `${entityBasePath}/${id}`,
    body,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  patch: (id, body) => makeTrackedPatchRequest(
    `patch_${tag}`,
    `${entityBasePath}/${id}`,
    body,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  delete: (id, body) => makeTrackedDeleteRequest(
    `delete_${tag}`,
    `${entityBasePath}/${id}`,
    body,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
  export: (filename, type = 'csv', search = null, page = 1, pageSize = 25) => makeTrackedGetRequest(
    `export_${tag}_${type}`,
    `${entityBasePath}/export/${type}?filename=${filename}search=${search}&pageNumber=${page}&pageSize=${pageSize}`,
    requestParams,
    endpointResponseTime,
    failedRequests
  ),
});

// timeToWait is in seconds
export const waitFor = (request, check, timeToWait = 10, maxAttempts = 200) => {
  for(var c = 0; c < maxAttempts; c++) {
    const response = request();
    const result = check(response);

    if (result) {
      return response;
    }

    sleep(timeToWait);
  }

  fail(`waitFor expired after ${timeToWait*maxAttempts} seconds`);
};

export const getDatadogMonitorEvents = (monitorTags, start, end = Date.now()) => {
  if (!__ENV.DATADOG_API_KEY || !__ENV.DATADOG_APP_KEY) {
    console.error('No datadog credentials provided, skipping...');
    return null;
  }

  const datadogApiRoot = 'https://api.datadoghq.com/api/v1';
  const urlParams = {
    api_key: __ENV.DATADOG_API_KEY,
    application_key: __ENV.DATADOG_APP_KEY,
    tags: monitorTags.join(','),
    start: Math.floor(start / 1000),
    end: Math.floor(end / 1000),
  };

  const paramsString = Object.keys(urlParams).map(k => `${k}=${urlParams[k]}`).join('&');

  const url = `${datadogApiRoot}/events?${paramsString}`;
  let response = http.get(url, {
    headers: { ['Content-Type']: 'application/x-www-form-urlencoded' },
  });

  let body = response.json();

  return body.events && body.events.length > 0 ? body.events : [];
};

export const randomInt = (min = Math.floor, max = Math.ceil) => Math.floor(Math.random() * (max - min + 1)) + min;

export const randomString = (length = 10, chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789') => {
  let str = '';
  for (let i = 0; i < length; i++) {
    str += chars.charAt(Math.floor(Math.random() * chars.length));
  }

  return str;
};

export const randomElement = (arr) => arr.length && arr.length > 0
  ? arr[randomInt(0, arr.length-1)]
  : null;

export const randomElements = (arr, num = 2) => {
  if (arr.length && arr.length >= num) {
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }

    return arr.slice(0, num);
  }

  return null;
};
